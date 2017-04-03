Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.SVG

Namespace Driver

    Public MustInherit Class GraphicsData

        ''' <summary>
        ''' 驱动程序的类型
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Driver As Drivers
        Public ReadOnly Property Size As Size

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="img">其实这个参数在基类<see cref="GraphicsData"/>之中是无用的，只是为了统一接口而设置的</param>
        ''' <param name="size"></param>
        Sub New(img As Object, size As Size)
            size = size
        End Sub

        Public MustOverride Function Save(path$) As Boolean

    End Class

    Public Class ImageData : Inherits GraphicsData

        Dim image As Drawing.Image

        Public Sub New(img As Object, size As Size)
            MyBase.New(img, size)

            If img.GetType() Is GetType(Bitmap) Then
                image = CType(DirectCast(img, Bitmap), Drawing.Image)
            Else
                image = DirectCast(img, Drawing.Image)
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
            Return image.SaveAs(path, ImageData.DefaultFormat)
        End Function
    End Class

    Public Class SVGData : Inherits GraphicsData

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
            With Size
                Dim sz$ = $"{ .Width},{ .Height}"
                Return engine.WriteSVG(path, sz)
            End With
        End Function
    End Class
End Namespace