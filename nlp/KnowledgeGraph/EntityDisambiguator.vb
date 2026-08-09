#Region "Microsoft.VisualBasic::df4b96449a32cc1fedbe76ae05687a68, nlp\KnowledgeGraph\EntityDisambiguator.vb"

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

    '   Total Lines: 246
    '    Code Lines: 142 (57.72%)
    ' Comment Lines: 58 (23.58%)
    '    - Xml Docs: 48.28%
    ' 
    '   Blank Lines: 46 (18.70%)
    '     File Size: 9.47 KB


    ' Class DisambiguationOptions
    ' 
    '     Properties: Alpha, ApplyBonferroniCorrection, PermutationIterations, SynonymJaccardThreshold, SynonymPValueThreshold
    ' 
    ' Class SynonymGroup
    ' 
    '     Properties: CanonicalLanguage, CanonicalName, Confidence, EntityIds, PairwiseResults
    ' 
    ' Class SynonymPairResult
    ' 
    '     Properties: AAPValue, AAZScore, AdamicAdar, BonferroniCorrectedPValue, EntityAId
    '                 EntityBId, IsSignificant, Jaccard, JaccardPValue, JaccardZScore
    '                 NameA, NameB
    ' 
    ' Class EntityDisambiguator
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: BuildSynonymGroups, Disambiguate, Find
    ' 
    '     Sub: Union
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' EntityDisambiguator.vb - 实体消歧模块
'
' 核心思想：
'   如果两个实体之间的 Jaccard 相似度和 Adamic-Adar 指数
'   远高于它们与其他实体的连接，则判定它们指代同一知识对象。
'
' 算法流程：
'   1. 计算所有实体对的 Jaccard 和 AA
'   2. 对每对实体执行置换检验，计算 p-value
'   3. 应用阈值筛选候选同义对：
'      - Jaccard > synonymJaccardThreshold (默认 0.5)
'      - AA p-value < synonymPValueThreshold (默认 0.01)
'   4. 将候选同义对传递闭包为同义组
'   5. 应用 Bonferroni 校正控制多重检验误差
' ============================================================================

Imports std = System.Math

''' <summary>
''' 实体消歧配置参数。
''' </summary>
Public Class DisambiguationOptions

    ''' <summary>同义判定的 Jaccard 阈值。</summary>
    Public Property SynonymJaccardThreshold As Double = 0.45

    ''' <summary>同义判定的 p-value 阈值。</summary>
    Public Property SynonymPValueThreshold As Double = 0.01

    ''' <summary>置换检验迭代次数。</summary>
    Public Property PermutationIterations As Integer = 5000

    ''' <summary>是否应用 Bonferroni 校正。</summary>
    Public Property ApplyBonferroniCorrection As Boolean = True

    ''' <summary>全局显著性水平 α。</summary>
    Public Property Alpha As Double = 0.05

End Class

''' <summary>
''' 同义实体组：被判定为指代同一知识对象的实体集合。
''' </summary>
Public Class SynonymGroup

    ''' <summary>组内的实体 ID 列表。</summary>
    Public Property EntityIds As New List(Of Integer)

    ''' <summary>组内实体对的详细得分。</summary>
    Public Property PairwiseResults As New List(Of SynonymPairResult)

    ''' <summary>整体置信度（取最小 p-value 对应的置信度）。</summary>
    Public Property Confidence As Double

    ''' <summary>建议的规范名称（度数最大的实体）。</summary>
    Public Property CanonicalName As String

    ''' <summary>规范名称的语言。</summary>
    Public Property CanonicalLanguage As String

End Class

