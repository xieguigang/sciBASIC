#Region "Microsoft.VisualBasic::d24e686cc9417fbab598b831708482a6, gr\network-visualization\Network.IO.Extensions\IO\FileStream\csv\Network.vb"

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

    '     Class NetworkTables
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Links, SearchNetworkTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace FileStream

    ''' <summary>
    ''' The csv table file format of the network graph object model: <see cref="NetworkGraph"/>.
    ''' (在这个对象之中包括了一个<see cref="Node"/>和<see cref="edges"/>节点和边列表，可以直接用于CSV文件的读写操作)
    ''' </summary>
    Public Class NetworkTables : Inherits Network(Of Node, NetworkEdge)

        Sub New()
        End Sub

        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of NetworkEdge))
            Call MyBase.New()

            Me.nodes = nodes.ToArray
            Me.edges = edges.ToArray
        End Sub

        Sub New(edges As IEnumerable(Of NetworkEdge), nodes As IEnumerable(Of Node))
            Call MyBase.New()

            Me.nodes = nodes.ToArray
            Me.edges = edges.ToArray
        End Sub

        ''' <summary>
        ''' 获取指定节点的连接数量
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Function Links(node As String) As Integer
            Dim count% = edges _
                .Where(Function(x) x.Contains(node)) _
                .Count
            Return count
        End Function

        Public Shared Function SearchNetworkTable(directory As String) As (edges$, nodes$)
            Dim list As New List(Of String)(ls - l - r - "*.csv" <= directory)
            Dim edgeFile$ = $"{directory}/network-edges.csv"
            Dim nodeFile$ = $"{directory}/nodes.csv"

            If Not edgeFile.FileExists Then
                For Each path$ In list
                    If InStr(path.BaseName, "edge", CompareMethod.Text) > 0 Then
                        edgeFile = path
                        Exit For
                    End If
                Next

                If Not String.IsNullOrEmpty(edgeFile) Then
                    Call list.Remove(edgeFile)
                Else
                    Throw New MemberAccessException("Edge file not found in the directory: " & directory)
                End If
            End If

            If Not nodeFile.FileExists Then
                For Each path$ In list
                    If InStr(path.BaseName, "node", CompareMethod.Text) > 0 Then
                        nodeFile = path
                        Exit For
                    End If
                Next

                If String.IsNullOrEmpty(nodeFile) Then
                    Throw New MemberAccessException("Node file not found in the directory: " & directory)
                End If
            End If

            Return (edgeFile, nodeFile)
        End Function
    End Class
End Namespace
