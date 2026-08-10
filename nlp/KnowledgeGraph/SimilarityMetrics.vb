#Region "Microsoft.VisualBasic::84fc4beb32df7a08630b1764a02325e5, nlp\KnowledgeGraph\SimilarityMetrics.vb"

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

    '   Total Lines: 200
    '    Code Lines: 104 (52.00%)
    ' Comment Lines: 66 (33.00%)
    '    - Xml Docs: 51.52%
    ' 
    '   Blank Lines: 30 (15.00%)
    '     File Size: 8.07 KB


    ' Class SimilarityResult
    ' 
    '     Properties: AdamicAdar, CommonNeighbors, CosineSimilarity, EntityAId, EntityBId
    '                 InclusionAB, InclusionBA, Jaccard, MaxInclusion, SharedAttributeIds
    ' 
    ' Module SimilarityMetrics
    ' 
    '     Function: AdamicAdarIndex, CommonNeighborsCount, ComputeAllPairs, ComputeSimilarity, CosineSim
    '               InclusionRatio, JaccardSimilarity
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' SimilarityMetrics.vb - 图相似度度量模块
'
' 实现基于二部图结构的实体间相似度计算：
'
' 1. Jaccard 相似度：|N(A) ∩ N(B)| / |N(A) ∪ N(B)|
'    衡量两个实体共享属性的比例，取值 [0, 1]。
'    同义实体（如 water / 水）的 Jaccard 通常 > 0.5。
'
' 2. Adamic-Adar 指数：Σ_{z ∈ N(A) ∩ N(B)} 1 / log(|N(z)|)
'    对共享属性按其稀有度加权——越稀有的共享属性贡献越大。
'    能区分"因为常见属性碰巧重叠"与"因为有深层关联而共享"。
'
' 3. 包含率（Inclusion Ratio）：|N(A) ∩ N(B)| / |N(B)|
'    衡量 B 的属性中有多少也属于 A，用于 Is-A 关系推断。
'    若 A is-a B，则 B 的核心属性应大多出现在 A 中。
'
' 4. 共同邻居数（Common Neighbors）：|N(A) ∩ N(B)|
'    最朴素的相似度指标，仅计数共享属性。
'
' 5. 余弦相似度（Cosine Similarity）：|N(A) ∩ N(B)| / sqrt(|N(A)| × |N(B)|)
'    补偿了实体度数差异的影响。
' ============================================================================

Imports System.Collections.Generic
Imports std = System.Math

