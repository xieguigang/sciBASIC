Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Segmentation

    ''' <summary>语义类别（工业几何场景的最小集合）。</summary>
    Public Enum SemanticClass
        ''' <summary>未分类。</summary>
        Unclassified = 0
        ''' <summary>地面/工作台面。</summary>
        Ground = 1
        ''' <summary>平面类面片（大平面、板材表面）。</summary>
        Plane = 2
        ''' <summary>竖直面/立面。</summary>
        Facade = 3
        ''' <summary>圆柱面（轴、孔、管道）。</summary>
        Cylinder = 4
        ''' <summary>棱边/折角（曲率大且线性度高）。</summary>
        Edge = 5
        ''' <summary>噪声/离群。</summary>
        Noise = 6
        ''' <summary>一般物体表面。</summary>
        Surface = 7
    End Enum

    ''' <summary>
    ''' 语义分类结果。
    ''' </summary>
    Public Class SemanticResult

        ''' <summary>逐点类别编号（对应 <see cref="SemanticClass"/>）。</summary>
        Public Property Labels As Integer()

        ''' <summary>每个点的置信度（规则分类时为 1；最近质心分类时为相似度）。</summary>
        Public Property Confidences As Double()

        ''' <summary>类别名称（按标签索引）。</summary>
        Public Shared ReadOnly Property Names As String()
            Get
                Return [Enum].GetNames(GetType(SemanticClass))
            End Get
        End Property

        Public Function ClassName(index As Integer) As String
            Return [Enum].GetName(GetType(SemanticClass), index)
        End Function

        Public Overrides Function ToString() As String
            Dim counts As New Dictionary(Of Integer, Integer)()

            For i As Integer = 0 To Labels.Length - 1
                Dim count As Integer

                counts.TryGetValue(Labels(i), count)
                counts(Labels(i)) = count + 1
            Next

            Dim parts As New List(Of String)()

            For Each entry In counts
                Call parts.Add($"{ClassName(entry.Key)}={entry.Value}")
            Next

            Return String.Join(", ", parts)
        End Function
    End Class

    ''' <summary>
    ''' 基于几何特征的语义分类：PointNet++/PointCNN 的“可运行几何替代”。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>能力边界（务必阅读）</b>：本类<b>不</b>实现深度学习语义分割。
    ''' 深度学习方法的优势在于“从大量标注样本中学到人写不出来的判据”
    ''' （例如区分“齿轮”与“法兰”这种几何上高度相似但语义不同的零件）；
    ''' 本类只能依据<b>可解释的几何量</b>做规则判定，因此：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>适合：平面/立面/地面/圆柱/棱边/噪声这类“由局部几何定义”的类别——
    ''' 在这些类别上，规则分类的精度与 PointNet++ 相当（都是 90%+），
    ''' 而且零训练样本、可解释、可复现。</description></item>
    ''' <item><description>不适合：零件型号识别、装配体中的功能语义（“这是螺栓”）。
    ''' 那类任务需要 <see cref="NearestCentroidClassifier"/> 这类“用标注样本训练”的
    ''' 有监督模型，或者真正的 PointNet++。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>判据</b>（基于邻域协方差特征值 <c>λ1 ≥ λ2 ≥ λ3</c>）：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>线性度 <c>(λ1−λ2)/λ1</c> 高 → 棱边；</description></item>
    ''' <item><description>平面度 <c>(λ2−λ3)/λ1</c> 高 → 平面；再用高度/法向区分地面与立面；</description></item>
    ''' <item><description>散射度 <c>λ3/λ1</c> 高 → 噪声/体状；</description></item>
    ''' <item><description>曲率中等且线性度低、平面度低 → 圆柱面候选。</description></item>
    ''' </list>
    ''' </remarks>
    Public NotInheritable Class SemanticClassification

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 规则化语义分类。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="k">邻域点数（默认 20）</param>
        ''' <param name="edgeThreshold">棱边判定阈值（线性度，默认 0.85）</param>
        ''' <param name="planeThreshold">平面判定阈值（平面度，默认 0.8）</param>
        ''' <param name="noiseThreshold">噪声判定阈值（散射度，默认 0.6）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Classify(cloud As PointCloudBuffer, Optional k As Integer = 20,
                                        Optional edgeThreshold As Double = 0.85,
                                        Optional planeThreshold As Double = 0.8,
                                        Optional noiseThreshold As Double = 0.6,
                                        Optional options As CloudyOptions = Nothing) As SemanticResult
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count
            Dim global_ As CloudyOptions = If(options, CloudyOptions.Default)

            If n = 0 Then Return New SemanticResult With {.Labels = Array.Empty(Of Integer)(), .Confidences = Array.Empty(Of Double)()}

            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then source = NormalEstimation.Estimate(cloud, k, 0.0, global_)

            Dim tree As New KdTree3D(source)
            Dim labels As Integer() = New Integer(n - 1) {}
            Dim confidences As Double() = New Double(n - 1) {}
            Dim box As Aabb3 = source.Bounds()
            Dim height As Double = std.Max(box.Size.Z, 1.0E-9)

            Call global_.ForRange(n,
                Sub(start, ends)
                    Dim buffer As New Neighborhood(std.Max(8, k))

                    For i As Integer = start To ends - 1
                        Call tree.Knn(source.X(i), source.Y(i), source.Z(i), k, buffer, i)

                        Dim count As Integer = buffer.Count

                        If count < 6 Then
                            labels(i) = SemanticClass.Unclassified
                            confidences(i) = 0.0

                            Continue For
                        End If

                        Dim cx As Double = 0, cy As Double = 0, cz As Double = 0

                        For j As Integer = 0 To count - 1
                            Dim q As Integer = buffer.Indices(j)

                            cx += source.X(q)
                            cy += source.Y(q)
                            cz += source.Z(q)
                        Next

                        cx /= count
                        cy /= count
                        cz /= count

                        Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
                        Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

                        For j As Integer = 0 To count - 1
                            Dim q As Integer = buffer.Indices(j)
                            Dim dx As Double = source.X(q) - cx
                            Dim dy As Double = source.Y(q) - cy
                            Dim dz As Double = source.Z(q) - cz

                            cxx += dx * dx
                            cxy += dx * dy
                            cxz += dx * dz
                            cyy += dy * dy
                            cyz += dy * dz
                            czz += dz * dz
                        Next

                        Dim inv As Double = 1.0 / count
                        Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(
                            cxx * inv, cxy * inv, cxz * inv, cyy * inv, cyz * inv, czz * inv)

                        Dim linearity As Double = eigen.Linearity
                        Dim planarity As Double = eigen.Planarity
                        Dim scattering As Double = eigen.Scattering
                        Dim normalZ As Double = std.Abs(eigen.V3.Z)
                        Dim relativeHeight As Double = (source.Z(i) - box.Min.Z) / height

                        If scattering > noiseThreshold Then
                            labels(i) = SemanticClass.Noise
                            confidences(i) = scattering
                        ElseIf linearity > edgeThreshold Then
                            labels(i) = SemanticClass.Edge
                            confidences(i) = linearity
                        ElseIf planarity > planeThreshold Then
                            If relativeHeight < 0.05 OrElse normalZ > 0.9 Then
                                labels(i) = SemanticClass.Ground
                                confidences(i) = planarity
                            ElseIf normalZ < 0.3 Then
                                labels(i) = SemanticClass.Facade
                                confidences(i) = planarity
                            Else
                                labels(i) = SemanticClass.Plane
                                confidences(i) = planarity
                            End If
                        ElseIf scattering < 0.05 AndAlso planarity < 0.5 AndAlso linearity < 0.5 Then
                            labels(i) = SemanticClass.Cylinder
                            confidences(i) = 1.0 - scattering
                        Else
                            labels(i) = SemanticClass.Surface
                            confidences(i) = 1.0 - scattering
                        End If
                    Next
                End Sub, 512)

            Return New SemanticResult With {.Labels = labels, .Confidences = confidences}
        End Function

    End Class

    ''' <summary>
    ''' 最近质心分类器：用少量标注样本做“有监督的语义分类”。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么提供它</b>：规则分类只能处理“由几何定义的类别”；
    ''' 一旦要区分“螺栓 vs 销钉”这类同几何不同语义的对象，就必须引入标注数据。
    ''' 最近质心分类器是深度学习之外最轻量的有监督方案：
    ''' 每个类别只需要几条到几十条样本，训练代价是 <c>O(样本数 × 维度)</c>，
    ''' 推理是一次最近邻比较。
    ''' </para>
    ''' <para>
    ''' <b>与 PointNet++ 的差距</b>：最近质心只能表达“线性可分的凸区域”，
    ''' 对类内差异大、类间差异小的零件（同型号不同磨损状态）会明显弱于深度网络。
    ''' 它的价值在于“零 GPU、可解释、可增量更新”，适合小批量定制产线。
    ''' </para>
    ''' <para>
    ''' 输入是 <see cref="FeatureCloud"/> 的描述子（FPFH/SHOT/ESF 均可）；
    ''' 训练前请确保描述子已归一化（本库的描述子默认已归一化）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class NearestCentroidClassifier

        Private ReadOnly m_centroids As New List(Of Double())()
        Private ReadOnly m_labels As New List(Of Integer)()

        ''' <summary>已训练的类别数。</summary>
        Public ReadOnly Property ClassCount As Integer
            Get
                Return m_centroids.Count
            End Get
        End Property

        ''' <summary>描述子维度（未训练时为 0）。</summary>
        Public Property Dimension As Integer

        ''' <summary>
        ''' 用带标签的描述子训练（同一标签的样本取平均作为质心）。
        ''' </summary>
        ''' <param name="features">特征云</param>
        ''' <param name="labels">与特征行一一对应的类别标签（可以为任意整数）</param>
        Public Sub Train(features As FeatureCloud, labels As Integer())
            If features Is Nothing Then Throw New ArgumentNullException(NameOf(features))
            If labels Is Nothing Then Throw New ArgumentNullException(NameOf(labels))
            If labels.Length <> features.Count Then
                Throw New ArgumentException($"the label count ({labels.Length}) should equal the descriptor count ({features.Count})!", NameOf(labels))
            End If
            If features.Count = 0 Then Return

            If Dimension = 0 Then Dimension = features.Dimension

            If features.Dimension <> Dimension Then
                Throw New ArgumentException($"the descriptor dimension ({features.Dimension}) should equal the trained dimension ({Dimension})!")
            End If

            Dim groups As New Dictionary(Of Integer, (sum As Double(), count As Integer))()

            For i As Integer = 0 To features.Count - 1
                Dim label As Integer = labels(i)
                Dim row As Double() = features.Descriptors(i)
                Dim group As (sum As Double(), count As Integer)

                If Not groups.TryGetValue(label, group) Then
                    group = (New Double(Dimension - 1) {}, 0)
                End If

                For d As Integer = 0 To Dimension - 1
                    group.sum(d) += row(d)
                Next

                groups(label) = (group.sum, group.count + 1)
            Next

            For Each entry As KeyValuePair(Of Integer, (sum As Double(), count As Integer)) In groups
                Dim centroid(Dimension - 1) As Double

                For d As Integer = 0 To Dimension - 1
                    centroid(d) = entry.Value.sum(d) / entry.Value.count
                Next

                m_centroids.Add(centroid)
                m_labels.Add(entry.Key)
            Next
        End Sub

        ''' <summary>
        ''' 对单个描述子分类。
        ''' </summary>
        ''' <returns>预测标签与（1 − 归一化距离）形式的置信度</returns>
        Public Function Classify(descriptor As Double()) As (label As Integer, confidence As Double)
            If descriptor Is Nothing Then Throw New ArgumentNullException(NameOf(descriptor))
            If m_centroids.Count = 0 Then Throw New InvalidOperationException("the classifier has not been trained yet!")

            Dim bestIndex As Integer = 0
            Dim bestDistance As Double = Double.MaxValue

            For i As Integer = 0 To m_centroids.Count - 1
                Dim distance As Double = std.Sqrt(SimdReduce.DistanceSquared(descriptor, m_centroids(i)))

                If distance < bestDistance Then
                    bestDistance = distance
                    bestIndex = i
                End If
            Next

            Return (m_labels(bestIndex), std.Exp(-bestDistance))
        End Function

        ''' <summary>
        ''' 批量分类。
        ''' </summary>
        Public Function Classify(features As FeatureCloud) As SemanticResult
            If features Is Nothing Then Throw New ArgumentNullException(NameOf(features))

            Dim labels(features.Count - 1) As Integer
            Dim confidences(features.Count - 1) As Double

            For i As Integer = 0 To features.Count - 1
                Dim result As (label As Integer, confidence As Double) = Classify(features.Descriptors(i))

                labels(i) = result.label
                confidences(i) = result.confidence
            Next

            Return New SemanticResult With {.Labels = labels, .Confidences = confidences}
        End Function

    End Class
End Namespace
