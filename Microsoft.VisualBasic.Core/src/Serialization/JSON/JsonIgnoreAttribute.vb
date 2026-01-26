Namespace Serialization.JSON

    ' fix for the missing attribute from .NET core System.Text.Json.Serialization

#If NET48 Then
Public Class JsonIgnoreAttribute : Inherits Attribute
End Class
#End If

End Namespace
