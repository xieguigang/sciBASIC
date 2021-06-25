Namespace Filters

    Public Enum Matrix2DFilters
        Median
        Min
        Max
        Mean
    End Enum

    ''' <summary>
    ''' https://zhuanlan.zhihu.com/p/73421455
    ''' </summary>
    Module Matrix2DFiltersAlgorithm

        Public Function sf_medianFilter(ByVal sMat As Byte(,)) As Byte(,)
            Dim mH = sMat.GetLength(0)
            Dim mW = sMat.GetLength(1)
            Dim Mat = New Byte(mH - 1, mW - 1) {}
            Dim block = New Byte(8) {}

            For y = 1 To mH - 1 - 1

                For x = 1 To mW - 1 - 1
                    block(0) = sMat(y - 1, x + 1)
                    block(1) = sMat(y - 1, x)
                    block(2) = sMat(y - 1, x - 1)
                    block(3) = sMat(y, x + 1)
                    block(4) = sMat(y, x)
                    block(5) = sMat(y, x - 1)
                    block(6) = sMat(y + 1, x + 1)
                    block(7) = sMat(y + 1, x)
                    block(8) = sMat(y + 1, x - 1)
                    sf_BubbleSort(block)
                    Mat(y, x) = block(4)
                Next
            Next

            Return Mat
        End Function

        Public Sub sf_BubbleSort(ByRef Mat As Byte())
            Dim len = Mat.GetLength(0)
            Dim temp As Byte = 0

            For i = 0 To len - 1 - 1

                For j = i + 1 To len - 1

                    If Mat(i) > Mat(j) Then
                        temp = Mat(i)
                        Mat(i) = Mat(j)
                        Mat(j) = temp
                    End If
                Next
            Next
        End Sub

        Public Function sf_minvalFilter(ByVal sMat As Byte(,)) As Byte(,)
            Dim mH = sMat.GetLength(0)
            Dim mW = sMat.GetLength(1)
            Dim Mat = New Byte(mH - 1, mW - 1) {}
            Dim temp As Byte = 0

            For y = 1 To mH - 1 - 1

                For x = 1 To mW - 1 - 1
                    temp = sMat(y, x)
                    temp = If(temp < sMat(y + 1, x), temp, sMat(y + 1, x))
                    temp = If(temp < sMat(y - 1, x), temp, sMat(y - 1, x))
                    temp = If(temp < sMat(y, x - 1), temp, sMat(y, x - 1))
                    temp = If(temp < sMat(y + 1, x - 1), temp, sMat(y + 1, x - 1))
                    temp = If(temp < sMat(y - 1, x - 1), temp, sMat(y - 1, x - 1))
                    temp = If(temp < sMat(y, x + 1), temp, sMat(y, x + 1))
                    temp = If(temp < sMat(y + 1, x + 1), temp, sMat(y + 1, x + 1))
                    temp = If(temp < sMat(y - 1, x + 1), temp, sMat(y - 1, x + 1))
                    Mat(y, x) = temp
                Next
            Next

            Return Mat
        End Function

        Public Function sf_maxvalFilter(ByVal sMat As Byte(,)) As Byte(,)
            Dim mH = sMat.GetLength(0)
            Dim mW = sMat.GetLength(1)
            Dim Mat = New Byte(mH - 1, mW - 1) {}
            Dim temp As Byte = 0

            For y = 1 To mH - 1 - 1

                For x = 1 To mW - 1 - 1
                    temp = sMat(y, x)
                    temp = If(temp > sMat(y + 1, x), temp, sMat(y + 1, x))
                    temp = If(temp > sMat(y - 1, x), temp, sMat(y - 1, x))
                    temp = If(temp > sMat(y, x - 1), temp, sMat(y, x - 1))
                    temp = If(temp > sMat(y + 1, x - 1), temp, sMat(y + 1, x - 1))
                    temp = If(temp > sMat(y - 1, x - 1), temp, sMat(y - 1, x - 1))
                    temp = If(temp > sMat(y, x + 1), temp, sMat(y, x + 1))
                    temp = If(temp > sMat(y + 1, x + 1), temp, sMat(y + 1, x + 1))
                    temp = If(temp > sMat(y - 1, x + 1), temp, sMat(y - 1, x + 1))
                    Mat(y, x) = temp
                Next
            Next

            Return Mat
        End Function

        Public Function sf_meanFilter(ByVal sMat As Byte(,)) As Byte(,)
            Dim mH = sMat.GetLength(0)
            Dim mW = sMat.GetLength(1)
            Dim Mat = New Byte(mH - 1, mW - 1) {}
            Dim temp = 0

            For y = 1 To mH - 1 - 1

                For x = 1 To mW - 1 - 1
                    temp = sMat(y - 1, x + 1)
                    temp += sMat(y - 1, x)
                    temp += sMat(y - 1, x - 1)
                    temp += sMat(y, x + 1)
                    temp += sMat(y, x)
                    temp += sMat(y, x - 1)
                    temp += sMat(y + 1, x + 1)
                    temp += sMat(y + 1, x)
                    temp += sMat(y + 1, x - 1)
                    Mat(y, x) = CByte(temp / 9)
                Next
            Next

            Return Mat
        End Function
    End Module
End Namespace