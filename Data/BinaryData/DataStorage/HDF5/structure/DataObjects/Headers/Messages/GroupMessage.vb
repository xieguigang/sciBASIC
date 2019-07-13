#Region "Microsoft.VisualBasic::678f2462a3b99381db3a84b181b6680f, Data\BinaryData\DataStorage\HDF5\structure\DataObjects\Headers\Messages\GroupMessage.vb"

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

    '     Class GroupMessage
    ' 
    '         Properties: bTreeAddress, nameHeapAddress
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
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct.messages

    ''' <summary>
    ''' The Symbol Table Message
    ''' </summary>
    Public Class GroupMessage : Inherits Message

        ''' <summary>
        ''' This value is the address of the v1 B-tree containing the symbol table 
        ''' entries for the group.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property bTreeAddress() As Long

        ''' <summary>
        ''' This value is the address of the local heap containing the link names 
        ''' for the symbol table entries for the group.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property nameHeapAddress() As Long

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.bTreeAddress = ReadHelper.readO([in], sb)
            Me.nameHeapAddress = ReadHelper.readO([in], sb)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} {bTreeAddress} -> {nameHeapAddress}"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("GroupMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("btree address : " & Me.bTreeAddress)
            console.WriteLine("nameheap address : " & Me.nameHeapAddress)
            console.WriteLine("GroupMessage <<<")
        End Sub

    End Class

End Namespace
