#Region "Microsoft.VisualBasic::90ac2d7d69ee44c36d049059919386d5, mime\text%html\HTML\CSS\Render\CssPropertyAttribute.vb"

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

    '     Class CssPropertyAttribute
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Used to mark a property as a Css property.
    ''' The <see cref="Name"/> property is used to specify the oficial CSS name
    ''' </summary>
    Public Class CssPropertyAttribute : Inherits Attribute

        ''' <summary>
        ''' Gets or sets the name of the CSS property
        ''' </summary>
        Public Property Name() As String

        ''' <summary>
        ''' Creates a new CssPropertyAttribute
        ''' </summary>
        ''' <param name="name">Name of the Css property</param>
        Public Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class
End Namespace
