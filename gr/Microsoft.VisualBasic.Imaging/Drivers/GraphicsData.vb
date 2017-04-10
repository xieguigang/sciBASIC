Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.Imaging.SVG.XML

Namespace Driver

    ''' <summary>
    ''' gdi+ images: <see cref="Drawing.Image"/>, <see cref="Bitmap"/> / SVG image: <see cref="SVGXml"/>
    ''' </summary>
    Public MustInherit Class GraphicsData

        ''' <summary>
        ''' The graphics engine driver type indicator, 
        ''' 
        ''' + for <see cref="Drivers.GDI"/> -> <see cref="ImageData"/>(<see cref="Drawing.Image"/>, <see cref="Bitmap"/>)
        ''' + for <see cref="Drivers.SVG"/> -> <see cref="SVGData"/>(<see cref="SVGXml"/>)
        ''' 
        ''' (驱动程序的类型)
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Driver As Drivers
        Public ReadOnly Property Size As Size

        Public ReadOnly Property Width As Integer
            Get
                Return Size.Width
            End Get
        End Property

        Public ReadOnly Property Height As Integer
            Get
                Return Size.Height
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="img">其实这个参数在基类<see cref="GraphicsData"/>之中是无用的，只是为了统一接口而设置的</param>
        ''' <param name="size"></param>
        Sub New(img As Object, size As Size)
            Me.Size = size
        End Sub

        Public MustOverride Function Save(path$) As Boolean
        Public MustOverride Function Save(out As Stream) As Boolean

    End Class

    ''' <summary>
    ''' Get image value from <see cref="ImageData.Image"/>
    ''' </summary>
    Public Class ImageData : Inherits GraphicsData

        Public ReadOnly Property Image As Drawing.Image

        Public Sub New(img As Object, size As Size)
            MyBase.New(img, size)

            If img.GetType() Is GetType(Bitmap) Then
                Image = CType(DirectCast(img, Bitmap), Drawing.Image)
            Else
                Image = DirectCast(img, Drawing.Image)
            End If
        End Sub

        ''' <summary>
        ''' Default image save format for <see cref="Bitmap"/>
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property DefaultFormat As ImageFormats = ImageFormats.Png

        Public Overrides ReadOnly Property Driver As Drivers
            Get
                Return Drivers.GDI
            End Get
        End Property

        Public Overrides Function Save(path As String) As Boolean
            If path.ExtensionSuffix.TextEquals("svg") Then
                Call $"The gdi+ image file save path: {path.ToFileURL} ending with *.svg file extension suffix!".Warning
            End If
            Return Image.SaveAs(path, ImageData.DefaultFormat)
        End Function

        Public Overrides Function Save(out As Stream) As Boolean
            Try
                Call Image.Save(out, DefaultFormat.GetFormat)
            Catch ex As Exception
                Call App.LogException(ex)
                Return False
            End Try

            Return True
        End Function
    End Class

    Public Class SVGData : Inherits GraphicsData

        Friend ReadOnly Property SVG As SVGDataCache
            Get
                Return engine.__svgData
            End Get
        End Property

        Dim engine As GraphicsSVG

        Public Sub New(img As Object, size As Size)
            MyBase.New(img, size)
            Me.engine = img
        End Sub

        Public Overrides ReadOnly Property Driver As Drivers
            Get
                Return Drivers.SVG
            End Get
        End Property

        Public Overrides Function Save(path As String) As Boolean
            If Not path.ExtensionSuffix.TextEquals("svg") Then
                Call $"The SVG image file save path: {path.ToFileURL} not ending with *.svg file extension suffix!".Warning
            End If

            With Size
                Dim sz$ = $"{ .Width},{ .Height}"
                Return engine.WriteSVG(path, sz)
            End With
        End Function

        Public Overrides Function Save(out As Stream) As Boolean
            With Size
                Dim sz$ = $"{ .Width},{ .Height}"
                Return engine.WriteSVG(out, size:=sz)
            End With
        End Function
    End Class
End Namespace