#Region "Microsoft.VisualBasic::db0731c1b3b2da14a20ad1c15c49de7f, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Exception\TraceBuffer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 87
    '    Code Lines: 66 (75.86%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 21 (24.14%)
    '     File Size: 3.00 KB


    '     Class TraceBuffer
    ' 
    '         Properties: StackTrace
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Decode, DecodeFrames, EachFrame
    ' 
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace ApplicationServices.Debugging.Diagnostics

    Public Class TraceBuffer : Inherits RawStream

        Public Property StackTrace As StackFrame()

        Sub New()
        End Sub

        Sub New(raw As Byte())
            Using buffer As New MemoryStream(raw)
                StackTrace = DecodeFrames(buffer)
            End Using
        End Sub

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

        Public Shared Function DecodeFrames(buffer As Stream) As StackFrame()
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

            Return frames
        End Function

        Public Overrides Sub Serialize(buffer As Stream)
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
        End Sub
    End Class
End Namespace
