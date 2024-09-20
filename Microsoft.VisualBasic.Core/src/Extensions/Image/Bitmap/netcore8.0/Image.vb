Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Then
    Public MustInherit Class Image

        Public MustOverride ReadOnly Property Size As Size

    End Class
#End If
End Namespace