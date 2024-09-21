#Region "Microsoft.VisualBasic::d2a635716b040094980c94ae6df53e55, Microsoft.VisualBasic.Core\src\Net\HTTP\Stream\Base64Codec.vb"

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

    '   Total Lines: 206
    '    Code Lines: 119 (57.77%)
    ' Comment Lines: 58 (28.16%)
    '    - Xml Docs: 87.93%
    ' 
    '   Blank Lines: 29 (14.08%)
    '     File Size: 7.60 KB


    '     Module Base64Codec
    ' 
    '         Function: __getImageFromBase64, __toBase64String, Base64RawBytes, Base64String, DecodeBase64
    '                   GetImage, IsBase64Pattern, (+4 Overloads) ToBase64String, (+2 Overloads) ToStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Net.Http

    ''' <summary>
    ''' Tools API for encoded the image into a base64 string.
    ''' </summary>
    Public Module Base64Codec

        ReadOnly base64Chrs As Index(Of Char) = Enumerable.Range(Asc("a"c), 26) _
            .JoinIterates(Enumerable.Range(Asc("A"c), 26)) _
            .Select(Function(a) Chr(a)) _
            .JoinIterates({"/"c, "+"c, "="c}) _
            .JoinIterates({"0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c}) _
            .Indexing

        Public Function IsBase64Pattern(str As String) As Boolean
            Dim allChars As Char() = str.Distinct.OrderBy(Function(c) c).ToArray
            Dim test As Boolean = allChars.All(Function(c) c Like base64Chrs)

            Return test
        End Function

#Region "text"

        ''' <summary>
        ''' 将普通文本进行base64编码
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="encoding"></param>
        ''' <param name="gzip">
        ''' do gzip compression and then encoded as base64 string?
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Base64String(text$,
                                     Optional encoding As Encoding = Nothing,
                                     Optional gzip As Boolean = False) As String

            Dim bytes As Byte() = (encoding Or UTF8).GetBytes(text)

            If gzip Then
                bytes = New MemoryStream(bytes).GZipStream.ToArray
            End If

            Return bytes.ToBase64String
        End Function

        ''' <summary>
        ''' 将base64字符串还原为原来的字符串文本
        ''' </summary>
        ''' <param name="base64$"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function DecodeBase64(base64$,
                                     Optional encoding As Encoding = Nothing,
                                     Optional ungzip As Boolean = False) As String

            Dim bytes As Byte() = Convert.FromBase64String(base64)

            If ungzip Then
                bytes = bytes.UnGzipStream.ToArray
            End If

            Return (encoding Or UTF8).GetString(bytes)
        End Function

        ''' <summary>
        ''' 将任意的字节数据序列转换为base64字符串
        ''' </summary>
        ''' <param name="byts"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToBase64String(byts As IEnumerable(Of Byte)) As String
            Return Convert.ToBase64String(byts.ToArray)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToBase64String(byts As MemoryStream) As String
            Return Convert.ToBase64String(byts.ToArray)
        End Function
#End Region

#Region "image/png"

        ''' <summary>
        ''' Function to Get Image from Base64 Encoded String
        ''' </summary>
        ''' <param name="Base64String"></param>
        ''' <param name="format"></param>
        ''' <returns></returns>
        <Extension> Public Function GetImage(Base64String As String, Optional format As ImageFormats = ImageFormats.Png) As Bitmap
            Try
                If String.IsNullOrEmpty(Base64String) Then
                    ' Checking The Base64 string validity
                    Return Nothing
                Else
                    Return Base64String.__getImageFromBase64(format)
                End If
            Catch ex As Exception
                Call ex.PrintException
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 将base64编码的字符串还原为原始的数据流
        ''' </summary>
        ''' <param name="base64"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Base64RawBytes(base64 As String) As Byte()
            Return Convert.FromBase64String(base64)
        End Function

        ''' <summary>
        ''' Function to Get Image from Base64 Encoded String
        ''' </summary>
        ''' <param name="base64String"></param>
        ''' <param name="format"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function __getImageFromBase64(base64String$, format As ImageFormats) As Bitmap
            Dim bytData As Byte(), streamImage As Bitmap

            ' Convert Base64 to Byte Array
            bytData = Convert.FromBase64String(base64String)

            ' Using Memory stream to save image
            Using ms As New MemoryStream(bytData)
                ' Converting image from Memory stream
                streamImage = Image.FromStream(ms)
            End Using

            Return streamImage
        End Function

        ''' <summary>
        ''' Convert the Image from Input to Base64 Encoded String
        ''' </summary>
        ''' <param name="img"></param>
        ''' <returns>
        ''' this function will returns empty string if the error happends
        ''' </returns>
        <Extension>
        Public Function ToBase64String(img As Image, Optional format As ImageFormats = ImageFormats.Png) As String
            Try
                Return __toBase64String(img, format)
            Catch ex As Exception
                Call ex.PrintException
                Return ""
            End Try
        End Function

        ''' <summary>
        ''' Convert the Image from Input to Base64 Encoded String
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToBase64String(bmp As Bitmap, Optional format As ImageFormats = ImageFormats.Png) As String
            Return ToBase64String(img:=bmp, format:=format)
        End Function

        <Extension>
        Public Function ToStream(image As Image, format As ImageFormats) As MemoryStream
            With New MemoryStream
#If NET48 Then
                Call image.Save(.ByRef, format.GetFormat)
#Else
                Call image.Save(.ByRef, format)
#End If
                Call .Flush()
                Call .Seek(Scan0, SeekOrigin.Begin)
                Return .ByRef
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function __toBase64String(image As Image, format As ImageFormats) As String
            Dim s = image.ToStream(format)
            Dim buffer = s.ToArray

            Return Convert.ToBase64String(buffer)
        End Function
#End Region
    End Module
End Namespace
