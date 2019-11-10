Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract

Namespace Analysis.Model

    ''' <summary>
    ''' 两个节点对象之间的重复的边链接的集合
    ''' </summary>
    ''' <remarks>
    ''' 所有的<see cref="IInteraction.source"/>和<see cref="IInteraction.target"/>都是一样的
    ''' </remarks>
    Public Class EdgeSet(Of Edge As IInteraction) : Inherits List(Of Edge)

        Sub New()
            Call MyBase.New
        End Sub

        Public Overrides Function ToString() As String
            Dim first As Edge = Me.First

            If Count = 1 Then
                Return $"[{first.source}, {first.target}]"
            Else
                Return $"[{first.source}, {first.target}] have {Count} duplicated connections."
            End If
        End Function
    End Class
End Namespace