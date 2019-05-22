Imports Microsoft.VisualBasic.Data.IO.HDF5.IO

Namespace HDF5.type

    Public MustInherit Class DataType

        Public ReadOnly Property version As Integer
        Public ReadOnly Property [class] As DataTypes
        Public ReadOnly Property size As Integer

        Sub New([in] As BinaryReader)
            Call [in].Mark()

            Dim flag As BitSet = BitSet.ValueOf([in].readByte)

            version = flag.ToInteger(4, 4)
            [class] = flag.ToInteger(0, 4)

            If version = 0 Then
                Throw New Exception("Unrecognized datatype version 0 detected")
            End If
            If version = 3 Then
                Throw New Exception("VAX byte ordered datatype encountered")
            End If

            Call [in].Reset()

            Select Case [class]
                Case DataTypes.DATATYPE_VARIABLE_LENGTH
                Case Else
                    Throw New NotImplementedException
            End Select
        End Sub
    End Class
End Namespace