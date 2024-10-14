#Region "Microsoft.VisualBasic::0c1f510848346a2b860be8e63152b0f6, gr\Microsoft.VisualBasic.Imaging\Drivers\Models\SVGData.vb"

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

    '   Total Lines: 111
    '    Code Lines: 63 (56.76%)
    ' Comment Lines: 30 (27.03%)
    '    - Xml Docs: 96.67%
    ' 
    '   Blank Lines: 18 (16.22%)
    '     File Size: 3.82 KB


    '     Class SVGData
    ' 
    '         Properties: Driver, SVG, title, XmlComment
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetDataURI, GetSVGXml, Render, (+2 Overloads) Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Namespace Driver

    ''' <summary>
    ''' SVG graphic data
    ''' </summary>
    Public Class SVGData : Inherits GraphicsData

        Public ReadOnly Property SVG As SVGDataLayers

        ''' <summary>
        ''' The xml comment text when generates the svg document text
        ''' </summary>
        ''' <returns></returns>
        Public Property XmlComment As String
        ''' <summary>
        ''' title of current svg graphics plot document
        ''' </summary>
        ''' <returns></returns>
        Public Property title As String

        ''' <summary>
        ''' <paramref name="img"/> parameter is <see cref="GraphicsSVG"/>
        ''' </summary>
        ''' <param name="img"></param>
        ''' <param name="size"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(img As Object, size As Size, padding As Padding)
            Call MyBase.New(img, size, padding)

            If TypeOf img Is SVGDataLayers Then
                SVG = img
            ElseIf TypeOf img Is GraphicsSVG Then
                ' get svg document data
                SVG = DirectCast(img, GraphicsSVG).__svgData
            Else
                Throw New InvalidCastException(img.GetType.FullName)
            End If
        End Sub

        Sub New(canvas As GraphicsSVG, padding As Padding)
            Call Me.New(canvas, canvas.Size, padding)
        End Sub

        Public Overrides ReadOnly Property Driver As Drivers
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Drivers.SVG
            End Get
        End Property

        Const InvalidSuffix$ = "The SVG image file save path: {0} not ending with *.svg file extension suffix!"

        ''' <summary>
        ''' convert the svg document to base64 encoded data uri
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetDataURI() As DataURI
            Dim layoutSize = Layout.Size
            Dim sz$ = $"{layoutSize.Width},{layoutSize.Height}"

            Using data As New MemoryStream
                Call SVG.WriteSVG(data, sz, XmlComment, title:=title)
                Call data.Seek(Scan0, SeekOrigin.Begin)

                Return New DataURI(data, content_type)
            End Using
        End Function

        ''' <summary>
        ''' get xml document text of current svg graphics plot
        ''' </summary>
        ''' <returns></returns>
        Public Function GetSVGXml() As String
            Using buffer As New MemoryStream
                Call Save(out:=buffer)
                Call buffer.Flush()

                Return Encoding.UTF8.GetString(buffer.ToArray)
            End Using
        End Function

        ''' <summary>
        ''' Save the image as svg file.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Overrides Function Save(path As String) As Boolean
            If Not path.ExtensionSuffix.TextEquals("svg") Then
                Call String.Format(InvalidSuffix, path.ToFileURL).Warning
            End If

            Using s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True)
                Return Save(s)
            End Using
        End Function

        Public Overrides Function Save(out As Stream) As Boolean
            With Layout.Size
                Dim sz$ = $"{ .Width},{ .Height}"
                Return SVG.WriteSVG(out, size:=sz, comments:=XmlComment, title:=title)
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Render() As Image
            Return Renderer.DrawImage(Me)
        End Function
    End Class
End Namespace
