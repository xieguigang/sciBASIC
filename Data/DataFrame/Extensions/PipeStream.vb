Imports System.IO
Imports System.Runtime.CompilerServices

Public Module PipeStream

    <Extension>
    Public Iterator Function LoadStream(Of T As Class)(input As StreamReader, Optional strict As Boolean = False, Optional maps As Dictionary(Of String, String) = Nothing) As IEnumerable(Of T)

    End Function
End Module
