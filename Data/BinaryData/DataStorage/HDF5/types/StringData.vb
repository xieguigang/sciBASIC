Namespace HDF5.type

    Public Class StringData : Inherits DataType

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Return GetType(String)
            End Get
        End Property
    End Class
End Namespace