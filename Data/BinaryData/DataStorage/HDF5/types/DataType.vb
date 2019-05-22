Imports Microsoft.VisualBasic.Data.IO.HDF5.IO

Namespace HDF5.type

    Public MustInherit Class DataType

        Public ReadOnly Property version As Integer
        Public ReadOnly Property [class] As Integer
        Public ReadOnly Property size As Integer

        Sub New([in] As BinaryReader)
            Call [in].Mark()

            Dim flag As BitSet = BitSet.valueof([in].readByte)

        End Sub
    End Class
End Namespace