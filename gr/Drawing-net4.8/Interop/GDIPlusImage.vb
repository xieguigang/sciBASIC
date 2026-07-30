#Region "Microsoft.VisualBasic::9d937ce5146d4f6ee3910e0334a1fb6e, gr\Drawing-net4.8\Interop\GDIPlusImage.vb"

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

    '   Total Lines: 97
    '    Code Lines: 78 (80.41%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (19.59%)
    '     File Size: 3.56 KB


    '     Class GDIPlusImage
    ' 
    '         Properties: Previews, Size
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: ConvertToBitmapStream, CreateCanvas2D, CTypeGDIPlusImage, GetMemoryBitmap, LoadImage
    ' 
    '         Sub: Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Imaging

Namespace Interop

    Public Class GDIPlusImage : Inherits Microsoft.VisualBasic.Imaging.Image
        Implements IVisualStudioPreviews

        Public Overrides ReadOnly Property Size As Size
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return bitmap.Size
            End Get
        End Property

        Public ReadOnly Property Previews As String Implements IVisualStudioPreviews.Previews
            Get
                Return GeneratePreviewHtml(Me)
            End Get
        End Property

        ReadOnly bitmap As System.Drawing.Bitmap

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(img As System.Drawing.Image)
            bitmap = New System.Drawing.Bitmap(img)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(img As System.Drawing.Bitmap)
            bitmap = img
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(img As Microsoft.VisualBasic.Imaging.Bitmap)
            Call Me.New(DirectCast(img, Microsoft.VisualBasic.Imaging.Image))
        End Sub

        Sub New(img As Microsoft.VisualBasic.Imaging.Image)
            bitmap = CTypeGDIPlusImage(img)
        End Sub

        Public Shared Function CTypeGDIPlusImage(img As Microsoft.VisualBasic.Imaging.Image) As System.Drawing.Bitmap
            Dim bitmap As New System.Drawing.Bitmap(
               width:=img.Width,
               height:=img.Height,
               format:=System.Drawing.Imaging.PixelFormat.Format32bppArgb
            )

            Using buffer As BitmapBuffer = BitmapBuffer.FromBitmap(bitmap)
                Array.ConstrainedCopy(
                    img.GetMemoryBitmap.RawBuffer, Scan0,
                    buffer.RawBuffer, Scan0,
                    buffer.RawBuffer.Length
                )
            End Using

            Return bitmap
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Save(s As IO.Stream, format As ImageFormats)
            Call bitmap.Save(s, format.GetFormat)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetMemoryBitmap() As BitmapImage.BitmapBuffer
            Return BitmapBuffer.FromBitmap(bitmap)
        End Function

        Protected Overrides Function ConvertToBitmapStream() As IO.MemoryStream
            Dim s As Stream = New MemoryStream
            Call Save(s, ImageFormats.Png)
            Call s.Flush()
            Return s
        End Function

        Public Function CreateCanvas2D(direct_access As Boolean) As IGraphics
            Return bitmap.CreateCanvas2D(direct_access)
        End Function

        Public Shared Narrowing Operator CType(img As GDIPlusImage) As System.Drawing.Bitmap
            Return img.bitmap
        End Operator

        Public Shared Narrowing Operator CType(img As GDIPlusImage) As System.Drawing.Image
            Return img.bitmap
        End Operator

        Public Shared Function LoadImage(path As String, Optional base64 As Boolean = False, Optional throwEx As Boolean = True) As System.Drawing.Image
            Return path.LoadImage(base64, throwEx)
        End Function
    End Class
End Namespace
