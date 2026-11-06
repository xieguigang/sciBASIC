Imports Microsoft.VisualBasic.DeepLearning.GNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 一个用于半监督节点分类的图样本：图结构 + 节点特征 + 标签 + 三段掩码。
''' </summary>
''' <remarks>
''' 掩码是半监督 GCN 的核心输入：只有 train 段参与损失，但三段（含 test）的节点
''' 都会参与消息传递 —— 这正是 GCN 的归纳偏置所在。
''' </remarks>
Public Class GraphSample

    ''' <summary>样本名称（用于报告）</summary>
    Public Property Name As String

    ''' <summary>图结构（边表；节点特征单独存放，避免两处不一致）</summary>
    Public Property Graph As Graph

    ''' <summary>节点特征 [nodes, features]</summary>
    Public Property Features As Tensor

    ''' <summary>节点标签；无标签节点填 -1（会被训练与评估自动跳过）</summary>
    Public Property Labels As Integer()

    ''' <summary>类别数</summary>
    Public Property NumClasses As Integer

    ''' <summary>训练节点掩码</summary>
    Public Property TrainMask As Boolean()

    ''' <summary>验证节点掩码（早停监控用）</summary>
    Public Property ValidationMask As Boolean()

    ''' <summary>测试节点掩码（只用于最终报告）</summary>
    Public Property TestMask As Boolean()

    Public ReadOnly Property NumNodes As Integer
        Get
            Return Graph.NumNodes
        End Get
    End Property

    Public ReadOnly Property NumFeatures As Integer
        Get
            Return Features.Shape(1)
        End Get
    End Property

    ''' <summary>把节点特征回填到 <see cref="Graph"/>（供需要 graph.NodeFeatures 的旧接口使用）</summary>
    Public Sub SyncNodeFeatures()
        Me.Graph.NodeFeatures = Features
    End Sub

    Public Overrides Function ToString() As String
        Return $"{Name}(n={NumNodes}, m={Graph.NumEdges}, d={NumFeatures}, classes={NumClasses}, " &
               $"train={CountMask(TrainMask)}, val={CountMask(ValidationMask)}, test={CountMask(TestMask)})"
    End Function

    Friend Shared Function CountMask(mask As Boolean()) As Integer
        If mask Is Nothing Then Return 0

        Dim count = 0

        For Each flag In mask
            If flag Then count += 1
        Next

        Return count
    End Function

End Class
