#Region "Microsoft.VisualBasic::3b06a55aad325ccd94f12b2ad324a25a, gr\Microsoft.VisualBasic.Imaging\Drawing2D\g.vb"

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

    '   Total Lines: 319
    '    Code Lines: 189 (59.25%)
    ' Comment Lines: 92 (28.84%)
    '    - Xml Docs: 90.22%
    ' 
    '   Blank Lines: 38 (11.91%)
    '     File Size: 13.20 KB


    '     Delegate Sub
    ' 
    ' 
    '     Module g
    ' 
    '         Properties: ActiveDriver, DriverExtensionName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __getDriver, Allocate, CreateGraphics, (+3 Overloads) GraphicsPlots, (+2 Overloads) MeasureSize
    '                   MeasureWidthOrHeight, ParseDriverEnumValue
    ' 
    '         Sub: FillBackground, SetDriver
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.My.FrameworkInternal

#If NET48 Then
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports Microsoft.VisualBasic.Drawing
#End If

Namespace Drawing2D

    ''' <summary>
    ''' An abstract function interface for make graphics plot
    ''' </summary>
    ''' <param name="g">GDI+设备</param>
    ''' <param name="grct">绘图区域的大小</param>
    Public Delegate Sub IPlot(ByRef g As IGraphics, grct As GraphicsRegion)

    ''' <summary>
    ''' Data plots graphics engine common abstract. 
    ''' </summary>
    ''' <remarks>
    ''' (在命令行中使用``graphic_driver=svg``来切换默认的图形引擎为SVG矢量图作图引擎)
    ''' </remarks>
    <FrameworkConfig(GraphicDriverEnvironmentConfigName)>
    Public Module g

        ''' <summary>
        ''' 默认的页边距大小都是100个像素
        ''' </summary>
        Public Const DefaultPadding$ = "padding:100px 100px 100px 100px;"

        ''' <summary>
        ''' 与<see cref="DefaultPadding"/>相比而言，这个padding的值在坐标轴Axis的label的绘制上空间更加大
        ''' </summary>
        Public Const DefaultLargerPadding$ = "padding:100px 100px 150px 150px;"
        Public Const DefaultUltraLargePadding$ = "padding:250px 150px 300px 300px;"

        ''' <summary>
        ''' 所有的页边距都是零
        ''' </summary>
        Public Const ZeroPadding$ = "padding: 0px 0px 0px 0px;"
        Public Const MediumPadding$ = "padding: 45px 45px 45px 45px;"
        Public Const SmallPadding$ = "padding: 30px 30px 30px 30px;"
        Public Const TinyPadding$ = "padding: 5px 5px 5px 5px;"

        Friend Const GraphicDriverEnvironmentConfigName$ = "graphic_driver"

        ''' <summary>
        ''' 在这个模块的构造函数之中，程序会自动根据命令行所设置的环境参数来设置默认的图形引擎
        ''' 
        ''' ```
        ''' /@set graphic_driver=svg|gdi
        ''' ```
        ''' </summary>
        Sub New()
            Dim type$ = Strings.LCase(App.GetVariable(GraphicDriverEnvironmentConfigName))
            Dim defaultDriver = ParseDriverEnumValue(type)

            Driver.DefaultGraphicsDevice([default]:=defaultDriver)

            If VBDebugger.debugMode Then
                Call $"The default graphics driver value is config as {Driver.DefaultGraphicsDevice.Description}({type}).".__INFO_ECHO
            End If
        End Sub

        Public Function ParseDriverEnumValue(str As String) As Drivers
            Dim type$ = Strings.LCase(str)

            Select Case type
                Case "svg" : Return Drivers.SVG
                Case "gdi" : Return Drivers.GDI
                Case "ps" : Return Drivers.PS
                Case "wmf" : Return Drivers.WMF
                Case "pdf" : Return Drivers.PDF
                Case Else
                    Return Drivers.Default
            End Select
        End Function

        ''' <summary>
        ''' Get the result from commandline environment variable: ``graphic_driver``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ActiveDriver As Drivers
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Driver.DefaultGraphicsDevice
            End Get
        End Property

        ''' <summary>
        ''' Get the image file extension name for current default image driver.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DriverExtensionName As String
            Get
                Select Case ActiveDriver
                    Case Drivers.SVG : Return "svg"
                    Case Drivers.GDI, Drivers.Default
                        Return "png"
                    Case Drivers.PS : Return "ps"
                    Case Drivers.WMF : Return "wmf"
                    Case Drivers.PDF : Return "pdf"
                    Case Else
                        Throw New NotImplementedException(ActiveDriver.Description)
                End Select
            End Get
        End Property

        ''' <summary>
        ''' 这个函数不会返回<see cref="Drivers.Default"/>
        ''' </summary>
        ''' <param name="developerValue">程序开发人员所设计的驱动程序的值</param>
        ''' <returns></returns>
        Private Function __getDriver(developerValue As Drivers) As Drivers
            If developerValue <> Drivers.Default Then
                Return developerValue
            Else
                If Driver.DefaultGraphicsDevice = Drivers.Default Then
                    ' 默认为使用gdi引擎
                    Return Drivers.GDI
                Else
                    Return Driver.DefaultGraphicsDevice
                End If
            End If
        End Function

        ''' <summary>
        ''' 在代码中手动配置默认的驱动程序
        ''' </summary>
        ''' <param name="driver"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetDriver(driver As Drivers)
            Call Microsoft.VisualBasic.Imaging.Driver.DefaultGraphicsDevice(driver)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function MeasureWidthOrHeight(wh#, length%) As Single
            If wh > 0 AndAlso wh <= 1 Then
                Return wh * length
            Else
                Return wh
            End If
        End Function

        ReadOnly defaultSize As [Default](Of Size) = New Size(3600, 2000).AsDefault(Function(size) DirectCast(size, Size).IsEmpty)
        ReadOnly defaultPaddingValue As [Default](Of Padding) = CType(DefaultPadding, Padding).AsDefault(Function(pad) DirectCast(pad, Padding).IsEmpty)

        <Extension>
        Public Function GraphicsPlots(base As GraphicsData, plot As IPlot) As GraphicsData
            Dim region As GraphicsRegion = base.Layout

            Select Case base.Driver
                Case Drivers.GDI
                    Using g As IGraphics = Driver.CreateGraphicsDevice(base.AsGDIImage, driver:=Drivers.GDI)
                        Dim rect As New Rectangle(New Point, g.Size)
#If NET48 Then
                        If TypeOf g Is Graphics2D Then
                            With DirectCast(g, Graphics2D).Graphics
                                .CompositingQuality = CompositingQuality.HighQuality
                                .CompositingMode = CompositingMode.SourceOver
                                .InterpolationMode = InterpolationMode.HighQualityBicubic
                                .PixelOffsetMode = PixelOffsetMode.HighQuality
                                .SmoothingMode = SmoothingMode.HighQuality
                                .TextRenderingHint = TextRenderingHint.ClearTypeGridFit
                            End With
                        End If
#End If
                        Call plot(g, region)

                        Return New ImageData(DirectCast(g, GdiRasterGraphics).ImageResource, region.Size, region.Padding)
                    End Using
                Case Drivers.SVG
                    Throw New NotImplementedException
                Case Else
                    Throw New NotImplementedException(base.Driver.ToString)
            End Select
        End Function

        ''' <summary>
        ''' Data plots graphics engine. Default: <paramref name="size"/>:=(4300, 2000), <paramref name="padding"/>:=(100,100,100,100).
        ''' (用户可以通过命令行设置环境变量``graphic_driver``来切换图形引擎)
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="padding">页边距</param>
        ''' <param name="bg">颜色值或者图片资源文件的url或者文件路径</param>
        ''' <param name="plotAPI"></param>
        ''' <param name="driver">驱动程序是默认与当前的环境参数设置相关的</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GraphicsPlots(ByRef size As Size,
                                      ByRef padding As Padding,
                                      bg$,
                                      plotAPI As IPlot,
                                      Optional driver As Drivers = Drivers.Default,
                                      Optional dpi As Integer = 100) As GraphicsData

            Dim driverUsed As Drivers = g.__getDriver(developerValue:=driver)

            size = size Or defaultSize
            ' 20221211 default config will makes the zero-padding
            ' invalid, so we must removes this line
            '
            ' padding = padding Or defaultPaddingValue

            Return New DeviceDescription(bg) With {
                .dpi = dpi,
                .driverUsed = driverUsed,
                .padding = padding,
                .size = size
            }.GraphicsPlot(plotAPI)
        End Function

        ''' <summary>
        ''' 自动根据表达式的类型来进行纯色绘制或者图形纹理画刷绘制
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="bg$">
        ''' 1. 可能为颜色表达式
        ''' 2. 可能为图片的路径
        ''' 3. 可能为base64图片字符串
        ''' </param>
        <Extension>
        Public Sub FillBackground(ByRef g As IGraphics, bg$, rect As Rectangle)
            Dim bgColor As Color = bg.TranslateColor(throwEx:=False)

            If Not bgColor.IsEmpty Then
                Call g.FillRectangle(New SolidBrush(bgColor), rect)
            Else
                ' If the bg is not a file, then will try decode it as base64 string image. 
                Dim res As Image = bg.LoadImage(base64:=Not bg.FileExists)
                Call g.DrawImage(res, rect)
            End If
        End Sub

        ''' <summary>
        ''' <see cref="Graphics.MeasureString(String, Font)"/> extensions
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="g"></param>
        ''' <param name="font"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function MeasureSize(text$, g As IGraphics, font As Font) As SizeF
            Return g.MeasureString(text, font)
        End Function

        ''' <summary>
        ''' <see cref="Graphics.MeasureString(String, Font)"/> extensions
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="g"></param>
        ''' <param name="font"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function MeasureSize(text$, g As IGraphics, font As Font, scale As (x#, y#)) As SizeF
            Dim size As SizeF

            g.ScaleTransform(scale.x, scale.y)
            size = g.MeasureString(text, font)
            g.ScaleTransform(1, 1)

            Return size
        End Function

        ''' <summary>
        ''' Data plots graphics engine.
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="bg"></param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GraphicsPlots(plot As Action(Of IGraphics),
                                      ByRef size As Size,
                                      ByRef padding As Padding,
                                      bg$) As GraphicsData

            Return GraphicsPlots(size, padding, bg, Sub(ByRef g, rect) Call plot(g))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Allocate(Optional size As Size = Nothing,
                                 Optional padding$ = DefaultPadding,
                                 Optional bg$ = "white") As InternalCanvas

            Return New InternalCanvas With {
                .size = size,
                .bg = bg,
                .padding = padding
            }
        End Function

        <Extension>
        Public Function CreateGraphics(img As GraphicsData) As IGraphics
            If img.Driver = Drivers.SVG Then
                Dim svg = DirectCast(img, SVGData).SVG
                Dim g As New GraphicsSVG(svg, 300, 300)

                Return g
            Else
                Return Driver.CreateGraphicsDevice(DirectCast(img, ImageData).Image, driver:=img.Driver)
            End If
        End Function
    End Module
End Namespace
