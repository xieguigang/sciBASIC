#Region "Microsoft.VisualBasic::e1ab0930d8d88c012834fddf300b486c, Data\BinaryData\DataStorage\HDF5\structure\DataChunkIterator.vb"

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

    '     Class DataChunkIterator
    ' 
    '         Properties: root
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [next], hasNext
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
Imports System.Runtime.CompilerServices
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct

    Public Class DataChunkIterator : Inherits HDF5Ptr

        Public ReadOnly Property root As BTreeNode

        Public Sub New(sb As Superblock, layout As Layout)
            Call MyBase.New(layout.dataAddress)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.root = New BTreeNode(sb, layout, Me.m_address)
            Me.root.first([in], sb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasNext() As Boolean
            Return Me.root.hasNext()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public  Function [next]([in] As BinaryReader, sb As Superblock) As DataChunk
            Return Me.root.[next]([in], sb)
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Throw New NotImplementedException()
        End Sub
    End Class

End Namespace
