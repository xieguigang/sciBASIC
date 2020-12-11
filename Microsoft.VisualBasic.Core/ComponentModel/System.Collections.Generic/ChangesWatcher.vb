Namespace ComponentModel.Collection

    Public Class ChangesWatcher

        Dim exists As Index(Of String)
        Dim getItems As Func(Of IEnumerable(Of String))
        Dim lock As Boolean = False

        Public Event AddNew(newItem As String)
        Public Event Remove(item As String)

        Sub New(getItems As Func(Of IEnumerable(Of String)))
            Me.getItems = getItems
            Me.exists = getItems().Distinct.ToArray
        End Sub

        Private Sub DoRefresh()
            Dim pendings As String() = getItems().Distinct.ToArray

            For Each newItem As String In pendings
                If Not newItem Like exists Then
                    RaiseEvent AddNew(newItem)
                End If
            Next

            Dim pendingIndex As Index(Of String) = pendings

            For Each item As String In exists.Objects
                If Not item Like pendingIndex Then
                    RaiseEvent Remove(item)
                End If
            Next

            exists = pendingIndex
        End Sub

        Public Sub Refresh()
            If Not lock Then
                lock = True
                DoRefresh()
                lock = False
            End If
        End Sub

        Public Function Items() As IEnumerable(Of String)
            Return exists.Objects
        End Function

        Public Overrides Function ToString() As String
            Return $"Exists {exists.Count} items."
        End Function

    End Class
End Namespace