#Region "Microsoft.VisualBasic::decd68a23e8432690347a647562b1801, gr\network-visualization\Network.IO.Extensions\Extensions.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 131
'    Code Lines: 90 (68.70%)
' Comment Lines: 29 (22.14%)
'    - Xml Docs: 96.55%
' 
'   Blank Lines: 12 (9.16%)
'     File Size: 4.80 KB


' Module Extensions
' 
'     Function: GetConnections, GetNextConnects, RemoveDuplicated, RemoveSelfLoop, SearchIndex
'               Trim
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Language

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
    Public Function RemoveDuplicated(Of T As NetworkEdge)(edges As IEnumerable(Of T), Optional directed As Boolean = True, Optional ignoreTypes As Boolean = False) As T()
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
