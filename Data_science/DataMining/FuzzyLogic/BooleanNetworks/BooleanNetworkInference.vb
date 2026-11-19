Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports std = System.Math

Namespace BooleanNetworks

    ''' <summary>二值化表达矩阵（样本 × 基因，<c>Nothing</c> 表示缺失）。</summary>
    Public NotInheritable Class BooleanExpressionData

        Private ReadOnly _geneNames As String()
        Private ReadOnly _values As Boolean?(,)

        ''' <summary>以基因名和矩阵构造表达数据。</summary>
        Public Sub New(geneNames As String(), values As Boolean?(,))
            If geneNames Is Nothing OrElse geneNames.Length = 0 Then
                Throw New ArgumentException("gene names are required", NameOf(geneNames))
            End If
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If
            If values.GetLength(1) <> geneNames.Length Then
                Throw New ArgumentException($"matrix must have {geneNames.Length} columns", NameOf(values))
            End If

            _geneNames = DirectCast(geneNames.Clone(), String())
            _values = DirectCast(values.Clone(), Boolean?(,))
        End Sub

        ''' <summary>由无缺失的布尔矩阵构造表达数据。</summary>
        Public Sub New(geneNames As String(), values As Boolean(,))
            Call Me.New(geneNames, ToNullable(values))
        End Sub

        Private Shared Function ToNullable(values As Boolean(,)) As Boolean?(,)
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim nullable(values.GetLength(0) - 1, values.GetLength(1) - 1) As Boolean?

            For i As Integer = 0 To values.GetLength(0) - 1
                For j As Integer = 0 To values.GetLength(1) - 1
                    nullable(i, j) = values(i, j)
                Next
            Next

            Return nullable
        End Function

        ''' <summary>基因名数组。</summary>
        Public ReadOnly Property GeneNames As String()
            Get
                Return DirectCast(_geneNames.Clone(), String())
            End Get
        End Property

        ''' <summary>样本数。</summary>
        Public ReadOnly Property SampleCount As Integer
            Get
                Return _values.GetLength(0)
            End Get
        End Property

        ''' <summary>基因数。</summary>
        Public ReadOnly Property GeneCount As Integer
            Get
                Return _geneNames.Length
            End Get
        End Property

        ''' <summary>读取一个可能缺失的表达值。</summary>
        Public Function Value(sampleIndex As Integer, geneIndex As Integer) As Boolean?
            If sampleIndex < 0 OrElse sampleIndex >= SampleCount Then
                Throw New ArgumentOutOfRangeException(NameOf(sampleIndex))
            End If
            If geneIndex < 0 OrElse geneIndex >= GeneCount Then
                Throw New ArgumentOutOfRangeException(NameOf(geneIndex))
            End If

            Return _values(sampleIndex, geneIndex)
        End Function
    End Class

    ''' <summary>
    ''' 从二值化表达矩阵推断 BN 结构：逐基因枚举父节点组合，多数投票拟合真值表。
    ''' </summary>
    Public NotInheritable Class BooleanNetworkInference

        Private ReadOnly _maxParents As Integer
        Private ReadOnly _minScoreGain As Double

        ''' <summary>以最大父节点数构造推断器。</summary>
        Public Sub New(Optional maxParents As Integer = 4, Optional minScoreGain As Double = 1e-6)
            If maxParents < 0 OrElse maxParents > BooleanTruthTable.MaxMaskParents Then
                Throw New ArgumentOutOfRangeException(NameOf(maxParents), maxParents, "max parents out of range")
            End If

            _maxParents = maxParents
            _minScoreGain = minScoreGain
        End Sub

        ''' <summary>最大父节点数。</summary>
        Public ReadOnly Property MaxParents As Integer
            Get
                Return _maxParents
            End Get
        End Property

        ''' <summary>
        ''' 拟合整张网络；返回节点数组及每个基因的父集与一致性得分。
        ''' </summary>
        Public Function Fit(data As BooleanExpressionData) As InferenceResult
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If

            Dim nodes(data.GeneCount - 1) As BooleanNode
            Dim parents(data.GeneCount - 1)() As Integer
            Dim scores(data.GeneCount - 1) As Double
            Dim cods(data.GeneCount - 1) As Double
            Dim baseline(data.GeneCount - 1) As Integer

            For gene As Integer = 0 To data.GeneCount - 1
                Dim best As FittedRule = Nothing
                Dim baseCount As Integer = 0
                Dim baseErrors As Integer = 0

                Call InferenceHelper.Baseline(data, gene, baseCount, baseErrors)
                baseline(gene) = baseErrors

                Dim limit As Integer = std.Min(_maxParents, data.GeneCount - 1)

                For k As Integer = 0 To limit
                    Dim candidates As IEnumerable(Of Integer()) = InferenceHelper.Combinations(data.GeneCount, k, gene)

                    For Each candidate As Integer() In candidates
                        Dim fitted As FittedRule = InferenceHelper.FitRule(data, gene, candidate)

                        If fitted Is Nothing Then
                            Continue For
                        End If

                        If best Is Nothing OrElse fitted.Score > best.Score + _minScoreGain Then
                            best = fitted
                        End If
                    Next
                Next

                If best Is Nothing Then
                    Dim emptyParents(-1) As Integer
                    best = InferenceHelper.FitRule(data, gene, emptyParents)
                End If

                nodes(gene) = New BooleanNode(data.GeneNames(gene), best.Parents,
                                              New BooleanTruthTable(best.Parents.Length, best.TruthTable))
                parents(gene) = best.Parents
                scores(gene) = best.Score
                cods(gene) = InferenceHelper.ComputeCod(baseErrors, best.Errors, baseCount)
            Next

            Return New InferenceResult(nodes, parents, scores, cods, baseline)
        End Function
    End Class

    ''' <summary>单基因拟合结果。</summary>
    Public NotInheritable Class FittedRule
        Private ReadOnly _parents As Integer()
        Private ReadOnly _truthTable As Boolean()
        Private ReadOnly _score As Double
        Private ReadOnly _errors As Integer
        Private ReadOnly _usable As Integer

        ''' <summary>构造拟合结果。</summary>
        Public Sub New(parents As Integer(), truthTable As Boolean(), score As Double, errors As Integer, usable As Integer)
            _parents = parents
            _truthTable = truthTable
            _score = score
            _errors = errors
            _usable = usable
        End Sub

        ''' <summary>父节点索引。</summary>
        Public ReadOnly Property Parents As Integer()
            Get
                Return DirectCast(_parents.Clone(), Integer())
            End Get
        End Property

        ''' <summary>拟合得到的真值表。</summary>
        Public ReadOnly Property TruthTable As Boolean()
            Get
                Return DirectCast(_truthTable.Clone(), Boolean())
            End Get
        End Property

        ''' <summary>一致性得分（正确样本数）。</summary>
        Public ReadOnly Property Score As Double
            Get
                Return _score
            End Get
        End Property

        ''' <summary>不一致样本数。</summary>
        Public ReadOnly Property Errors As Integer
            Get
                Return _errors
            End Get
        End Property

        ''' <summary>参与拟合的有效样本数（Best-Fit Extension 跳过缺失）。</summary>
        Public ReadOnly Property UsableSamples As Integer
            Get
                Return _usable
            End Get
        End Property
    End Class

    ''' <summary>BN/PBN 推断的公共计算结果。</summary>
    Public NotInheritable Class InferenceResult
        Private ReadOnly _nodes As BooleanNode()
        Private ReadOnly _parents As Integer()()
        Private ReadOnly _scores As Double()
        Private ReadOnly _cods As Double()
        Private ReadOnly _baselineErrors As Integer()

        ''' <summary>构造推断结果。</summary>
        Public Sub New(nodes As BooleanNode(), parents As Integer()(), scores As Double(),
                       cods As Double(), baselineErrors As Integer())
            _nodes = nodes
            _parents = parents
            _scores = scores
            _cods = cods
            _baselineErrors = baselineErrors
        End Sub

        ''' <summary>拟合得到的节点数组。</summary>
        Public ReadOnly Property Nodes As BooleanNode()
            Get
                Return DirectCast(_nodes.Clone(), BooleanNode())
            End Get
        End Property

        ''' <summary>每个基因的父节点索引。</summary>
        Public ReadOnly Property Parents As Integer()()
            Get
                Return DirectCast(_parents.Clone(), Integer()())
            End Get
        End Property

        ''' <summary>每个基因最佳规则的一致性得分。</summary>
        Public ReadOnly Property Scores As Double()
            Get
                Return DirectCast(_scores.Clone(), Double())
            End Get
        End Property

        ''' <summary>每个基因最佳父集的变化系数（CoD）。</summary>
        Public ReadOnly Property Cods As Double()
            Get
                Return DirectCast(_cods.Clone(), Double())
            End Get
        End Property

        ''' <summary>每个基因在无父节点时的错误数。</summary>
        Public ReadOnly Property BaselineErrors As Integer()
            Get
                Return DirectCast(_baselineErrors.Clone(), Integer())
            End Get
        End Property
    End Class

    ''' <summary>推断算法内部辅助（父集枚举、Best-Fit Extension、多数投票与 CoD）。</summary>
    Public NotInheritable Class InferenceHelper

        Private Sub New()
        End Sub

        ''' <summary>枚举从 <paramref name="poolSize"/> 个索引中选取 <paramref name="k"/> 个、排除 target 的组合。</summary>
        Public Shared Function Combinations(poolSize As Integer, k As Integer, target As Integer) As IEnumerable(Of Integer())
            Dim pool As New List(Of Integer)()

            For i As Integer = 0 To poolSize - 1
                If i <> target Then
                    Call pool.Add(i)
                End If
            Next

            If k > pool.Count Then
                Return New Integer()() {}
            End If

            Dim result As New List(Of Integer())()
            Dim current(k - 1) As Integer

            Dim build As Action(Of Integer, Integer) = Nothing
            build = Sub(start As Integer, depth As Integer)
                        If depth = k Then
                            Call result.Add(DirectCast(current.Clone(), Integer()))
                            Return
                        End If

                        For i As Integer = start To pool.Count - (k - depth)
                            current(depth) = pool(i)
                            Call build(i + 1, depth + 1)
                        Next
                    End Sub

            Call build(0, 0)

            Return result
        End Function

        ''' <summary>Baseline：仅用多数类预测 target，返回有效样本数和错误数。</summary>
        Public Shared Sub Baseline(data As BooleanExpressionData, target As Integer,
                                   ByRef usable As Integer, ByRef errors As Integer)
            Dim trueCount As Integer = 0
            Dim falseCount As Integer = 0

            For s As Integer = 0 To data.SampleCount - 1
                Dim value As Boolean? = data.Value(s, target)

                If Not value.HasValue Then
                    Continue For
                End If

                If value.Value Then
                    trueCount += 1
                Else
                    falseCount += 1
                End If
            Next

            usable = trueCount + falseCount
            errors = std.Min(trueCount, falseCount)
        End Sub

        ''' <summary>
        ''' 用 Best-Fit Extension 拟合一条规则：缺失父值的样本跳过，缺失目标值的样本跳过。
        ''' </summary>
        Public Shared Function FitRule(data As BooleanExpressionData, target As Integer,
                                       parents As Integer()) As FittedRule
            Dim tableLength As Integer = 1 << parents.Length
            Dim votes(tableLength - 1) As Integer
            Dim counts(tableLength - 1) As Integer
            Dim usable As Integer = 0
            Dim errors As Integer = 0

            For s As Integer = 0 To data.SampleCount - 1
                Dim targetValue As Boolean? = data.Value(s, target)

                If Not targetValue.HasValue Then
                    Continue For
                End If

                Dim index As Integer = 0
                Dim complete As Boolean = True

                For j As Integer = 0 To parents.Length - 1
                    Dim parentValue As Boolean? = data.Value(s, parents(j))

                    If Not parentValue.HasValue Then
                        complete = False
                        Exit For
                    End If

                    If parentValue.Value Then
                        index = index Or (1 << j)
                    End If
                Next

                If Not complete Then
                    Continue For
                End If

                usable += 1
                counts(index) += 1

                If targetValue.Value Then
                    votes(index) += 1
                End If
            Next

            If usable = 0 Then
                Return Nothing
            End If

            Dim truth(tableLength - 1) As Boolean

            For i As Integer = 0 To tableLength - 1
                truth(i) = votes(i) * 2 > counts(i)
            Next

            For s As Integer = 0 To data.SampleCount - 1
                Dim targetValue As Boolean? = data.Value(s, target)

                If Not targetValue.HasValue Then
                    Continue For
                End If

                Dim index As Integer = 0
                Dim complete As Boolean = True

                For j As Integer = 0 To parents.Length - 1
                    Dim parentValue As Boolean? = data.Value(s, parents(j))

                    If Not parentValue.HasValue Then
                        complete = False
                        Exit For
                    End If
                    If parentValue.Value Then
                        index = index Or (1 << j)
                    End If
                Next

                If Not complete Then
                    Continue For
                End If

                If truth(index) <> targetValue.Value Then
                    errors += 1
                End If
            Next

            Return New FittedRule(parents, truth, CDbl(usable - errors), errors, usable)
        End Function

        ''' <summary>计算变化系数 CoD = (baselineErrors - errors) / baselineErrors。</summary>
        Public Shared Function ComputeCod(baselineErrors As Integer, errors As Integer,
                                          usable As Integer) As Double
            If baselineErrors <= 0 Then
                Return If(errors = 0, 1.0, 0.0)
            End If
            If usable <= 0 Then
                Return 0.0
            End If

            Dim cod As Double = (baselineErrors - errors) / CDbl(baselineErrors)

            If cod < 0.0 Then
                Return 0.0
            ElseIf cod > 1.0 Then
                Return 1.0
            Else
                Return cod
            End If
        End Function

        ''' <summary>真值表转紧凑文本。</summary>
        Public Shared Function TruthTableText(table As Boolean()) As String
            Dim sb As New StringBuilder()

            For Each value As Boolean In table
                Call sb.Append(If(value, "1"c, "0"c))
            Next

            Return sb.ToString()
        End Function
    End Class

End Namespace
