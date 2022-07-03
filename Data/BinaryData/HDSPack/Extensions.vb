Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Text

<HideModuleName>
Public Module Extensions

    <Extension>
    Public Function WriteText(pack As StreamPack,
                              text As String,
                              fileName As String,
                              Optional encoding As Encodings = Encodings.UTF8) As Boolean

        Dim buffer As Stream = pack.OpenBlock(fileName)
        Dim bin As New StreamWriter(buffer, encoding.CodePage)

        Call bin.WriteLine(text)
        Call bin.Flush()

        If TypeOf buffer Is StreamBuffer Then
            Call buffer.Dispose()
        End If

        Return True
    End Function

    <Extension>
    Public Function ReadText(pack As StreamPack, filename As String, Optional encoding As Encodings = Encodings.UTF8) As String
        If pack.GetObject(filename) Is Nothing Then
            Return Nothing
        Else
            Return New StreamReader(pack.OpenBlock(filename), encoding.CodePage).ReadToEnd
        End If
    End Function
End Module
