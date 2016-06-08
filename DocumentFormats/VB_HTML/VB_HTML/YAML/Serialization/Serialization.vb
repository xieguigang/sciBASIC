Imports System.Runtime.CompilerServices

Namespace YAML

    Public Module Serialization

        <Extension>
        Public Function LoadYAML(Of T)(path As String) As T


        End Function

        <Extension>
        Public Function WriteYAML(Of T)(obj As T, path As String, Optional encoding As Encodings = Encodings.Unicode) As Boolean

        End Function
    End Module
End Namespace