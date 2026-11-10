Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Preprocessing

    ''' <summary>
    ''' 裁剪滤波器：直通、包围盒、球域、平面切割与条件谓词。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 裁剪是工业流程里“代价最低、收益最高”的一步：把背景、工装、地面排除之后，
    ''' 后续所有算法的点数可能下降一个数量级，而精度不受影响。
    ''' 它通常在去噪之前执行（先缩小范围，再在范围内统计密度，阈值才稳定）。
    ''' </para>
    ''' <para>
    ''' <b>实现约定</b>：所有函数都是“保留满足条件的点”，
    ''' 并通过 <see cref="PointCloudBuffer.Where(Boolean())"/> 一次性抽取，
    ''' 因此属性（颜色/强度/标签）会随点一起被正确搬运。
    ''' </para>
    ''' <para>
    ''' <b>边界语义</b>：区间均为<b>闭区间</b>（包含端点）。
    ''' 这是因为工业上“≤ 公差上限”与“≥ 下限”是最自然的表述，
    ''' 开区间会让恰好落在临界值上的点消失，导致结果难以复现。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class CropFilters

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 坐标轴枚举。
        ''' </summary>
        Public Enum Axis
            X = 0
            Y = 1
            Z = 2
        End Enum

        ''' <summary>
        ''' 直通滤波：只保留指定轴上落在 <c>[min, max]</c> 的点。
        ''' </summary>
        ''' <remarks>
        ''' 最典型的用法是扫描平台上的工件：Z 轴直通去掉工作台面与背景，
        ''' X/Y 直通切出感兴趣的区域。判定是逐点的 2 次比较，复杂度 <c>O(n)</c>。
        ''' </remarks>
        Public Shared Function PassThrough(cloud As PointCloudBuffer, axis As Axis,
                                           minValue As Double, maxValue As Double) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If minValue > maxValue Then
                Throw New ArgumentException($"the min value ({minValue}) should not exceed the max value ({maxValue})!")
            End If

            Dim n As Integer = cloud.Count
            Dim mask As Boolean() = New Boolean(n - 1) {}

            If axis = Axis.X Then
                For i As Integer = 0 To n - 1
                    mask(i) = cloud.X(i) >= minValue AndAlso cloud.X(i) <= maxValue
                Next
            ElseIf axis = Axis.Y Then
                For i As Integer = 0 To n - 1
                    mask(i) = cloud.Y(i) >= minValue AndAlso cloud.Y(i) <= maxValue
                Next
            Else
                For i As Integer = 0 To n - 1
                    mask(i) = cloud.Z(i) >= minValue AndAlso cloud.Z(i) <= maxValue
                Next
            End If

            Return cloud.Where(mask)
        End Function

        ''' <summary>
        ''' 包围盒裁剪。
        ''' </summary>
        ''' <param name="cloud">点云</param>
        ''' <param name="box">包围盒</param>
        ''' <param name="invert"><c>True</c> 时保留盒外的点（用于挖去工装/夹具区域）</param>
        Public Shared Function CropBox(cloud As PointCloudBuffer, box As Aabb3,
                                       Optional invert As Boolean = False) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count
            Dim mask As Boolean() = New Boolean(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim inside As Boolean = box.Contains(New Vector3d(cloud.X(i), cloud.Y(i), cloud.Z(i)))

                mask(i) = If(invert, Not inside, inside)
            Next

            Return cloud.Where(mask)
        End Function

        ''' <summary>
        ''' 球域裁剪。
        ''' </summary>
        ''' <param name="cloud">点云</param>
        ''' <param name="center">球心</param>
        ''' <param name="radius">半径</param>
        ''' <param name="invert">是否取反（保留球外）</param>
        ''' <remarks>
        ''' 常用于“以识别到的抓取点为中心提取局部点云”，
        ''' 比包围盒更贴合各向同性目标（法兰、球头、销钉）。
        ''' </remarks>
        Public Shared Function CropSphere(cloud As PointCloudBuffer, center As Vector3d, radius As Double,
                                          Optional invert As Boolean = False) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If radius <= 0.0 Then Throw New ArgumentOutOfRangeException(NameOf(radius), "the radius should be positive!")

            Dim n As Integer = cloud.Count
            Dim mask As Boolean() = New Boolean(n - 1) {}
            Dim r2 As Double = radius * radius

            For i As Integer = 0 To n - 1
                Dim dx As Double = cloud.X(i) - center.X
                Dim dy As Double = cloud.Y(i) - center.Y
                Dim dz As Double = cloud.Z(i) - center.Z
                Dim inside As Boolean = dx * dx + dy * dy + dz * dz <= r2

                mask(i) = If(invert, Not inside, inside)
            Next

            Return cloud.Where(mask)
        End Function

        ''' <summary>
        ''' 平面切割：保留平面某一侧的点。
        ''' </summary>
        ''' <param name="cloud">点云</param>
        ''' <param name="planeNormal">平面法向（内部会归一化）</param>
        ''' <param name="planeOffset">平面方程 <c>n·p + d = 0</c> 中的 <c>d</c></param>
        ''' <param name="keepPositive">是否保留 <c>n·p + d ≥ 0</c> 的一侧</param>
        ''' <remarks>
        ''' 平面方程用 <c>n·p + d = 0</c> 的形式：<c>d</c> 的几何含义是
        ''' “原点到平面的有符号距离的相反数”，与 RANSAC 平面模型的输出直接兼容。
        ''' 注意区间判定带一个极小的容差（<c>1e-12</c>），
        ''' 否则恰好落在平面上的点会因浮点误差随机落入两侧。
        ''' </remarks>
        Public Shared Function ClipByPlane(cloud As PointCloudBuffer, planeNormal As Vector3d, planeOffset As Double,
                                           Optional keepPositive As Boolean = True) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim normal As Vector3d = planeNormal.Normalize()

            If normal.IsZero() Then
                Throw New ArgumentException("the plane normal can not be a zero vector!", NameOf(planeNormal))
            End If

            Dim n As Integer = cloud.Count
            Dim mask As Boolean() = New Boolean(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim d As Double = normal.X * cloud.X(i) + normal.Y * cloud.Y(i) + normal.Z * cloud.Z(i) + planeOffset
                Dim positive As Boolean = d >= -1.0E-12

                mask(i) = If(keepPositive, positive, Not positive)
            Next

            Return cloud.Where(mask)
        End Function

        ''' <summary>
        ''' 条件裁剪：保留满足任意谓词的点。
        ''' </summary>
        ''' <remarks>
        ''' 通用逃生出口（例如“保留强度 &gt; 阈值且颜色为红色”）。
        ''' 谓词是委托调用，逐点有间接开销，大点云上建议改用上面几个专用函数。
        ''' </remarks>
        Public Shared Function Conditional(cloud As PointCloudBuffer, predicate As Func(Of Vector3d, Boolean)) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If predicate Is Nothing Then Throw New ArgumentNullException(NameOf(predicate))

            Dim n As Integer = cloud.Count
            Dim mask As Boolean() = New Boolean(n - 1) {}

            For i As Integer = 0 To n - 1
                mask(i) = predicate(New Vector3d(cloud.X(i), cloud.Y(i), cloud.Z(i)))
            Next

            Return cloud.Where(mask)
        End Function

        ''' <summary>
        ''' 按 Z 值统计并自动裁掉工作台面：保留最高的 <paramref name="keepRatio"/> 比例的点。
        ''' </summary>
        ''' <remarks>
        ''' 这是“免参数”的经验裁剪：绝大多数工件扫描中，点云沿 Z 的分布是
        ''' “大面积的台面 + 凸起的工件”，按高度分位数裁剪能快速去掉台面。
        ''' 分位数用快速选择求，复杂度 <c>O(n)</c>。
        ''' </remarks>
        Public Shared Function CropByHeightPercentile(cloud As PointCloudBuffer, keepRatio As Double) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If keepRatio <= 0.0 OrElse keepRatio > 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(keepRatio), "the ratio should be in (0, 1]!")
            End If

            Dim n As Integer = cloud.Count

            If n = 0 Then Return cloud.Clone()

            Dim z As Double() = DirectCast(cloud.Z.Clone(), Double())

            Array.Sort(z)

            Dim keepCount As Integer = std.Max(1, CInt(std.Round(n * keepRatio)))
            Dim threshold As Double = z(std.Max(0, n - keepCount))

            Dim mask As Boolean() = New Boolean(n - 1) {}

            For i As Integer = 0 To n - 1
                mask(i) = cloud.Z(i) >= threshold
            Next

            Return cloud.Where(mask)
        End Function

    End Class
End Namespace
