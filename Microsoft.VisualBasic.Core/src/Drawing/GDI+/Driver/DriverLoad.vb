#Region "Microsoft.VisualBasic::82c8423344d9eb9b1b81b5e03cf5d4c8, Microsoft.VisualBasic.Core\src\Drawing\GDI+\Driver\DriverLoad.vb"

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

    '   Total Lines: 190
    '    Code Lines: 147 (77.37%)
    ' Comment Lines: 11 (5.79%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 32 (16.84%)
    '     File Size: 8.63 KB


    '     Module DriverLoad
    ' 
    '         Properties: CheckRasterImageLoader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateDefaultRasterGraphics, (+4 Overloads) CreateGraphicsDevice, DefaultGraphicsDevice, GetData, LoadFromStream
    '                   MeasureTextSize, UseGraphicsDevice
    ' 
    '         Sub: (+3 Overloads) Register
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices

Namespace Imaging.Driver

    Public Module DriverLoad

        Dim libgdiplus_raster As DeviceInterop
        Dim svg As DeviceInterop
        Dim pdf As DeviceInterop
        Dim ps As DeviceInterop
        Dim loadImage As Func(Of Stream, Image)
        Dim measureString As Func(Of String, Font, SizeF)

        Public ReadOnly Property CheckRasterImageLoader As Boolean
            Get
                Return Not loadImage Is Nothing
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub Register(interop As DeviceInterop, driver As Drivers)
            Select Case driver
                Case Drivers.GDI : libgdiplus_raster = interop
                Case Drivers.PDF : pdf = interop
                Case Drivers.SVG : svg = interop
                Case Drivers.PostScript : ps = interop
                Case Else
                    Throw New NotSupportedException(driver.Description)
            End Select
        End Sub

        Public Sub Register(loader As Func(Of Stream, Image))
            loadImage = loader
        End Sub

        Public Sub Register(measure As Func(Of String, Font, SizeF))
            measureString = measure
        End Sub

        ''' <summary>
        ''' load image from stream data
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function LoadFromStream(s As Stream) As Image
            If loadImage Is Nothing Then
                Throw New InvalidProgramException("missing image loader driver!")
            Else
                Return loadImage(s)
            End If
        End Function

        Public Function MeasureTextSize(text As String, font As Font) As SizeF
            If measureString Is Nothing Then
                Throw New InvalidProgramException("missing text size measurement driver!")
            Else
                Return measureString(text, font)
            End If
        End Function

        ''' <summary>
        ''' 用户所指定的图形引擎驱动程序类型，但是这个值会被开发人员设定的驱动程序类型的值所覆盖，
        ''' 通常情况下，默认引擎选用的是``gdi+``引擎
        ''' </summary>
        Public Function DefaultGraphicsDevice(Optional [default] As Drivers? = Nothing) As Drivers
            Static __default As Drivers = Drivers.GDI

            ' 20241015 the initialization of the static __default to gdi as default is not working
            ' config of the __default manually at here
            If __default = Drivers.Default AndAlso [default] Is Nothing Then
                __default = Drivers.GDI
            End If

            If Not [default] Is Nothing Then
                __default = [default]
            End If

            Return __default
        End Function

        Public Function GetData(g As IGraphics, padding As Integer()) As IGraphicsData
            Return UseGraphicsDevice(g.Driver).GetData(g, padding)
        End Function

        Public Function CreateGraphicsDevice(background As Image,
                                             Optional direct_access As Boolean = True,
                                             Optional driver As Drivers = Drivers.Default) As IGraphics
            If driver = Drivers.Default Then
                driver = DefaultGraphicsDevice()
            End If

            Return UseGraphicsDevice(driver).CreateCanvas2D(background, direct_access)
        End Function

        Public Function CreateGraphicsDevice(background As Bitmap,
                                             Optional direct_access As Boolean = True,
                                             Optional driver As Drivers = Drivers.Default) As IGraphics
            If driver = Drivers.Default Then
                driver = DefaultGraphicsDevice()
            End If

            Return UseGraphicsDevice(driver).CreateCanvas2D(background, direct_access)
        End Function

        Const skia_driver = "A skiasharp graphics driver was wrapped by the Microsoft.VisualBasic.Drawing(https://github.com/xieguigang/Microsoft.VisualBasic.Drawing) project, call the method at the very begining of your program startup: Microsoft.VisualBasic.Drawing.SkiaDriver.Register()."
        Const windows_gdi_driver = "The internal windows gdi+ graphics driver must be registered before calling the corresponding graphics code, call this method at the very begining of your program startup: Microsoft.VisualBasic.Imaging.Driver.ImageDriver.Register(); you can find this method in module: Microsoft.VisualBasic.Imaging.dll"

        Const missing_svg = "Missing the graphics device driver for svg graphics, you should register the corresponding graphics driver at first!" & vbCrLf
        Const missing_pdf = "Missing the graphics device driver for pdf drawing, you should register the corresponding pdf drawing driver at first!" & vbCrLf
        Const missing_gdi = "Missing the raster graphics driver for the image drawing, you should register the corresponding raster rendering driver at first!" & vbCrLf
        Const missing_ps = "Missing the graphics driver for the postscript generates, you should register the corresponding postscript rendering driver at first!" & vbCrLf

        Public Function UseGraphicsDevice(driver As Drivers) As DeviceInterop
            If driver = Drivers.Default Then
                driver = DefaultGraphicsDevice()
            End If

            Select Case driver
                Case Drivers.SVG
                    If svg Is Nothing Then
#If NET48 Then
                        Throw New MissingMethodException(missing_svg & windows_gdi_driver)
#Else
                        Throw New MissingMethodException(missing_svg & skia_driver)
#End If
                    End If

                    Return svg
                Case Drivers.PDF
                    If pdf Is Nothing Then
#If NET48 Then
                        Throw New NotSupportedException("Sorry, pdf graphics drawing is not yet supported for .NET Framework 4.8 application currently.")
#Else
                        Throw New MissingMethodException(missing_pdf & skia_driver)
#End If
                    End If

                    Return pdf
                Case Drivers.GDI
                    If libgdiplus_raster Is Nothing Then
#If NET48 Then
                        Throw New MissingMethodException(missing_gdi & windows_gdi_driver)
#Else
#If WINDOWS Then
                        Throw New MissingMethodException(missing_gdi & skia_driver & vbCrLf & "Or " & windows_gdi_driver)
#Else
                        Throw New MissingMethodException(missing_gdi & skia_driver)
#End If
#End If
                    End If

                    Return libgdiplus_raster
                Case Drivers.PostScript
                    If ps Is Nothing Then
                        Throw New MissingMethodException(missing_ps & windows_gdi_driver)
                    End If

                    Return ps
                Case Else
                    Throw New NotImplementedException(driver.Description)
            End Select
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateDefaultRasterGraphics(size As Size, fill_color As Color, Optional dpi As Integer = 100) As IGraphics
            Return UseGraphicsDevice(Drivers.GDI).CreateGraphic(size, fill_color, dpi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateGraphicsDevice(size As Size, fill_color As Color,
                                             Optional dpi As Integer = 100,
                                             Optional driver As Drivers = Drivers.Default) As IGraphics

            Return UseGraphicsDevice(driver).CreateGraphic(size, fill_color, dpi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateGraphicsDevice(size As Size,
                                             Optional fill As String = NameOf(Color.Transparent),
                                             Optional dpi As Integer = 100,
                                             Optional driver As Drivers = Drivers.Default) As IGraphics

            Return CreateGraphicsDevice(size, fill.TranslateColor, dpi, driver:=driver)
        End Function
    End Module
End Namespace
