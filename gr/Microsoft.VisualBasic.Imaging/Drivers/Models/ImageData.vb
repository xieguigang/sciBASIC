Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices

Namespace Driver

    ''' <summary>
    ''' Get image value from <see cref="ImageData.Image"/>
    ''' </summary>
    Public Class ImageData : Inherits GraphicsData

        ''' <summary>
        ''' GDI+ image
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Image As Drawing.Image

        Public Sub New(img As Object, size As Size)
            MyBase.New(img, size)

            If img.GetType() Is GetType(Bitmap) Then
                Image = CType(DirectCast(img, Bitmap), Drawing.Image)
            Else
                Image = DirectCast(img, Drawing.Image)
            End If
        End Sub

        Sub New(image As System.Drawing.Image)
            Call Me.New(image, image.Size)
        End Sub

        Sub New(bitmap As Bitmap)
            Call Me.New(bitmap, bitmap.Size)
        End Sub

        ''' <summary>
        ''' Default image save format for <see cref="Bitmap"/>
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property DefaultFormat As ImageFormats = ImageFormats.Png

        Public Overrides ReadOnly Property Driver As Drivers
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Drivers.GDI
            End Get
        End Property

        Const InvalidSuffix$ = "The gdi+ image file save path: {0} ending with *.svg file extension suffix!"

        ''' <summary>
        ''' Save the image as png
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Overrides Function Save(path As String) As Boolean
            If path.ExtensionSuffix.TextEquals("svg") Then
                Call String.Format(InvalidSuffix, path.ToFileURL).Warning
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

        ''' <summary>
        ''' 当进行连续绘图操作的时候，如果不释放image的内存会导致内存泄漏？？？
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overrides Sub Dispose(disposing As Boolean)
            MyBase.Dispose(disposing)

            If Not Image Is Nothing Then
                Call Image.Dispose()
            End If
        End Sub
    End Class
End Namespace