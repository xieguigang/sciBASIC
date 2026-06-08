#Region "Microsoft.VisualBasic::e782b47c202fc6cdf27698618c22f517, gr\Drawing-net4.8\Interop\BitmapBuffer.vb"

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

    '   Total Lines: 86
    '    Code Lines: 50 (58.14%)
    ' Comment Lines: 22 (25.58%)
    '    - Xml Docs: 86.36%
    ' 
    '   Blank Lines: 14 (16.28%)
    '     File Size: 3.34 KB


    '     Class BitmapBuffer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) FromBitmap, FromImage
    ' 
    '         Sub: Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

        ''' <summary>
        ''' create image read/write memory buffer
        ''' </summary>
        ''' <param name="curBitmap"></param>
        ''' <returns></returns>
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
