Namespace Imaging

#If NET8_0_OR_GREATER Then
    Public Class Font

        Public ReadOnly Property FamilyName As String
        Public ReadOnly Property Size As Single

        Sub New(name As String, size As Single)
            Me.FamilyName = name
            Me.Size = size
        End Sub

    End Class
#End If
End Namespace