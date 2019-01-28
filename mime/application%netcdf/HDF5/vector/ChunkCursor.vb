Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.netCDF.org.renjin.hdf5.chunked

Namespace org.renjin.hdf5.vector



    Public Class ChunkCursor


        Private ReadOnly vectorOffset As Long
        Private ReadOnly vectorLength As Long
        Private chunk As Chunk

        Public Sub New(vectorOffset As Long, vectorLength As Long, chunk As Chunk)

            Me.vectorOffset = vectorOffset
            Me.vectorLength = vectorLength
            Me.chunk = chunk
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function containsVectorIndex(vectorIndex As Integer) As Boolean
            Return vectorIndex >= vectorOffset AndAlso vectorIndex < (vectorOffset + vectorLength)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function valueAt(i As Integer) As Double
            Return chunk.getDoubleAt((CInt(i - vectorOffset)))
        End Function
    End Class

End Namespace