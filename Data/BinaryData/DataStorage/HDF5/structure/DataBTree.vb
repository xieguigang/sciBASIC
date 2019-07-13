#Region "Microsoft.VisualBasic::a08c765afdb007638a49fca93f607274, Data\BinaryData\DataStorage\HDF5\structure\DataBTree.vb"

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

    '     Class DataBTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumerateChunks, getChunkIterator, ToString
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.device

Namespace HDF5.struct

    Public Class DataBTree

        ReadOnly layout As Layout

        Public Sub New(layout As Layout)
            Me.layout = layout
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getChunkIterator(sb As Superblock) As DataChunkIterator
            Return New DataChunkIterator(sb, Me.layout)
        End Function

        Public Iterator Function EnumerateChunks(sb As Superblock) As IEnumerable(Of DataChunk)
            Dim reader As DataChunkIterator = getChunkIterator(sb)
            Dim file As BinaryReader = sb.FileReader(-1)

            Do While reader.hasNext()
                Yield reader.next(file, sb)
            Loop
        End Function

        Public Overrides Function ToString() As String
            Return layout.ToString
        End Function
    End Class

End Namespace
