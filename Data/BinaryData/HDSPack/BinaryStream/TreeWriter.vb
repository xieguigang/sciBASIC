Imports System.IO
Imports System.Runtime.CompilerServices

Friend Module TreeWriter

    <Extension>
    Public Function GetBuffer(root As StreamGroup) As Byte()
        Using ms As New MemoryStream
            Return ms.ToArray
        End Using
    End Function

End Module
