#Region "Microsoft.VisualBasic::fc06405674e7a31603603ff49acdbf3c, gr\Microsoft.VisualBasic.Imaging\Drivers\CreateGraphicsDriver.vb"

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

    '   Total Lines: 80
    '    Code Lines: 59 (73.75%)
    ' Comment Lines: 6 (7.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (18.75%)
    '     File Size: 3.26 KB


    '     Module ImageDriver
    ' 
    '         Function: GraphicsPlot
    ' 
    '         Sub: Register
    '         Class RasterInterop
    ' 
    '             Function: CreateCanvas2D, CreateGraphic, GetData
    ' 
    '         Class SvgInterop
    ' 
    '             Function: CreateCanvas2D, CreateGraphic, GetData
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.MIME.Html.CSS

#If NET48 Then
Imports Microsoft.VisualBasic.Drawing
#End If

Namespace Driver

    Public Module ImageDriver

        ''' <summary>
        ''' a unify method for create graphics plot
        ''' </summary>
        ''' <param name="desc">the graphics drawing context description</param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GraphicsPlot(desc As DeviceDescription, plot As IPlot) As GraphicsData
            Dim device = DriverLoad.UseGraphicsDevice(desc.driverUsed)

            Using g As IGraphics = device.CreateGraphic(desc.size, desc.background, desc.dpi)
                Call g.Clear(desc.background)
                Call plot(g, desc.GetRegion)

                Return device.GetData(g, desc.padding)
            End Using
        End Function

#If NET48 Then
        Public Sub Register()
            Call DriverLoad.Register(New RasterInterop, Drivers.GDI)
            Call DriverLoad.Register(New SvgInterop, Drivers.SVG)
        End Sub

        Private Class RasterInterop : Inherits DeviceInterop

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Overrides Function CreateGraphic(size As Size, fill As Color, dpi As Integer) As IGraphics
                Return Drawing.CreateGDIDevice(size.Width, size.Height,
                                              filled:=fill,
                                              dpi:=$"{dpi},{dpi}")
            End Function

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Overrides Function CreateCanvas2D(background As Bitmap, direct_access As Boolean) As IGraphics
                Return background.CreateCanvas2D(direct_access)
            End Function

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Overrides Function GetData(g As IGraphics, padding() As Integer) As IGraphicsData
                Return New ImageData(DirectCast(g, Graphics2D).ImageResource, g.Size, New Padding(padding))
            End Function
        End Class

        Private Class SvgInterop : Inherits DeviceInterop

            Public Overrides Function CreateGraphic(size As Size, fill As Color, dpi As Integer) As IGraphics
                Dim svg As New GraphicsSVG(size, dpi, dpi)
                svg.Clear(fill)
                Return svg
            End Function

            Public Overrides Function CreateCanvas2D(background As Bitmap, direct_access As Boolean) As IGraphics
                Dim svg As New GraphicsSVG(background.Size, 100, 100)
                svg.DrawImageUnscaled(background, New Point)
                Return svg
            End Function

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Overrides Function GetData(g As IGraphics, padding() As Integer) As IGraphicsData
                Return New SVGData(g, New Padding(padding))
            End Function
        End Class
#End If
    End Module
End Namespace
