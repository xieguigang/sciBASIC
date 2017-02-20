Imports System.Collections.Generic
Imports System.Text

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Used to mark a property as a Css property.
    ''' The <see cref="Name"/> property is used to specify the oficial CSS name
    ''' </summary>
    Public Class CssPropertyAttribute
        Inherits Attribute
#Region "Fields"
        Private _name As String
#End Region

#Region "Ctor"

        ''' <summary>
        ''' Creates a new CssPropertyAttribute
        ''' </summary>
        ''' <param name="name">Name of the Css property</param>
        Public Sub New(name As String)
            Me.Name = name
        End Sub
#End Region

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the name of the CSS property
        ''' </summary>
        Public Property Name() As String
            Get
                Return _name
            End Get
            Set
                _name = value
            End Set
        End Property


#End Region
    End Class
End Namespace