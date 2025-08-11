Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports std = System.Math

Namespace Filters

    Public Module Thresholding

        ''' <summary>
        ''' 将彩色图转换为灰度图
        ''' </summary>
        ''' <param name="bitmap"> 位图 </param>
        ''' <returns> 返回转换好的位图 </returns>
        Public Function convertToGray(bitmap As BitmapBuffer) As BitmapBuffer
            Dim width As Integer = bitmap.Width
            Dim height As Integer = bitmap.Height
            Dim result As New BitmapBuffer(width, height, 4)
            '根据宽高创建像素点数组,并将bitmap的rgb值赋给它
            Dim pixels As UInteger() = bitmap.GetARGBStream
            Dim alpha As Integer = &HFF << 24

            For i As Integer = 0 To height - 1
                For j As Integer = 0 To width - 1
                    '获取当前元素rgb值并以此修改为灰度化后的值
                    Dim rgb As Integer = pixels(width * i + j)
                    Dim red As Integer = (rgb And &HFF0000) >> 16
                    Dim green As Integer = (rgb And &HFF00) >> 8
                    Dim blue As Integer = rgb And &HFF
                    Dim grey As Integer = CInt(std.Truncate(CSng(red) * 0.299 + CSng(green) * 0.587 + CSng(blue) * 0.114))
                    grey = alpha Or (grey << 16) Or (grey << 8) Or grey
                    pixels(width * i + j) = grey
                Next
            Next

            result.WriteARGBStream(pixels)

            Return result
        End Function

        ''' <summary>
        ''' 通过中值滤波去噪
        ''' </summary>
        ''' <param name="bitmap"> 位图 </param>
        ''' <returns> 返回转换好的位图 </returns>
        Public Function MedianFilter(bitmap As BitmapBuffer) As BitmapBuffer
            '获取源位图的宽、高,并创建一个等宽高的bitmap
            Dim width As Integer = bitmap.Width
            Dim height As Integer = bitmap.Height
            Dim result As New BitmapBuffer(width, height)

            '根据宽高创建像素点数组,并将bitmap的rgb值赋给它，同时创建一个空的等大数组
            Dim pixels As UInteger() = bitmap.GetARGBStream
            Dim newpixels As UInteger() = result.GetARGBStream

            '创建一个3*3的模板用来辅助计算中值
            Dim [module](8) As Integer
            Dim alpha As Integer = &HFF << 24
            Dim mid As Integer
            For i As Integer = 0 To height - 1
                For j As Integer = 0 To width - 1
                    If i <> 0 AndAlso i <> height - 1 AndAlso j <> 0 AndAlso j <> width - 1 Then
                        [module](0) = pixels(width * (i - 1) + (j - 1))
                        [module](1) = pixels(width * (i - 1) + j)
                        [module](2) = pixels(width * (i - 1) + (j + 1))
                        [module](3) = pixels(width * i + (j - 1))
                        [module](4) = pixels(width * i + j)
                        [module](5) = pixels(width * i + (j + 1))
                        [module](6) = pixels(width * (i + 1) + (j - 1))
                        [module](7) = pixels(width * (i + 1) + j)
                        [module](8) = pixels(width * (i + 1) + (j + 1))
                        Array.Sort([module])
                        mid = [module](4)
                        newpixels(width * i + j) = alpha Or mid << 16 Or mid << 8 Or mid
                    Else
                        newpixels(width * i + j) = pixels(width * i + j)
                    End If
                Next j
            Next i
            result.WriteARGBStream(newpixels)
            Return result
        End Function

        ''' <summary>
        ''' 通过均值滤波去噪
        ''' </summary>
        ''' <param name="bitmap"> 位图 </param>
        ''' <returns> 返回转换好的位图 </returns>
        Public Function AverageFilter(bitmap As BitmapBuffer) As BitmapBuffer
            '获取源位图的宽、高,并创建一个等宽高的bitmap
            Dim width As Integer = bitmap.Width
            Dim height As Integer = bitmap.Height
            Dim result As New BitmapBuffer(width, height)

            '根据宽高创建像素点数组,并将bitmap的rgb值赋给它，同时创建一个空的等大数组
            Dim pixels As UInteger() = bitmap.GetARGBStream
            Dim newpixels As UInteger() = result.GetARGBStream

            '创建一个3*3的模板用来辅助计算均值
            Dim [module](8) As Integer
            Dim alpha As Integer = &HFF << 24

            For i As Integer = 0 To height - 1
                For j As Integer = 0 To width - 1
                    If i <> 0 AndAlso i <> height - 1 AndAlso j <> 0 AndAlso j <> width - 1 Then
                        [module](0) = pixels(width * (i - 1) + (j - 1))
                        [module](1) = pixels(width * (i - 1) + j)
                        [module](2) = pixels(width * (i - 1) + (j + 1))
                        [module](3) = pixels(width * i + (j - 1))
                        [module](4) = pixels(width * i + j)
                        [module](5) = pixels(width * i + (j + 1))
                        [module](6) = pixels(width * (i + 1) + (j - 1))
                        [module](7) = pixels(width * (i + 1) + j)
                        [module](8) = pixels(width * (i + 1) + (j + 1))

                        Dim sum As Integer = 0
                        For Each num As Integer In [module]
                            sum += num
                        Next num
                        Dim avg As Integer = sum \ 9
                        newpixels(width * i + j) = alpha Or avg << 16 Or avg << 8 Or avg
                    Else
                        newpixels(width * i + j) = pixels(width * i + j)
                    End If
                Next j
            Next i
            result.WriteARGBStream(newpixels)
            Return result
        End Function

        ''' <summary>
        ''' 通过OSTU二值化
        ''' </summary>
        ''' <param name="bitmap"> 位图 </param>
        ''' <returns> 返回转换好的位图 </returns>
        Public Function ostuFilter(bitmap As BitmapBuffer) As BitmapBuffer
            '获取源位图的宽、高,并创建一个等宽高的bitmap
            Dim width As Integer = bitmap.Width
            Dim height As Integer = bitmap.Height
            Dim result As New BitmapBuffer(width, height)

            '根据宽高创建像素点数组,并将bitmap的rgb值赋给它，同时创建一个空的等大数组
            Dim pixels As UInteger() = bitmap.GetARGBStream
            Dim newpixels As UInteger() = result.GetARGBStream

            '创建一个3*3的模板用来辅助计算均值
            Dim [module](8) As Integer
            Dim alpha As Integer = &HFF << 24
            Dim black As Integer = 0
            Dim white As Integer = 255
            'JAVA TO VB CONVERTER WARNING: The original Java variable was marked 'final':
            'ORIGINAL LINE: final int threshold = otsuThreshold(bitmap);
            Dim threshold As Integer = otsuThreshold(bitmap)
            black = alpha Or (black << 16) Or (black << 8) Or black
            white = alpha Or (white << 16) Or (white << 8) Or white
            For i As Integer = 0 To height - 1
                For j As Integer = 0 To width - 1
                    If (pixels(width * i + j) And &HFF) < threshold Then
                        newpixels(width * i + j) = black
                    Else
                        newpixels(width * i + j) = white
                    End If
                Next j
            Next i
            result.WriteARGBStream(newpixels)
            Return result
        End Function

        ''' <summary>
        ''' 通过OSTU寻找二值化的最佳阈值
        ''' </summary>
        ''' <param name="bitmap"> 位图 </param>
        ''' <returns> 返回阈值大小 </returns>
        Public Function otsuThreshold(bitmap As BitmapBuffer) As Integer
            '获取源位图的宽、高
            Dim width As Integer = bitmap.Width
            Dim height As Integer = bitmap.Height

            '根据宽高创建像素点数组,并将bitmap的rgb值赋给它
            Dim pixels As UInteger() = bitmap.GetARGBStream

            '创建两个大小为256的数组，用来保存灰度级中每个像素
            ' 在整幅图像中的个数和在图中所占比例,先暂时初始为0
            Dim pixelCount(255) As Integer
            Dim pixelPro(255) As Single
            For i As Integer = 0 To 255
                pixelCount(i) = 0
                pixelPro(i) = 0
            Next i

            '统计每个像素在整幅图像中的个数
            For i As Integer = 0 To height - 1
                For j As Integer = 0 To width - 1
                    Dim rgb As Integer = pixels(width * i + j)

                    Dim grey As Integer = rgb And &HFF

                    pixelCount(grey) += 1
                Next j
            Next i
            '统计每个像素占整幅图像中的比例
            For i As Integer = 0 To 255
                pixelPro(i) = CSng(pixelCount(i)) / (width * height)
            Next i

            '遍历灰度级[0,255]
            Dim w0 As Single, w1 As Single, u0tmp As Single, u1tmp As Single, u0 As Single, u1 As Single, u As Single, deltaTmp As Single, deltaMax As Single = 0
            Dim threshold As Integer = 0
            For i As Integer = 0 To 255 ' i作为阈值
                deltaTmp = 0
                u = deltaTmp
                u1 = u
                u0 = u1
                u1tmp = u0
                u0tmp = u1tmp
                w1 = u0tmp
                w0 = w1
                For j As Integer = 0 To 255

                    If j <= i Then '背景部分
                        w0 += pixelPro(j)
                        u0tmp += j * pixelPro(j)
                    Else '前景部分
                        w1 += pixelPro(j)
                        u1tmp += j * pixelPro(j)
                    End If
                Next j
                If w0 = 0 OrElse w1 = 0 Then
                    u0 = u0tmp
                    u1 = u1tmp
                    u = u0tmp + u1tmp
                    deltaTmp = 0
                Else
                    u0 = u0tmp / w0
                    u1 = u1tmp / w1
                    u = u0tmp + u1tmp
                    deltaTmp = w0 * (u0 - u) * (u0 - u) + w1 * (u1 - u) * (u1 - u)
                End If

                If deltaTmp > deltaMax Then
                    deltaMax = deltaTmp
                    threshold = i
                End If
            Next i

            Return threshold
        End Function
    End Module
End Namespace