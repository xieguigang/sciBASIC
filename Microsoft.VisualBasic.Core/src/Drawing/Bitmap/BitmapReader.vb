#Region "Microsoft.VisualBasic::e12ae7526532fdaa39dac05a867a41cc, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\BitmapReader.vb"

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

    '   Total Lines: 192
    '    Code Lines: 126 (65.62%)
    ' Comment Lines: 32 (16.67%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 34 (17.71%)
    '     File Size: 7.63 KB


    '     Class BitmapReader
    ' 
    '         Properties: BitsPerPixel, Compression, FileSizeInBytes, HeaderSize, HorizontalPixelsPerMeter
    '                     ImageHeight, ImageSizeInBytes, ImageWidth, NumberOfColorPlanes, NumberOfColorsUsed
    '                     NumberOfImportantColors, Offset, Reserved, RowSize, VerticalPixelsPerMeter
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetColorByteLocation, GetOffset, GetPixelColor, LoadMemory, RoundUpToNearestFour
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' a bitmap image data reader for help huge scientific image data processing.
    ''' (more than the 2GB limitation of gdi+ <see cref="Bitmap"/>.)
    ''' </summary>
    Public Class BitmapReader : Implements IDisposable

        ReadOnly s As BinaryReader

        Dim disposedValue As Boolean

        Const B As Integer = Asc("B")
        Const M As Integer = Asc("M")

        Public ReadOnly Property FileSizeInBytes As Integer
        Public ReadOnly Property Reserved As Integer
        Public ReadOnly Property Offset As Integer
        Public ReadOnly Property HeaderSize As Integer
        Public ReadOnly Property ImageWidth As Integer
        Public ReadOnly Property ImageHeight As Integer
        Public ReadOnly Property NumberOfColorPlanes As Integer
        Public ReadOnly Property BitsPerPixel As BitsPerPixel
        Public ReadOnly Property Compression As Integer
        Public ReadOnly Property ImageSizeInBytes As Integer
        Public ReadOnly Property HorizontalPixelsPerMeter As Integer
        Public ReadOnly Property VerticalPixelsPerMeter As Integer
        Public ReadOnly Property NumberOfColorsUsed As Integer
        Public ReadOnly Property NumberOfImportantColors As Integer
        Public ReadOnly Property RowSize As Integer

        Sub New(file As Stream)
            Dim bin As New BinaryReader(file)

            s = bin

            ' check of the magic number 
            If bin.ReadByte <> B OrElse bin.ReadByte <> M Then
                Throw New InvalidDataException("invalid magic number!")
            End If

            FileSizeInBytes = s.ReadInt32
            Reserved = s.ReadInt32
            Offset = s.ReadInt32
            HeaderSize = s.ReadInt32
            ImageWidth = s.ReadInt32
            ImageHeight = s.ReadInt32
            NumberOfColorPlanes = s.ReadInt16
            BitsPerPixel = s.ReadInt16
            Compression = s.ReadInt32
            ImageSizeInBytes = s.ReadInt32
            HorizontalPixelsPerMeter = s.ReadInt32
            VerticalPixelsPerMeter = s.ReadInt32
            NumberOfColorsUsed = s.ReadInt32
            NumberOfImportantColors = s.ReadInt32
            RowSize = RoundUpToNearestFour(ImageWidth * BitsPerPixel / 8.0)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <remarks>
        ''' the bitmap image data maybe load into memory at once if the file size is less then 2GB.
        ''' </remarks>
        Sub New(filename As String)
            Call Me.New(filename.Open(FileMode.Open, doClear:=False, [readOnly]:=True))
        End Sub

        Public Shared Function RoundUpToNearestFour(value As Integer) As Integer
            Return (value + 3) And Not 3
        End Function

        Private Function GetColorByteLocation(column As Integer, pixelByteLocation As Integer) As Integer
            If BitsPerPixel = BitsPerPixel.TwentyFour Then
                Return pixelByteLocation
            Else
                s.BaseStream.Seek(pixelByteLocation, SeekOrigin.Begin)
            End If

            Dim b As Byte = s.ReadByte
            Dim colorNumber As Integer

            Select Case BitsPerPixel
                Case BitsPerPixel.One : colorNumber = (b >> (7 - column Mod 8)) And &H1
                Case BitsPerPixel.Four : colorNumber = (b >> ((1 - column Mod 2) * 4)) And &HF
                Case BitsPerPixel.Eight
                    colorNumber = b
                Case Else
                    Throw New InvalidDataException
            End Select

            Return 54 + colorNumber * 4
        End Function

        Public Function GetOffset(row As Integer, column As Integer) As Integer
            row = ImageHeight - 1 - row

            Select Case BitsPerPixel
                Case BitsPerPixel.One : Return Offset + row * RowSize + column / 8
                Case BitsPerPixel.Four : Return Offset + row * RowSize + column / 2
                Case BitsPerPixel.Eight : Return Offset + row * RowSize + column * 1
                Case BitsPerPixel.TwentyFour
                    Return Offset + row * RowSize + column * 3
                Case Else
                    Throw New InvalidDataException
            End Select
        End Function

        ''' <summary>
        ''' get pixel via given x,y location
        ''' </summary>
        ''' <param name="row">the y location</param>
        ''' <param name="column">the x location</param>
        ''' <returns></returns>
        Public Function GetPixelColor(row As Integer, column As Integer) As Color
            Dim pixelByteLocation = GetOffset(row, column)
            Dim colorByteLocation = GetColorByteLocation(column, pixelByteLocation)

            ' (0,0) ??
            If colorByteLocation < 0 Then
                colorByteLocation = Offset
            End If

            s.BaseStream.Seek(colorByteLocation, SeekOrigin.Begin)

            ' bitmap has no alpha channel
            Dim Blue = s.ReadByte
            Dim Green = s.ReadByte
            Dim Red = s.ReadByte

            Return Color.FromArgb(Red, Green, Blue)
        End Function

        Public Function LoadMemory() As BitmapBuffer
            Dim buf As Byte() = New Byte(ImageWidth * ImageHeight * 4 - 1) {}
            Dim buf_offset As Integer = Scan0

            For column As Integer = 0 To ImageWidth - 1
                For row As Integer = 0 To ImageHeight - 1
                    Dim pixelByteLocation = GetOffset(row, column)
                    Dim colorByteLocation = GetColorByteLocation(column, pixelByteLocation)

                    ' (0,0) ??
                    If colorByteLocation < 0 Then
                        colorByteLocation = Offset
                    End If

                    Call s.BaseStream.Seek(colorByteLocation, SeekOrigin.Begin)

                    ' bitmap has no alpha channel
                    buf(buf_offset + 0) = s.ReadByte
                    buf(buf_offset + 1) = s.ReadByte
                    buf(buf_offset + 2) = s.ReadByte

                    buf_offset += 4
                Next
            Next

            Return New BitmapBuffer(buf, New Size(ImageWidth, ImageHeight), channel:=4)
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call s.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
