Imports System.Text

Namespace HDF5.type

    Public Class VariableLength : Inherits DataType

        Public Property type As Integer
        Public Property paddingType As Integer
        Public Property encoding As Encoding

    End Class
End Namespace