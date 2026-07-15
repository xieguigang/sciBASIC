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
