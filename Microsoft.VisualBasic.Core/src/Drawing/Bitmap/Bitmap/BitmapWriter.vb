#Region "Microsoft.VisualBasic::92f2aa8fd3a949f7a352817cb4f51ceb, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BitmapWriter.vb"

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

    '   Total Lines: 226
    '    Code Lines: 175 (77.43%)
    ' Comment Lines: 12 (5.31%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 39 (17.26%)
    '     File Size: 10.41 KB


    '     Class BitmapWriter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CalcMask, GetPixel, ScalePixelData
    ' 
    '         Sub: (+2 Overloads) Dispose, (+2 Overloads) SaveBitFieldsImage, (+2 Overloads) SaveColorImage, SaveRgb888Image, SetPixel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Buffers
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports std = System.Math

Namespace Imaging.BitmapImage.StreamWriter

    ' implements of bitmap data reader
    ' https://github.com/xieguigang/sciBASIC/blob/master/Microsoft.VisualBasic.Core/src/Extensions/Image/Bitmap/BtimapReader.vb
    '
    Public Class BitmapWriter : Implements IDisposable

        Private fileHeader As BitmapFileHeader = BitmapFileHeader.GetDefault()
        Private infoHeader As BitmapInfoHeader = BitmapInfoHeader.GetDefault()
        Private bufferImage As Byte() = Array.Empty(Of Byte)()

        Public Sub New(width As Integer, height As Integer)
            infoHeader.Width = width
            infoHeader.Height = height
            bufferImage = ArrayPool(Of Byte).Shared.Rent(width * height * 3)
        End Sub

        Public Function GetPixel(x As Integer, y As Integer) As (Byte, Byte, Byte)
            If infoHeader.Width <= x Then
                Throw New ArgumentException($"Overflow x.({x}) : Width({infoHeader.Width})")
            End If
            If infoHeader.Height <= y Then
                Throw New ArgumentException($"Overflow y.({y}) : Height({infoHeader.Height})")
            End If
            Dim x_pos = x * 3
            Dim widthSize = infoHeader.Width * 3

            Dim blue = bufferImage(x_pos + 0 + y * widthSize)
            Dim green = bufferImage(x_pos + 1 + y * widthSize)
            Dim red = bufferImage(x_pos + 2 + y * widthSize)
            Return (red, green, blue)
        End Function

        Public Sub SetPixel(x As Integer, y As Integer, red As Byte, green As Byte, blue As Byte)
            If infoHeader.Width <= x Then
                Throw New ArgumentException($"Overflow x.({x}) : Width({infoHeader.Width})")
            End If
            If infoHeader.Height <= y Then
                Throw New ArgumentException($"Overflow y.({y}) : Height({infoHeader.Height})")
            End If
            Dim x_pos = x * 3
            Dim widthSize = infoHeader.Width * 3
            bufferImage(x_pos + 0 + y * widthSize) = blue
            bufferImage(x_pos + 1 + y * widthSize) = green
            bufferImage(x_pos + 2 + y * widthSize) = red
        End Sub

        Public Sub SaveColorImage(fileStream As Stream)
            ' Data
            Dim fileHeaderSpan = StructSerializer.StructureToByte(fileHeader)
            Dim infoHeaderSpan = StructSerializer.StructureToByte(infoHeader)
            Dim srcImageSpan = bufferImage.AsSpan()

            Dim srcImageWidthSize = infoHeader.Width * 3
            Dim dstColorBit = BitmapColorBit.Bit24
            Dim dstImageWidth = GetImageWidthSize(infoHeader.Width, dstColorBit)
            Dim dstPaddingWidthSize = dstImageWidth - srcImageWidthSize
            Dim dstImageTotalSize = (srcImageWidthSize + dstPaddingWidthSize) * CUInt(infoHeader.Height)

            fileHeader.Size = CUInt(fileHeaderSpan.Length) + CUInt(infoHeaderSpan.Length) + CUInt(dstImageTotalSize)
            fileHeader.OffBits = CUInt(fileHeaderSpan.Length) + CUInt(infoHeaderSpan.Length)
            infoHeader.SizeImage = CUInt(dstImageTotalSize)
            infoHeader.BitCount = BitmapColorBit.Bit24
            infoHeader.Compression = BitmapCompression.Rgb

            fileStream.Write(fileHeaderSpan, Scan0, fileHeaderSpan.Length)
            fileStream.Write(infoHeaderSpan, Scan0, infoHeaderSpan.Length)
            ' ImageData
            Dim dstPaddingBytes = New Byte(dstPaddingWidthSize - 1) {}
            Dim heightIndex = infoHeader.Height - 1

            While 0 <= heightIndex
                Dim lineSpan = srcImageSpan.Slice(srcImageWidthSize * heightIndex, srcImageWidthSize).ToArray
                If dstPaddingWidthSize = 0 Then
                    fileStream.Write(lineSpan, Scan0, lineSpan.Length)
                Else
                    fileStream.Write(lineSpan, Scan0, lineSpan.Length)
                    fileStream.Write(dstPaddingBytes, Scan0, dstPaddingBytes.Length)
                End If

                heightIndex -= 1
            End While

            Call fileStream.Flush()
        End Sub

        ''' <summary>
        ''' Save
        ''' </summary>
        ''' <param name="path"></param>
        Public Sub SaveColorImage(path As String)
            Using fileStream As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Call SaveColorImage(fileStream)
            End Using
        End Sub

        Private Shared Function CalcMask(mask As Integer) As (Long, Long, Integer)
            Dim castMask = CLng(mask)
            Dim calcDiv = castMask And -castMask
            Dim calcMax = castMask / calcDiv
            Dim calcShift = std.Log(calcDiv, 2)

            Return (calcDiv, calcMax, calcShift)
        End Function

        Private Shared Function ScalePixelData(data As Byte, max As Long) As Long
            If max = 0 Then
                Return 0
            End If
            Dim one = data / Byte.MaxValue
            Return one * max
        End Function

        Public Sub SaveBitFieldsImage(fileStream As Stream, colorBit As BitmapColorBit, redMask As Integer, greenMask As Integer, blueMask As Integer)
            Select Case colorBit
                Case BitmapColorBit.Bit16, BitmapColorBit.Bit32
                Case Else
                    Throw New ArgumentException($"Compression with this bit depth is not possible.")
            End Select

            ' Data
            Dim fileHeaderSpan = StructSerializer.StructureToByte(fileHeader)
            Dim infoHeaderSpan = StructSerializer.StructureToByte(infoHeader)

            Dim srcWidthSize = infoHeader.Width * 3
            Dim dstImageTotalSize = srcWidthSize * CUInt(infoHeader.Height)

            fileHeader.Size = CUInt(fileHeaderSpan.Length) + CUInt(infoHeaderSpan.Length) + CUInt(dstImageTotalSize)
            fileHeader.OffBits = CUInt(fileHeaderSpan.Length) + CUInt(infoHeaderSpan.Length) + 12
            infoHeader.SizeImage = CUInt(dstImageTotalSize)
            infoHeader.BitCount = colorBit
            infoHeader.Compression = BitmapCompression.BitFields

            fileStream.Write(fileHeaderSpan, Scan0, fileHeaderSpan.Length)
            fileStream.Write(infoHeaderSpan, Scan0, infoHeaderSpan.Length)
            fileStream.Write(BitConverter.GetBytes(redMask), Scan0, RawStream.INT32)
            fileStream.Write(BitConverter.GetBytes(greenMask), Scan0, RawStream.INT32)
            fileStream.Write(BitConverter.GetBytes(blueMask), Scan0, RawStream.INT32)
            Dim redDiv As Long, redMax As Long, redShift As Integer

            ' ImageData

            CalcMask(redMask).Set(redDiv, redMax, redShift)

            Dim greenDiv As Long, greenMax As Long, greenShift As Integer
            CalcMask(greenMask).Set(greenDiv, greenMax, greenShift)
            Dim blueDiv As Long, blueMax As Long, blueShift As Integer
            CalcMask(blueMask).Set(blueDiv, blueMax, blueShift)
            Dim redPixel As Byte, greenPixel As Byte, bluePixel As Byte



            Dim heightIndex = infoHeader.Height - 1

            While 0 <= heightIndex
                Dim widthIndex = 0

                While widthIndex < infoHeader.Width
                    GetPixel(widthIndex, heightIndex).Set(redPixel, greenPixel, bluePixel)
                    Select Case colorBit
                        Case BitmapColorBit.Bit16
                            Dim data = CShort(0)
                            data = data Or CShort(BitmapWriter.ScalePixelData(redPixel, redMax) << redShift)
                            data = data Or CShort(BitmapWriter.ScalePixelData(greenPixel, greenMax) << greenShift)
                            data = data Or CShort(BitmapWriter.ScalePixelData(bluePixel, blueMax) << blueShift)
                            fileStream.Write(BitConverter.GetBytes(data), Scan0, RawStream.ShortInt)
                            Exit Select
                        Case BitmapColorBit.Bit32
                            Dim data = 0
                            data = data Or CInt(BitmapWriter.ScalePixelData(redPixel, redMax) << redShift)
                            data = data Or CInt(BitmapWriter.ScalePixelData(greenPixel, greenMax) << greenShift)
                            data = data Or CInt(BitmapWriter.ScalePixelData(bluePixel, blueMax) << blueShift)
                            fileStream.Write(BitConverter.GetBytes(data), Scan0, RawStream.INT32)
                            Exit Select
                        Case Else
                            Throw New ArgumentException($"Compression with this bit depth is not possible.")
                    End Select

                    widthIndex += 1
                End While

                heightIndex -= 1
            End While

            Call fileStream.Flush()
        End Sub

        Public Sub SaveBitFieldsImage(path As String, colorBit As BitmapColorBit, redMask As Integer, greenMask As Integer, blueMask As Integer)
            ' Save
            Using fileStream As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Call SaveBitFieldsImage(fileStream, colorBit, redMask, greenMask, blueMask)
            End Using
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SaveRgb888Image(path As String)
            SaveBitFieldsImage(path, BitmapColorBit.Bit32, &HFF0000, &HFF00, &HFF)
        End Sub

#Region "IDisposable"
        Private disposedValue As Boolean
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ArrayPool(Of Byte).Shared.Return(bufferImage)
                End If
                bufferImage = Array.Empty(Of Byte)()
                disposedValue = True
            End If
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace

