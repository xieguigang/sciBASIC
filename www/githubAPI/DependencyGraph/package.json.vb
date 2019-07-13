#Region "Microsoft.VisualBasic::58d88ab90ff4d2470f0de67536b7ce5d, www\githubAPI\DependencyGraph\package.json.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class package
    ' 
    '     Properties: author, dependencies, description, devDependencies, keywords
    '                 license, main, name, repository, scripts
    '                 version
    ' 
    '     Function: ToString
    ' 
    ' Class scripts
    ' 
    '     Properties: test
    ' 
    ' Class repository
    ' 
    '     Properties: type, url
    ' 
    ' Class dependencies
    ' 
    '     Properties: distributions, eslint, summary, tap
    ' 
    ' /********************************************************************************/

#End Region

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
