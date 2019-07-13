#Region "Microsoft.VisualBasic::6552772fd7830640a4447926f07f1562, Data\BinaryData\DataStorage\HDF5\structure\StructureMember.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class StructureMember
    ' 
    '         Properties: dims, message, name, offset
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct

    Public Class StructureMember : Inherits HDF5Ptr

        Public ReadOnly Property name As String
        Public ReadOnly Property offset As Integer
        Public ReadOnly Property dims As Integer
        Public ReadOnly Property message As DataTypeMessage

        Public Sub New(sb As Superblock, address As Long, version As Integer, byteSize As Integer)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.name = [in].readASCIIString()

            If version < 3 Then
                [in].skipBytes(ReadHelper.padding(Me.name.Length + 1, 8))
                Me.offset = [in].readInt()
            Else
                Me.offset = CInt(ReadHelper.readVariableSizeMax([in], byteSize))
            End If

            If version = 1 Then
                Me.dims = [in].readByte()

                [in].skipBytes(3)
                ' ignore dimension info for now
                [in].skipBytes(24)
            End If

            Me.message = New DataTypeMessage(sb, [in].offset)
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {message} = [&{address}, {offset}]"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("StructureMember >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("name : " & Me.name)
            console.WriteLine("offset : " & Me.offset)
            console.WriteLine("m_dims : " & Me.dims)

            If Me.message IsNot Nothing Then
                Me.message.printValues(console)
            End If

            console.WriteLine("StructureMember >>>")
        End Sub
    End Class

End Namespace
