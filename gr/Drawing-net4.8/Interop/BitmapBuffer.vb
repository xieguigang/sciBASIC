Imports System.Drawing
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

Namespace Interop

    Public Class BitmapBuffer : Inherits BitmapImage.BitmapBuffer

        Protected raw As System.Drawing.Bitmap

        ''' <summary>
        ''' constructor for gdi+ image data object
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="byts"></param>
        ''' <param name="raw"></param>
        ''' <param name="handle"></param>
        ''' <param name="channel"></param>
        Public Sub New(ptr As IntPtr, byts%, raw As System.Drawing.Bitmap, handle As BitmapData, channel As Integer)
            MyBase.New(ptr, byts, raw.Size, stride:=handle.Stride, channel:=channel, handle:=handle)
            Me.raw = raw
        End Sub

        Public Overloads Shared Function FromImage(res As System.Drawing.Image) As BitmapBuffer
            Dim copy As New System.Drawing.Bitmap(res.Width, res.Height, format:=System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            Dim g As Graphics = Graphics.FromImage(copy)

            Call g.DrawImageUnscaled(res, New Point)
            Call g.Flush()
            Call g.Dispose()

            Return BitmapBuffer.FromBitmap(copy)
        End Function

        Public Overloads Shared Function FromBitmap(curBitmap As System.Drawing.Bitmap) As BitmapBuffer
            Return FromBitmap(curBitmap, ImageLockMode.ReadWrite)
        End Function

        ''' <summary>
        ''' 使用这个函数进行写数据的话，会修改到原图
        ''' </summary>
        ''' <param name="curBitmap"></param>
        ''' <param name="mode"></param>
        ''' <returns></returns>
        Public Overloads Shared Function FromBitmap(curBitmap As System.Drawing.Bitmap, mode As ImageLockMode) As BitmapBuffer
            ' Lock the bitmap's bits.  
            Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
            Dim bmpData As BitmapData = curBitmap.LockBits(
                rect:=rect,
                flags:=mode,
                format:=curBitmap.PixelFormat
            )

            ' Get the address of the first line.
            Dim ptr As IntPtr = bmpData.Scan0
            ' Declare an array to hold the bytes of the bitmap.
            Dim bytes As Integer = std.Abs(bmpData.Stride) * curBitmap.Height
            Dim pixels As Integer = curBitmap.Width * curBitmap.Height
            Dim channels As Integer

            If bytes = pixels * 3 Then
                channels = 3
            ElseIf bytes = pixels * 4 Then
                channels = 4
            Else
                Throw New NotImplementedException
            End If

            Return New BitmapBuffer(ptr, bytes, curBitmap, bmpData, channels)
        End Function

        Protected Overrides Sub Dispose(disposing As Boolean)
            MyBase.Dispose(disposing)

            If TypeOf handle Is BitmapData Then
                Call raw.UnlockBits(DirectCast(handle, BitmapData))
            End If
        End Sub
    End Class
End Namespace