#Region "Microsoft.VisualBasic::9784272b77744c87faf227e87334da8e, Microsoft.VisualBasic.Core\src\Drawing\GDI+\GdiRasterGraphics.vb"

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

    '   Total Lines: 82
    '    Code Lines: 39 (47.56%)
    ' Comment Lines: 26 (31.71%)
    '    - Xml Docs: 88.46%
    ' 
    '   Blank Lines: 17 (20.73%)
    '     File Size: 2.46 KB


    '     Interface GdiRasterGraphics
    ' 
    '         Properties: ImageResource
    ' 
    '     Interface ISvgGraphics
    ' 
    '         Function: GetSvgText
    ' 
    '     Class IGraphicsData
    ' 
    '         Properties: content_type
    ' 
    '         Function: Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Net.Http

#If NET48 Then
Imports System.Drawing
#Else
Imports Microsoft.VisualBasic.Imaging
#End If

Namespace Imaging.Driver

    ''' <summary>
    ''' 
    ''' </summary>
    Public Interface GdiRasterGraphics

        ReadOnly Property ImageResource As Image

    End Interface

    Public Interface ISvgGraphics

        Function GetSvgText() As String

    End Interface

    Public MustInherit Class IGraphicsData

        ''' <summary>
        ''' The graphics engine driver type indicator, 
        ''' 
        ''' + for <see cref="Drivers.GDI"/> -> ImageData(<see cref="Image"/>, <see cref="Bitmap"/>)
        ''' + for <see cref="Drivers.SVG"/> -> SVGData(SVGDocument)
        ''' 
        ''' (驱动程序的类型)
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Driver As Drivers

        ''' <summary>
        ''' http content type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property content_type As String
            Get
                Select Case Driver
                    Case Drivers.GDI : Return "image/png"
                    Case Drivers.PostScript : Return "application/postscript"
                    Case Drivers.SVG : Return "image/svg+xml"
                    Case Drivers.WMF : Return "application/x-wmf"

                    Case Else
                        Return "application/octet-stream"
                End Select
            End Get
        End Property

        Public MustOverride ReadOnly Property Width As Integer
        Public MustOverride ReadOnly Property Height As Integer

        Public MustOverride Function GetDataURI() As DataURI

        ''' <summary>
        ''' Save the image graphics to file
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        Public Overridable Function Save(path$) As Boolean
            Using s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True)
                Return Save(s)
            End Using
        End Function

        ''' <summary>
        ''' Save the image graphics to a specific output stream
        ''' </summary>
        ''' <param name="out"></param>
        ''' <returns></returns>
        Public MustOverride Function Save(out As Stream) As Boolean

    End Class
End Namespace
