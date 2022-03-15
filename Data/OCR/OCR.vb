#Region "Microsoft.VisualBasic::bedb57243843e70d9a9c426704b21f4a, sciBASIC#\Data\OCR\OCR.vb"

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
    '    Code Lines: 133
    ' Comment Lines: 29
    '   Blank Lines: 30
    '     File Size: 7.24 KB


    ' Module OCR
    ' 
    '     Function: GetCharacters, Projection, SliceSegments, Slicing
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module OCR

    <Extension>
    Public Iterator Function GetCharacters(view As Image, library As Library) As IEnumerable(Of (position As Rectangle, obj As Char, score#))
        For Each block In view.ToVector(size:=library.Window, fillDeli:=True)
            Dim pixels As Vector = block.Maps
            Dim subject As New OpticalCharacter With {
                .PixelsVector = pixels
            }
            Dim find = library.Match(subject)

            If find.score > 0 Then
                Yield (New Rectangle(block.Key, library.Window), find.recognized, find.score)
            End If
        Next
    End Function

    ''' <summary>
    ''' 将图片按照指定的方向投影为亮度向量
    ''' </summary>
    ''' <param name="view"></param>
    ''' <param name="horizontal">当这个参数为真的时候，表示按照X轴进行投影，反之则是按照Y轴进行投影操作</param>
    ''' <param name="background"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function Projection(view As Image, Optional horizontal As Boolean = True, Optional background As Color = Nothing) As Vector
        Dim pixelScan As Func(Of Color, Double) = BackgroundMatch(background Or blank)

        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(view)
            Dim pixels As Double()

            If horizontal Then
                ' 将点投影到X横轴，向量的下标就是X坐标
                pixels = New Double(bitmap.Width - 1) {}

                For y As Integer = 0 To bitmap.Height - 1
                    For x As Integer = 0 To bitmap.Width - 1
                        pixels(x) += pixelScan(bitmap.GetPixel(x, y))
                    Next
                Next

            Else
                ' 将点投影到Y竖轴，向量的下标就是Y坐标
                pixels = New Double(bitmap.Height - 1) {}

                For x As Integer = 0 To bitmap.Width - 1
                    For y As Integer = 0 To bitmap.Height - 1
                        pixels(y) += pixelScan(bitmap.GetPixel(x, y))
                    Next
                Next

            End If

            Return pixels.AsVector
        End Using
    End Function

    ''' <summary>
    ''' 因为切片的大小和标准库之中的图片的大小肯定不一样
    ''' 则无法直接使用SSM算法来计算相似度
    ''' 在这里需要进行切割为例如3*3的小块，降低细节，使用动态规划的方法来计算全局相似度
    ''' </summary>
    ''' <param name="slice"></param>
    ''' <param name="[step]"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SliceSegments(slice As Image, [step] As Size, Optional background As Color = Nothing) As Vector
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(slice)
            Dim vector As New List(Of Double)
            Dim pixelScan As Func(Of Color, Double) = BackgroundMatch(background Or blank)
            Dim n% = 0

            For y As Integer = 0 To bitmap.Height - 1 Step [step].Height
                For x As Integer = 0 To bitmap.Width - 1 Step [step].Width

                    ' 统计出这个小块内的1的数量
                    For i As Integer = x To x + [step].Width
                        If i = bitmap.Width Then
                            Exit For
                        End If

                        For j As Integer = y To y + [step].Height
                            If j = bitmap.Height Then
                                Exit For
                            End If
                            n += pixelScan(bitmap.GetPixel(i, j))
                        Next
                    Next

                    vector += n
                Next
            Next

            Return vector.AsVector
        End Using
    End Function

    ''' <summary>
    ''' 使用两个方向的投影对图片上的文字区域进行自动切片
    ''' </summary>
    ''' <param name="view"></param>
    ''' <param name="threshold#"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Slicing(view As Image, Optional threshold# = 1) As IEnumerable(Of Map(Of Rectangle, Image))
        Dim pixelX As Vector = view.Projection(True)
        Dim pixelY As Vector = view.Projection(False)
        Dim xproject = pixelX.Split(Function(d) d <= threshold).ToArray
        Dim yproject = pixelY.Split(Function(d) d <= threshold).ToArray
        Dim x%, y%
        Dim right%, bottom%
        Dim slice As Image
        Dim rect As Rectangle

        If xproject.Length = 1 AndAlso yproject.Length = 1 Then
            ' 只有一个唯一的切片，已经无法再进行切片了
            ' 返回原始图片
            rect = New Rectangle With {
                .X = 0,
                .Y = 0,
                .Width = pixelX.Length,
                .Height = pixelY.Length
            }

            Yield New Map(Of Rectangle, Image)(rect, view)
            Return
        ElseIf pixelX.Sum = 0R OrElse pixelY.Sum = 0R Then
            ' 整张图片都是空白的，没有任何内容
            Return
        End If

        For i As Integer = 0 To yproject.Length - 1
            y = bottom + 1

            If yproject(i).Length = 0 Then
                bottom += 1
                Continue For
            Else
                bottom = yproject(i).Length + y
            End If

            x = 0
            right = 0

            For j As Integer = 0 To xproject.Length - 1
                x = right + 1

                If xproject(j).Length = 0 Then
                    right += 1
                    Continue For
                Else
                    right = xproject(j).Length + x
                End If

                rect = New Rectangle With {
                    .X = x,
                    .Y = y,
                    .Width = xproject(j).Length - 1,
                    .Height = yproject(i).Length - 1
                }

                If rect.Width > 0 AndAlso rect.Height > 0 Then
                    slice = view.ImageCrop(rect)

                    If slice.Projection.Sum > 0R Then
                        Dim pos As Point = rect.Location

                        ' 不是空白的切片
                        For Each subRegion In slice.Slicing(threshold)
                            rect = New Rectangle With {
                                .Location = pos + subRegion.Key.Location,
                                .Size = subRegion.Key.Size
                            }

                            Yield New Map(Of Rectangle, Image)(
                                rect, subRegion.Maps
                            )
                        Next
                    End If
                End If
            Next
        Next
    End Function
End Module
