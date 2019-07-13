#Region "Microsoft.VisualBasic::41c1a6877cece6a3c73ac62306e8fa5f, Data\BinaryData\DataStorage\HDF5\structure\SymbolicLinkScratchpadFormat.vb"

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

    '     Class SymbolicLinkScratchpadFormat
    ' 
    '         Properties: offsetToLinkValue, totalSymbolicLinkScratchpadFormatSize
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
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct


    Public Class SymbolicLinkScratchpadFormat : Inherits HDF5Ptr

        Public  ReadOnly Property offsetToLinkValue As Integer
        Public  ReadOnly Property totalSymbolicLinkScratchpadFormatSize As Integer

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.offsetToLinkValue = [in].readInt()
            Me.totalSymbolicLinkScratchpadFormatSize = 4
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("SymbolicLinkScratchpadFormat >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("offset to link value : " & Me.offsetToLinkValue)

            console.WriteLine("total symbolic link scratchpad format size : " & Me.totalSymbolicLinkScratchpadFormatSize)
            console.WriteLine("SymbolicLinkScratchpadFormat <<<")
        End Sub
    End Class

End Namespace
