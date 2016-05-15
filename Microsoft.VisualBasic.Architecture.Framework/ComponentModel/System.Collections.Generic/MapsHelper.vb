Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    Public Structure MapsHelper(Of T)
        ReadOnly __default As T
        ReadOnly __maps As IReadOnlyDictionary(Of String, T)

        Sub New(map As IReadOnlyDictionary(Of String, T), Optional [default] As T = Nothing)
            __default = [default]
            __maps = map
        End Sub

        Public Function GetValue(key As String) As T
            If __maps.ContainsKey(key) Then
                Return __maps(key)
            Else
                Return __default
            End If
        End Function
    End Structure
End Namespace