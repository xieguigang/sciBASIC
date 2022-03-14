Namespace Analysis.FastUnfolding

    Public Class KeyMaps

        ReadOnly maps As New Dictionary(Of String, List(Of String))

        Default Public Property Item(key As String) As List(Of String)
            Get
                If Not maps.ContainsKey(key) Then
                    maps.Add(key, New List(Of String))
                End If

                Return maps(key)
            End Get
            Set(value As List(Of String))
                maps(key) = value
            End Set
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of String)
            Get
                Return maps.Keys
            End Get
        End Property

        Sub New()
        End Sub

    End Class
End Namespace