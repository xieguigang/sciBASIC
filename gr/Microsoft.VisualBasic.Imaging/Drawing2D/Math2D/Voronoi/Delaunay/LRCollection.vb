
Namespace Drawing2D.Math2D.DelaunayVoronoi
    Public Class LRCollection(Of T)
        Private left As T
        Private right As T
        Default Public Property Item(index As LR) As T
            Get
                Return If(index Is LR.LEFT, left, right)
            End Get
            Set(value As T)
                If index Is LR.LEFT Then
                    left = value
                Else
                    right = value
                End If
            End Set
        End Property

        Public Sub Clear()
            left = Nothing
            right = Nothing
        End Sub
    End Class
End Namespace
