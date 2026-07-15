Imports System.ComponentModel

Namespace Microsoft.VisualBasic.Data.visualize.Network.Layouts.ForceDirected

    ''' <summary>
    ''' Force-Directed 力导向布局参数，可在 PropertyGrid 中编辑。
    ''' 与 <see cref="Planner"/> 构造函数参数一一对应。
    ''' </summary>
    Public Class ForceDirectedParameters

        <Category("受力"), DisplayName("排斥因子"), Description("节点间排斥力强度，值越大节点越分散")>
        Public Property EjectFactor As Integer = 6

        <Category("受力"), DisplayName("吸引因子"), Description("边两端节点的吸引力强度")>
        Public Property CondenseFactor As Integer = 3

        <Category("位移"), DisplayName("最大位移X"), Description("单步迭代中节点的水平位移上限")>
        Public Property MaxTx As Integer = 4

        <Category("位移"), DisplayName("最大位移Y"), Description("单步迭代中节点的垂直位移上限")>
        Public Property MaxTy As Integer = 3

        <Category("距离"), DisplayName("距离阈值"), Description("排斥力生效的距离区间，格式如 30,250")>
        Public Property DistThreshold As String = "30,250"

        <Category("画布"), DisplayName("画布宽度"), Description("布局画布尺寸（像素），影响 ideal 距离 k")>
        Public Property CanvasWidth As Integer = 1000

        <Category("画布"), DisplayName("画布高度"), Description("布局画布尺寸（像素），影响 ideal 距离 k")>
        Public Property CanvasHeight As Integer = 1000

        <Category("迭代"), DisplayName("迭代次数"), Description("调用 Collide 的步数")>
        Public Property Iterations As Integer = 300

        Public Overrides Function ToString() As String
            Return $"Force-Directed (iter={Iterations})"
        End Function
    End Class

End Namespace
