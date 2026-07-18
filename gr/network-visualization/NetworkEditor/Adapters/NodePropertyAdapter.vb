#Region "Microsoft.VisualBasic::733d95af3d32b1aaeb9b2ef4c8026cbf, gr\network-visualization\NetworkEditor\Adapters\NodePropertyAdapter.vb"

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

    '   Total Lines: 148
    '    Code Lines: 131 (88.51%)
    ' Comment Lines: 4 (2.70%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (8.78%)
    '     File Size: 4.88 KB


    '     Class NodePropertyAdapter
    ' 
    '         Properties: Group, GroupColor, Label, Mass, Radius
    '                     X, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: EnsurePos
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports NetworkEditor.Models

Namespace NetworkEditor.Adapters

    ''' <summary>
    ''' 将选中的一个或多个节点的属性包装为 PropertyGrid 可编辑对象。
    ''' 屏蔽 NodeData.color(Brush) / initialPostion(AbstractVector) 不可直接编辑的问题。
    ''' </summary>
    Public Class NodePropertyAdapter

        Private ReadOnly state As EditorState
        Private ReadOnly nodes As Node()

        Public Sub New(state As EditorState, nodes As IEnumerable(Of Node))
            Me.state = state
            Me.nodes = nodes.ToArray()
        End Sub

        <Category("标识"), DisplayName("标签"), [ReadOnly](True)>
        Public ReadOnly Property Label As String
            Get
                If nodes.Length = 0 Then
                    Return ""
                End If
                Return nodes(0).data.label
            End Get
        End Property

        <Category("分组"), DisplayName("分组")>
        Public Property Group As String
            Get
                If nodes.Length = 0 Then
                    Return ""
                End If
                Return state.GetNodeGroup(nodes(0))
            End Get
            Set(value As String)
                For Each n In nodes
                    state.SetNodeGroup(n, value)
                Next
            End Set
        End Property

        <Category("位置"), DisplayName("X")>
        Public Property X As Double
            Get
                EnsurePos()
                Return nodes(0).data.initialPostion.x
            End Get
            Set(value As Double)
                For Each n In nodes
                    EnsurePos(n)
                    n.data.initialPostion.x = value
                Next
            End Set
        End Property

        <Category("位置"), DisplayName("Y")>
        Public Property Y As Double
            Get
                EnsurePos()
                Return nodes(0).data.initialPostion.y
            End Get
            Set(value As Double)
                For Each n In nodes
                    EnsurePos(n)
                    n.data.initialPostion.y = value
                Next
            End Set
        End Property

        <Category("外观"), DisplayName("半径")>
        Public Property Radius As Double
            Get
                If nodes.Length = 0 Then
                    Return 8
                End If
                If nodes(0).data.size Is Nothing OrElse nodes(0).data.size.Length = 0 Then
                    Return 8
                End If
                Return nodes(0).data.size(0)
            End Get
            Set(value As Double)
                Dim r = If(value <= 0, 8, value)
                For Each n In nodes
                    n.data.size = {r}
                Next
            End Set
        End Property

        <Category("物理"), DisplayName("质量")>
        Public Property Mass As Double
            Get
                If nodes.Length = 0 Then
                    Return 1
                End If
                Return nodes(0).data.mass
            End Get
            Set(value As Double)
                For Each n In nodes
                    n.data.mass = value
                Next
            End Set
        End Property

        <Category("分组"), DisplayName("分组颜色")>
        Public Property GroupColor As Color
            Get
                If nodes.Length = 0 Then
                    Return Color.White
                End If
                Dim g = state.GetNodeGroup(nodes(0))
                If g = "" Then
                    Return Color.FromArgb(&HFF9AA4B2)
                End If
                Return state.GroupColors(g)
            End Get
            Set(value As Color)
                Dim g = state.GetNodeGroup(nodes(0))
                If g = "" Then
                    g = "group" & (state.GroupColors.Groups.Length + 1)
                    For Each n In nodes
                        state.SetNodeGroup(n, g)
                    Next
                End If
                state.GroupColors(g) = value
            End Set
        End Property

        Private Sub EnsurePos(Optional n As Node = Nothing)
            If n Is Nothing Then
                If nodes.Length > 0 Then
                    Call EnsurePos(nodes(0))
                End If
                Return
            End If
            If n.data.initialPostion Is Nothing Then
                n.data.initialPostion = New FDGVector2(0, 0)
            End If
        End Sub
    End Class

End Namespace

