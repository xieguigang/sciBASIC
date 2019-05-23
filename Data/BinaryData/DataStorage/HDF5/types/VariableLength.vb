Imports System.Text

Namespace HDF5.type

    Public Class VariableLength : Inherits DataType

        Public Property type As Integer
        Public Property paddingType As Integer
        Public Property encoding As Encoding

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Return GetType(String)
            End Get
        End Property

    End Class
End Namespace