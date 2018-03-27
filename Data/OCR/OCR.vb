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
    ''' <param name="horizontal"></param>
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

    <Extension>
    Public Iterator Function Slicing(view As Image) As IEnumerable(Of Map(Of Rectangle, Image))
        Dim xproject = view.Projection(True).Split(Function(d) d = 0R).ToArray
        Dim yproject = view.Projection(False).Split(Function(d) d = 0R).ToArray
        Dim x%, y%
        Dim right%, bottom%
        Dim slice As Image
        Dim rect As Rectangle

        For i As Integer = 0 To yproject.Length - 1
            y = bottom

            If yproject(i).Length = 0 Then
                bottom += 1
                Continue For
            Else
                bottom = yproject(i).Length + y
            End If

            x = 0
            right = 0

            For j As Integer = 0 To xproject.Length - 1
                x = right

                If xproject(j).Length = 0 Then
                    right += 1
                    Continue For
                Else
                    right = xproject(j).Length + x
                End If

                rect = New Rectangle With {
                    .X = x,
                    .Y = y,
                    .Width = xproject(j).Length,
                    .Height = yproject(i).Length
                }
                slice = view.ImageCrop(rect)

                Yield New Map(Of Rectangle, Image)(rect, slice)
            Next
        Next
    End Function
End Module
