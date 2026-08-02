' ============================================================================
' Program.vb - 知识图谱实体消歧与本体论推断 Demo
'
' 展示内容：
'   1. 知识图谱统计信息
'   2. 全部实体对相似度排名
'   3. 实体消歧结果（同义实体检测，含 p-value 和 z-score）
'   4. 本体论关系推断（Is-A / Sibling / Related）
'   5. 总结
' ============================================================================

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text
Imports KnowledgeGraph

Module Program

    Private Const BAR As String = "══════════════════════════════════════════════════════════════════════════"
    Private Const THIN As String = "──────────────────────────────────────────────────────────────────────────"

    Function Main(args As String()) As Integer
        ' 设置控制台编码
        Console.OutputEncoding = Encoding.UTF8

        Console.WriteLine(BAR)
        Console.WriteLine("  知识图谱实体消歧与本体论推断系统")
        Console.WriteLine("  Knowledge Graph Entity Disambiguation & Ontology Inference")
        Console.WriteLine(BAR)
        Console.WriteLine()

        ' ================================================================
        ' Step 1: 构建知识图谱
        ' ================================================================
        Console.WriteLine("[1] 构建知识图谱...")
        Dim graph = TestKnowledgeBase.BuildTestGraph()

        PrintGraphStats(graph)
        Console.WriteLine()

        ' ================================================================
        ' Step 2: 计算全部实体对相似度
        ' ================================================================
        Console.WriteLine("[2] 计算全部实体对相似度...")
        Console.WriteLine()
        PrintSimilarityRanking(graph)
        Console.WriteLine()

        ' ================================================================
        ' Step 3: 实体消歧
        ' ================================================================
        Console.WriteLine("[3] 实体消歧（同义实体检测）...")
        Console.WriteLine()
        Dim disambiguator As New EntityDisambiguator(graph, New DisambiguationOptions With {
            .SynonymJaccardThreshold = 0.45,
            .SynonymPValueThreshold = 0.01,
            .PermutationIterations = 5000
        })
        Dim synonymGroups As List(Of SynonymGroup) = disambiguator.Disambiguate()
        PrintDisambiguationResults(graph, synonymGroups)
        Console.WriteLine()

        ' ================================================================
        ' Step 4: 本体论推断
        ' ================================================================
        Console.WriteLine("[4] 本体论关系推断...")
        Console.WriteLine()
        Dim inferer As New OntologyInferer(graph, New OntologyOptions With {
            .PermutationIterations = 5000
        })
        Dim relations As List(Of OntologyRelation) = inferer.InferRelations(synonymGroups)
        PrintOntologyRelations(graph, relations)
        Console.WriteLine()

        ' ================================================================
        ' Step 5: 非同义对照分析
        ' ================================================================
        Console.WriteLine("[5] 非同义对照分析...")
        Console.WriteLine()
        PrintNonSynonymAnalysis(graph)
        Console.WriteLine()

        ' ================================================================
        ' 总结
        ' ================================================================
        PrintSummary(synonymGroups, relations)

        Return 0
    End Function

    ' ================================================================
    ' 输出方法
    ' ================================================================

    Private Sub PrintGraphStats(graph As KnowledgeGraph)
        Console.WriteLine($"  实体节点数: {graph.Entities.Count}")
        Console.WriteLine($"  属性节点数: {graph.Attributes.Count}")
        Console.WriteLine($"  边连接总数: {graph.TotalEdges}")
        Console.WriteLine()

        ' 实体列表
        Console.WriteLine("  实体列表:")
        For i As Integer = 0 To graph.Entities.Count - 1
            Dim e = graph.Entities(i)
            Console.WriteLine($"    {i + 1,2}. {e.Name,-12} ({e.Language}) [{e.EntityType}]  度={graph.GetEntityDegree(i)}")
        Next
        Console.WriteLine()

        ' 属性类别统计
        Console.WriteLine("  属性类别统计:")
        Dim stats = graph.GetAttributeCategoryStats()
        For Each kvp In stats
            Console.WriteLine($"    {kvp.Key,-20}: {kvp.Value}")
        Next
    End Sub

    Private Sub PrintSimilarityRanking(graph As KnowledgeGraph)
        Dim allPairs As List(Of SimilarityResult) = SimilarityMetrics.ComputeAllPairs(graph)

        Console.WriteLine(BAR)
        Console.WriteLine("  全部实体对相似度排名 (Top 20)")
        Console.WriteLine(BAR)
        Console.WriteLine($"  {"排名",-4} {"实体A",-12} {"实体B",-12} {"Jaccard",8} {"AA指数",10} {"共享数",6} {"包含率",8}")
        Console.WriteLine(THIN)

        Dim count As Integer = Math.Min(20, allPairs.Count)
        For i As Integer = 0 To count - 1
            Dim r = allPairs(i)
            If r.Jaccard = 0 Then Exit For
            Dim nameA As String = graph.Entities(r.EntityAId).Name
            Dim nameB As String = graph.Entities(r.EntityBId).Name
            Console.WriteLine($"  {i + 1,4} {nameA,-12} {nameB,-12} {r.Jaccard,8:F4} {r.AdamicAdar,10:F3} {r.CommonNeighbors,6} {r.MaxInclusion,8:F3}")
        Next
    End Sub

    Private Sub PrintDisambiguationResults(graph As KnowledgeGraph, groups As List(Of SynonymGroup))
        Console.WriteLine(BAR)
        Console.WriteLine("  实体消歧结果（同义词检测）")
        Console.WriteLine(BAR)

        If groups.Count = 0 Then
            Console.WriteLine("  未检测到同义实体组。")
            Return
        End If

        For gi As Integer = 0 To groups.Count - 1
            Dim group = groups(gi)
            Console.WriteLine()
            Console.WriteLine($"  [同义组 {gi + 1}] 整体置信度: {group.Confidence:F3}")

            For Each pair In group.PairwiseResults
                Console.WriteLine($"    {pair.NameA} ↔ {pair.NameB}")
                Console.WriteLine($"      Jaccard:        {pair.Jaccard:F4}")
                Console.WriteLine($"      AA 指数:         {pair.AdamicAdar:F3}")
                Console.WriteLine($"      Jaccard p-value: {pair.JaccardPValue:F6}  z={pair.JaccardZScore:F2}  {GetStars(pair.JaccardPValue)}")
                Console.WriteLine($"      AA p-value:      {pair.AAPValue:F6}  z={pair.AAZScore:F2}  {GetStars(pair.AAPValue)}")
                Console.WriteLine($"      Bonferroni校正:  {pair.BonferroniCorrectedPValue:F6}  {GetStars(pair.BonferroniCorrectedPValue)}")
            Next

            Dim entityNames As New List(Of String)
            For Each eid In group.EntityIds
                entityNames.Add(graph.Entities(eid).Name)
            Next
            Console.WriteLine($"      → 结论: {String.Join("、", entityNames)} 指代同一知识对象")
            Console.WriteLine($"      → 规范名称: {group.CanonicalName} ({group.CanonicalLanguage})")
        Next
    End Sub

    Private Sub PrintOntologyRelations(graph As KnowledgeGraph, relations As List(Of OntologyRelation))
        Console.WriteLine(BAR)
        Console.WriteLine("  本体论关系推断结果")
        Console.WriteLine(BAR)

        If relations.Count = 0 Then
            Console.WriteLine("  未推断出本体论关系。")
            Return
        End If

        ' 按类型分组输出
        Dim currentType As OntologyRelationType = CType(-1, OntologyRelationType)

        For Each rel In relations
            If rel.RelationType <> currentType Then
                currentType = rel.RelationType
                Console.WriteLine()
                Select Case currentType
                    Case OntologyRelationType.IsA
                        Console.WriteLine("  ── Is-A (上下位关系) ──")
                    Case OntologyRelationType.SiblingOf
                        Console.WriteLine("  ── Sibling-Of (同层级关系) ──")
                    Case OntologyRelationType.HasFunction
                        Console.WriteLine("  ── Has-Function (功能关联) ──")
                    Case OntologyRelationType.RelatedTo
                        Console.WriteLine("  ── Related-To (一般关联) ──")
                End Select
            End If

            Console.WriteLine($"    [{rel.RelationType}] {rel.SubjectName} → {rel.ObjectName}")
            Console.WriteLine($"      Jaccard: {rel.Jaccard:F4}  AA: {rel.AdamicAdar:F3}  包含率: {rel.InclusionRatio:F3}")
            Console.WriteLine($"      置信度: {rel.Confidence:F3}  p-value: {rel.PValue:F6}  {GetStars(rel.PValue)}")
            Console.WriteLine($"      共享属性: {String.Join(", ", rel.SharedAttributes.ToArray())}")
        Next
    End Sub

    Private Sub PrintNonSynonymAnalysis(graph As KnowledgeGraph)
        Console.WriteLine(BAR)
        Console.WriteLine("  非同义对照分析（名称相似但指代不同对象）")
        Console.WriteLine(BAR)
        Console.WriteLine()

        ' 苹果(水果) vs 苹果公司
        Dim pingguoId As Integer = -1, appleIncId As Integer = -1
        For i As Integer = 0 To graph.Entities.Count - 1
            If graph.Entities(i).Name = "苹果" Then pingguoId = i
            If graph.Entities(i).Name = "苹果公司" Then appleIncId = i
        Next

        If pingguoId >= 0 AndAlso appleIncId >= 0 Then
            Dim sim = SimilarityMetrics.ComputeSimilarity(graph, pingguoId, appleIncId)
            Console.WriteLine($"  对照: 苹果 (水果) vs 苹果公司 (科技公司)")
            Console.WriteLine($"    名称相似度: 均含「苹果」一词")
            Console.WriteLine($"    Jaccard:    {sim.Jaccard:F4}")
            Console.WriteLine($"    AA 指数:    {sim.AdamicAdar:F3}")
            Console.WriteLine($"    共享属性:   {sim.CommonNeighbors} 个")
            Console.WriteLine($"    结论:       虽然名称都包含「苹果」，但属性无交集")
            Console.WriteLine($"                → 判定为不同知识对象")
        End If

        Console.WriteLine()

        ' water vs 老虎
        Dim waterId As Integer = -1, tigerId As Integer = -1
        For i As Integer = 0 To graph.Entities.Count - 1
            If graph.Entities(i).Name = "water" Then waterId = i
            If graph.Entities(i).Name = "老虎" Then tigerId = i
        Next

        If waterId >= 0 AndAlso tigerId >= 0 Then
            Dim sim = SimilarityMetrics.ComputeSimilarity(graph, waterId, tigerId)
            Console.WriteLine($"  对照: water (物质) vs 老虎 (生物)")
            Console.WriteLine($"    Jaccard:    {sim.Jaccard:F4}")
            Console.WriteLine($"    AA 指数:    {sim.AdamicAdar:F3}")
            Console.WriteLine($"    共享属性:   {sim.CommonNeighbors} 个")
            Console.WriteLine($"    结论:       完全不相关，无属性交集")
        End If
    End Sub

    Private Sub PrintSummary(synonymGroups As List(Of SynonymGroup), relations As List(Of OntologyRelation))
        Console.WriteLine()
        Console.WriteLine(BAR)
        Console.WriteLine("  总结")
        Console.WriteLine(BAR)
        Console.WriteLine()

        Dim mergedEntities As Integer = 0
        For Each g In synonymGroups
            mergedEntities += g.EntityIds.Count - 1
        Next

        Console.WriteLine($"  检测到同义实体组: {synonymGroups.Count} 组")
        Console.WriteLine($"  合并实体数量:     {mergedEntities} 个")
        Console.WriteLine($"  推断本体论关系:   {relations.Count} 条")

        Dim isA = relations.Where(Function(r) r.RelationType = OntologyRelationType.IsA).Count()
        Dim sibling = relations.Where(Function(r) r.RelationType = OntologyRelationType.SiblingOf).Count()
        Dim hasFunc = relations.Where(Function(r) r.RelationType = OntologyRelationType.HasFunction).Count()
        Dim related = relations.Where(Function(r) r.RelationType = OntologyRelationType.RelatedTo).Count()

        Console.WriteLine($"    Is-A:        {isA}")
        Console.WriteLine($"    Sibling-Of:  {sibling}")
        Console.WriteLine($"    Has-Function:{hasFunc}")
        Console.WriteLine($"    Related-To:  {related}")
        Console.WriteLine()
        Console.WriteLine("  显著性标记: *** p<0.001  ** p<0.01  * p<0.05  . p<0.1")
        Console.WriteLine()
        Console.WriteLine(BAR)
    End Sub

    Private Function GetStars(pValue As Double) As String
        If pValue < 0.001 Then Return "***"
        If pValue < 0.01 Then Return "**"
        If pValue < 0.05 Then Return "*"
        If pValue < 0.1 Then Return "."
        Return ""
    End Function

End Module
