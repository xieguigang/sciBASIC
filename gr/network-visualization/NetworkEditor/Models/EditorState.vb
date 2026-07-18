#Region "Microsoft.VisualBasic::b2081823b3329cbf0a7a6fec726c88af, gr\network-visualization\NetworkEditor\Models\EditorState.vb"

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

    '   Total Lines: 101
    '    Code Lines: 78 (77.23%)
    ' Comment Lines: 7 (6.93%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (15.84%)
    '     File Size: 3.54 KB


    '     Class EditorState
    ' 
    '         Properties: EdgeColors, Graph, GroupColors, SelectedEdge, SelectedNodes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetEdgeColor, GetNodeColor, GetNodeGroup
    ' 
    '         Sub: ClearSelection, RaiseGraphChanged, RaiseSelectionChanged, SelectEdge, SelectNode
    '              SetNodeGroup, ToggleNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports NetworkEditor.Models

Namespace NetworkEditor.Models

    ''' <summary>
    ''' 编辑器全局状态：网络图模型、选中集合、分组颜色、视口无关的编辑器侧数据
    ''' </summary>
    Public Class EditorState

        Public Property Graph As NetworkGraph
        Public ReadOnly Property SelectedNodes As New List(Of Node)
        Public Property SelectedEdge As Edge
        Public ReadOnly Property GroupColors As New GroupColorMap
        ''' <summary>编辑器侧边颜色覆盖（按边 ID）</summary>
        Public ReadOnly Property EdgeColors As New Dictionary(Of String, Color)

        Public Event SelectionChanged As EventHandler
        Public Event GraphChanged As EventHandler

        Public Sub New()
            Graph = New NetworkGraph()
        End Sub

        Public Sub ClearSelection()
            SelectedNodes.Clear()
            SelectedEdge = Nothing
            RaiseEvent SelectionChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' 仅触发选中变更事件（不修改选中集合），用于框选后通知 UI 刷新
        ''' </summary>
        Public Sub RaiseSelectionChanged()
            RaiseEvent SelectionChanged(Me, EventArgs.Empty)
        End Sub

        Public Sub SelectNode(n As Node, Optional additive As Boolean = False)
            If Not additive Then
                SelectedNodes.Clear()
            End If
            If Not SelectedNodes.Contains(n) Then
                SelectedNodes.Add(n)
            End If
            SelectedEdge = Nothing
            RaiseEvent SelectionChanged(Me, EventArgs.Empty)
        End Sub

        Public Sub ToggleNode(n As Node)
            If SelectedNodes.Contains(n) Then
                SelectedNodes.Remove(n)
            Else
                SelectedNodes.Add(n)
            End If
            SelectedEdge = Nothing
            RaiseEvent SelectionChanged(Me, EventArgs.Empty)
        End Sub

        Public Sub SelectEdge(e As Edge)
            SelectedNodes.Clear()
            SelectedEdge = e
            RaiseEvent SelectionChanged(Me, EventArgs.Empty)
        End Sub

        Public Sub SetNodeGroup(n As Node, group As String)
            If n.data.Properties Is Nothing Then
                n.data.Properties = New Dictionary(Of String, String)
            End If
            n.data.Properties("group") = group
        End Sub

        Public Function GetNodeGroup(n As Node) As String
            If n.data.Properties IsNot Nothing AndAlso n.data.Properties.ContainsKey("group") Then
                Return n.data.Properties("group")
            End If
            Return ""
        End Function

        Public Function GetNodeColor(n As Node) As Color
            Dim g = GetNodeGroup(n)
            If g <> "" AndAlso GroupColors.Contains(g) Then
                Return GroupColors(g)
            End If
            Return Color.FromArgb(&HFF9AA4B2)
        End Function

        Public Function GetEdgeColor(e As Edge) As Color
            If EdgeColors.ContainsKey(e.ID) Then
                Return EdgeColors(e.ID)
            End If
            Return Color.FromArgb(&HFF4A90D9)
        End Function

        Public Sub RaiseGraphChanged()
            RaiseEvent GraphChanged(Me, EventArgs.Empty)
        End Sub
    End Class

End Namespace

