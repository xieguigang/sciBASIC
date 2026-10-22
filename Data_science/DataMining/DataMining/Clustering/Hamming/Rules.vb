Imports System.Collections.Generic
Imports System.Linq

Namespace Hamming

    ''' <summary>
    ''' 合取项中的一个字面量：把某一位（特征）固定成指定的二进制取值。
    ''' 
    ''' 例如 ``x1`` 表示「第 1 位必须为 1」，``¬x1`` 表示「第 1 位必须为 0」。
    ''' </summary>
    Public Structure Literal

        ''' <summary>
        ''' 位下标（0 基，也就是特征下标）
        ''' </summary>
        Public ReadOnly Feature As Integer

        ''' <summary>
        ''' 该位被固定的取值
        ''' </summary>
        Public ReadOnly Value As Boolean

        ''' <summary>
        ''' 构造一个字面量。
        ''' </summary>
        ''' <param name="feature">位下标（0 基）</param>
        ''' <param name="value">该位被固定的取值</param>
        Public Sub New(feature As Integer, value As Boolean)
            Me.Feature = feature
            Me.Value = value
        End Sub

        ''' <summary>
        ''' 判断当前字面量是否与给定的位模式相容。
        ''' </summary>
        ''' <param name="pattern">待判断的位模式</param>
        ''' <returns></returns>
        Public Function Matches(pattern As BitPattern) As Boolean
            If pattern Is Nothing Then
                Throw New ArgumentNullException(NameOf(pattern))
            End If

            Return pattern.BitAt(Feature) = Value
        End Function

        ''' <summary>
        ''' 回显为 ``x1`` / ``¬x1`` 形式的可读文本。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            If Value Then
                Return $"x{Feature}"
            Else
                Return $"¬x{Feature}"
            End If
        End Function

        ''' <summary>
        ''' 按位下标与取值判断两个字面量是否相同。
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj IsNot Literal Then
                Return False
            End If

            Dim other As Literal = DirectCast(obj, Literal)

            Return other.Feature = Feature AndAlso other.Value = Value
        End Function

        ''' <summary>
        ''' 由位下标与取值组合而成的哈希值。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetHashCode() As Integer
            Return (Feature * 397) Xor If(Value, 1, 0)
        End Function

        ''' <summary>
        ''' 字面量相等比较。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Operator =(x As Literal, y As Literal) As Boolean
            Return x.Equals(y)
        End Operator

        ''' <summary>
        ''' 字面量不等比较。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Operator <>(x As Literal, y As Literal) As Boolean
            Return Not x.Equals(y)
        End Operator
    End Structure

    ''' <summary>
    ''' 一条合取规则（AND 项）：当所有字面量都满足时规则命中。
    ''' 
    ''' 空字面量集合表示「恒真」，只会出现在单类别数据或者无法进一步泛化的场景中。
    ''' </summary>
    Public Class ConjunctionRule

        Private ReadOnly _literals As Literal()
        Private ReadOnly _label As Integer
        Private ReadOnly _covered As Integer()

        ''' <summary>
        ''' 构造一条合取规则。
        ''' </summary>
        ''' <param name="literals">规则包含的字面量集合（AND 项）</param>
        ''' <param name="label">该规则所判定的类别标签</param>
        ''' <param name="covered">构造该规则时覆盖的样本下标（可选，仅作为溯源信息）</param>
        Public Sub New(literals As Literal(), label As Integer, Optional covered As Integer() = Nothing)
            If literals Is Nothing Then
                _literals = New Literal() {}
            Else
                _literals = literals
            End If

            _label = label
            _covered = covered
        End Sub

        ''' <summary>
        ''' 规则包含的字面量集合（AND 项）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Literals As Literal()
            Get
                Return _literals
            End Get
        End Property

        ''' <summary>
        ''' 该规则所判定的类别标签
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Label As Integer
            Get
                Return _label
            End Get
        End Property

        ''' <summary>
        ''' 构造该规则时覆盖的样本下标（仅用于追溯规则的来源簇）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Covered As Integer()
            Get
                Return _covered
            End Get
        End Property

        ''' <summary>
        ''' 规则的特异度：字面量越多表示规则越具体（越不容易误判其它类）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Specificity As Integer
            Get
                Return _literals.Length
            End Get
        End Property

        ''' <summary>
        ''' 判断位模式是否满足这条合取规则。
        ''' </summary>
        ''' <param name="pattern">待判断的位模式</param>
        ''' <returns></returns>
        Public Function Matches(pattern As BitPattern) As Boolean
            If pattern Is Nothing Then
                Throw New ArgumentNullException(NameOf(pattern))
            End If

            For i As Integer = 0 To _literals.Length - 1
                If pattern.BitAt(_literals(i).Feature) <> _literals(i).Value Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 当前规则的字面量集合是否包含指定的字面量。
        ''' </summary>
        ''' <param name="literal">待检查的字面量</param>
        ''' <returns></returns>
        Public Function Contains(literal As Literal) As Boolean
            For i As Integer = 0 To _literals.Length - 1
                If _literals(i).Feature = literal.Feature Then
                    Return _literals(i).Value = literal.Value
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 判断当前规则是否比 <paramref name="other"/> 更一般（字面量集合是其子集）。
        ''' </summary>
        ''' <param name="other">另一条合取规则</param>
        ''' <returns></returns>
        Public Function Subsumes(other As ConjunctionRule) As Boolean
            If other Is Nothing Then
                Return False
            End If
            If other._label <> _label Then
                Return False
            End If
            If other._literals.Length <= _literals.Length Then
                Return False
            End If

            For i As Integer = 0 To _literals.Length - 1
                If Not other.Contains(_literals(i)) Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 回显为 ``x0 ∧ ¬x3`` 形式的合取项，恒真规则回显为 ``TRUE``。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            If _literals.Length = 0 Then
                Return "TRUE"
            End If

            Return String.Join(" ∧ ", _literals.Select(Function(l) l.ToString))
        End Function
    End Class

    ''' <summary>
    ''' 某一个类别的判别函数：由若干条合取规则以 OR 合并而成（DNF）。
    ''' </summary>
    Public Class ClassRule

        Private ReadOnly _label As Integer
        Private ReadOnly _rules As ConjunctionRule()

        ''' <summary>
        ''' 构造一个类别的判别函数。
        ''' </summary>
        ''' <param name="label">类别标签</param>
        ''' <param name="rules">该类别的合取规则集合（OR 关系）</param>
        Public Sub New(label As Integer, rules As ConjunctionRule())
            _label = label
            _rules = If(rules, New ConjunctionRule() {})
        End Sub

        ''' <summary>
        ''' 类别标签
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Label As Integer
            Get
                Return _label
            End Get
        End Property

        ''' <summary>
        ''' 该类别的合取规则集合（OR 关系）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Rules As ConjunctionRule()
            Get
                Return _rules
            End Get
        End Property

        ''' <summary>
        ''' 位模式是否命中该类的任意一条合取规则。
        ''' </summary>
        ''' <param name="pattern">待判断的位模式</param>
        ''' <returns></returns>
        Public Function Matches(pattern As BitPattern) As Boolean
            For i As Integer = 0 To _rules.Length - 1
                If _rules(i).Matches(pattern) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 返回命中的最具体规则的特异度（没有命中时返回 -1）。
        ''' </summary>
        ''' <param name="pattern">待判断的位模式</param>
        ''' <returns></returns>
        Public Function BestScore(pattern As BitPattern) As Integer
            Dim score As Integer = -1

            For i As Integer = 0 To _rules.Length - 1
                If _rules(i).Matches(pattern) AndAlso _rules(i).Specificity > score Then
                    score = _rules(i).Specificity
                End If
            Next

            Return score
        End Function

        ''' <summary>
        ''' 回显为 ``(x0 ∧ ¬x3) ∨ (x1)`` 形式的判别函数。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            If _rules.Length = 0 Then
                Return "FALSE"
            End If

            Return String.Join(" ∨ ", _rules.Select(Function(r) $"({r.ToString})"))
        End Function
    End Class

    ''' <summary>
    ''' Hamming 聚类（Muselli–Liberati 监督式规则提取）的输出：一组可读的布尔规则。
    ''' 
    ''' 每一条规则都是「若干位的取值固定」的合取项，同一类别的多条规则以 OR 合并，
    ''' 因此整个模型等价于一个只含 AND / OR / NOT 门的两级逻辑网络。
    ''' </summary>
    Public Class RuleSet

        Private ReadOnly _classes As ClassRule()

        ''' <summary>
        ''' 构造规则集合。类别会按照标签升序重新排列，以保证预测结果的确定性。
        ''' </summary>
        ''' <param name="classes">各个类别的判别函数</param>
        ''' <param name="report">训练集上的精度报告（可选）</param>
        Public Sub New(classes As ClassRule(), Optional report As RuleSetReport = Nothing)
            If classes Is Nothing Then
                _classes = New ClassRule() {}
            Else
                _classes = classes.OrderBy(Function(c) c.Label).ToArray
            End If

            Me.Report = report
        End Sub

        ''' <summary>
        ''' 各个类别的判别函数（按标签升序排列）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Classes As ClassRule()
            Get
                Return _classes
            End Get
        End Property

        ''' <summary>
        ''' 训练集上的精度报告，在没有提供训练标签时为 ``Nothing``
        ''' </summary>
        ''' <returns></returns>
        Public Property Report As RuleSetReport

        ''' <summary>
        ''' 对单个位模式进行分类。
        ''' 
        ''' 规则命中冲突时取「最具体」（字面量最多）的那条规则所属的类别；
        ''' 特异度也相同时取较小的类别标签。没有任何规则命中时返回 ``-1``。
        ''' </summary>
        ''' <param name="pattern">待预测的位模式</param>
        ''' <returns>类别标签，未命中返回 -1</returns>
        Public Function Predict(pattern As BitPattern) As Integer
            If pattern Is Nothing Then
                Throw New ArgumentNullException(NameOf(pattern))
            End If

            Dim bestLabel As Integer = -1
            Dim bestScore As Integer = -1

            For i As Integer = 0 To _classes.Length - 1
                Dim score As Integer = _classes(i).BestScore(pattern)

                ' 类别已按标签升序排列，因此严格大于才替换，天然实现「并列取小标签」
                If score > bestScore Then
                    bestScore = score
                    bestLabel = _classes(i).Label
                End If
            Next

            Return bestLabel
        End Function

        ''' <summary>
        ''' 批量分类。
        ''' </summary>
        ''' <param name="patterns">待预测的位模式集合</param>
        ''' <returns>与输入等长的类别标签数组，未命中为 -1</returns>
        Public Function PredictAll(patterns As BitPattern()) As Integer()
            If patterns Is Nothing Then
                Throw New ArgumentNullException(NameOf(patterns))
            End If

            Dim labels As Integer() = New Integer(patterns.Length - 1) {}

            For i As Integer = 0 To patterns.Length - 1
                labels(i) = Predict(patterns(i))
            Next

            Return labels
        End Function

        ''' <summary>
        ''' 回显为多行的规则文本，每一行对应一个类别。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return String.Join(Environment.NewLine, _classes.Select(Function(c) $"class {c.Label} <= {c.ToString}"))
        End Function
    End Class

    ''' <summary>
    ''' 单个类别的精度统计。
    ''' </summary>
    Public Class ClassReport

        Friend Sub New(label As Integer,
                       support As Integer,
                       predicted As Integer,
                       truePositive As Integer,
                       falsePositive As Integer,
                       falseNegative As Integer,
                       covered As Integer)

            Me.Label = label
            Me.Support = support
            Me.Predicted = predicted
            Me.TruePositive = truePositive
            Me.FalsePositive = falsePositive
            Me.FalseNegative = falseNegative
            Me.Covered = covered
        End Sub

        ''' <summary>
        ''' 类别标签
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Label As Integer

        ''' <summary>
        ''' 训练集中属于该类别的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Support As Integer

        ''' <summary>
        ''' 被预测成该类别的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Predicted As Integer

        ''' <summary>
        ''' 正确命中该类别的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TruePositive As Integer

        ''' <summary>
        ''' 被误判成该类别的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FalsePositive As Integer

        ''' <summary>
        ''' 属于该类别但未被命中的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FalseNegative As Integer

        ''' <summary>
        ''' 被该类别的规则（不论文本是否正确）覆盖的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Covered As Integer

        ''' <summary>
        ''' 精确率 ``tp / (tp + fp)``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Precision As Double
            Get
                Return SafeRatio(TruePositive, TruePositive + FalsePositive)
            End Get
        End Property

        ''' <summary>
        ''' 召回率 ``tp / (tp + fn)``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Recall As Double
            Get
                Return SafeRatio(TruePositive, TruePositive + FalseNegative)
            End Get
        End Property

        ''' <summary>
        ''' F1 值（精确率与召回率的调和平均）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property F1 As Double
            Get
                Dim precision As Double = Precision
                Dim recall As Double = Recall

                If precision + recall = 0 Then
                    Return 0
                Else
                    Return 2 * precision * recall / (precision + recall)
                End If
            End Get
        End Property

        Private Shared Function SafeRatio(numerator As Integer, denominator As Integer) As Double
            If denominator <= 0 Then
                Return 0
            Else
                Return numerator / denominator
            End If
        End Function

        ''' <summary>
        ''' 输出单行统计文本
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"class {Label}: support={Support}, predicted={Predicted}, tp={TruePositive}, fp={FalsePositive}, fn={FalseNegative}, precision={Precision.ToString("F4")}, recall={Recall.ToString("F4")}, f1={F1.ToString("F4")}"
        End Function
    End Class

    ''' <summary>
    ''' 规则集合在训练集上的精度报告。
    ''' </summary>
    Public Class RuleSetReport

        Private ReadOnly _labels As Integer()

        Friend Sub New(labels As Integer(),
                       perClass As ClassReport(),
                       confusion As Integer()(),
                       correct As Integer,
                       samples As Integer,
                       unmatched As Integer)

            _labels = labels
            Me.PerClass = perClass
            Me.ConfusionMatrix = confusion
            Me.Correct = correct
            Me.Samples = samples
            Me.Unmatched = unmatched
        End Sub

        ''' <summary>
        ''' 混淆矩阵的行列标签（升序）。
        ''' 
        ''' 行代表真实类别，列代表预测类别；未命中的样本不计入矩阵，
        ''' 而是记录在 <see cref="Unmatched"/> 中。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Labels As Integer()
            Get
                Return _labels
            End Get
        End Property

        ''' <summary>
        ''' 混淆矩阵，``ConfusionMatrix(actual)(predicted)``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ConfusionMatrix As Integer()()

        ''' <summary>
        ''' 每一个类别的精度统计（按标签升序）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PerClass As ClassReport()

        ''' <summary>
        ''' 预测正确的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Correct As Integer

        ''' <summary>
        ''' 参与评估的样本总数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Samples As Integer

        ''' <summary>
        ''' 没有任何规则命中（预测为 -1）的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Unmatched As Integer

        ''' <summary>
        ''' 整体准确率 ``correct / samples``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OverallAccuracy As Double
            Get
                If Samples <= 0 Then
                    Return 0
                Else
                    Return Correct / Samples
                End If
            End Get
        End Property

        ''' <summary>
        ''' 在训练集上构建精度报告。
        ''' </summary>
        ''' <param name="rules">待评估的规则集合</param>
        ''' <param name="patterns">训练集的位模式</param>
        ''' <param name="labels">训练集的真实类别标签</param>
        ''' <returns></returns>
        Public Shared Function Build(rules As RuleSet, patterns As BitPattern(), labels As Integer()) As RuleSetReport
            If rules Is Nothing Then
                Throw New ArgumentNullException(NameOf(rules))
            End If
            If patterns Is Nothing Then
                Throw New ArgumentNullException(NameOf(patterns))
            End If
            If labels Is Nothing Then
                Throw New ArgumentNullException(NameOf(labels))
            End If
            If patterns.Length <> labels.Length Then
                Throw New ArgumentException($"the patterns count {patterns.Length} is not equal to the labels count {labels.Length}!", NameOf(labels))
            End If

            Dim predicted As Integer() = rules.PredictAll(patterns)
            Dim labelSet As Integer() = labels _
                .Concat(predicted.Where(Function(l) l >= 0)) _
                .Distinct() _
                .OrderBy(Function(l) l) _
                .ToArray

            Dim index As New Dictionary(Of Integer, Integer)()

            For i As Integer = 0 To labelSet.Length - 1
                index(labelSet(i)) = i
            Next

            Dim confusion As Integer()() = New Integer(labelSet.Length - 1)() {}

            For i As Integer = 0 To confusion.Length - 1
                confusion(i) = New Integer(labelSet.Length - 1) {}
            Next

            Dim correct As Integer = 0
            Dim unmatched As Integer = 0

            For i As Integer = 0 To patterns.Length - 1
                Dim tag As Integer = predicted(i)

                If tag < 0 Then
                    unmatched += 1
                Else
                    confusion(index(labels(i)))(index(tag)) += 1

                    If tag = labels(i) Then
                        correct += 1
                    End If
                End If
            Next

            Dim perClass As New List(Of ClassReport)()

            For Each label As Integer In labelSet
                Dim support As Integer = 0
                Dim predictedCount As Integer = 0
                Dim truePositive As Integer = 0
                Dim covered As Integer = 0

                For i As Integer = 0 To patterns.Length - 1
                    If labels(i) = label Then
                        support += 1
                    End If
                    If predicted(i) = label Then
                        predictedCount += 1
                    End If
                    If labels(i) = label AndAlso predicted(i) = label Then
                        truePositive += 1
                    End If
                    If labels(i) = label AndAlso MatchesClass(rules, label, patterns(i)) Then
                        covered += 1
                    End If
                Next

                Call perClass.Add(New ClassReport(
                    label:=label,
                    support:=support,
                    predicted:=predictedCount,
                    truePositive:=truePositive,
                    falsePositive:=predictedCount - truePositive,
                    falseNegative:=support - truePositive,
                    covered:=covered
                ))
            Next

            Return New RuleSetReport(labelSet, perClass.ToArray, confusion, correct, patterns.Length, unmatched)
        End Function

        Private Shared Function MatchesClass(rules As RuleSet, label As Integer, pattern As BitPattern) As Boolean
            For i As Integer = 0 To rules.Classes.Length - 1
                If rules.Classes(i).Label = label Then
                    Return rules.Classes(i).Matches(pattern)
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 输出整体准确率与分类别统计的文本表格。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim lines As New List(Of String)()

            Call lines.Add($"overall accuracy = {OverallAccuracy.ToString("F4")} ({Correct}/{Samples}), unmatched = {Unmatched}")

            For i As Integer = 0 To PerClass.Length - 1
                Call lines.Add(PerClass(i).ToString)
            Next

            Return String.Join(Environment.NewLine, lines)
        End Function
    End Class
End Namespace
