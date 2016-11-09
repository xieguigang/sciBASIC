Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.JSON.Parser

Public Module Extensions

    <Extension>
    Public Function ParseJsonStr(JsonStr As String) As JsonElement
        Dim value As JsonElement = New JsonParser().OpenJSON(JsonStr)
        Return value
    End Function

    Public Function ParseJsonFile(JsonFile As String) As JsonElement
        Dim value As JsonElement = New JsonParser().Open(JsonFile)
        Return value
    End Function
End Module