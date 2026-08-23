Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module MatrixFormat

    ReadOnly hostBuf As New NetworkByteOrderBuffer

    Const magic As String = "scibasic.net/data-matrix"

    <Extension>
    Public Function WriteData(m As DataMatrix, file As Stream) As Boolean
        Using wr As New BinaryWriter(file)
            Call wr.Write(Encoding.ASCII.GetBytes(magic))
            Call wr.Write(m.GetLabels.GetJson)

            For Each row As Double() In m.matrix
                Call wr.Write(hostBuf.GetBytes(row))
            Next
        End Using

        Return True
    End Function

    Public Function ReadData(file As Stream) As DataMatrix
        Using rd As New BinaryReader(file)
            Dim testMagic = Encoding.ASCII.GetString(rd.ReadBytes(magic.Length))

            If testMagic <> magic Then
                Throw New InvalidDataException("Invalid magic header of the target matrix file!")
            Else
                Return rd.ReadMatrix
            End If
        End Using
    End Function

    <Extension>
    Private Function ReadMatrix(rd As BinaryReader) As DataMatrix
        Dim labels As String() = rd.ReadString.LoadJSON(Of String())
        Dim mat As Double()() = New Double(labels.Length - 1)() {}
        Dim width As Integer = labels.Length * RawStream.DblFloat

        For i As Integer = 0 To labels.Length - 1
            mat(i) = hostBuf.ParseDouble(rd.ReadBytes(width))
        Next

        Return New DataMatrix(labels, mat)
    End Function

End Module
