Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.Hamming

''' <summary>
''' Hamming 聚类（k-modes 与 Muselli–Liberati 规则提取）的非交互自测。
''' 
''' ```bash
''' dotnet run --project test/test.vbproj -- hamming
''' ```
''' </summary>
Friend Module HammingSelfTest

    ''' <summary>
    ''' 执行全部断言，返回实际执行的检查数量。
    ''' </summary>
    ''' <param name="failures">断言失败信息的收集列表</param>
    ''' <returns>执行的检查数量</returns>
    Public Function Run(failures As List(Of String)) As Integer
        Dim checks As Integer = 0

        Call RunDistanceChecks(failures, checks)
        Call RunBitPatternChecks(failures, checks)
        Call RunEncoderChecks(failures, checks)
        Call RunKmodesChecks(failures, checks)
        Call RunRuleChecks(failures, checks)
        Call RunTableChecks(failures, checks)

        Return checks
    End Function

    Private Sub RunDistanceChecks(failures As List(Of String), ByRef checks As Integer)
        ' readme 中的四个 8 位串示例
        Dim a As String = "10110010"
        Dim b As String = "10100011"
        Dim c As String = "01001100"
        Dim d As String = "01101100"

        ' readme 第四节的四个 8 位串：按位统计的实际距离为 2 / 1 / 7 / 7
        ' （原文正文给出的 3 / 1 / 6 / 5 与其列出的位串并不自洽，这里以位串本身为准）
        Call Check(failures, checks, "d_H(A, B) = 2", HammingDistance.Between(a, b) = 2)
        Call Check(failures, checks, "d_H(C, D) = 1", HammingDistance.Between(c, d) = 1)
        Call Check(failures, checks, "d_H(A, C) = 7", HammingDistance.Between(a, c) = 7)
        Call Check(failures, checks, "d_H(B, C) = 7", HammingDistance.Between(b, c) = 7)
        Call Check(failures, checks, "d_H(A, A) = 0", HammingDistance.Between(a, a) = 0)
        Call Check(failures, checks, "distance is symmetric", HammingDistance.Between(a, b) = HammingDistance.Between(b, a))

        ' 三角不等式在全部三元组上都成立
        Dim all As String() = {a, b, c, d}
        Dim triangle As Boolean = True

        For i As Integer = 0 To all.Length - 1
            For j As Integer = 0 To all.Length - 1
                For k As Integer = 0 To all.Length - 1
                    If HammingDistance.Between(all(i), all(k)) > HammingDistance.Between(all(i), all(j)) + HammingDistance.Between(all(j), all(k)) Then
                        triangle = False
                    End If
                Next
            Next
        Next

        Call Check(failures, checks, "triangle inequality holds", triangle)

        ' 类别行（非二进制）
        Call Check(failures, checks, "categorical integer rows", HammingDistance.Between(New Integer() {1, 2, 3}, New Integer() {1, 9, 3}) = 1)

        Dim left As Char() = {"a"c, "b"c, "c"c}
        Dim right As Char() = {"a"c, "x"c, "y"c}

        Call Check(failures, checks, "categorical char rows", HammingDistance.Between(left, right) = 2)

        ' 加权距离：a 与 b 只在第 3、7 位不同
        Dim weights As Double() = {1, 10, 100, 1000, 1, 1, 1, 1}

        Call Check(failures, checks, "weighted hamming distance", HammingDistance.Weighted(a, b, weights) = 1001)

        ' 相似度换算
        Call Check(failures, checks, "similarity of equal patterns", HammingDistance.Similarity(0, 8) = 1)
        Call Check(failures, checks, "similarity of opposite patterns", HammingDistance.Similarity(8, 8) = 0)

        Dim thrown As Boolean = False

        Try
            Call HammingDistance.Between("101", "10")
        Catch ex As ArgumentException
            thrown = True
        End Try

        Call Check(failures, checks, "length mismatch throws", thrown)
    End Sub

    Private Sub RunBitPatternChecks(failures As List(Of String), ByRef checks As Integer)
        Dim a As String = "10110010"
        Dim b As String = "10100011"
        Dim pa As BitPattern = BitPattern.FromString(a)
        Dim pb As BitPattern = BitPattern.FromString(b)

        Call Check(failures, checks, "bit length", pa.BitLength = 8)
        Call Check(failures, checks, "bit string roundtrip", pa.ToString() = a)
        Call Check(failures, checks, "bit pattern distance", pa.DistanceTo(pb) = 2)
        Call Check(failures, checks, "popcount of the pattern", pa.CountOnes() = 4)
        Call Check(failures, checks, "bit access", pa.BitAt(0) AndAlso pa.BitAt(2) AndAlso pa.BitAt(3) AndAlso pa.BitAt(6))
        Call Check(failures, checks, "bit access for zero", Not pa.BitAt(1) AndAlso Not pa.BitAt(4))

        ' 跨机器字边界（65 位）的打包与回显
        Dim big As String = New String("0"c, 64) & "1"
        Dim pbig As BitPattern = BitPattern.FromString(big)

        Call Check(failures, checks, "65 bits length", pbig.BitLength = 65)
        Call Check(failures, checks, "65 bits roundtrip", pbig.ToString() = big)
        Call Check(failures, checks, "65 bits tail bit", pbig.BitAt(64))
        Call Check(failures, checks, "65 bits distance", pbig.DistanceTo(BitPattern.FromString(New String("0"c, 65))) = 1)

        ' 位运算
        Call Check(failures, checks, "xor with itself", pa.XorWith(pa).CountOnes() = 0)
        Call Check(failures, checks, "not of pattern", pa.NotOf().ToString() = "01001101")
        Call Check(failures, checks, "not distance = dimension", pa.NotOf().DistanceTo(pa) = 8)
        Call Check(failures, checks, "and with itself", pa.AndWith(pa).ToString() = a)

        ' 数值向量二值化
        Call Check(failures, checks, "from values with threshold", BitPattern.FromValues(New Double() {0, 1, 2}, 0.5).ToString() = "011")
        Call Check(failures, checks, "from booleans", BitPattern.FromBooleans(New Boolean() {True, False, True}).ToString() = "101")
        Call Check(failures, checks, "from words", BitPattern.FromWords(pa.Words, 8).ToString() = a)
        Call Check(failures, checks, "operator equality", pa = BitPattern.FromString(a))

        Dim thrown As Boolean = False

        Try
            Call BitPattern.FromString("10a1")
        Catch ex As ArgumentException
            thrown = True
        End Try

        Call Check(failures, checks, "invalid binary character throws", thrown)

        thrown = False

        Try
            Call BitPattern.FromString("101").DistanceTo(BitPattern.FromString("10"))
        Catch ex As ArgumentException
            thrown = True
        End Try

        Call Check(failures, checks, "pattern length mismatch throws", thrown)

        ' 加权距离走位打包路径
        Dim weights As Double() = {1, 10, 100, 1000, 1, 1, 1, 1}

        Call Check(failures, checks, "weighted distance on words", HammingDistance.Weighted(pa.Words, pb.Words, weights) = 1001)
    End Sub

    Private Sub RunEncoderChecks(failures As List(Of String), ByRef checks As Integer)
        Dim a As String = "10110010"
        Dim b As String = "10100011"
        Dim c As String = "01001100"
        Dim d As String = "01101100"
        Dim data As HammingData = HammingEncoder.FromBitStrings({a, b, c, d})

        Call Check(failures, checks, "encoded samples", data.Samples = 4)
        Call Check(failures, checks, "encoded dimension", data.Dimension = 8)
        Call Check(failures, checks, "encoded as binary", data.IsBinary)
        Call Check(failures, checks, "encoded first bit", data.Codes(0)(0) = 1)
        Call Check(failures, checks, "encode/decode roundtrip", data.ToPatterns()(2).ToString() = c)

        ' 三值定类列：不能走位运算快速路径
        Dim mixed As HammingData = HammingEncoder.FromCategories(New String()() {
            New String() {"a", "x"},
            New String() {"b", "x"},
            New String() {"c", "y"}
        })

        Call Check(failures, checks, "mixed categories are not binary", Not mixed.IsBinary)
        Call Check(failures, checks, "mixed cardinality", mixed.Cardinality(0) = 3)
        Call Check(failures, checks, "mixed distance (same column)", HammingDistance.Between(mixed.Codes(0), mixed.Codes(1)) = 1)
        Call Check(failures, checks, "mixed distance (two columns)", HammingDistance.Between(mixed.Codes(0), mixed.Codes(2)) = 2)

        Dim thrown As Boolean = False

        Try
            Call mixed.ToPatterns()
        Catch ex As InvalidOperationException
            thrown = True
        End Try

        Call Check(failures, checks, "packing non-binary data throws", thrown)

        ' 字符类别行
        Dim chars As HammingData = HammingEncoder.FromCategories(New Char()() {
            New Char() {"r"c, "b"c},
            New Char() {"r"c, "s"c}
        })

        Call Check(failures, checks, "char categories", chars.Samples = 2 AndAlso chars.Dimension = 2)

        ' 数值矩阵：默认按阈值二值化
        Dim values As HammingData = HammingEncoder.FromValues(New Double()() {
            New Double() {0, 1},
            New Double() {1, 0}
        })

        Call Check(failures, checks, "numeric values are binary", values.IsBinary)
        Call Check(failures, checks, "numeric values distance", HammingDistance.Between(values.Codes(0), values.Codes(1)) = 2)
    End Sub

    Private Sub RunKmodesChecks(failures As List(Of String), ByRef checks As Integer)
        Dim a As String = "10110010"
        Dim b As String = "10100011"
        Dim c As String = "01001100"
        Dim d As String = "01101100"
        Dim engine As New KmodesEngine()
        Dim result As KmodesResult = engine.RunBitStrings({a, b, c, d}, 2, seed:=1234)

        Call Check(failures, checks, "kmodes k", result.K = 2)
        Call Check(failures, checks, "kmodes label count", result.Labels.Length = 4)
        Call Check(failures, checks, "kmodes groups {A,B}", result.Labels(0) = result.Labels(1))
        Call Check(failures, checks, "kmodes groups {C,D}", result.Labels(2) = result.Labels(3))
        Call Check(failures, checks, "kmodes separates the two clusters", result.Labels(0) <> result.Labels(2))
        Call Check(failures, checks, "kmodes total cost", result.Cost = 3)
        Call Check(failures, checks, "kmodes cluster sizes", result.ClusterSizes.Sum() = 4)
        Call Check(failures, checks, "kmodes converged", result.Converged)
        Call Check(failures, checks, "kmodes no empty cluster", result.EmptyClusters = 0)

        ' 同一个种子必须得到完全相同的结果
        Dim again As KmodesResult = engine.RunBitStrings({a, b, c, d}, 2, seed:=1234)

        Call Check(failures, checks, "kmodes is reproducible", again.Labels.SequenceEqual(result.Labels))
        Call Check(failures, checks, "kmodes seed is reported", again.Seed = 1234)

        ' k = 1：所有样本同簇，代价等于按位众数质心的总距离
        Dim singleCluster As KmodesResult = engine.RunBitStrings({a, b, c, d}, 1, seed:=7)

        Call Check(failures, checks, "k=1 single cluster", singleCluster.K = 1)
        Call Check(failures, checks, "k=1 all labels zero", singleCluster.Labels.All(Function(l) l = 0))
        Call Check(failures, checks, "k=1 centroid is the bitwise majority", singleCluster.CentroidPatterns()(0).ToString() = "00100000")
        Call Check(failures, checks, "k=1 cost", singleCluster.Cost = 13)

        ' 完全相同的样本：初始化退化为顺序取未选样本，不应崩溃
        Dim identical As KmodesResult = engine.RunBitStrings({"1111", "1111", "1111"}, 3, seed:=42)

        Call Check(failures, checks, "identical samples k", identical.K = 3)
        Call Check(failures, checks, "identical samples cost", identical.Cost = 0)
        Call Check(failures, checks, "identical samples empty clusters", identical.EmptyClusters = 2)
        Call Check(failures, checks, "identical samples sizes", identical.ClusterSizes.Sum() = 3)

        ' 非二值的定类数据走逐列比较路径
        Dim categories As HammingData = HammingEncoder.FromCategories(New String()() {
            New String() {"a", "x"},
            New String() {"a", "x"},
            New String() {"b", "y"},
            New String() {"b", "z"}
        })
        Dim catResult As KmodesResult = engine.Run(categories, 2, seed:=99)

        Call Check(failures, checks, "categorical data is not binary", Not categories.IsBinary)
        Call Check(failures, checks, "categorical groups {0,1}", catResult.Labels(0) = catResult.Labels(1))
        Call Check(failures, checks, "categorical groups {2,3}", catResult.Labels(2) = catResult.Labels(3))
        Call Check(failures, checks, "categorical separates the two clusters", catResult.Labels(0) <> catResult.Labels(2))

        Dim thrown As Boolean = False

        Try
            Call engine.RunBitStrings({a, b}, 3, seed:=1)
        Catch ex As ArgumentException
            thrown = True
        End Try

        Call Check(failures, checks, "k > samples throws", thrown)

        ' 串行与并行路径必须给出一致的结果
        Dim serial As KmodesResult = engine.RunBitStrings({a, b, c, d}, 2, seed:=1234, parallel:=False)

        Call Check(failures, checks, "serial path matches", serial.Labels.SequenceEqual(result.Labels))

        ' 随机初始化路径
        Dim randomInit As KmodesResult = engine.RunBitStrings({a, b, c, d}, 2, seed:=5, initialization:=KmodesInitialization.Random)

        Call Check(failures, checks, "random initialization k", randomInit.K = 2)
        Call Check(failures, checks, "random initialization sizes", randomInit.ClusterSizes.Sum() = 4)

        ' 样本数量超过并行阈值时，并行分配与串行分配必须一致
        Dim rnd As New Random(2026)
        Dim bulk As New List(Of Double())()

        For i As Integer = 0 To 799
            Dim row As Double() = New Double(15) {}

            For j As Integer = 0 To 15
                If i Mod 2 = 0 Then
                    row(j) = If(rnd.NextDouble() < 0.8, 1.0, 0.0)
                Else
                    row(j) = If(rnd.NextDouble() < 0.2, 1.0, 0.0)
                End If
            Next

            Call bulk.Add(row)
        Next

        Dim bulkRows As Double()() = bulk.ToArray
        Dim bulkParallel As KmodesResult = engine.Run(bulkRows, 2, seed:=2026)
        Dim bulkSerial As KmodesResult = engine.Run(bulkRows, 2, seed:=2026, parallel:=False)

        Call Check(failures, checks, "parallel assignment matches serial", bulkParallel.Labels.SequenceEqual(bulkSerial.Labels))
        Call Check(failures, checks, "bulk cluster sizes", bulkParallel.ClusterSizes.Sum() = 800)
    End Sub

    Private Sub RunRuleChecks(failures As List(Of String), ByRef checks As Integer)
        Dim engine As New HammingClusteringEngine()

        ' AND 函数：仅当两位都为 1 时属于类别 1
        Dim andPatterns As BitPattern() = {
            BitPattern.FromString("00"),
            BitPattern.FromString("01"),
            BitPattern.FromString("10"),
            BitPattern.FromString("11")
        }
        Dim andLabels As Integer() = {0, 0, 0, 1}
        Dim rules As RuleSet = engine.Run(andPatterns, andLabels)

        Call Check(failures, checks, "and training accuracy", rules.Report.OverallAccuracy = 1)
        Call Check(failures, checks, "and training accuracy count", rules.Report.Correct = 4)
        Call Check(failures, checks, "and class count", rules.Classes.Length = 2)
        Call Check(failures, checks, "and predictions", rules.PredictAll(andPatterns).SequenceEqual(andLabels))

        Dim confusionTotal As Integer = 0

        For Each row As Integer() In rules.Report.ConfusionMatrix
            For Each v As Integer In row
                confusionTotal += v
            Next
        Next

        Call Check(failures, checks, "confusion matrix covers all predictions", confusionTotal = 4 - rules.Report.Unmatched)
        Call Check(failures, checks, "report labels", rules.Report.Labels.SequenceEqual(New Integer() {0, 1}))
        Call Check(failures, checks, "per class recall", rules.Report.PerClass.All(Function(x) x.Recall = 1))

        Dim classOne As ClassRule = rules.Classes.First(Function(x) x.Label = 1)

        Call Check(failures, checks, "and class 1 has rules", classOne.Rules.Length >= 1)
        Call Check(failures, checks, "and class 1 rule is a conjunction of two literals", classOne.Rules(0).Specificity = 2)
        Call Check(failures, checks, "and rules are readable", rules.ToString().Contains("class 1 <= (x0 ∧ x1)"))
        Call Check(failures, checks, "and class 0 uses negated literals", rules.ToString().Contains("¬x0") AndAlso rules.ToString().Contains("¬x1"))

        ' 剪枝不应增加规则数量
        Dim unpruned As RuleSet = engine.Run(andPatterns, andLabels, prune:=False)
        Dim prunedCount As Integer = rules.Classes.Select(Function(x) x.Rules.Length).Sum()
        Dim unprunedCount As Integer = unpruned.Classes.Select(Function(x) x.Rules.Length).Sum()

        Call Check(failures, checks, "pruning does not add rules", prunedCount <= unprunedCount)
        Call Check(failures, checks, "unpruned rules are still consistent", unpruned.Report.OverallAccuracy = 1)

        ' 冗余位：第 2 位恒为 0 且与类别无关，剪枝之后不应出现在规则中
        Dim redundantPatterns As BitPattern() = {
            BitPattern.FromString("001"),
            BitPattern.FromString("011"),
            BitPattern.FromString("101"),
            BitPattern.FromString("111")
        }
        Dim redundantLabels As Integer() = {0, 0, 1, 1}
        Dim redundantRules As RuleSet = engine.Run(redundantPatterns, redundantLabels)
        Dim usesRedundantFeature As Boolean = False

        For Each cls As ClassRule In redundantRules.Classes
            For Each rule As ConjunctionRule In cls.Rules
                For Each literal As Literal In rule.Literals
                    If literal.Feature = 2 Then
                        usesRedundantFeature = True
                    End If
                Next
            Next
        Next

        Call Check(failures, checks, "pruning removes redundant literals", Not usesRedundantFeature)
        Call Check(failures, checks, "redundant feature accuracy", redundantRules.Report.OverallAccuracy = 1)

        ' 单类别：判别函数退化为恒真
        Dim constantPatterns As BitPattern() = {BitPattern.FromString("01"), BitPattern.FromString("10")}
        Dim constantRules As RuleSet = engine.Run(constantPatterns, New Integer() {5, 5})

        Call Check(failures, checks, "single class count", constantRules.Classes.Length = 1)
        Call Check(failures, checks, "single class accuracy", constantRules.Report.OverallAccuracy = 1)
        Call Check(failures, checks, "single class constant rule", constantRules.ToString().Contains("TRUE"))
        Call Check(failures, checks, "single class predicts everything", constantRules.Predict(BitPattern.FromString("11")) = 5)

        ' 没有命中的规则集合返回 -1
        Dim emptyRules As New RuleSet(New ClassRule() {})

        Call Check(failures, checks, "empty rule set returns -1", emptyRules.Predict(BitPattern.FromString("1010")) = -1)
        Call Check(failures, checks, "empty rule set report is nothing", emptyRules.Report Is Nothing)

        ' 规则取自 k-modes 的标签列时同样可以得到训练集一致的模型
        Dim clusterPatterns As BitPattern() = {
            BitPattern.FromString("10110010"),
            BitPattern.FromString("10100011"),
            BitPattern.FromString("01001100"),
            BitPattern.FromString("01101100")
        }
        Dim clusterLabels As Integer() = {1, 1, 2, 2}
        Dim clusterRules As RuleSet = engine.Run(clusterPatterns, clusterLabels)

        Call Check(failures, checks, "readme example rules accuracy", clusterRules.Report.OverallAccuracy = 1)
        Call Check(failures, checks, "readme example rule count", clusterRules.Classes.Length = 2)
    End Sub

    Private Sub RunTableChecks(failures As List(Of String), ByRef checks As Integer)
        Dim features As Double()() = {
            New Double() {1, 0, 1, 1, 0, 0, 1, 0},
            New Double() {1, 0, 1, 0, 0, 0, 1, 1},
            New Double() {0, 1, 0, 0, 1, 1, 0, 0},
            New Double() {0, 1, 1, 0, 1, 1, 0, 0}
        }
        Dim table As New NumericTable(features, New String() {"A", "B", "C", "D"})

        Call table.hammingCluster(2, seed:=7)

        Dim clusterLabels As Double() = table.GetLabel("cluster")

        Call Check(failures, checks, "table cluster column length", clusterLabels.Length = 4)
        Call Check(failures, checks, "table cluster labels are 1-based", clusterLabels.All(Function(v) v >= 1 AndAlso v <= 2))

        Dim codes As Integer() = clusterLabels.Select(Function(v) CInt(v)).ToArray()

        Call Check(failures, checks, "table groups {A,B}", codes(0) = codes(1))
        Call Check(failures, checks, "table groups {C,D}", codes(2) = codes(3))
        Call Check(failures, checks, "table separates the two clusters", codes(0) <> codes(2))

        Dim patterns As BitPattern() = table.hammingPatterns()

        Call Check(failures, checks, "table patterns", patterns.Length = 4 AndAlso patterns(0).ToString() = "10110010")

        Dim rules As RuleSet = table.hammingRuleExtract()

        Call Check(failures, checks, "table rules accuracy", rules.Report.OverallAccuracy = 1)
        Call Check(failures, checks, "table rules class count", rules.Classes.Length = 2)

        Call table.hammingRulePredict(rules)

        Dim predicted As Integer() = table.GetLabel("hamming_rule").Select(Function(v) CInt(v)).ToArray()

        Call Check(failures, checks, "table prediction column length", predicted.Length = 4)
        Call Check(failures, checks, "table prediction matches the cluster column", predicted.SequenceEqual(codes))

        ' 直接使用特征矩阵的入口
        Dim engine As New KmodesEngine()
        Dim matrixResult As KmodesResult = engine.Run(features, 2, seed:=7)

        Call Check(failures, checks, "matrix entry labels", matrixResult.Labels.Length = 4)
        Call Check(failures, checks, "matrix entry matches the table entry", matrixResult.Labels.Select(Function(l) l + 1).SequenceEqual(codes))
    End Sub

    Private Sub Check(failures As List(Of String), ByRef checks As Integer, name As String, condition As Boolean)
        checks += 1

        If Not condition Then
            Call failures.Add("hamming: " & name)
        End If
    End Sub
End Module
