#Region "Microsoft.VisualBasic::ClusteringResult, Data_science\DataMining\DataMining\Evaluation\Results\ClusteringResult.vb"

Imports System.Linq

Namespace Evaluation

    ''' <summary>
    ''' 聚类结果的统一数据模型：``(特征矩阵, 簇标签)``，可选附带真值标签。
    ''' 
    ''' 该类型刻意使用**纯数值**的 ``Double()()`` 与 ``Integer()`` 表示，
    ''' 以避免依赖已经标记为 ``Obsolete`` 的 <c>ClusterEntity</c> / <c>KMeans.Bisecting.Cluster</c>。
    ''' </summary>
    Public Class ClusteringResult : Implements IEvaluationResult

        Public Property Name As String Implements IEvaluationResult.Name

        ''' <summary>
        ''' 行主序的特征矩阵 ``features(sample)(feature)``。
        ''' </summary>
        ''' <returns></returns>
        Public Property Features As Double()()

        ''' <summary>
        ''' 每一个样本所属的簇编号。
        ''' </summary>
        ''' <returns></returns>
        Public Property ClusterLabels As Integer()

        ''' <summary>
        ''' 可选的真实类别标签。提供之后才能计算 Purity/ARI/NMI 等外部指标。
        ''' </summary>
        ''' <returns></returns>
        Public Property GroundTruth As Integer()

        ''' <summary>
        ''' Silhouette 等 O(n^2) 指标的最大参与样本数量（``0`` 表示不限制）。
        ''' 当样本数量超过该值时，会等间距抽样计算并给出近似值。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxPoints As Integer = 0

        Public ReadOnly Property Kind As ResultKinds Implements IEvaluationResult.Kind
            Get
                Return ResultKinds.clustering
            End Get
        End Property

        Public ReadOnly Property Size As Integer
            Get
                Return If(Features Is Nothing, 0, Features.Length)
            End Get
        End Property

        ''' <summary>
        ''' 是否提供了真值标签（用于外部指标）。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasGroundTruth As Boolean
            Get
                Return GroundTruth IsNot Nothing AndAlso GroundTruth.Length = Size AndAlso Size > 0
            End Get
        End Property

        ''' <summary>
        ''' 簇的数量。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ClusterCount As Integer
            Get
                If ClusterLabels Is Nothing Then
                    Return 0
                End If

                Return ClusterLabels.Distinct.Count
            End Get
        End Property

        Public Shared Function Create(features As Double()(),
                                      clusterLabels As Integer(),
                                      Optional name As String = Nothing,
                                      Optional groundTruth As Integer() = Nothing,
                                      Optional maxPoints As Integer = 0) As ClusteringResult

            If features Is Nothing OrElse clusterLabels Is Nothing Then
                Throw New ArgumentNullException("the features and cluster labels can not be nothing!")
            End If

            If features.Length <> clusterLabels.Length Then
                Throw New DataMisalignedException($"the features({features.Length}) and cluster labels({clusterLabels.Length}) must have the same length!")
            End If

            If groundTruth IsNot Nothing AndAlso groundTruth.Length <> features.Length Then
                Throw New DataMisalignedException($"the ground truth({groundTruth.Length}) must have the same length as the features({features.Length})!")
            End If

            Return New ClusteringResult With {
                .Name = If(String.IsNullOrEmpty(name), "clustering", name),
                .Features = features,
                .ClusterLabels = clusterLabels,
                .GroundTruth = groundTruth,
                .MaxPoints = maxPoints
            }
        End Function

    End Class

End Namespace

#End Region
