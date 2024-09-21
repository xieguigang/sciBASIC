Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Then

    ''' <summary>
    ''' The stroke pen wrapper for .net 8.0
    ''' </summary>
    Public Class Pen

        Public Property Color As Color
        Public Property Width As Single = 1
        Public Property DashStyle As DashStyle

        Sub New(color As Color)
            _Color = color
        End Sub

        Sub New(brush As SolidBrush, width As Single)
            _Color = brush.Color
            _Width = width
        End Sub

    End Class

    Public Enum DashStyle
        Solid
        Dash
        Dot
        DashDot
        DashDotDot
        Custom
    End Enum

    Public NotInheritable Class Pens

        Public Shared ReadOnly Property Black As New Pen(Color.Black)

        Private Sub New()
        End Sub

    End Class
#End If
End Namespace