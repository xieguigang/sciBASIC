Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Then

    Public MustInherit Class Brush

    End Class

    Public Class SolidBrush : Inherits Brush

        Public Property Color As Color

        Sub New(color As Color)
            Me.Color = color
        End Sub
    End Class

    Public Class TextureBrush : Inherits Brush

        Public Property Image As Image

        Sub New(image As Image)
            Me.Image = image
        End Sub

    End Class

    Public NotInheritable Class Brushes

        Public Shared ReadOnly Property Red As New SolidBrush(Color.Red)
        Public Shared ReadOnly Property Black As New SolidBrush(Color.Black)
        Public Shared ReadOnly Property White As New SolidBrush(Color.White)
        Public Shared ReadOnly Property Gray As New SolidBrush(Color.Gray)
        Public Shared ReadOnly Property LightGray As New SolidBrush(Color.LightGray)
        Public Shared ReadOnly Property SkyBlue As New SolidBrush(Color.SkyBlue)
        Public Shared ReadOnly Property Violet As New SolidBrush(Color.Violet)

        Private Sub New()
        End Sub
    End Class
#End If
End Namespace