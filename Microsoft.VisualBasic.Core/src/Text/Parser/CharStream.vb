Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace Text.Parser

    Public Class CharStream

        ReadOnly str As StreamReader

        Public ReadOnly Property EndRead As Boolean
            Get
                Return str.EndOfStream AndAlso buffer.EndRead
            End Get
        End Property

        Dim buffer As CharPtr

        Sub New()
        End Sub

        Sub New(s As StreamReader)
            str = s
        End Sub

        Public Function ReadNext() As Char
            If buffer.EndRead Then
                buffer = str.ReadLine
            End If

            Return ++buffer
        End Function

        Private Function ReadNextC() As SeqValue(Of Char)
            If buffer.EndRead Then
                buffer = str.ReadLine
            End If

            Return +buffer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(chars As CharStream) As SeqValue(Of Char)
            Return chars.ReadNextC
        End Operator

        Public Shared Operator Like(chars As CharStream, str As String) As Boolean
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(str)
            Dim s As Stream = chars.str.BaseStream

            If s.Length <> bytes.Length Then
                Return False
            Else
                Dim chunk As Byte() = New Byte(bytes.Length - 1) {}
                Call s.Seek(Scan0, SeekOrigin.Begin)
                Call s.Read(chunk, Scan0, chunk.Length)
                Return chunk.SequenceEqual(bytes)
            End If
        End Operator

        Public Shared Operator =(chars As CharStream, str As String) As Boolean
            Return chars Like str
        End Operator

        Public Shared Operator <>(chars As CharStream, str As String) As Boolean
            Return Not chars Like str
        End Operator

        Public Shared Widening Operator CType(str As String) As CharStream
            Dim s As New MemoryStream(Encoding.UTF8.GetBytes(str))
            Dim reader As New StreamReader(s)
            Return New CharStream(reader)
        End Operator

    End Class

End Namespace