#Region "Microsoft.VisualBasic::01dd9258ec19a4f2afa3f90c7ba0e4b0, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\image.vb"

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

    '   Total Lines: 124
    '    Code Lines: 91 (73.39%)
    ' Comment Lines: 18 (14.52%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 15 (12.10%)
    '     File Size: 4.75 KB


    '     Class SvgImage
    ' 
    '         Properties: Height, HRef, Width, X, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+3 Overloads) Create, GetGDIObject, SaveAs
    ' 
    '         Sub: SetImage, SetRectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;image> SVG element includes images inside SVG documents. It can display raster image files or other SVG files.
    '''
    ''' The only image formats SVG software must support are JPEG, PNG, And other SVG files. Animated GIF behavior Is undefined.
    '''
    ''' SVG files displayed With &lt;image> are treated As an image: external resources aren't loaded, :visited styles aren't
    ''' applied, and they cannot be interactive. To include dynamic SVG elements, try &lt;use> with an external URL. To include 
    ''' SVG files and run scripts inside them, try &lt;object> inside of &lt;foreignObject>.
    ''' </summary>
    Public NotInheritable Class SvgImage : Inherits SvgBasicShape

        Public Property X As Double
            Get
                Return Element.GetAttribute("x", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x", value)
            End Set
        End Property

        Public Property Y As Double
            Get
                Return Element.GetAttribute("y", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y", value)
            End Set
        End Property

        Public Property Width As Double
            Get
                Return Element.GetAttribute("width", Attributes.Size.Width)
            End Get
            Set(value As Double)
                Element.SetAttribute("width", value)
            End Set
        End Property

        Public Property Height As Double
            Get
                Return Element.GetAttribute("height", Attributes.Size.Height)
            End Get
            Set(value As Double)
                Element.SetAttribute("height", value)
            End Set
        End Property

        ''' <summary>
        ''' the image data, usually be a base64 encoded uri string value.
        ''' </summary>
        ''' <returns></returns>
        Public Property HRef As String
            Get
                Return Element.GetAttribute(Of String)("xlink:href", "")
            End Get
            Set(value As String)
                Element.SetAttribute("xlink:href", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetImage(img As Image)
            HRef = New DataURI(img).ToString
        End Sub

        Public Sub SetRectangle(rect As RectangleF)
            Width = rect.Width
            Height = rect.Height
            X = rect.Left
            Y = rect.Top
        End Sub

        ''' <summary>
        ''' create a new empty image node
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <returns></returns>
        Friend Overloads Shared Function Create(parent As XmlElement) As SvgImage
            Dim element = parent.OwnerDocument.CreateElement("image")
            parent.AppendChild(element)
            Return New SvgImage(element)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function Create(parent As XmlElement, image As Bitmap, Optional size As SizeF = Nothing) As SvgImage
            Return Create(parent, CType(image, Image), size)
        End Function

        Public Overloads Shared Function Create(parent As XmlElement, image As Image, Optional size As SizeF = Nothing) As SvgImage
            Dim element = parent.OwnerDocument.CreateElement("image")
            parent.AppendChild(element)
            Dim img As New SvgImage(element)
            img.HRef = New DataURI(image).ToString
            With size Or image.Size.SizeF.AsDefault(Function() size.IsEmpty)
                img.Width = .Width
                img.Height = .Height
            End With
            Return img
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetGDIObject() As Bitmap
            Return Base64Codec.GetImage(DataURI.URIParser(HRef).base64)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SaveAs(fileName$, Optional format As ImageFormats = ImageFormats.Png) As Boolean
#If NET48 Then
            Return GetGDIObject.SaveAs(fileName, format, autoDispose:=True)
#Else
            Using s As Stream = fileName.Open(FileMode.OpenOrCreate, doClear:=True)
                Call GetGDIObject.Save(s, format)
            End Using

            Return True
#End If
        End Function
    End Class
End Namespace
