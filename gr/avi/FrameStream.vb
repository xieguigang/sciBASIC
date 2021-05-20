#Region "Microsoft.VisualBasic::ee1594793836462f564cc264ea8e05f2, gr\avi\FrameStream.vb"

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

    ' Class FrameStream
    ' 
    '     Properties: length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Public Class FrameStream

    Public ReadOnly Property length As Integer

    ReadOnly temp$
    ReadOnly begin As Long

    Sub New(ref$, buf As Byte())
        length = buf.Length
        temp = ref

        Using writer As New BinaryWriter(ref.Open(FileMode.OpenOrCreate, doClear:=False))
            begin = writer.BaseStream.Length + 1L

            ' 因为writer的seek函数的offset为Integer类型
            ' 直接调用seek会溢出
            writer.BaseStream.Seek(begin, SeekOrigin.Begin)
            writer.Write(buf)
            writer.Flush()
        End Using
    End Sub

    Public Overrides Function ToString() As String
        Return $"&{begin.ToHexString} [{length} bytes]"
    End Function

    Public Shared Narrowing Operator CType(stream As FrameStream) As Byte()
        Using reader As New BinaryReader(stream.temp.Open(FileMode.Open, doClear:=False))
            Call reader.BaseStream.Seek(stream.begin, SeekOrigin.Begin)
            Return reader.ReadBytes(stream.length)
        End Using
    End Operator
End Class
