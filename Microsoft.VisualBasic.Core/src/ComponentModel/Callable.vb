Namespace ComponentModel

    Public MustInherit Class Callable(Of T)

        Public MustOverride Function [call]() As T
    End Class
End Namespace