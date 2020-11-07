Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace ApplicationServices.Debugging.Diagnostics

    Public Class TraceBuffer : Inherits RawStream

        Public Property StackTrace As StackFrame()

        Private Shared Function EachFrame(frame As StackFrame) As Byte()
            Dim strings As String() = {frame.File, frame.Line, frame.Method.Module, frame.Method.Namespace, frame.Method.Method}
            Dim bytes As Byte() = RawStream.GetBytes(strings)

            Return bytes
        End Function

        Private Shared Function Decode(bytes As Byte()) As StackFrame
            Using ms As New MemoryStream(bytes)
                Dim strings() As String = RawStream.GetData(ms, TypeCode.String)
                Dim frame As New StackFrame With {
                    .File = strings(0),
                    .Line = strings(1),
                    .Method = New Method With {
                        .[Module] = strings(2),
                        .[Namespace] = strings(3),
                        .Method = strings(4)
                    }
                }

                Return frame
            End Using
        End Function

        Public Shared Function DecodeFrames(buffer As Stream) As TraceBuffer
            Dim sizeof As Integer
            Dim bytes As Byte()
            Dim int_size As Byte() = New Byte(3) {}

            buffer.Read(int_size, Scan0, 4)
            buffer.Read(int_size, Scan0, 4)
            sizeof = BitConverter.ToInt32(int_size, Scan0)

            Dim frames As StackFrame() = New StackFrame(sizeof - 1) {}

            For i As Integer = 0 To frames.Length - 1
                buffer.Read(int_size, Scan0, 4)
                sizeof = BitConverter.ToInt32(int_size, Scan0)
                bytes = New Byte(sizeof - 1) {}
                buffer.Read(bytes, Scan0, sizeof)

                frames(i) = Decode(bytes)
            Next

            Return New TraceBuffer With {.StackTrace = frames}
        End Function

        Public Overrides Function Serialize() As Byte()
            Using buffer As New MemoryStream
                Dim totalSize As Integer
                Dim bytes As Byte()

                buffer.Write(BitConverter.GetBytes(Scan0), Scan0, 4)
                buffer.Write(BitConverter.GetBytes(StackTrace.Length), Scan0, 4)

                For Each frame As StackFrame In StackTrace
                    bytes = EachFrame(frame)
                    buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
                    buffer.Write(bytes, Scan0, bytes.Length)

                    totalSize += 4 + bytes.Length
                Next

                buffer.Seek(Scan0, SeekOrigin.Begin)
                buffer.Write(BitConverter.GetBytes(totalSize), Scan0, 4)

                buffer.Flush()

                Return buffer.ToArray
            End Using
        End Function
    End Class
End Namespace