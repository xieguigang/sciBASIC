Imports System.IO
Imports Microsoft.VisualBasic.Data.Repository

Namespace Net.WebClient

    Public Class HashHelper

        Dim hashcode As New Dictionary(Of String, (checksum As String, size As Long))

        Public Sub Add(filepath As String)
            hashcode(filepath.ToLower.MD5) = (FileHashCheckSum.ComputeHash(filepath), filepath.FileLength)
        End Sub

        Public Function Check(filepath As String) As Boolean
            Dim key As String = filepath.ToLower.MD5

            If Not hashcode.ContainsKey(key) Then
                Return False
            End If

            Dim checksum As String = FileHashCheckSum.ComputeHash(filepath)
            Dim len As Long = filepath.FileLength

            With hashcode(key)
                Return .size = len AndAlso .checksum = checksum
            End With
        End Function

        Public Sub Save(filepath As String)
            Using bin As New BinaryWriter(filepath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
                Call bin.Write(hashcode.Count)

                For Each file In hashcode
                    Call bin.Write(file.Key)
                    Call bin.Write(file.Value.checksum)
                    Call bin.Write(file.Value.size)
                Next
            End Using
        End Sub

        Public Shared Function Load(filepath As String) As HashHelper
            If Not filepath.FileExists(True) Then
                Return New HashHelper
            End If

            Using bin As New BinaryReader(filepath.Open(FileMode.Open, doClear:=False, [readOnly]:=True))
                Dim hashset As New Dictionary(Of String, (String, Long))
                Dim n As Integer = bin.ReadInt32

                For i As Integer = 1 To n
                    Dim key As String = bin.ReadString
                    Dim checksum As String = bin.ReadString
                    Dim len As Long = bin.ReadInt64

                    hashset(key) = (checksum, len)
                Next

                Return New HashHelper With {.hashcode = hashset}
            End Using
        End Function

    End Class
End Namespace