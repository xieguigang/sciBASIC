Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Driver.CSS

    Public Class Driver : Inherits ExportAPIAttribute

        Sub New(name$)
            Call MyBase.New(name)
        End Sub
    End Class

    Public Class CSSSelector : Inherits Attribute

        Public ReadOnly Property Type As Types

        Sub New(type As Types)
            Me.Type = type
        End Sub

        Public Overrides Function ToString() As String
            Return Type.ToString
        End Function
    End Class

    Public Enum Types

        ''' <summary>
        ''' <see cref="CSSFont"/>
        ''' </summary>
        Font
        ''' <summary>
        ''' <see cref="MIME.Markup.HTML.CSS.Stroke"/>
        ''' </summary>
        Stroke
        ''' <summary>
        ''' <see cref="Fill"/>
        ''' </summary>
        Brush
        ''' <summary>
        ''' <see cref="CSSsize"/>
        ''' </summary>
        Size
        ''' <summary>
        ''' <see cref="MIME.Markup.HTML.CSS.Padding"/>
        ''' </summary>
        Padding
    End Enum
End Namespace