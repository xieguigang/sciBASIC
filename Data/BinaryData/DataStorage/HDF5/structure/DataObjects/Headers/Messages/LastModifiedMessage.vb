#Region "Microsoft.VisualBasic::491329b2caa9c810f6a9e06be6836d21, Data\BinaryData\DataStorage\HDF5\structure\DataObjects\Headers\Messages\LastModifiedMessage.vb"

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

    '     Class LastModifiedMessage
    ' 
    '         Properties: seconds, version
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
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct.messages

    Public Class LastModifiedMessage : Inherits Message

        Public ReadOnly Property version() As Integer
        Public ReadOnly Property seconds() As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.version = [in].readByte()

            [in].skipBytes(3)

            Me.seconds = [in].readInt()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} {seconds}"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("LastModifiedMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("seconds : " & Me.seconds)

            console.WriteLine("LastModifiedMessage <<<")
        End Sub
    End Class

End Namespace
