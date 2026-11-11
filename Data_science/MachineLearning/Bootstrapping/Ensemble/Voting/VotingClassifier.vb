Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace Ensemble

    ''' <summary>
    ''' Voting 分类器：把多个异质分类器的输出聚合成一个概率向量。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <c>Soft</c> 模式对概率做加权平均；<c>Hard</c> 模式先按 <c>argmax</c> 取标签，
    ''' 再按加权票数聚合成概率向量（票数占比），因此两种模式都满足
    ''' <c>IClassifier</c> 的概率契约。
    ''' </para>
    ''' <para>
    ''' 每个基模型的概率向量都会先通过 <c>ProbabilityMath.AlignTo</c> 对齐到
    ''' 全局 <c>Classes</c>，因此基模型的类别顺序不同也不会互相污染。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class VotingClassifier : Implements IClassifier

        Private ReadOnly learners As IClassifierLearner()
        Private ReadOnly modelWeights As Double()
        Private ReadOnly votingMode As VotingMode
        Private ReadOnly trained As IClassifier()
        Private classesArray As Double()

        ''' <param name="learners">基学习器，至少两个才有多样性</param>
        ''' <param name="weights">非负权重，为空时等权；内部会归一化</param>
        ''' <param name="mode">硬投票或软投票</param>
        Public Sub New(learners As IEnumerable(Of IClassifierLearner),
                       Optional weights As Double() = Nothing,
                       Optional mode As VotingMode = VotingMode.Soft)
            If learners Is Nothing Then
                Throw New ArgumentNullException(NameOf(learners))
            End If

            Dim items As IClassifierLearner() = learners.ToArray()

            If items.Length = 0 Then
                Throw New ArgumentException("the voting classifier requires at least one base learner!", NameOf(learners))
            End If
            If items.Any(Function(m) m Is Nothing) Then
                Throw New ArgumentException("the base learner list contains a NULL entry!", NameOf(learners))
            End If

            Me.learners = items
            Me.modelWeights = ProbabilityMath.NormalizeWeights(weights, items.Length)
            Me.votingMode = mode
            Me.trained = New IClassifier(items.Length - 1) {}
        End Sub

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
            End Get
        End Property

        ''' <summary>基模型数量</summary>
        Public ReadOnly Property BaseModelCount As Integer
            Get
                Return trained.Length
            End Get
        End Property

        ''' <summary>训练后的基模型</summary>
        Public ReadOnly Property BaseModels As IClassifier()
            Get
                Return trained
            End Get
        End Property

        ''' <summary>归一化后的聚合权重</summary>
        Public ReadOnly Property Weights As Double()
            Get
                Return modelWeights
            End Get
        End Property

        ''' <summary>当前投票模式</summary>
        Public ReadOnly Property Voting As VotingMode
            Get
                Return votingMode
            End Get
        End Property

        ''' <summary>
        ''' 训练全部基模型；标签编码只在这里做一次。
        ''' </summary>
        Public Function Train(x As Double()(), y As Double()) As VotingClassifier
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix can not be empty!", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException(
                    $"the feature rows ({x.Length}) and labels ({If(y Is Nothing, 0, y.Length)}) do not agree!",
                    NameOf(y))
            End If

            Dim encoder As LabelEncoder = LabelEncoder.Fit(y)
            Dim yIndex As Integer() = encoder.Encode(y)

            For i As Integer = 0 To learners.Length - 1
                trained(i) = learners(i).Fit(x, yIndex, encoder.Classes)
            Next

            classesArray = encoder.Classes

            Return Me
        End Function

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            RequireTrained()

            Dim size As Integer = classesArray.Length
            Dim aggregate As Double() = New Double(size - 1) {}

            If votingMode = VotingMode.Hard Then
                Dim votes As Double() = New Double(size - 1) {}

                For i As Integer = 0 To trained.Length - 1
                    Dim aligned As Double() = ProbabilityMath.AlignTo(
                        trained(i).PredictProbability(x), trained(i).Classes, classesArray)
                    Dim best As Integer = ProbabilityMath.ArgMax(aligned)

                    votes(best) += modelWeights(i)
                Next

                Return ProbabilityMath.Normalize(votes)
            End If

            For i As Integer = 0 To trained.Length - 1
                Dim aligned As Double() = ProbabilityMath.AlignTo(
                    trained(i).PredictProbability(x), trained(i).Classes, classesArray)

                For k As Integer = 0 To size - 1
                    aggregate(k) += modelWeights(i) * aligned(k)
                Next
            Next

            Return ProbabilityMath.Normalize(aggregate)
        End Function

        ''' <summary>批量预测类别标签</summary>
        Public Function PredictBatch(x As Double()()) As Double()
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If

            Dim result As Double() = New Double(x.Length - 1) {}

            For i As Integer = 0 To x.Length - 1
                result(i) = ProbabilityMath.PredictClass(Me, x(i))
            Next

            Return result
        End Function

        ''' <summary>单个样本的类别标签</summary>
        Public Function PredictLabel(x As Double()) As Double
            Return ProbabilityMath.PredictClass(Me, x)
        End Function

        Private Sub RequireTrained()
            If classesArray Is Nothing Then
                Throw New InvalidOperationException(
                    "the voting classifier has not been trained yet; call Train() before predicting!")
            End If
        End Sub

    End Class

End Namespace
