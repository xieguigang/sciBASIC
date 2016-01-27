Public Class LoopArray(Of T)

    Dim __innerArray As T()
    Dim __p As Integer

    Sub New(source As IEnumerable(Of T))
        __innerArray = source.ToArray
    End Sub

    Public Function [GET]() As T
        If __p < __innerArray.Length - 1 Then
            __p += 1
        Else
            __p = 0
        End If

        Return __innerArray(__p)
    End Function
End Class
