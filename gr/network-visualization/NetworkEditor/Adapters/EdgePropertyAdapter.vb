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
