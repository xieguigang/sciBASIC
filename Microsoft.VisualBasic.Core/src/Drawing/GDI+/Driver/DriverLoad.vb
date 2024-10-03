Imports System.Drawing
Imports System.Runtime.CompilerServices

#If NET48 Then
#Else
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
#End If

Namespace Imaging.Driver

    Public Delegate Function CreateGraphic(size As Size, fill As Color, dpi As Integer) As IGraphics
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="background"></param>
    ''' <param name="direct_access">
    ''' create the graphics canvas directly based on the input background image resource if set this parameter to true, 
    ''' or make a copy of the image and then create the graphics canvas if set this parameter false.
    ''' </param>
    ''' <returns></returns>
    Public Delegate Function CreateCanvas2D(background As Bitmap, direct_access As Boolean) As IGraphics

    Public Module DriverLoad

        Public libgdiplus_raster As CreateGraphic
        Public svg As CreateGraphic
        Public pdf As CreateGraphic

        Public libgdiplus_canvas As CreateCanvas2D
        Public svg_canvas As CreateCanvas2D
        Public pdf_canvas As CreateCanvas2D

        Sub New()
        End Sub

        ''' <summary>
        ''' 用户所指定的图形引擎驱动程序类型，但是这个值会被开发人员设定的驱动程序类型的值所覆盖，
        ''' 通常情况下，默认引擎选用的是``gdi+``引擎
        ''' </summary>
        Public Function DefaultGraphicsDevice(Optional [default] As Drivers? = Nothing) As Drivers
            Static __default As Drivers = Drivers.GDI

            If Not [default] Is Nothing Then
                __default = [default]
            End If

            Return __default
        End Function

        Public Function CreateGraphicsDevice(background As Bitmap,
                                             Optional direct_access As Boolean = True,
                                             Optional driver As Drivers = Drivers.Default) As IGraphics
            If driver = Drivers.Default Then
                driver = DefaultGraphicsDevice()
            End If

            Select Case driver
                Case Drivers.GDI : Return libgdiplus_canvas(background, direct_access)
                Case Drivers.PDF : Return pdf_canvas(background, direct_access)
                Case Drivers.SVG : Return svg_canvas(background, direct_access)
                Case Else
                    Throw New NotImplementedException(driver.Description)
            End Select
        End Function

        Public Function CreateGraphicsDevice(size As Size, fill_color As Color,
                                             Optional dpi As Integer = 100,
                                             Optional driver As Drivers = Drivers.Default) As IGraphics

            If driver = Drivers.Default Then
                driver = DefaultGraphicsDevice()
            End If

            If svg Is Nothing OrElse pdf Is Nothing OrElse libgdiplus_raster Is Nothing Then

            End If

            Select Case driver
                Case Drivers.SVG : Return svg(size, fill_color, dpi)
                Case Drivers.PDF : Return pdf(size, fill_color, dpi)
                Case Drivers.GDI : Return libgdiplus_raster(size, fill_color, dpi)
                Case Else
                    Throw New NotImplementedException(driver.Description)
            End Select
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