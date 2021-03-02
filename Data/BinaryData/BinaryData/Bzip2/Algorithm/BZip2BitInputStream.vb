' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports System.IO

Namespace Bzip2
    ''' <summary>
    ''' Implements a bit-wise input stream
    ''' </summary>
    Friend Class BZip2BitInputStream
#Region "Private fields"
        ' The stream from which bits are read
        Private ReadOnly inputStream As Stream

        ' A buffer of bits read from the input stream that have not yet been returned
        Private bitBuffer As UInteger

        ' The number of bits currently buffered in bitBuffer
        Private bitCount As Integer
#End Region

#Region "Public methods"
        ''' <summary>Public constructor</summary>
        ''' <param name="inputStream">The input stream to wrap</param>
        Public Sub New(inputStream As Stream)
            Me.inputStream = inputStream
        End Sub

        ''' <summary>Reads a single bit from the wrapped input stream</summary>
        ''' <return>true if the bit read was 1, otherwise false</return>
        ''' <exception cref="Exception">if no more bits are available in the input stream</exception>
        Public Function ReadBoolean() As Boolean
            If bitCount > 0 Then
                bitCount -= 1
            Else
                Dim byteRead As Integer = inputStream.ReadByte()
                If byteRead < 0 Then Throw New Exception("Insufficient data")
                bitBuffer = bitBuffer << 8 Or CUInt(byteRead)
                bitCount += 7
            End If

            Return (bitBuffer And 1 << bitCount) <> 0
        End Function

        ''' <summary>Reads a zero-terminated unary number from the wrapped input stream</summary>
        ''' <return>The unary number</return>
        ''' <exception cref="Exception">if no more bits are available in the input stream</exception>
        Public Function ReadUnary() As UInteger
            Dim unaryCount As UInteger = 0

            While True

                If bitCount > 0 Then
                    bitCount -= 1
                Else
                    Dim byteRead = inputStream.ReadByte()

                    If byteRead < 0 Then
                        Throw New Exception("Insufficient data")
                    End If

                    bitBuffer = bitBuffer << 8 Or CUInt(byteRead)
                    bitCount += 7
                End If

                If (bitBuffer And 1 << bitCount) = 0 Then Return unaryCount
                unaryCount += 1
            End While

            Throw New Exception("This exception will be never happends!")
        End Function

        ''' <summary>Reads up to 32 bits from the wrapped input stream</summary>
        ''' <param name="count">The number of bits to read (maximum 32)</param>
        ''' <return>The bits requested, right-aligned within the integer</return>
        ''' <exception cref="Exception">if no more bits are available in the input stream</exception>
        Public Function ReadBits(count As Integer) As UInteger
            If bitCount < count Then
                While bitCount < count
                    Dim byteRead As Integer = inputStream.ReadByte()
                    If byteRead < 0 Then Throw New Exception("Insufficient data")
                    bitBuffer = bitBuffer << 8 Or CUInt(byteRead)
                    bitCount += 8
                End While
            End If

            bitCount -= count
            Return bitBuffer >> bitCount And (1 << count) - 1
        End Function

        ' *
        '  Reads 32 bits of input as an integer
        '  @return The integer read
        '  @ if 32 bits are not available in the input stream
        ' 

        Public Function ReadInteger() As UInteger
            Return ReadBits(16) << 16 Or ReadBits(16)
        End Function
#End Region
    End Class
End Namespace