''' <summary>
''' 一对同义实体的详细测试结果。
''' </summary>
Public Class SynonymPairResult

    Public Property EntityAId As Integer
    Public Property EntityBId As Integer
    Public Property NameA As String
    Public Property NameB As String
    Public Property Jaccard As Double
    Public Property AdamicAdar As Double
    Public Property JaccardPValue As Double
    Public Property AAPValue As Double
    Public Property JaccardZScore As Double
    Public Property AAZScore As Double
    Public Property BonferroniCorrectedPValue As Double
    Public Property IsSignificant As Boolean

End Class

''' <summary>
''' 实体消歧器。
''' </summary>
Public Class EntityDisambiguator

    Private _graph As KnowledgeGraph
    Private _options As DisambiguationOptions

    Public Sub New(graph As KnowledgeGraph, Optional options As DisambiguationOptions = Nothing)
        _graph = graph
        _options = If(options, New DisambiguationOptions())
    End Sub

    ''' <summary>
    ''' 执行实体消歧，返回检测到的同义实体组。
    ''' </summary>
    Public Function Disambiguate() As List(Of SynonymGroup)
        Dim n As Integer = _graph.Entities.Count
        Dim numComparisons As Integer = n * (n - 1) \ 2
        Dim bonferroniAlpha As Double = If(_options.ApplyBonferroniCorrection,
                                           _options.Alpha / numComparisons,
                                           _options.Alpha)

        ' 统计检验器
        Dim tester As New StatisticalTest(_graph, seed:=42)

        ' Step 1: 计算所有实体对的相似度和 p-value
        Dim candidatePairs As New List(Of SynonymPairResult)

        For i As Integer = 0 To n - 1
            For j As Integer = i + 1 To n - 1
                Dim sim As SimilarityResult = SimilarityMetrics.ComputeSimilarity(_graph, i, j)

                ' 快速过滤：Jaccard 低于阈值的跳过置换检验
                If sim.Jaccard < _options.SynonymJaccardThreshold Then
                    Continue For
                End If

                ' 执行置换检验
                Dim jacTest As PermutationTestResult = tester.TestJaccard(i, j, _options.PermutationIterations)
                Dim aaTest As PermutationTestResult = tester.TestAdamicAdarFull(i, j, _options.PermutationIterations)

                ' Bonferroni 校正
                Dim correctedP As Double = std.Min(1.0, std.Min(jacTest.PValue, aaTest.PValue) * numComparisons)

                Dim pairResult As New SynonymPairResult With {
                    .EntityAId = i,
                    .EntityBId = j,
                    .NameA = _graph.Entities(i).Name,
                    .NameB = _graph.Entities(j).Name,
                    .Jaccard = sim.Jaccard,
                    .AdamicAdar = sim.AdamicAdar,
                    .JaccardPValue = jacTest.PValue,
                    .AAPValue = aaTest.PValue,
                    .JaccardZScore = jacTest.ZScore,
                    .AAZScore = aaTest.ZScore,
                    .BonferroniCorrectedPValue = correctedP,
                    .IsSignificant = (correctedP < _options.Alpha) AndAlso
                                     (sim.Jaccard >= _options.SynonymJaccardThreshold)
                }

                candidatePairs.Add(pairResult)
            Next
        Next

        ' Step 2: 筛选显著的同义对
        Dim significantPairs As List(Of SynonymPairResult) =
            candidatePairs.Where(Function(p) p.IsSignificant).ToList()

        ' Step 3: 传递闭包 → 同义组
        Return BuildSynonymGroups(significantPairs)
    End Function

    ''' <summary>
    ''' 使用并查集将显著同义对合并为同义组。
    ''' </summary>
    Private Function BuildSynonymGroups(pairs As List(Of SynonymPairResult)) As List(Of SynonymGroup)
        Dim n As Integer = _graph.Entities.Count
        Dim parent(n - 1) As Integer
        For i As Integer = 0 To n - 1
            parent(i) = i
        Next

        ' 并查集合并
        For Each pair In pairs
            Union(parent, pair.EntityAId, pair.EntityBId)
        Next

        ' 按根节点分组
        Dim groupMap As New Dictionary(Of Integer, List(Of Integer))
        For i As Integer = 0 To n - 1
            Dim root As Integer = Find(parent, i)
            If Not groupMap.ContainsKey(root) Then
                groupMap(root) = New List(Of Integer)
            End If
            groupMap(root).Add(i)
        Next

        ' 构建同义组对象（只保留含 2 个以上实体的组）
        Dim result As New List(Of SynonymGroup)
        For Each kvp In groupMap
            If kvp.Value.Count < 2 Then Continue For

            Dim group As New SynonymGroup With {.EntityIds = kvp.Value}

            ' 添加组内对的详细结果
            For i As Integer = 0 To group.EntityIds.Count - 1
                For j As Integer = i + 1 To group.EntityIds.Count - 1
                    Dim aId As Integer = group.EntityIds(i)
                    Dim bId As Integer = group.EntityIds(j)
                    Dim pairMatch As SynonymPairResult = pairs.FirstOrDefault(
                        Function(p) (p.EntityAId = aId AndAlso p.EntityBId = bId) OrElse
                                    (p.EntityAId = bId AndAlso p.EntityBId = aId))
                    If pairMatch IsNot Nothing Then
                        group.PairwiseResults.Add(pairMatch)
                    End If
                Next
            Next

            ' 置信度 = 1 - 最小校正 p-value
            If group.PairwiseResults.Count > 0 Then
                Dim minP As Double = group.PairwiseResults.Min(Function(p) p.BonferroniCorrectedPValue)
                group.Confidence = std.Max(0.0, std.Min(1.0, 1.0 - minP))
            End If

            ' 规范名称：度数最大的实体
            Dim canonicalId As Integer = group.EntityIds(0)
            Dim maxDeg As Integer = _graph.GetEntityDegree(canonicalId)
            For Each eid In group.EntityIds
                Dim deg As Integer = _graph.GetEntityDegree(eid)
                If deg > maxDeg Then
                    maxDeg = deg
                    canonicalId = eid
                End If
            Next
            group.CanonicalName = _graph.Entities(canonicalId).Name
            group.CanonicalLanguage = _graph.Entities(canonicalId).Language

            result.Add(group)
        Next

        Return result
    End Function

    ' --- 并查集辅助方法 ---

    Private Function Find(parent As Integer(), x As Integer) As Integer
        While parent(x) <> x
            parent(x) = parent(parent(x))  ' 路径压缩
            x = parent(x)
        End While
        Return x
    End Function

    Private Sub Union(parent As Integer(), x As Integer, y As Integer)
        Dim rx As Integer = Find(parent, x)
        Dim ry As Integer = Find(parent, y)
        If rx <> ry Then
            parent(rx) = ry
        End If
    End Sub

End Class

