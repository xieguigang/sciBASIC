#Region "Microsoft.VisualBasic::410c52e1917302e6a75ee8cecbd1268f, mime\text%html\HTML\CSS\Render\CssAnonymousBox.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class CssAnonymousBox
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class CssAnonymousSpaceBox
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Represents an anonymous inline box
    ''' </summary>
    ''' <remarks>
    ''' To learn more about anonymous inline boxes visit:
    ''' http://www.w3.org/TR/CSS21/visuren.html#anonymous
    ''' </remarks>
    Public Class CssAnonymousBox : Inherits CssBox

#Region "Ctor"
        Public Sub New(parentBox As CssBox)
            MyBase.New(parentBox)
        End Sub
#End Region
    End Class

    ''' <summary>
    ''' Represents an anonymous inline box which contains nothing but blank spaces
    ''' </summary>
    Public Class CssAnonymousSpaceBox : Inherits CssAnonymousBox

        Public Sub New(parentBox As CssBox)
            MyBase.New(parentBox)
        End Sub
    End Class
End Namespace
