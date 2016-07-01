Imports System.Runtime.CompilerServices

Public Module SchemasAPI

    <Extension>
    Public Function SaveData(Of T As Class)(source As IEnumerable(Of T), DIR As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean

    End Function
End Module
