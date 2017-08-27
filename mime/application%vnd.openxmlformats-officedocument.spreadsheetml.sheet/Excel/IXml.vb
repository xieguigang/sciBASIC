Imports System.Text

Public MustInherit Class IXml

    Protected MustOverride Function filePath() As String
    Protected MustOverride Function toXml() As String

    Public Overrides Function ToString() As String
        Return filePath()
    End Function

    Public Function WriteXml(dir$) As Boolean
        Dim path$ = dir & "/" & filePath()
        Dim xml$ = toXml()
        Return xml.SaveTo(path, Encoding.UTF8)
    End Function
End Class
