Imports System.Collections.Generic
Imports System.Text

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Represents an anonymous block box
    ''' </summary>
    ''' <remarks>
    ''' To learn more about anonymous block boxes visit CSS spec:
    ''' http://www.w3.org/TR/CSS21/visuren.html#anonymous-block-level
    ''' </remarks>
    Public Class CssAnonymousBlockBox
        Inherits CssBox
        Public Sub New(parent As CssBox)
            MyBase.New(parent)
            Display = CssConstants.Block
        End Sub

        Public Sub New(parent As CssBox, insertBefore As CssBox)
            Me.New(parent)
            Dim index As Integer = parent.Boxes.IndexOf(insertBefore)

            If index < 0 Then
                Throw New Exception("insertBefore box doesn't exist on parent")
            End If
            parent.Boxes.Remove(Me)
            parent.Boxes.Insert(index, Me)
        End Sub
    End Class

    ''' <summary>
    ''' Represents an AnonymousBlockBox which contains only blank spaces
    ''' </summary>
    Public Class CssAnonymousSpaceBlockBox
        Inherits CssAnonymousBlockBox
        Public Sub New(parent As CssBox)
            MyBase.New(parent)
            Display = CssConstants.None
        End Sub

        Public Sub New(parent As CssBox, insertBefore As CssBox)
            MyBase.New(parent, insertBefore)
            Display = CssConstants.None
        End Sub
    End Class
End Namespace