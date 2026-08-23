Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module MatrixFormat

    ReadOnly netorder As New NetworkByteOrderBuffer

    <Extension>
    Public Function WriteData(m As DataMatrix, file As Stream) As Boolean
        Using wr As New BinaryWriter(file)
            Call wr.Write(m.GetLabels.GetJson)

            For Each row As Double() In m.matrix
                Call wr.Write(netorder.GetBytes(row))
            Next
        End Using

        Return True
    End Function

    Public Function ReadData(file As Stream) As DataMatrix
        Using rd As New BinaryReader(file)
            Dim labels As String() = rd.ReadString.LoadJSON(Of String())
            Dim mat As Double()() = New Double(labels.Length - 1)() {}
            Dim width As Integer = labels.Length * RawStream.DblFloat

            For i As Integer = 0 To labels.Length - 1
                mat(i) = netorder.ParseDouble(rd.ReadBytes(width))
            Next

            Return New DataMatrix(labels, mat)
        End Using
    End Function

End Module
