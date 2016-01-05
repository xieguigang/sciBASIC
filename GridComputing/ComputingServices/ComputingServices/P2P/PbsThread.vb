Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Net.Protocol

''' <summary>
''' 每一个进程实例都是一条计算线程
''' </summary>
''' <remarks></remarks>
Public Class PbsThread

    ''' <summary>
    ''' 进程实例在服务器之上的唯一标识符
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlAttribute> Public Property Guid As String
    ''' <summary>
    ''' 运行于进程实例之中的细胞模型在数据库之中的模型编号
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TaskGuid As String
    ''' <summary>
    ''' 该进程实例的远程节点的主机描述信息
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property RemoteHostEp As IPEndPoint
    ''' <summary>
    ''' 当前节点的进程实例所处的节点主机的当前系统负荷
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlAttribute> Public Property ServerLoad As String

    Public Function GetUpdatedServerLoad() As Double

    End Function

    Public Property NodeType As ThreadTypes

    Public Enum ThreadTypes
        ''' <summary>
        ''' <see cref="ComputingServices.ServicesComponents.NodeServices"/>
        ''' </summary>
        NodeServices
        ''' <summary>
        ''' <see cref="ComputingServices.ServicesComponents.Node"/>
        ''' </summary>
        GridNode
    End Enum

    Public Function Copy() As PbsThread
        Return New PbsThread With {
            .Guid = Guid,
            .TaskGuid = TaskGuid,
            .RemoteHostEp = RemoteHostEp,
            .ServerLoad = WindowsServices.CPU_Usages.NextValue
        }
    End Function

    Public Function SendMessage(request As RequestStream) As RequestStream
        Return New Net.AsynInvoke(RemoteHostEp.GetIPEndPoint).SendMessage(request, OperationTimeOut:=5 * 1000)
    End Function

    ''' <summary>
    ''' 在刚开始初始化的时候，是还没有<see cref="TaskGuid"></see>的，因为这个属性是来自于服务器之上的自动分配
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function InitNodeID(ep As IPEndPoint, NodeType As ThreadTypes) As PbsThread
        Return New PbsThread With {
            .Guid = $"{System.Guid.NewGuid.ToString}-{AsynInvoke.LocalIPAddress}:Grid-{Process.GetCurrentProcess.Id}",
            .ServerLoad = WindowsServices.CPU_Usages.NextValue,
            .RemoteHostEp = ep,
            .NodeType = NodeType
        }
    End Function

End Class
