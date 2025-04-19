#Region "Microsoft.VisualBasic::08b9ca56f3ea777f1a16ceee7c897695, Data\BinaryData\BinaryData\Extensions\Extensions.vb"

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

    '   Total Lines: 161
    '    Code Lines: 95 (59.01%)
    ' Comment Lines: 42 (26.09%)
    '    - Xml Docs: 97.62%
    ' 
    '   Blank Lines: 24 (14.91%)
    '     File Size: 4.90 KB


    ' Interface IMagicBlock
    ' 
    '     Properties: magic
    ' 
    ' Module Extensions
    ' 
    '     Function: CheckMagicNumber, OpenBinaryReader, ReadAsDoubleVector, ReadAsInt64Vector, skip
    '               (+3 Overloads) VerifyMagicSignature
    ' 
    '     Sub: WriteByte
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Text

Public Interface IMagicBlock

    ''' <summary>
    ''' Magic字符串为数据块的起始位置处的指定字节数量ASCII字符串数据
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property magic As String

End Interface

<HideModuleName> Public Module Extensions

    <Extension>
    Public Function skip(buffer As Stream, nbytes As Integer) As Long
        If buffer.Position + nbytes > buffer.Length Then
            nbytes = buffer.Length - buffer.Position
        End If

        Call buffer.Seek(nbytes, SeekOrigin.Current)

        Return nbytes
    End Function

    ''' <summary>
    ''' check the magic number inside <see cref="RequestStream.ChunkBuffer"/>
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="magic"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function will verify the given target stream <paramref name="s"/> is null or empty.
    ''' </remarks>
    <Extension>
    Public Function CheckMagicNumber(s As RequestStream, magic As IEnumerable(Of Byte)) As Boolean
        Dim i As Integer = 0

        If s Is Nothing OrElse s.ChunkBuffer Is Nothing Then
            Return False
        End If

        Dim bin As Byte() = s.ChunkBuffer

        For Each b As Byte In magic
            If bin.Length = i Then
                Return False
            ElseIf bin(i) <> b Then
                Return False
            End If

            i += 1
        Next

        Return True
    End Function

    <Extension>
    Public Function CheckMagicNumber(s As Stream, magic As IReadOnlyCollection(Of Byte)) As Boolean
        Dim buf As Byte() = New Byte(magic.Count - 1) {}
        s.Read(buf, 0, buf.Length)
        Return buf.SequenceEqual(magic)
    End Function

    <Extension>
    Public Function VerifyMagicSignature(block As IMagicBlock, buffer As BinaryDataReader) As Boolean
        Return block.VerifyMagicSignature(buffer.ReadString(block.magic.Length))
    End Function

    ''' <summary>
    ''' 使用整形数存储的验证数据
    ''' </summary>
    ''' <param name="block"></param>
    ''' <param name="signature$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function VerifyMagicSignature(block As IMagicBlock, signature$) As Boolean
        Dim magic$ = block.magic

        If magic.Length <> signature.Length Then
            Return False
        Else
            For i As Integer = 0 To magic.Length - 1
                If magic(i) <> signature(i) Then
                    Return False
                End If
            Next
        End If

        Return True
    End Function

    ''' <summary>
    ''' 使用base64存储的验证数据
    ''' </summary>
    ''' <param name="block"></param>
    ''' <param name="signature"></param>
    ''' <returns></returns>
    <Extension>
    Public Function VerifyMagicSignature(block As IMagicBlock, signature As IEnumerable(Of Byte)) As Boolean
        Dim magic As Byte() = block.magic.Base64RawBytes
        Dim i As Integer = Scan0

        For Each [byte] As Byte In signature
            If [byte] <> magic(i) Then
                Return False
            Else
                i += 1
            End If
        Next

        If i <> magic.Length Then
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' Open <see cref="BinaryDataReader"/>
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function OpenBinaryReader(path$, Optional encoding As Encodings = Encodings.ASCII) As BinaryDataReader
        Return New BinaryDataReader(path.Open(FileMode.Open, doClear:=False, [readOnly]:=True), encoding)
    End Function

    ''' <summary>
    ''' 整个文件都是<see cref="Double"/>类型的值
    ''' </summary>
    ''' <param name="bin"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ReadAsDoubleVector(bin As BinaryDataReader) As IEnumerable(Of Double)
        Using bin
            Do While Not bin.EndOfStream
                Yield bin.ReadDouble
            Loop
        End Using
    End Function

    ''' <summary>
    ''' 整个文件都是<see cref="Long"/>类型的值
    ''' </summary>
    ''' <param name="bin"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ReadAsInt64Vector(bin As BinaryDataReader) As IEnumerable(Of Long)
        Using bin
            Do While Not bin.EndOfStream
                Yield bin.ReadInt64
            Loop
        End Using
    End Function

    <Extension>
    Public Sub WriteByte(stream As Stream, sbytes As SByte())
        Call stream.Write(sbytes.CastByte, Scan0, sbytes.Length)
    End Sub
End Module
