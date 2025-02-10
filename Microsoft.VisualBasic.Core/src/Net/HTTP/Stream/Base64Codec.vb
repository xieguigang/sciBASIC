#Region "Microsoft.VisualBasic::a23747f345dc328cee4ec068e24c24cf, Microsoft.VisualBasic.Core\src\Net\HTTP\Stream\Base64Codec.vb"

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

    '   Total Lines: 238
    '    Code Lines: 142 (59.66%)
    ' Comment Lines: 65 (27.31%)
    '    - Xml Docs: 89.23%
    ' 
    '   Blank Lines: 31 (13.03%)
    '     File Size: 8.80 KB


    '     Module Base64Codec
    ' 
    '         Function: __getImageFromBase64, __toBase64String, Base64RawBytes, Base64String, DecodeBase64
    '                   GetImage, IsBase64Pattern, (+4 Overloads) ToBase64String, ToStream
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
        ''' <param name="base64">a base64 encoded text string data for make decode</param>
        ''' <param name="encoding"></param>
        ''' <param name="strict">
        ''' throw exception when error occurs during base64 decode? this function will 
        ''' returns empty string if not strict and exception happends. 
        ''' </param>
        ''' <returns>
        ''' this function may returns empty string when the given base64 string is invalid 
        ''' and case the base64 decoder error and set strict parameter to false.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function DecodeBase64(base64$,
                                     Optional encoding As Encoding = Nothing,
                                     Optional ungzip As Boolean = False,
                                     Optional strict As Boolean = True) As String

            Dim bytes As Byte()

            If strict Then
                bytes = Convert.FromBase64String(base64)
            Else
                Try
                    bytes = Convert.FromBase64String(base64)
                Catch ex As Exception
                    Call App.LogException(ex)
                    Return ""
                End Try
            End If

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
        <Extension>
        Public Function GetImage(Base64String As String, Optional format As ImageFormats = ImageFormats.Png) As Image
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
            If base64 Is Nothing OrElse base64 = "" Then
                Return {}
            Else
                Return Convert.FromBase64String(base64)
            End If
        End Function

        ''' <summary>
        ''' Function to Get Image from Base64 Encoded String
        ''' </summary>
        ''' <param name="base64String"></param>
        ''' <param name="format"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function __getImageFromBase64(base64String$, format As ImageFormats) As Image
            Dim bytData As Byte(), streamImage As Image

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
                Dim format_gdi As ImageFormat = ImageFormat.Png

                Select Case format
                    Case ImageFormats.Jpeg : format_gdi = ImageFormat.Jpeg
                    Case ImageFormats.Gif : format_gdi = ImageFormat.Gif
                    Case Else
                        Call $"The given image format flag is not supported, use png as default.".Warning
                End Select

                Call image.Save(.ByRef, format_gdi)
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
