Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Collision3D

    ''' <summary>
    ''' 补充的凸多面体工厂：圆台、半球、棱柱、棱锥、薄板。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="ConvexMesh3D"/> 原有的工厂覆盖了盒体、圆柱、圆锥、楔形与球面网格，
    ''' 但缺少"圆台 / 半球"这两类常见的积木形状。这里补齐，全部返回严格凸的
    ''' 网格（顶点 + 面环，绕向由 <see cref="ConvexMesh3D"/> 的构造函数统一修正），
    ''' 因此可以直接交给 <see cref="ConvexPolyhedronCollider3D"/> 做碰撞，
    ''' 也可以直接扇形三角化后交给渲染。
    '''
    ''' 轴向约定与原有工厂一致：局部 **Y 轴** 为轴向。
    ''' </remarks>
    Public Module ConvexPrimitives3D

        ''' <summary>
        ''' 圆台（截头圆锥）：底面半径 <paramref name="bottomRadius"/>，
        ''' 顶面半径 <paramref name="topRadius"/>，高度 <paramref name="height"/>。
        ''' 顶面半径为 0 时退化为圆锥。
        ''' </summary>
        Public Function CreateFrustum(bottomRadius As Double, topRadius As Double, height As Double,
                                      Optional segment As Integer = 16) As ConvexMesh3D

            Dim seg As Integer = std.Max(3, segment)
            Dim r1 As Double = std.Max(1.0E-4, bottomRadius)
            Dim r2 As Double = std.Max(0.0, topRadius)
            Dim h As Double = std.Max(1.0E-4, height)

            If r2 < 1.0E-4 Then
                Return ConvexMesh3D.CreateCone(r1, h, seg)
            End If

            Dim vs(2 * seg - 1) As Vector3

            For i As Integer = 0 To seg - 1
                Dim a As Double = 2.0 * std.PI * i / seg
                Dim cx As Double = std.Cos(a)
                Dim cz As Double = std.Sin(a)

                vs(i) = New Vector3(r1 * cx, -h * 0.5, r1 * cz)
                vs(seg + i) = New Vector3(r2 * cx, h * 0.5, r2 * cz)
            Next

            Dim fs As New List(Of Integer())()

            For i As Integer = 0 To seg - 1
                Dim j As Integer = (i + 1) Mod seg

                fs.Add(New Integer() {i, j, seg + j, seg + i})
            Next

            Dim top(seg - 1) As Integer
            Dim bottom(seg - 1) As Integer

            For i As Integer = 0 To seg - 1
                top(i) = seg + i
                bottom(seg - 1 - i) = i
            Next

            fs.Add(top)
            fs.Add(bottom)

            Return New ConvexMesh3D(vs, fs.ToArray()).Centered()
        End Function

        ''' <summary>
        ''' 半球：平面在局部 <c>y = 0</c>，球冠朝 <c>+Y</c>。
        ''' </summary>
        ''' <param name="radius">球半径</param>
        ''' <param name="segment">赤道方向的分段数</param>
        ''' <param name="rings">从极点到赤道的环数（1 表示只有一圈三角带）</param>
        Public Function CreateHemisphere(radius As Double,
                                         Optional segment As Integer = 16,
                                         Optional rings As Integer = 6) As ConvexMesh3D

            Dim seg As Integer = std.Max(3, segment)
            Dim rv As Integer = std.Max(1, rings)
            Dim r As Double = std.Max(1.0E-4, radius)

            Dim vs(1 + rv * seg - 1) As Vector3

            ' 极点
            vs(0) = New Vector3(0.0, r, 0.0)

            For j As Integer = 1 To rv
                Dim theta As Double = 0.5 * std.PI * j / rv
                Dim y As Double = r * std.Cos(theta)
                Dim rr As Double = r * std.Sin(theta)

                For i As Integer = 0 To seg - 1
                    Dim phi As Double = 2.0 * std.PI * i / seg

                    vs(1 + (j - 1) * seg + i) = New Vector3(rr * std.Cos(phi), y, rr * std.Sin(phi))
                Next
            Next

            Dim fs As New List(Of Integer())()

            ' 极点扇形
            For i As Integer = 0 To seg - 1
                fs.Add(New Integer() {0, 1 + i, 1 + ((i + 1) Mod seg)})
            Next

            ' 中间四边形带
            For j As Integer = 0 To rv - 2
                For i As Integer = 0 To seg - 1
                    Dim a As Integer = 1 + j * seg + i
                    Dim b As Integer = 1 + j * seg + ((i + 1) Mod seg)
                    Dim c As Integer = 1 + (j + 1) * seg + ((i + 1) Mod seg)
                    Dim d As Integer = 1 + (j + 1) * seg + i

                    fs.Add(New Integer() {a, b, c, d})
                Next
            Next

            ' 底面（赤道环，所在平面 y = 0）
            Dim bottom(seg - 1) As Integer
            Dim lastRing As Integer = 1 + (rv - 1) * seg

            For i As Integer = 0 To seg - 1
                bottom(seg - 1 - i) = lastRing + i
            Next

            fs.Add(bottom)

            Return New ConvexMesh3D(vs, fs.ToArray()).Centered()
        End Function

        ''' <summary>正棱柱（<paramref name="sides"/> 边形为底面，局部 Y 轴为轴向）。</summary>
        Public Function CreatePrism(sides As Integer, radius As Double, height As Double) As ConvexMesh3D
            Return ConvexMesh3D.CreateCylinder(radius, height, std.Max(3, sides))
        End Function

        ''' <summary>正棱锥（<paramref name="sides"/> 边形为底面，顶点朝 +Y）。</summary>
        Public Function CreatePyramid(sides As Integer, radius As Double, height As Double) As ConvexMesh3D
            Return ConvexMesh3D.CreateCone(radius, height, std.Max(3, sides))
        End Function

        ''' <summary>薄板（长 × 厚 × 宽），等价于一个指定尺寸的长方体。</summary>
        Public Function CreatePlank(width As Double, thickness As Double, depth As Double) As ConvexMesh3D
            Return ConvexMesh3D.CreateBox(width, thickness, depth)
        End Function

        ''' <summary>
        ''' 圆盘（很扁的圆柱）：平放在 <c>y = 0</c> 附近，适合做"硬币/托盘"类积木。
        ''' </summary>
        Public Function CreateDisc(radius As Double, thickness As Double, Optional segment As Integer = 20) As ConvexMesh3D
            Return ConvexMesh3D.CreateCylinder(radius, std.Max(1.0E-4, thickness), std.Max(3, segment))
        End Function
    End Module

End Namespace
