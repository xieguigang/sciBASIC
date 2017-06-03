#Region "Microsoft.VisualBasic::9795f63625436c37378200a8c9f21c0c, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\IO\FileStream\Network.vb"

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

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace FileStream

    ''' <summary>
    ''' The csv table file format of the network graph object model: <see cref="NetworkGraph"/> 
    ''' </summary>
    Public Class Network : Inherits Network(Of Node, NetworkEdge)

        Sub New()
        End Sub

        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of NetworkEdge))
            Call MyBase.New()

            Me.Nodes = nodes.ToArray
            Me.Edges = edges.ToArray
        End Sub

        Sub New(edges As IEnumerable(Of NetworkEdge), nodes As IEnumerable(Of Node))
            Call MyBase.New()

            Me.Nodes = nodes.ToArray
            Me.Edges = edges.ToArray
        End Sub

        ''' <summary>
        ''' 获取指定节点的连接数量
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Function Links(node As String) As Integer
            Dim count% = Edges _
                .Where(Function(x) x.Contains(node)) _
                .Count
            Return count
        End Function

        ''' <summary>
        ''' Load network graph data from a saved network data direcotry.
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <returns></returns>
        Public Overloads Shared Function Load(DIR As String) As Network
            Dim list As New List(Of String)(ls - l - r - "*.csv" <= DIR)
            Dim edgeFile$ = $"{DIR}/network-edges.csv"
            Dim nodeFile$ = $"{DIR}/nodes.csv"

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
                    Throw New MemberAccessException("Edge file not found in the directory: " & DIR)
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
                    Throw New MemberAccessException("Node file not found in the directory: " & DIR)
                End If
            End If

            Return New Network With {
                .Edges = edgeFile.LoadCsv(Of NetworkEdge),
                .Nodes = nodeFile.LoadCsv(Of Node)
            }
        End Function
    End Class
End Namespace
