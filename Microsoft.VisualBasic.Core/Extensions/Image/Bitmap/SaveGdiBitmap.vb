Imports System.Drawing
Imports System.Drawing.Imaging

Namespace Imaging.BitmapImage

    Public Interface SaveGdiBitmap

        ''' <summary>
        ''' <see cref="Image.Save"/>
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <param name="format"></param>
        Function Save(stream As IO.Stream, format As ImageFormat) As Boolean
    End Interface
End Namespace