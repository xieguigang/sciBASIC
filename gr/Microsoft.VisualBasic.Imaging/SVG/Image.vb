#Region "Microsoft.VisualBasic::0dd93b231b14ab3fafa580e501945ccb, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\Image.vb"

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
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.FileIO

Namespace SVG

    ''' <summary>
    ''' Does SVG support embedding of bitmap images?
    ''' 
    ''' + http://stackoverflow.com/questions/6249664/does-svg-support-embedding-of-bitmap-images
    ''' </summary>
    Public Class Image

        <XmlAttribute> Public Property x As Single
        <XmlAttribute> Public Property y As Single
        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute("image.data")> Public Property data As String

        ''' <summary>
        ''' ``data:image/png;base64,...``
        ''' </summary>
        Const base64Header As String = "data:image/png;base64,"

        Public Function GetGDIObject() As Bitmap
            Return Base64Codec.GetImage(Mid(data, base64Header.Length + 1))
        End Function

        Sub New()
        End Sub

        Sub New(image As Bitmap, Optional size As Size = Nothing)
            data = base64Header & image.ToBase64String
            If size.IsEmpty Then
                size = image.Size
            End If
            width = size.Width
            height = size.Height
        End Sub

        Sub New(url As String, Optional size As Size = Nothing)
            Call Me.New(MapNetFile(url).LoadImage, size)
        End Sub

        Public Overrides Function ToString() As String
            Return $"<image x=""{x}"" y=""{y}"" width=""{width}"" height=""{height}"" xlink:href=""{data}"">"
        End Function

        Public Function SaveAs(fileName As String, Optional format As ImageFormats = ImageFormats.Png) As Boolean
            Return GetGDIObject.SaveAs(fileName, format)
        End Function
    End Class
End Namespace
