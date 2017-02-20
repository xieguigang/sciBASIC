Imports System.Collections.Generic
Imports System.Text

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Represents an anonymous inline box
    ''' </summary>
    ''' <remarks>
    ''' To learn more about anonymous inline boxes visit:
    ''' http://www.w3.org/TR/CSS21/visuren.html#anonymous
    ''' </remarks>
    Public Class CssAnonymousBox
        Inherits CssBox
#Region "Ctor"

        Public Sub New(parentBox As CssBox)

            MyBase.New(parentBox)
        End Sub

#End Region
    End Class

    ''' <summary>
    ''' Represents an anonymous inline box which contains nothing but blank spaces
    ''' </summary>
    Public Class CssAnonymousSpaceBox
        Inherits CssAnonymousBox
        Public Sub New(parentBox As CssBox)

            MyBase.New(parentBox)
        End Sub
    End Class
End Namespace