Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure

Namespace HDF5.type

    Public Class VariableLength

        Public ReadOnly Property type As Integer
        Public ReadOnly Property paddingType As Integer
        Public ReadOnly Property encoding As Encoding
        Public ReadOnly Property parent As DataTypeMessage

        Sub New([in] As BinaryReader)

        End Sub

    End Class
End Namespace