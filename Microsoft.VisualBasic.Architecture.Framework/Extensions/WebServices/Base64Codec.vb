#Region "Microsoft.VisualBasic::17b968357faf5c82a34668e83990f17c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\WebServices\Base64Codec.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Namespace Net.Http

    ''' <summary>
    ''' Tools API for encoded the image into a base64 string.
    ''' </summary>
    Public Module Base64Codec

        <Extension>
        Public Function ToBase64String(byts As IEnumerable(Of Byte)) As String
            Return Convert.ToBase64String(byts.ToArray)
        End Function

        ''' <summary>
        ''' Function to Get Image from Base64 Encoded String
        ''' </summary>
        ''' <param name="Base64String"></param>
        ''' <param name="format"></param>
        ''' <returns></returns>
        <Extension> Public Function GetImage(Base64String As String, Optional format As ImageFormats = ImageFormats.Png) As Bitmap
            Try
                If String.IsNullOrEmpty(Base64String) Then Return Nothing  ''Checking The Base64 string validity
                Return __getImageFromBase64(Base64String, GetFormat(format))
            Catch ex As Exception
                Call ex.PrintException
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Function to Get Image from Base64 Encoded String
        ''' </summary>
        ''' <param name="Base64String"></param>
        ''' <param name="format"></param>
        ''' <returns></returns>
        Private Function __getImageFromBase64(Base64String As String, format As ImageFormat) As Bitmap
            Dim bytData As Byte(), streamImage As Bitmap

            bytData = Convert.FromBase64String(Base64String) ''Convert Base64 to Byte Array

            Using ms As New MemoryStream(bytData) ''Using Memory stream to save image
                streamImage = Image.FromStream(ms) ''Converting image from Memory stream
            End Using

            Return streamImage
        End Function

        ''' <summary>
        ''' Convert the Image from Input to Base64 Encoded String
        ''' </summary>
        ''' <param name="ImageInput"></param>
        ''' <returns></returns>
        <Extension> Public Function ToBase64String(ImageInput As Image, Optional format As ImageFormats = ImageFormats.Png) As String
            Try
                Return __toBase64String(ImageInput, GetFormat(format))
            Catch ex As Exception
                Call ex.PrintException
                Return ""
            End Try
        End Function

        ''' <summary>
        ''' Convert the Image from Input to Base64 Encoded String
        ''' </summary>
        ''' <returns></returns>
        <Extension> Public Function ToBase64String(bmp As Bitmap, Optional format As ImageFormats = ImageFormats.Png) As String
            Return ToBase64String(ImageInput:=bmp, format:=format)
        End Function

        Private Function __toBase64String(image As Image, format As ImageFormat) As String
            Dim ms As MemoryStream = New MemoryStream()
            image.Save(ms, format)
            Dim Base64Op As String = Convert.ToBase64String(ms.ToArray())
            Return Base64Op
        End Function
    End Module
End Namespace
