#Region "Microsoft.VisualBasic::a9c1da0ffdf49734bc8c4ea805201973, gr\Microsoft.VisualBasic.Imaging\Drivers\CreateGraphicsDriver.vb"

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

'   Total Lines: 99
'    Code Lines: 72 (72.73%)
' Comment Lines: 2 (2.02%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 25 (25.25%)
'     File Size: 3.78 KB


'     Module ImageDriver
' 
'         Function: GraphicsPlot, handleGdiPlusRaster, handlePdf, handlePostScript, handleSVG
'                   handleWmfVector
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.PostScript
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.MIME.Html.CSS

<Assembly: InternalsVisibleTo("Microsoft.VisualBasic.Imaging.PDF")>

Namespace Driver

    Public Module ImageDriver

        Friend pdfDriver As Func(Of Size, IGraphics)
        Friend getPdfImage As Func(Of IGraphics, Size, Padding, GraphicsData)

        Public Delegate Function HookGraphicsDrawingDevice(desc As DeviceDescription, plot As IPlot) As GraphicsData
        Public Delegate Function OpenGraphicsDevice(size As Size, background As Color, dpi As Integer) As IGraphics

        Dim raster As OpenGraphicsDevice
        Dim svg As OpenGraphicsDevice
        Dim pdf As OpenGraphicsDevice

        Sub New()
#If NET48 Then
            raster = AddressOf Drawing.Drawing.OpenGraphicsDevice
#End If
        End Sub

        Private Function handleSVG(g As DeviceDescription, plot As IPlot) As GraphicsData
            Dim dpiXY = g.dpi
            Dim svg As New GraphicsSVG(g.size, dpiXY.Width, dpiXY.Height)

            Call svg.Clear(g.background)
            Call plot(svg, g.GetRegion)

            Return New SVGData(svg, g.size, g.padding)
        End Function

        Private Function handlePostScript(g As DeviceDescription, plot As IPlot) As GraphicsData
            Dim dpiXY = g.dpi
            Dim ps As New GraphicsPS(g.size, dpiXY)

            Throw New NotImplementedException
        End Function

        Private Function handleWmfVector(g As DeviceDescription, plot As IPlot) As GraphicsData
            Dim wmfstream As New MemoryStream
            Dim dpiXY = g.dpi

            Using wmf As New Wmf(g.size, wmfstream, g.bgHtmlColor, dpi:=dpiXY)
                Call plot(wmf, g.GetRegion)
                Call wmf.Flush()
            End Using

            Return New WmfData(wmfstream, g.size, g.padding)
        End Function

        Private Function handlePdf(d As DeviceDescription, plot As IPlot) As GraphicsData
            Dim g As IGraphics = pdfDriver(d.size)

            Call g.Clear(d.background)
            Call plot(g, d.GetRegion)
            Call g.Flush()

            Return getPdfImage(g, d.size, d.padding)
        End Function

        Public Function handleGdiPlusRaster(d As DeviceDescription, plot As IPlot) As GraphicsData
            Dim dpi As String = $"{d.dpi.Width},{d.dpi.Height}"

            ' using gdi+ graphics driver
            ' 在这里使用透明色进行填充，防止当bg参数为透明参数的时候被CreateGDIDevice默认填充为白色
            Using g As IGraphics = raster(d.size, d.bgHtmlColor.TranslateColor, dpi:=dpi)
                Dim rect As New Rectangle(New Point, d.size)
                Call plot(g, d.GetRegion)
#If NET48 Then
                Return New ImageData(DirectCast(g, Graphics2D).ImageResource, d.size, d.padding)
#Else
                Throw New NotImplementedException
#End If
            End Using
        End Function

        <Extension>
        Public Function GraphicsPlot(g As DeviceDescription, plot As IPlot) As GraphicsData
            Select Case g.driverUsed
                Case Drivers.SVG : Return handleSVG(g, plot)
                Case Drivers.PS : Return handlePostScript(g, plot)
                Case Drivers.WMF : Return handleWmfVector(g, plot)
                Case Drivers.PDF : Return handlePdf(g, plot)

                Case Else
                    Return handleGdiPlusRaster(g, plot)
            End Select
        End Function
    End Module
End Namespace
