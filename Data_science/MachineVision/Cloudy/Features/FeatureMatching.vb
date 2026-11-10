Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' 描述子匹配：Lowe 比值检验 + 双向一致性过滤。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么要比值检验</b>：在重复纹理/对称零件上，一个源特征往往与多个目标特征
    ''' 距离都很近。此时“最近邻”的选择几乎是随机的，直接送入 RANSAC 会产生
    ''' 大量“看起来合理”的错误对应。Lowe 的判据是：
    ''' </para>
    ''' <code>
    ''' d(最近) / d(次近) ≤ 0.8
    ''' </code>
    ''' <para>
    ''' 即“最近邻必须显著优于次近邻”，否则该特征被认为不可靠而丢弃。
    ''' 0.8 是经验值：调小（0.6）会得到更少但更准的对应，调大（0.9）则相反。
    ''' </para>
    ''' <para>
    ''' <b>双向一致性（互查）</b>：同时要求 <c>target</c> 侧也选中同一个源特征。
    ''' 这一步能消除“多对一”的汇聚（很多源特征同时匹配到同一个目标特征），
    ''' 对配准的初值质量提升明显，代价是多一遍 <c>O(m·n·dim)</c> 的扫描。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：暴力匹配 <c>O(n·m·dim)</c>。关键点通常在 1e3~1e4 量级、
    ''' 维度 33，总计约 1e9 次乘加，并行后可以接受；
    ''' 若需要更大规模，应改用倒排索引/乘积量化等近似最近邻结构。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class FeatureMatching

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 一条特征对应关系。
        ''' </summary>
        Public Structure Correspondence

            ''' <summary>源特征下标（<see cref="FeatureCloud.Descriptors"/> 的行号）。</summary>
            Public ReadOnly SourceIndex As Integer

            ''' <summary>目标特征下标。</summary>
            Public ReadOnly TargetIndex As Integer

            ''' <summary>描述子之间的欧氏距离。</summary>
            Public ReadOnly Distance As Double

            ''' <summary>比值 <c>d1 / d2</c>（越小越可靠）。</summary>
            Public ReadOnly Ratio As Double

            Public Sub New(sourceIndex As Integer, targetIndex As Integer, distance As Double, ratio As Double)
                Me.SourceIndex = sourceIndex
                Me.TargetIndex = targetIndex
                Me.Distance = distance
                Me.Ratio = ratio
            End Sub

            Public Overrides Function ToString() As String
                Return $"{SourceIndex} -> {TargetIndex} (d={Distance:G4}, ratio={Ratio:G3})"
            End Function
        End Structure

        ''' <summary>
        ''' 匹配两组特征。
        ''' </summary>
        ''' <param name="source">源特征云</param>
        ''' <param name="target">目标特征云</param>
        ''' <param name="ratioThreshold">Lowe 比值阈值（默认 0.8；&lt;= 0 表示不做比值检验）</param>
        ''' <param name="mutual">是否做双向一致性检查（默认 <c>True</c>）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Match(source As FeatureCloud, target As FeatureCloud,
                                     Optional ratioThreshold As Double = 0.8,
                                     Optional mutual As Boolean = True,
                                     Optional options As CloudyOptions = Nothing) As Correspondence()
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))

            If source.Count = 0 OrElse target.Count = 0 Then Return Array.Empty(Of Correspondence)()

            If source.Dimension <> target.Dimension Then
                Throw New ArgumentException($"the descriptor dimensions not agree: {source.Dimension} vs {target.Dimension}!")
            End If

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = source.Count
            Dim m As Integer = target.Count

            Dim bestTarget As Integer() = New Integer(n - 1) {}
            Dim bestDistance As Double() = New Double(n - 1) {}
            Dim secondDistance As Double() = New Double(n - 1) {}

            Call opt.ForRange(n,
                Sub(start, ends)
                    For i As Integer = start To ends - 1
                        Dim row As Double() = source.Descriptors(i)
                        Dim best As Integer = -1
                        Dim d1 As Double = Double.MaxValue
                        Dim d2 As Double = Double.MaxValue

                        For j As Integer = 0 To m - 1
                            Dim d As Double = std.Sqrt(SimdReduce.DistanceSquared(row, target.Descriptors(j)))

                            If d < d1 Then
                                d2 = d1
                                d1 = d
                                best = j
                            ElseIf d < d2 Then
                                d2 = d
                            End If
                        Next

                        bestTarget(i) = best
                        bestDistance(i) = d1
                        secondDistance(i) = d2
                    Next
                End Sub, 256)

            ' 双向一致性：目标侧的最优源特征
            Dim bestSource As Integer() = Nothing

            If mutual Then
                bestSource = New Integer(m - 1) {}
                Dim bestSourceDistance As Double() = New Double(m - 1) {}

                For j As Integer = 0 To m - 1
                    bestSource(j) = -1
                    bestSourceDistance(j) = Double.MaxValue
                Next

                Call opt.ForRange(m,
                    Sub(start, ends)
                        For j As Integer = start To ends - 1
                            Dim row As Double() = target.Descriptors(j)
                            Dim best As Integer = -1
                            Dim d1 As Double = Double.MaxValue

                            For i As Integer = 0 To n - 1
                                Dim d As Double = std.Sqrt(SimdReduce.DistanceSquared(row, source.Descriptors(i)))

                                If d < d1 Then
                                    d1 = d
                                    best = i
                                End If
                            Next

                            bestSource(j) = best
                            bestSourceDistance(j) = d1
                        Next
                    End Sub, 256)
            End If

            Dim result As New List(Of Correspondence)()

            For i As Integer = 0 To n - 1
                If bestTarget(i) < 0 Then Continue For

                Dim j As Integer = bestTarget(i)
                Dim d1 As Double = bestDistance(i)
                Dim d2 As Double = secondDistance(i)

                Dim ratio As Double = If(d2 > 0.0 AndAlso Not Double.IsInfinity(d2), d1 / d2, 1.0)

                If ratioThreshold > 0.0 AndAlso ratio > ratioThreshold Then Continue For
                If mutual AndAlso bestSource(j) <> i Then Continue For

                Call result.Add(New Correspondence(i, j, d1, ratio))
            Next

            Return result.ToArray()
        End Function

        ''' <summary>
        ''' 把对应关系转换成点下标对（配合 <see cref="FeatureCloud.Keypoints"/> 使用）。
        ''' </summary>
        ''' <returns>源点下标数组与目标点下标数组（等长）</returns>
        Public Shared Function ToPointPairs(source As FeatureCloud, target As FeatureCloud,
                                            correspondences As Correspondence()) As (source As Integer(), target As Integer())
            If correspondences Is Nothing OrElse correspondences.Length = 0 Then
                Return (Array.Empty(Of Integer)(), Array.Empty(Of Integer)())
            End If

            Dim a(correspondences.Length - 1) As Integer
            Dim b(correspondences.Length - 1) As Integer

            For i As Integer = 0 To correspondences.Length - 1
                a(i) = source.Keypoints.Indices(correspondences(i).SourceIndex)
                b(i) = target.Keypoints.Indices(correspondences(i).TargetIndex)
            Next

            Return (a, b)
        End Function

    End Class
End Namespace
