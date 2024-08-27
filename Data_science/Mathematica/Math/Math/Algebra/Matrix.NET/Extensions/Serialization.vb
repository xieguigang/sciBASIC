
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' data serialization of the numeric matrix
    ''' </summary>
    Public Module Serialization

        ReadOnly network As New NetworkByteOrderBuffer

        <Extension>
        Public Sub Save(m As NumericMatrix, s As Stream)
            Dim bin As New BinaryWriter(s)

            Call bin.Write(m.RowDimension)
            Call bin.Write(m.ColumnDimension)

            For Each row As Double() In m.Array
                Call bin.Write(network.GetBytes(row))
            Next

            Call bin.Flush()
        End Sub

        Public Function Load(s As Stream) As NumericMatrix
            Dim bin As New BinaryReader(s)
            Dim rows = bin.ReadInt32
            Dim cols = bin.ReadInt32
            Dim m As New List(Of Double())
            Dim buffer_size = HeapSizeOf.double * cols
            Dim buf As Byte()

            For i As Integer = 0 To rows - 1
                buf = bin.ReadBytes(buffer_size)
                m.Add(network.decode(buf))
            Next

            Return New NumericMatrix(m)
        End Function
    End Module
End Namespace