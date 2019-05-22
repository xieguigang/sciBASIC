Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.device

Namespace HDF5.type

    Public Class VariableLength : Inherits DataType

        Public ReadOnly Property type As Integer
        Public ReadOnly Property paddingType As Integer
        Public ReadOnly Property encoding As Encoding

        Sub New([in] As BinaryReader)
            Call MyBase.New([in])

            type = classBits.ToInteger(0, 4)
            paddingType = classBits.ToInteger(4, 4)

            Select Case classBits.ToInteger(8, 4)
                Case 0
                    encoding = Encoding.ASCII
                Case 1
                    encoding = Encoding.UTF8
                Case Else
                    Throw New InvalidProgramException
            End Select
        End Sub
    End Class
End Namespace