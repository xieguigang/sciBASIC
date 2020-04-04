Imports Microsoft.VisualBasic.Language

Namespace Text.Parser

    Public Class CharBuffer

        ReadOnly buffer As New List(Of Char)

        Default Public ReadOnly Property GetChar(i As Integer) As Char
            Get
                Return buffer(i)
            End Get
        End Property

        ''' <summary>
        ''' get current size of the data in this char buffer object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Integer
            Get
                Return buffer.Count
            End Get
        End Property

        Public Sub Clear()
            Call buffer.Clear()
        End Sub

        Public Overrides Function ToString() As String
            Return buffer.CharString
        End Function

        Public Shared Widening Operator CType(c As Char) As CharBuffer
            Return New CharBuffer + c
        End Operator

        Public Shared Operator +(buf As CharBuffer, c As Char) As CharBuffer
            buf.buffer.Add(c)
            Return buf
        End Operator

        Public Shared Operator +(buf As CharBuffer, c As Char?) As CharBuffer
            buf.buffer.Add(c)
            Return buf
        End Operator

        Public Shared Operator +(buf As CharBuffer, c As Value(Of Char)) As CharBuffer
            buf.buffer.Add(c.Value)
            Return buf
        End Operator

        Public Shared Operator *(buf As CharBuffer, n As Integer) As CharBuffer
            If n = 0 Then
                buf.Clear()
            Else
                Dim template As Char() = buf.buffer.ToArray

                For i As Integer = 1 To n - 1
                    buf.buffer.AddRange(template)
                Next
            End If

            Return buf
        End Operator

        Public Shared Operator =(buf As CharBuffer, size As Integer) As Boolean
            Return buf.buffer.Count = size
        End Operator

        Public Shared Operator <>(buf As CharBuffer, size As Integer) As Boolean
            Return buf.buffer.Count <> size
        End Operator

        Public Shared Operator >(buf As CharBuffer, size As Integer) As Boolean
            Return buf.buffer.Count > size
        End Operator

        Public Shared Operator <(buf As CharBuffer, size As Integer) As Boolean
            Return buf.buffer.Count < size
        End Operator
    End Class
End Namespace