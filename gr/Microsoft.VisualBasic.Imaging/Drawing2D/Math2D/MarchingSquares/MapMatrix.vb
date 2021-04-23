Imports stdNum = System.Math

Namespace Drawing2D.Math2D.MarchingSquares

    Friend Class MapMatrix

        Dim data As Double(,)
        Dim grid_w#
        Dim grid_h#
        Dim x_num% = 100
        Dim y_num% = 100
        Dim w#, h#

        Dim HeightDots()
        Dim min#
        Dim max#

        ''' <summary>
        ''' 数据插值
        ''' </summary>
        Public Sub InitData()
            x_num = CInt(w / grid_w)
            y_num = CInt(h / grid_h)
            data = New Double(x_num - 1, y_num - 1) {}
            Dim measure_data = New IntMeasureData(HeightDots.Length - 1) {}

            For i = HeightDots.Length - 1 To 0 Step -1
                measure_data(i) = New IntMeasureData(HeightDots(i), x_num, y_num)
            Next

            min = Single.MaxValue
            max = Single.MinValue

            For i As Integer = 0 To x_num - 1

                For j As Integer = 0 To y_num - 1
                    Dim value As Single = 0
                    Dim find = False

                    For Each imd As IntMeasureData In measure_data

                        If i = imd.X AndAlso j = imd.Y Then
                            value = imd.Z
                            find = True
                            Exit For
                        End If
                    Next

                    If Not find Then
                        Dim lD As Double = 0
                        Dim DV As Double = 0

                        For Each imd As IntMeasureData In measure_data
                            Dim d As Double = 1.0 / ((imd.X - i) * (imd.X - i) + (imd.Y - j) * (imd.Y - j))
                            lD += d
                            DV += imd.Z * d
                        Next

                        value = CSng(DV / lD)
                    End If

                    data(i, j) = value
                    min = stdNum.Min(min, value)
                    max = stdNum.Max(max, value)
                Next
            Next
        End Sub
    End Class
End Namespace