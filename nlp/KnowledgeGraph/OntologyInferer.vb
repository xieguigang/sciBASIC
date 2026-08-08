#Region "Microsoft.VisualBasic::92b86b8d1a5ee51c66eb336d41860e0b, nlp\KnowledgeGraph\OntologyInferer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 360
    '    Code Lines: 198 (55.00%)
    ' Comment Lines: 101 (28.06%)
    '    - Xml Docs: 52.48%
    ' 
    '   Blank Lines: 61 (16.94%)
    '     File Size: 14.15 KB


    ' Enum OntologyRelationType
    ' 
    '     HasFunction, IsA, RelatedTo, SiblingOf
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class OntologyRelation
    ' 
    '     Properties: AdamicAdar, Confidence, Description, InclusionRatio, Jaccard
    '                 ObjectId, ObjectName, PValue, RelationType, SharedAttributes
    '                 SubjectId, SubjectName
    ' 
    ' Class OntologyOptions
    ' 
    '     Properties: IsAInclusionThreshold, PermutationIterations, RelatedMinJaccard, SiblingJaccardThreshold, TaxonomicAttributeKeywords
    ' 
    ' Class OntologyInferer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ComputeConfidence, DeduplicateRelations, DetermineRelationType, HasFunctionalAttributes, HasTaxonomicAttributes
    '               InferRelations, MakePairKey
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' OntologyInferer.vb - 本体论关系推断模块
'
' 在实体消歧之后，对非同义实体推断本体论关系：
'
' 1. Is-A（下位→上位）：A is-a B
'    判据：InclusionRatio(A⊇B) = |N(A)∩N(B)| / |N(B)| > 0.5
'    且 |N(A)| >= |N(B)|（A 更具体，有更多属性）
'    例：老虎 is-a 哺乳动物
'
' 2. Sibling-Of（同层级）：A sibling-of B
'    判据：Jaccard > 0.15 且不满足 Is-A 条件
'    且存在共享的"分类性"属性（如 family、genus、type 等）
'    例：老虎 sibling-of 狮子
'
' 3. Related-To（关联）：A related-to B
'    判据：0 < Jaccard < 0.15，但有非零 AA 且统计显著
'    例：解渴 related-to water（功能性关联）
'
' 4. Has-Function（功能关联）：A has-function B
'    判据：A 和 B 共享"功能性"属性（如 thirst_quenching）
'    是 Related-To 的一个子类型
' ============================================================================

Imports std = System.Math

''' <summary>
''' 本体论关系类型。
''' </summary>
Public Enum OntologyRelationType
    IsA
    SiblingOf
    RelatedTo
    HasFunction
End Enum

''' <summary>
''' 推断出的本体论关系。
''' </summary>
Public Class OntologyRelation

    ''' <summary>主体实体 ID（如"老虎"）。</summary>
    Public Property SubjectId As Integer

    ''' <summary>客体实体 ID（如"哺乳动物"）。</summary>
    Public Property ObjectId As Integer

    ''' <summary>关系类型。</summary>
    Public Property RelationType As OntologyRelationType

    ''' <summary>Jaccard 相似度。</summary>
    Public Property Jaccard As Double

    ''' <summary>Adamic-Adar 指数。</summary>
    Public Property AdamicAdar As Double

    ''' <summary>包含率（Is-A 关系的核心指标）。</summary>
    Public Property InclusionRatio As Double

    ''' <summary>置信度 [0, 1]。</summary>
    Public Property Confidence As Double

    ''' <summary>p-value。</summary>
    Public Property PValue As Double

    ''' <summary>共享属性名称列表。</summary>
    Public Property SharedAttributes As New List(Of String)

    ''' <summary>关系描述文本。</summary>
    Public ReadOnly Property Description As String
        Get
            Dim typeName As String = RelationType.ToString()
            Return $"{SubjectName} {typeName} {ObjectName}"
        End Get
    End Property

    ''' <summary>主体名称。</summary>
    Public Property SubjectName As String

    ''' <summary>客体名称。</summary>
    Public Property ObjectName As String

End Class

''' <summary>
''' 本体论推断配置。
''' </summary>
Public Class OntologyOptions

    ''' <summary>Is-A 关系的包含率阈值。</summary>
    Public Property IsAInclusionThreshold As Double = 0.5

    ''' <summary>Sibling 关系的 Jaccard 阈值。</summary>
    Public Property SiblingJaccardThreshold As Double = 0.15

    ''' <summary>Related 关系的最小 Jaccard。</summary>
    Public Property RelatedMinJaccard As Double = 0.01

    ''' <summary>置换检验迭代次数。</summary>
    Public Property PermutationIterations As Integer = 5000

    ''' <summary>分类性属性的关键词（用于判断 Sibling 关系）。</summary>
    Public Property TaxonomicAttributeKeywords As String() = {
        "class_", "order_", "family_", "genus_", "species_", "type_", "kingdom_", "phylum_"
    }

