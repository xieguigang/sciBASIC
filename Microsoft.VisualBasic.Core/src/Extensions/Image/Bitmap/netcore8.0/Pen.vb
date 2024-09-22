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
        Public Property Brush As Brush

        Sub New(color As Color, Optional width As Single = 1)
            _Color = color
            _Width = width
            _Brush = New SolidBrush(color)
        End Sub

        Sub New(brush As SolidBrush, Optional width As Single = 1)
            _Brush = brush
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