''' <summary>
''' 相似度度量结果：包含一对实体之间的所有相似度指标。
''' </summary>
Public Class SimilarityResult

    Public Property EntityAId As Integer
    Public Property EntityBId As Integer
    Public Property Jaccard As Double
    Public Property AdamicAdar As Double
    Public Property CommonNeighbors As Integer
    Public Property CosineSimilarity As Double
    Public Property InclusionAB As Double  ' B 的属性中属于 A 的比例
    Public Property InclusionBA As Double  ' A 的属性中属于 B 的比例
    Public Property SharedAttributeIds As New List(Of Integer)

    Public ReadOnly Property MaxInclusion As Double
        Get
            Return std.Max(InclusionAB, InclusionBA)
        End Get
    End Property

End Class

''' <summary>
''' 图相似度计算模块。
''' </summary>
Public Module SimilarityMetrics

    ''' <summary>
    ''' 计算两个实体之间的 Jaccard 相似度。
    ''' J(A, B) = |N(A) ∩ N(B)| / |N(A) ∪ N(B)|
    ''' </summary>
    Public Function JaccardSimilarity(graph As KnowledgeGraph, entityA As Integer, entityB As Integer) As Double
        Dim attrsA As HashSet(Of Integer) = graph.GetEntityAttributes(entityA)
        Dim attrsB As HashSet(Of Integer) = graph.GetEntityAttributes(entityB)

        Dim unionCount As Integer = attrsA.Count + attrsB.Count
        If unionCount = 0 Then Return 0.0

        ' 计算交集
        attrsA.IntersectWith(attrsB)
        Dim intersectionCount As Integer = attrsA.Count

        ' 并集 = |A| + |B| - |A ∩ B|
        unionCount -= intersectionCount

        If unionCount = 0 Then Return 0.0
        Return CDbl(intersectionCount) / unionCount
    End Function

    ''' <summary>
    ''' 计算两个实体之间的 Adamic-Adar 指数。
    ''' AA(A, B) = Σ_{z ∈ N(A) ∩ N(B)} 1 / log(|N(z)|)
    ''' 其中 |N(z)| 是属性 z 连接的实体数量。
    ''' 稀有共享属性（连接少数实体的属性）贡献更大的权重。
    ''' </summary>
    Public Function AdamicAdarIndex(graph As KnowledgeGraph, entityA As Integer, entityB As Integer) As Double
        Dim [shared] As HashSet(Of Integer) = graph.GetSharedAttributes(entityA, entityB)

        Dim sum As Double = 0.0
        For Each attrId As Integer In [shared]
            Dim degree As Integer = graph.GetAttributeDegree(attrId)
            If degree > 1 Then
                ' 1 / log(degree)：度数越高（越常见），贡献越小
                sum += 1.0 / std.Log(degree)
            ElseIf degree = 1 Then
                ' 度为 1 的属性不可能被两个实体共享，但防御性处理
                sum += 0.0
            End If
        Next

        Return sum
    End Function

    ''' <summary>
    ''' 计算共同邻居数（共享属性数）。
    ''' </summary>
    Public Function CommonNeighborsCount(graph As KnowledgeGraph, entityA As Integer, entityB As Integer) As Integer
        Return graph.GetSharedAttributes(entityA, entityB).Count
    End Function

    ''' <summary>
    ''' 计算余弦相似度。
    ''' cos(A, B) = |N(A) ∩ N(B)| / sqrt(|N(A)| × |N(B)|)
    ''' </summary>
    Public Function CosineSim(graph As KnowledgeGraph, entityA As Integer, entityB As Integer) As Double
        Dim degA As Integer = graph.GetEntityDegree(entityA)
        Dim degB As Integer = graph.GetEntityDegree(entityB)
        If degA = 0 OrElse degB = 0 Then Return 0.0

        Dim common As Integer = CommonNeighborsCount(graph, entityA, entityB)
        Return CDbl(common) / std.Sqrt(CDbl(degA) * degB)
    End Function

    ''' <summary>
    ''' 计算包含率：B 的属性中有多少也属于 A。
    ''' InclusionRatio(A ⊇ B) = |N(A) ∩ N(B)| / |N(B)|
    ''' 若该值接近 1，说明 B 的所有属性都在 A 中，A 可能是 B 的上位词或同义词。
    ''' </summary>
    Public Function InclusionRatio(graph As KnowledgeGraph, supersetEntity As Integer, subsetEntity As Integer) As Double
        Dim degSubset As Integer = graph.GetEntityDegree(subsetEntity)
        If degSubset = 0 Then Return 0.0

        Dim [shared] As Integer = CommonNeighborsCount(graph, supersetEntity, subsetEntity)
        Return CDbl([shared]) / degSubset
    End Function

    ''' <summary>
    ''' 计算两个实体之间的全部相似度指标。
    ''' </summary>
    Public Function ComputeSimilarity(graph As KnowledgeGraph, entityA As Integer, entityB As Integer) As SimilarityResult
        Dim [shared] As HashSet(Of Integer) = graph.GetSharedAttributes(entityA, entityB)
        Dim degA As Integer = graph.GetEntityDegree(entityA)
        Dim degB As Integer = graph.GetEntityDegree(entityB)
        Dim unionCount As Integer = degA + degB - [shared].Count

        Dim result As New SimilarityResult With {
            .EntityAId = entityA,
            .EntityBId = entityB,
            .CommonNeighbors = [shared].Count,
            .SharedAttributeIds = [shared].ToList()
        }

        ' Jaccard
        If unionCount > 0 Then
            result.Jaccard = CDbl([shared].Count) / unionCount
        End If

        ' Adamic-Adar
        Dim aaSum As Double = 0.0
        For Each attrId As Integer In [shared]
            Dim deg As Integer = graph.GetAttributeDegree(attrId)
            If deg > 1 Then
                aaSum += 1.0 / std.Log(deg)
            End If
        Next
        result.AdamicAdar = aaSum

        ' Cosine
        If degA > 0 AndAlso degB > 0 Then
            result.CosineSimilarity = CDbl([shared].Count) / std.Sqrt(CDbl(degA) * degB)
        End If

        ' Inclusion ratios
        If degB > 0 Then result.InclusionAB = CDbl([shared].Count) / degB
        If degA > 0 Then result.InclusionBA = CDbl([shared].Count) / degA

        Return result
    End Function

    ''' <summary>
    ''' 计算图中所有实体对的相似度，按 Jaccard 降序排列。
    ''' </summary>
    Public Function ComputeAllPairs(graph As KnowledgeGraph) As List(Of SimilarityResult)
        Dim results As New List(Of SimilarityResult)
        Dim n As Integer = graph.Entities.Count

        For i As Integer = 0 To n - 1
            For j As Integer = i + 1 To n - 1
                results.Add(ComputeSimilarity(graph, i, j))
            Next
        Next

        ' 按 Jaccard 降序，其次按 AA 降序
        results.Sort(Function(a, b) 
                         Dim cmp As Integer = b.Jaccard.CompareTo(a.Jaccard)
                         If cmp = 0 Then cmp = b.AdamicAdar.CompareTo(a.AdamicAdar)
                         Return cmp
                     End Function)
        Return results
    End Function

End Module
