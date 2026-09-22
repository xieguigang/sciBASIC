#Region "Microsoft.VisualBasic::TreeShapExplainer, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\Core\TreeShapExplainer.vb"

Imports System.Linq
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue.TreeShap

Namespace ShapleyValue

    ''' <summary>
    ''' 基于 TreeSHAP 算法（论文 arXiv:1802.03888）的树集成 SHAP 解释器。
    ''' 
    ''' 该解释器把多棵 <see cref="PkTree"/> 的逐特征贡献相加，
    ''' 并且把所有树的期望值（cover 加权叶值均值）与一个常数偏置项
    ''' （<see cref="Intercept"/>，例如梯度提升模型的 base score）相加作为最终的基线值。
    ''' 
    ''' 因此对于任意样本 x 都有：``sum(Explain(x)) + Baseline = 集成的加性输出``。
    ''' </summary>
    Public Class TreeShapExplainer : Implements IShapExplainer

        ''' <summary>
        ''' 组成该集成的所有二叉树。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Trees As PkTree()

        ''' <summary>
        ''' 每一棵树的期望输出 E[f_t]。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TreeBaselines As Double()

        ''' <summary>
        ''' 叠加在树集成输出之上的常数偏置项（例如 GBM 的 first_round_pred）。
        ''' </summary>
        ''' <returns></returns>
        Public Property Intercept As Double

        Public Property FeatureNames As String() Implements IShapExplainer.FeatureNames

        Private ReadOnly _baseline As Double

        ''' <param name="trees">组成集成的所有二叉树</param>
        ''' <param name="treeBaselines">每一棵树的期望输出，缺省时会根据树的 cover 自动计算</param>
        ''' <param name="featureNames">特征名字</param>
        ''' <param name="intercept">叠加的常数偏置项</param>
        Public Sub New(trees As IEnumerable(Of PkTree),
                       featureNames As String(),
                       Optional treeBaselines As IEnumerable(Of Double) = Nothing,
                       Optional intercept As Double = 0)

            Me.Trees = If(trees Is Nothing, New PkTree() {}, trees.ToArray)
            Me.FeatureNames = featureNames
            Me.Intercept = intercept

            If treeBaselines Is Nothing Then
                Me.TreeBaselines = Me.Trees.Select(AddressOf ExpectedValue).ToArray
            Else
                Me.TreeBaselines = treeBaselines.ToArray
            End If

            Dim total As Double = intercept

            For Each b As Double In Me.TreeBaselines
                total += b
            Next

            _baseline = total
        End Sub

        Public ReadOnly Property Baseline As Double Implements IShapExplainer.Baseline
            Get
                Return _baseline
            End Get
        End Property

        Public ReadOnly Property Size As Integer Implements IShapExplainer.Size
            Get
                Return If(FeatureNames Is Nothing, 0, FeatureNames.Length)
            End Get
        End Property

        ''' <summary>
        ''' 计算样本的逐特征 TreeSHAP 贡献值（不包含基线值）。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Explain(x As Double()) As Double() Implements IShapExplainer.Explain
            Dim n As Integer = x.Length
            Dim phi As Double() = New Double(n - 1) {}

            For Each tree As PkTree In Trees
                If tree Is Nothing OrElse tree.root Is Nothing Then
                    Continue For
                End If

                ' 传入 0 作为 expected value，使得返回数组的尾部槽位保持为 0，
                ' 从而只取前面的逐特征贡献值
                Dim calculator As New ShapOptimized(x, tree, 0)
                Dim contributions As Double() = calculator.calculateContributions()

                For i As Integer = 0 To n - 1
                    phi(i) += contributions(i)
                Next
            Next

            Return phi
        End Function

        ''' <summary>
        ''' 计算单棵树的期望输出，即按照节点 cover 进行加权的叶值均值。
        ''' </summary>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        Public Shared Function ExpectedValue(tree As PkTree) As Double
            If tree Is Nothing OrElse tree.root Is Nothing Then
                Return 0
            End If

            Return expectedValue(tree.root)
        End Function

        Private Shared Function expectedValue(node As PkNode) As Double
            If node.LeafProp Then
                Return node.leafValue
            End If

            Dim total As Double = node.dataCount
            Dim yesValue As Double = expectedValue(node.yes)
            Dim noValue As Double = expectedValue(node.no)

            If total <= 0 Then
                Return (yesValue + noValue) / 2
            Else
                Return (yesValue * node.yes.dataCount + noValue * node.no.dataCount) / total
            End If
        End Function

    End Class

End Namespace

#End Region
