Public Class package
    Public Property name As String
    Public Property description As String
    Public Property version As String
    Public Property author As String
    Public Property main As String
    Public Property scripts As scripts
    Public Property repository As repository
    Public Property keywords As String()
    Public Property dependencies As dependencies
    Public Property devDependencies As dependencies
    Public Property license As String

    Public Overrides Function ToString() As String
        Return $"[{name}] {description}"
    End Function
End Class

Public Class scripts
    Public Property test As String
End Class

Public Class repository
    Public Property type As String
    Public Property url As String
End Class

Public Class dependencies
    Public Property summary As String
    Public Property distributions As String
    Public Property eslint As String
    Public Property tap As String
End Class