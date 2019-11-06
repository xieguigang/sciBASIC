Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.ComponentModel.Collection

<HideModuleName>
Public Module Extensions

    <Extension>
    Public Function SearchIndex(net As NetworkTables, from As Boolean) As Dictionary(Of String, Index(Of String))
        Dim getKey = Function(e As NetworkEdge)
                         If from Then
                             Return e.fromNode
                         Else
                             Return e.toNode
                         End If
                     End Function
        Dim getValue = Function(e As NetworkEdge)
                           If from Then
                               Return e.toNode
                           Else
                               Return e.fromNode
                           End If
                       End Function
        Dim index = net.edges _
                .Select(Function(edge)
                            Return (key:=getKey(edge), Value:=getValue(edge))
                        End Function) _
                .GroupBy(Function(t) t.key) _
                .ToDictionary(Function(k) k.Key,
                              Function(g)
                                  Return New Index(Of String)(g.Select(Function(o) o.Value).Distinct)
                              End Function)
        Return index
    End Function

    ''' <summary>
    ''' 移除的重复的边
    ''' </summary>
    ''' <remarks></remarks>
    ''' <param name="directed">是否忽略方向？</param>
    ''' <param name="ignoreTypes">是否忽略边的类型？</param>
    <Extension>
    Public Function RemoveDuplicated(Of T As NetworkEdge)(
                                                    edges As IEnumerable(Of T),
                                                    Optional directed As Boolean = True,
                                                    Optional ignoreTypes As Boolean = False) As T()
        Dim uid = Function(edge As T) As String
                      If directed Then
                          Return edge.GetDirectedGuid(ignoreTypes)
                      Else
                          Return edge.GetNullDirectedGuid(ignoreTypes)
                      End If
                  End Function
        Dim LQuery = edges _
                .GroupBy(uid) _
                .Select(Function(g) g.First) _
                .ToArray

        Return LQuery
    End Function

    ''' <summary>
    ''' 移除自身与自身的边
    ''' </summary>
    ''' <remarks></remarks>
    <Extension>
    Public Function RemoveSelfLoop(Of T As NetworkEdge)(edges As IEnumerable(Of T)) As T()
        Dim LQuery = LinqAPI.Exec(Of T) <=
 _
                From x As T
                In edges
                Where Not x.selfLoop
                Select x

        Return LQuery
    End Function

    ''' <summary>
    ''' 这个查找函数是忽略掉了方向了的
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="node"></param>
    ''' <returns></returns>
    <Extension, ExportAPI("GetConnections")>
    Public Function GetConnections(source As IEnumerable(Of FileStream.NetworkEdge), node As String) As FileStream.NetworkEdge()
        Dim LQuery = LinqAPI.Exec(Of FileStream.NetworkEdge) <=
 _
            From x As FileStream.NetworkEdge
            In source.AsParallel
            Where Not String.IsNullOrEmpty(x.GetConnectedNode(node))
            Select x

        Return LQuery
    End Function

    ''' <summary>
    ''' 查找To关系的节点边
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="from"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("Get.Connects.Next")>
    <Extension>
    Public Function GetNextConnects(source As IEnumerable(Of FileStream.NetworkEdge), from As String) As FileStream.NetworkEdge()
        Dim LQuery = LinqAPI.Exec(Of FileStream.NetworkEdge) <=
 _
            From x As FileStream.NetworkEdge
            In source.AsParallel
            Where from.TextEquals(x.fromNode)
            Select x

        Return LQuery
    End Function

    ''' <summary>
    ''' Removes all of the selfloop and duplicated edges
    ''' </summary>
    ''' <param name="network"></param>
    ''' <param name="doNothing"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Trim(network As FileStream.NetworkTables, Optional doNothing As Boolean = False) As FileStream.NetworkTables
        If Not doNothing Then
            Call network.RemoveSelfLoop()
            Call network.RemoveDuplicated()
        End If

        Return network
    End Function
End Module
