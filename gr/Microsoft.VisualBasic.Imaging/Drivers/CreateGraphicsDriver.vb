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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D

#If NET48 Then
Imports Microsoft.VisualBasic.Drawing
#End If

Namespace Driver

    Public Module ImageDriver

        Private Function handleSVG(d As DeviceDescription, plot As IPlot) As GraphicsData
            Dim device = DriverLoad.UseGraphicsDevice(Drivers.SVG)

            Using g As IGraphics = device.CreateGraphic(d.size, d.background, d.dpi)
                Call g.Clear(g.Background)
                Call plot(g, d.GetRegion)

                Return New SVGData(device, d.size, d.padding)
            End Using
        End Function

        Private Function handlePostScript(g As DeviceDescription, plot As IPlot) As GraphicsData
            Throw New NotImplementedException
        End Function

        Private Function handleWmfVector(g As DeviceDescription, plot As IPlot) As GraphicsData
            Throw New NotImplementedException
        End Function

        Private Function handlePdf(d As DeviceDescription, plot As IPlot) As GraphicsData
            Using g As IGraphics = DriverLoad.CreateGraphicsDevice(d.size, d.bgHtmlColor, d.dpi, driver:=Drivers.PDF)
                Call g.Clear(d.background)
                Call plot(g, d.GetRegion)
                Call g.Flush()
                Throw New NotImplementedException
            End Using
        End Function

        Public Function handleGdiPlusRaster(d As DeviceDescription, plot As IPlot) As GraphicsData
            ' using gdi+ graphics driver
            ' 在这里使用透明色进行填充，防止当bg参数为透明参数的时候被CreateGDIDevice默认填充为白色
            Using g As IGraphics = DriverLoad.CreateGraphicsDevice(d.size, d.bgHtmlColor, d.dpi, driver:=Drivers.GDI)
                Dim rect As New Rectangle(New Point, d.size)
                Call plot(g, d.GetRegion)
                Return New ImageData(DirectCast(g, GdiRasterGraphics).ImageResource, d.size, d.padding)
            End Using
        End Function

        ''' <summary>
        ''' a unify method for create graphics plot
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
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
