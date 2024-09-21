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

        Public Property Texture As Image

        Sub New(image As Image)
            Texture = image
        End Sub

    End Class

    Public NotInheritable Class Brushes

        Public Shared ReadOnly Property Black As New SolidBrush(Color.Black)

        Private Sub New()
        End Sub
    End Class
#End If
End Namespace