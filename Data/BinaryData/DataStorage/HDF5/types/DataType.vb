Imports Microsoft.VisualBasic.Data.IO.HDF5.device

Namespace HDF5.type

    Public MustInherit Class DataType

        Public ReadOnly Property version As Integer
        Public ReadOnly Property [class] As DataTypes
        Public ReadOnly Property size As Integer

        Protected ReadOnly classBits As BitSet

        Protected Sub New([in] As BinaryReader)
            Dim flag As BitSet = BitSet.ValueOf([in].readByte)

            version = flag.ToInteger(4, 4)
            [class] = flag.ToInteger(0, 4)
            classBits = BitSet.ValueOf([in].readBytes(3))
            size = [in].readInt
        End Sub

        Public Shared Function ReadDataType([in] As BinaryReader) As DataType
            Dim flag As BitSet

            [in].Mark()
            flag = BitSet.ValueOf([in].readByte)

            Dim version = flag.ToInteger(4, 4)
            Dim [class] = flag.ToInteger(0, 4)

            If version = 0 Then
                Throw New Exception("Unrecognized datatype version 0 detected")
            End If
            If version = 3 Then
                Throw New Exception("VAX byte ordered datatype encountered")
            End If

            Call [in].Reset()

            Select Case [class]
                Case DataTypes.DATATYPE_VARIABLE_LENGTH
                    Return New VariableLength([in])
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function
    End Class
End Namespace