End Class

''' <summary>
''' 本体论推断器。
''' </summary>
Public Class OntologyInferer

    Private _graph As KnowledgeGraph
    Private _options As OntologyOptions

    Public Sub New(graph As KnowledgeGraph, Optional options As OntologyOptions = Nothing)
        _graph = graph
        _options = If(options, New OntologyOptions())
    End Sub

    ''' <summary>
    ''' 推断所有实体间的本体论关系。
    ''' 跳过已判定为同义的对（由调用方传入同义组信息）。
    ''' </summary>
    ''' <param name="synonymGroups">已检测到的同义组（跳过组内对）。</param>
    Public Function InferRelations(synonymGroups As List(Of SynonymGroup)) As List(Of OntologyRelation)
        Dim relations As New List(Of OntologyRelation)
        Dim n As Integer = _graph.Entities.Count

        ' 构建同义集合（用于跳过同义对）
        Dim synonymSet As New HashSet(Of Long)
        For Each group In synonymGroups
            For i As Integer = 0 To group.EntityIds.Count - 1
                For j As Integer = i + 1 To group.EntityIds.Count - 1
                    Dim a As Integer = group.EntityIds(i)
                    Dim b As Integer = group.EntityIds(j)
                    synonymSet.Add(MakePairKey(a, b))
                Next
            Next
        Next

        ' 统计检验器
        Dim tester As New StatisticalTest(_graph, seed:=123)

        ' 比较数（用于 Bonferroni 校正）
        Dim numComparisons As Integer = n * (n - 1) \ 2

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                If i = j Then Continue For
                If synonymSet.Contains(MakePairKey(i, j)) Then Continue For

                Dim sim As SimilarityResult = SimilarityMetrics.ComputeSimilarity(_graph, i, j)

                ' 跳过无共享属性的对
                If sim.CommonNeighbors = 0 Then Continue For

                ' 判断关系类型
                Dim relType As OntologyRelationType = DetermineRelationType(i, j, sim)

                ' 跳过无关对
                If relType = OntologyRelationType.RelatedTo AndAlso sim.Jaccard < _options.RelatedMinJaccard Then
                    Continue For
                End If

                ' 执行置换检验
                Dim aaTest As PermutationTestResult = tester.TestAdamicAdarFull(i, j, _options.PermutationIterations)
                Dim correctedP As Double = std.Min(1.0, aaTest.PValue * numComparisons)

                ' 计算置信度
                Dim confidence As Double = ComputeConfidence(relType, sim, aaTest)

                ' 获取共享属性名称
                Dim sharedNames As List(Of String) = _graph.GetAttributeNames(sim.SharedAttributeIds)

                Dim rel As New OntologyRelation With {
                    .SubjectId = i,
                    .ObjectId = j,
                    .RelationType = relType,
                    .Jaccard = sim.Jaccard,
                    .AdamicAdar = sim.AdamicAdar,
                    .InclusionRatio = sim.InclusionAB,  ' B 的属性在 A 中的比例
                    .Confidence = confidence,
                    .PValue = correctedP,
                    .SharedAttributes = sharedNames,
                    .SubjectName = _graph.Entities(i).Name,
                    .ObjectName = _graph.Entities(j).Name
                }

                relations.Add(rel)
            Next
        Next

        ' 按关系类型优先级排序：Is-A > Sibling > Related
        relations.Sort(Function(a, b)
                           Dim typeCmp As Integer = a.RelationType.CompareTo(b.RelationType)
                           If typeCmp <> 0 Then Return typeCmp
                           Return b.Confidence.CompareTo(a.Confidence)
                       End Function)

        ' 去重：对于 Sibling 和 Related，只保留方向中置信度更高的那个
        Return DeduplicateRelations(relations)
    End Function

    ''' <summary>
    ''' 根据相似度指标判断关系类型。
    ''' </summary>
    Private Function DetermineRelationType(entityA As Integer, entityB As Integer,
                                          sim As SimilarityResult) As OntologyRelationType
        Dim degA As Integer = _graph.GetEntityDegree(entityA)
        Dim degB As Integer = _graph.GetEntityDegree(entityB)

        ' --- Is-A 检测 ---
        ' A is-a B：B 的核心属性大部分在 A 中，且 A 的属性数 >= B
        If sim.InclusionAB >= _options.IsAInclusionThreshold AndAlso degA >= degB Then
            ' 确保 A 比 B 更具体
            Return OntologyRelationType.IsA
        End If

        ' --- Sibling 检测 ---
        ' 高 Jaccard 但不是 Is-A，且有共享的分类性属性
        If sim.Jaccard >= _options.SiblingJaccardThreshold Then
            Dim hasTaxonomic As Boolean = HasTaxonomicAttributes(sim.SharedAttributeIds)
            If hasTaxonomic Then
                Return OntologyRelationType.SiblingOf
            End If
            ' 即使没有分类性属性，高 Jaccard 也算 Sibling
            Return OntologyRelationType.SiblingOf
        End If

        ' --- Has-Function / Related-To ---
        If sim.Jaccard >= _options.RelatedMinJaccard Then
            If HasFunctionalAttributes(sim.SharedAttributeIds) Then
                Return OntologyRelationType.HasFunction
            End If
            Return OntologyRelationType.RelatedTo
        End If

        Return OntologyRelationType.RelatedTo
    End Function

    ''' <summary>
    ''' 检查共享属性中是否包含分类性属性。
    ''' </summary>
    Private Function HasTaxonomicAttributes(attrIds As List(Of Integer)) As Boolean
        For Each id In attrIds
            Dim name As String = _graph.Attributes(id).Name
            For Each keyword In _options.TaxonomicAttributeKeywords
                If name.StartsWith(keyword, StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next
        Next
        Return False
    End Function

    ''' <summary>
    ''' 检查共享属性中是否包含功能性属性。
    ''' </summary>
    Private Function HasFunctionalAttributes(attrIds As List(Of Integer)) As Boolean
        For Each id In attrIds
            Dim name As String = _graph.Attributes(id).Name
            If name.Contains("thirst") OrElse name.Contains("hydrat") OrElse
               name.Contains("refresh") OrElse name.Contains("quench") OrElse
               name.Contains("cool") OrElse name.Contains("relieve") Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' 计算关系置信度。
    ''' </summary>
    Private Function ComputeConfidence(relType As OntologyRelationType,
                                       sim As SimilarityResult,
                                       test As PermutationTestResult) As Double
        Select Case relType
            Case OntologyRelationType.IsA
                ' Is-A 置信度 = 包含率 × (1 - p-value)
                Return sim.InclusionAB * (1.0 - test.PValue)

            Case OntologyRelationType.SiblingOf
                ' Sibling 置信度 = Jaccard × (1 - p-value)
                Return sim.Jaccard * (1.0 - test.PValue)

            Case OntologyRelationType.HasFunction, OntologyRelationType.RelatedTo
                ' Related 置信度 = AA 归一化 × (1 - p-value)
                Dim aaNorm As Double = std.Min(1.0, sim.AdamicAdar / 10.0)
                Return aaNorm * (1.0 - test.PValue)

            Case Else
                Return 0.0
        End Select
    End Function

    ''' <summary>
    ''' 去重：对于 Sibling 和 Related，两个方向只保留一个。
    ''' Is-A 需要保留方向。
    ''' </summary>
    Private Function DeduplicateRelations(relations As List(Of OntologyRelation)) As List(Of OntologyRelation)
        Dim result As New List(Of OntologyRelation)
        Dim seen As New HashSet(Of Long)

        For Each rel In relations
            If rel.RelationType = OntologyRelationType.IsA Then
                ' Is-A 保留方向
                Dim key As Long = MakePairKey(rel.SubjectId, rel.ObjectId)
                If Not seen.Contains(key) Then
                    seen.Add(key)
                    result.Add(rel)
                End If
            Else
                ' Sibling / Related 取无向对，保留置信度更高的方向
                Dim key As Long = MakePairKey(rel.SubjectId, rel.ObjectId)
                Dim reverseKey As Long = MakePairKey(rel.ObjectId, rel.SubjectId)

                If seen.Contains(key) OrElse seen.Contains(reverseKey) Then
                    ' 已有该对，比较置信度
                    Dim existingIdx As Integer = -1
                    For idx As Integer = 0 To result.Count - 1
                        Dim r As OntologyRelation = result(idx)
                        If (r.SubjectId = rel.SubjectId AndAlso r.ObjectId = rel.ObjectId) OrElse
                           (r.SubjectId = rel.ObjectId AndAlso r.ObjectId = rel.SubjectId) Then
                            existingIdx = idx
                            Exit For
                        End If
                    Next

                    If existingIdx >= 0 AndAlso rel.Confidence > result(existingIdx).Confidence Then
                        result(existingIdx) = rel
                    End If
                Else
                    seen.Add(key)
                    result.Add(rel)
                End If
            End If
        Next

        ' 最终按类型和置信度排序
        result.Sort(Function(a, b)
                        Dim typeCmp As Integer = a.RelationType.CompareTo(b.RelationType)
                        If typeCmp <> 0 Then Return typeCmp
                        Return b.Confidence.CompareTo(a.Confidence)
                    End Function)

        Return result
    End Function

    ''' <summary>
    ''' 生成有序对键（用于去重）。
    ''' </summary>
    Private Function MakePairKey(a As Integer, b As Integer) As Long
        Dim lo As Integer = std.Min(a, b)
        Dim hi As Integer = std.Max(a, b)
        Return CLng(lo) * 100000 + hi
    End Function

End Class

