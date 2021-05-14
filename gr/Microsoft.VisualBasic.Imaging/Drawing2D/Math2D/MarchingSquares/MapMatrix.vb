Imports System.Drawing
Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' 将稀疏矩阵数据转换为稠密矩阵数据
    ''' </summary>
    Public Class MapMatrix

        ''' <summary>
        ''' 插值后得到的稠密矩阵数据
        ''' </summary>
        Friend data As Double(,)
        Friend x_num% = 100
        Friend y_num% = 100

#Region "the input parameters"
        Friend grid_w#
        Friend grid_h#
        Friend w#, h#
#End Region

        ''' <summary>
        ''' 实际的测量结果数据，一般为一个稀疏矩阵
        ''' </summary>
        ReadOnly dots() As MeasureData

        Friend min#
        Friend max#

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="raw">现实世界中的原始测量结果数据</param>
        ''' <param name="size">画布的大小</param>
        ''' <param name="gridSize">网格的大小</param>
        Sub New(raw As IEnumerable(Of MeasureData), size As SizeF, gridSize As SizeF)
            dots = raw.ToArray
            w = size.Width
            h = size.Height
            grid_w = gridSize.Width
            grid_h = gridSize.Height
        End Sub

        ''' <summary>
        ''' 返回一个稠密状态的结果矩阵
        ''' </summary>
        ''' <returns>
        ''' 以行扫描的方式返回结果
        ''' </returns>
        Public Iterator Function GetMatrixInterpolation() As IEnumerable(Of IntMeasureData())
            Dim x, y As Double
            Dim dx As Double = grid_w
            Dim dy As Double = grid_h
            Dim scan As New List(Of IntMeasureData)

            For i As Integer = 0 To x_num - 1
                For j As Integer = 0 To y_num - 1
                    scan += New IntMeasureData With {
                        .X = x,
                        .Y = y,
                        .Z = data(i, j)
                    }
                    y += dy
                Next

                y = 0
                x = x + dx

                Yield scan.PopAll
            Next
        End Function

        ''' <summary>
        ''' 数据插值
        ''' </summary>
        Friend Function InitData() As MapMatrix
            Dim measure_data = New IntMeasureData(dots.Length - 1) {}
            Dim d As Double

            x_num = CInt(w / grid_w)
            y_num = CInt(h / grid_h)
            data = New Double(x_num - 1, y_num - 1) {}

            For i = dots.Length - 1 To 0 Step -1
                measure_data(i) = New IntMeasureData(dots(i), x_num, y_num)
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
                            d = 1.0 / ((imd.X - i) * (imd.X - i) + (imd.Y - j) * (imd.Y - j))
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

            Return Me
        End Function
    End Class
End Namespace