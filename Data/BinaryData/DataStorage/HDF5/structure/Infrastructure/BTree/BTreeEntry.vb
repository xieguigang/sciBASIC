#Region "Microsoft.VisualBasic::2ebf7d123acdf441271395677ac33018, Data\BinaryData\DataStorage\HDF5\structure\Infrastructure\BTree\BTreeEntry.vb"

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

    '     Class BTreeEntry
    ' 
    '         Properties: key, targetAddress
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

Namespace HDF5.struct.BTree


    Public Class BTreeEntry : Inherits HDF5Ptr

        Public ReadOnly Property targetAddress As Long
        Public ReadOnly Property key As Long

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.key = ReadHelper.readL([in], sb)
            Me.targetAddress = ReadHelper.readO([in], sb)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} [{key} => &{targetAddress}]"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("BTreeEntry >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("key : " & Me.key)
            console.WriteLine("target address : " & Me.targetAddress)
            console.WriteLine("BTreeEntry <<<")
        End Sub
    End Class

End Namespace
