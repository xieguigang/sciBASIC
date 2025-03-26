#Region "Microsoft.VisualBasic::e480d5cd7d67dc8e34972cf9bfc5f304, gr\Microsoft.VisualBasic.Imaging\Drivers\CreateGraphicsDriver.vb"

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

    '   Total Lines: 142
    '    Code Lines: 105 (73.94%)
    ' Comment Lines: 9 (6.34%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 28 (19.72%)
    '     File Size: 6.11 KB


    '     Module ImageDriver
    ' 
    '         Function: CheckElementWriter, GraphicsPlot
    ' 
    '         Sub: Register, RegisterPostScript
    '         Class RasterInterop
    ' 
    '             Function: (+2 Overloads) CreateCanvas2D, CreateGraphic, GetData
    ' 
    '         Class SvgInterop
    ' 
    '             Function: (+2 Overloads) CreateCanvas2D, CreateGraphic, GetData
    ' 
    '         Class PostScriptInterop
    ' 
    '             Function: (+2 Overloads) CreateCanvas2D, CreateGraphic, GetData
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.PostScript
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.MIME.Html.CSS

#If NET48 Then
Imports Microsoft.VisualBasic.Drawing
#End If

Namespace Driver

    Public Module ImageDriver

        <Extension>
        Public Function CheckElementWriter(g As IGraphics) As IElementCommentWriter
            Dim context As Object = g.GetContextInfo

            If context IsNot Nothing Then
                If context.GetType.ImplementInterface(Of IElementCommentWriter) Then
                    Return DirectCast(context, IElementCommentWriter)
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' a unify method for create graphics plot
        ''' </summary>
        ''' <param name="desc">the graphics drawing context description</param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GraphicsPlot(desc As DeviceDescription, plot As IPlot) As GraphicsData
            Dim device = DriverLoad.UseGraphicsDevice(desc.driverUsed)
            Dim css As New CSSEnvirnment(desc.size)

            Using g As IGraphics = device.CreateGraphic(desc.size, desc.background, desc.dpi)
                Call g.Clear(desc.background)
                Call plot(g, desc.GetRegion)

                Return device.GetData(g, PaddingLayout.EvaluateFromCSS(css, desc.padding))
            End Using
        End Function

        Public Sub RegisterPostScript()
            Call DriverLoad.Register(New PostScriptInterop, Drivers.PostScript)
        End Sub

#If NET48 Then

        ''' <summary>
        ''' register the default System.Drawing.Common graphics driver for .net 4.8 runtime
        ''' </summary>
        Public Sub Register()
            Static gfx As Graphics = Graphics.FromImage(New Bitmap(10, 10))

            Call DriverLoad.Register(New RasterInterop, Drivers.GDI)
            Call DriverLoad.Register(New SvgInterop, Drivers.SVG)
            Call DriverLoad.Register(Function(text As String, font As Font)
                                         Return gfx.MeasureString(text, font)
                                     End Function)
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

            Public Overrides Function CreateCanvas2D(background As Image, direct_access As Boolean) As IGraphics
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

            Public Overrides Function CreateCanvas2D(background As Image, direct_access As Boolean) As IGraphics
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

        Private Class PostScriptInterop : Inherits DeviceInterop

            Public Overrides Function CreateGraphic(size As Size, fill As Color, dpi As Integer) As IGraphics
                Dim g As New GraphicsPostScript(size, New Size(dpi, dpi))
                g.Clear(fill)
                Return g
            End Function

            Public Overrides Function CreateCanvas2D(background As Bitmap, direct_access As Boolean) As IGraphics
                Return CreateCanvas2D(CType(background, Image), direct_access)
            End Function

            Public Overrides Function CreateCanvas2D(background As Image, direct_access As Boolean) As IGraphics
                Dim g As New GraphicsPostScript(background.Size, New Size(100, 100))
                Call g.DrawImageUnscaled(background, New Point)
                Return g
            End Function

            Public Overrides Function GetData(g As IGraphics, padding() As Integer) As IGraphicsData
                Return New PostScriptData(g.GetContextInfo, g.Size, New Padding(padding))
            End Function
        End Class
    End Module
End Namespace
