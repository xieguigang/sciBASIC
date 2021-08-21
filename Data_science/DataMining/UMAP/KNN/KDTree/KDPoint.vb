Namespace KNN.KDTreeMethod

    Public Class KDPoint

        Public Property vector As Double()
        Public Property id As Integer

        Public ReadOnly Property size As Integer
            Get
                Return vector.Length
            End Get
        End Property

    End Class
End Namespace