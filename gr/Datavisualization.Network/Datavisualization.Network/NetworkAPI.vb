#Region "Microsoft.VisualBasic::6a4231c45d727e4ca045d3dc552c82ce, ..\visualbasic_App\gr\Datavisualization.Network\Datavisualization.Network\NetworkAPI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports ______NETWORK__ =
    Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network(Of
    Microsoft.VisualBasic.Data.visualize.Network.FileStream.Node,
    Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkEdge)

<[PackageNamespace]("DataVisualization.Network", Publisher:="xie.guigang@gmail.com")>
Public Module NetworkAPI

    <ExportAPI("Read.Network")>
    Public Function ReadnetWork(file As String) As FileStream.NetworkEdge()
        Return file.LoadCsv(Of FileStream.NetworkEdge)(False).ToArray
    End Function

    <ExportAPI("Find.NewSession")>
    Public Function CreatePathwayFinder(Network As IEnumerable(Of FileStream.NetworkEdge)) As PathFinder(Of FileStream.NetworkEdge)
        Return New PathFinder(Of FileStream.NetworkEdge)(Network.ToArray)
    End Function

    <ExportAPI("Find.Path.Shortest")>
    Public Function FindShortestPath(finder As PathFinder(Of FileStream.NetworkEdge), start As String, ends As String) As FileStream.NetworkEdge()
        Dim result = finder.FindShortestPath(start, ends)
        Dim List As List(Of FileStream.NetworkEdge) = New List(Of FileStream.NetworkEdge)
        For Each Line In result
            Call List.AddRange(Line.Value)
        Next
        Return List.ToArray
    End Function

    <ExportAPI("Find.Path.Shortest")>
    <Extension> Public Function FindShortestPath(net As IEnumerable(Of FileStream.NetworkEdge), start As String, ends As String) As FileStream.NetworkEdge()
        Dim finder As New PathFinder(Of FileStream.NetworkEdge)(net.ToArray)
        Return FindShortestPath(finder, start, ends)
    End Function

    <ExportAPI("Get.NetworkEdges")>
    Public Function GetNHetworkEdges(Network As ______NETWORK__) As FileStream.NetworkEdge()
        Return Network.Edges
    End Function

    <ExportAPI("Get.NetworkNodes")>
    Public Function GetNetworkNodes(Network As ______NETWORK__) As FileStream.Node()
        Return Network.Nodes
    End Function

    <ExportAPI("Save")>
    Public Function SaveNetwork(network As ______NETWORK__, <Parameter("DIR.Export")> Export As String) As Boolean
        Return network.Save(Export, Encodings.UTF8)
    End Function

    <ExportAPI("Write.Network")>
    Public Function WriteNetwork(Network As FileStream.NetworkEdge(), <Parameter("Path.Save")> SaveTo As String) As Boolean
        Return Network.SaveTo(SaveTo, False)
    End Function

    ''' <summary>
    ''' 这个查找函数是忽略掉了方向了的
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="node"></param>
    ''' <returns></returns>
    <Extension, ExportAPI("GetConnections")>
    Public Function GetConnections(source As IEnumerable(Of FileStream.NetworkEdge), node As String) As FileStream.NetworkEdge()
        Dim LQuery = (From x As FileStream.NetworkEdge In source.AsParallel
                      Where Not String.IsNullOrEmpty(x.GetConnectedNode(node))
                      Select x).ToArray
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
        Dim LQuery = (From x As FileStream.NetworkEdge In source.AsParallel
                      Where String.Equals(from, x.FromNode, StringComparison.OrdinalIgnoreCase)
                      Select x).ToArray
        Return LQuery
    End Function
End Module
