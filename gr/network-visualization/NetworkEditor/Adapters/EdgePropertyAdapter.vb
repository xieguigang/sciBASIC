#Region "Microsoft.VisualBasic::05b494f62b88bfa9bd3ef1b93cb6ca22, gr\network-visualization\NetworkEditor\Adapters\EdgePropertyAdapter.vb"

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

    '   Total Lines: 80
    '    Code Lines: 65 (81.25%)
    ' Comment Lines: 3 (3.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (15.00%)
    '     File Size: 2.37 KB


    '     Class EdgePropertyAdapter
    ' 
    '         Properties: Color, ID, Label, Length, Source
    '                     Target, Weight
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports NetworkEditor.Models

Namespace NetworkEditor.Adapters

    ''' <summary>
    ''' 将选中边的属性包装为 PropertyGrid 可编辑对象
    ''' </summary>
    Public Class EdgePropertyAdapter

        Private ReadOnly state As EditorState
        Private ReadOnly edge As Edge

        Public Sub New(state As EditorState, edge As Edge)
            Me.state = state
            Me.edge = edge
        End Sub

        <Category("标识"), DisplayName("边ID"), [ReadOnly](True)>
        Public ReadOnly Property ID As String
            Get
                Return edge.ID
            End Get
        End Property

        <Category("标识"), DisplayName("标签"), [ReadOnly](True)>
        Public ReadOnly Property Label As String
            Get
                Return edge.data.label
            End Get
        End Property

        <Category("端点"), DisplayName("源节点"), [ReadOnly](True)>
        Public ReadOnly Property Source As String
            Get
                Return edge.U.data.label
            End Get
        End Property

        <Category("端点"), DisplayName("目标节点"), [ReadOnly](True)>
        Public ReadOnly Property Target As String
            Get
                Return edge.V.data.label
            End Get
        End Property

        <Category("属性"), DisplayName("权重")>
        Public Property Weight As Double
            Get
                Return edge.weight
            End Get
            Set(value As Double)
                edge.weight = value
            End Set
        End Property

        <Category("属性"), DisplayName("长度")>
        Public Property Length As Double
            Get
                Return edge.data.length
            End Get
            Set(value As Double)
                edge.data.length = value
            End Set
        End Property

        <Category("外观"), DisplayName("颜色")>
        Public Property Color As Color
            Get
                Return state.GetEdgeColor(edge)
            End Get
            Set(value As Color)
                state.EdgeColors(edge.ID) = value
            End Set
        End Property
    End Class

End Namespace

