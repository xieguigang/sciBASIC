#Region "Microsoft.VisualBasic::58c68a222e9473324c6c128238b698fd, Data\BinaryData\DataStorage\HDF5\structure\DataObjects\Headers\Messages\ContinueMessage.vb"

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

    '     Class ContinueMessage
    ' 
    '         Properties: length, offset, totalObjectHeaderMessageContinueSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
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
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct.messages


    Public Class ContinueMessage : Inherits Message

        Public ReadOnly Property offset As Long
        Public ReadOnly Property length As Long
        Public ReadOnly Property totalObjectHeaderMessageContinueSize As Integer

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.offset = ReadHelper.readO([in], sb)
            Me.length = ReadHelper.readL([in], sb)
            Me.totalObjectHeaderMessageContinueSize = sb.sizeOfOffsets + sb.sizeOfLengths
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("ObjectHeaderMessageContinue >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("offset : " & Me.offset)
            console.WriteLine("length : " & Me.length)
            console.WriteLine("total header message continue size : " & Me.totalObjectHeaderMessageContinueSize)
            console.WriteLine("ObjectHeaderMessageContinue <<<")
        End Sub
    End Class

End Namespace
