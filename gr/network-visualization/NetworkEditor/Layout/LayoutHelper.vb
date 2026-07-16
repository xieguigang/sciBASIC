Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Namespace NetworkEditor.Layout

    ''' <summary>
    ''' 布局前的公共准备：保证每个节点都有初始位置，避免算法读取 .x/.y 时空引用
    ''' </summary>
    Public Module LayoutHelper

        Public Sub EnsurePositions(g As NetworkGraph, Optional width% = 1000, Optional height% = 1000)
            For Each n As Node In g.vertex
                If n.data.initialPostion Is Nothing Then
                    n.data.initialPostion = New FDGVector2(Rnd() * width, Rnd() * height)
                End If
            Next
        End Sub

    End Module

End Namespace
