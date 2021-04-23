Imports System.Drawing

Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' https://blog.csdn.net/lweiyue/article/details/91490460
    ''' </summary>
    Public Class MarchingSquares

        Dim grid_w#
        Dim grid_h#
        Dim x_num% = 100
        Dim y_num% = 100
        Dim data As Double(,)

        ''' <summary>
        ''' 获取某个阈值下的图形数据
        ''' </summary>
        ''' <param name="threshold">阈值</param>
        ''' <returns>图形数据</returns>
        Public Function CreateMapData(ByVal threshold As Single) As List(Of List(Of PointF))
            Dim binary_data = New Byte(x_num - 1, y_num - 1) {}

            For i As Integer = 0 To x_num - 1

                For j As Integer = 0 To y_num - 1
                    binary_data(i, j) = CByte(If(data(i, j) >= threshold, 1, 0))
                Next
            Next

            Dim shapes As List(Of List(Of PointF)) = New List(Of List(Of PointF))()

            For i As Integer = 1 To x_num - 1

                For j As Integer = 1 To y_num - 1
                    Dim num As Integer = (binary_data(i - 1, j - 1) << 3) + (binary_data(i, j - 1) << 2) + (binary_data(i, j) << 1) + binary_data(i - 1, j)
                    Dim points As List(Of PointF) = New List(Of PointF)()

                    Select Case num
                        Case 0
                        Case 1
                            AddLeft(points, i, j, threshold)
                            AddLeftBottom(points, i, j)
                            AddBottom(points, i, j, threshold)
                        Case 2
                            AddRight(points, i, j, threshold)
                            AddRightBottom(points, i, j)
                            AddBottom(points, i, j, threshold)
                        Case 3
                            AddLeft(points, i, j, threshold)
                            AddLeftBottom(points, i, j)
                            AddRightBottom(points, i, j)
                            AddRight(points, i, j, threshold)
                        Case 4
                            AddTop(points, i, j, threshold)
                            AddRightTop(points, i, j)
                            AddRight(points, i, j, threshold)
                        Case 5
                            AddLeft(points, i, j, threshold)
                            AddRightTop(points, i, j)
                            AddRight(points, i, j, threshold)
                            AddBottom(points, i, j, threshold)
                            AddLeftBottom(points, i, j)
                            AddLeft(points, i, j, threshold)
                        Case 6
                            AddTop(points, i, j, threshold)
                            AddRightTop(points, i, j)
                            AddRightBottom(points, i, j)
                            AddBottom(points, i, j, threshold)
                        Case 7
                            AddTop(points, i, j, threshold)
                            AddRightTop(points, i, j)
                            AddRightBottom(points, i, j)
                            AddLeftBottom(points, i, j)
                            AddLeft(points, i, j, threshold)
                        Case 8
                            AddTop(points, i, j, threshold)
                            AddLeftTop(points, i, j)
                            AddLeft(points, i, j, threshold)
                        Case 9
                            AddTop(points, i, j, threshold)
                            AddLeftTop(points, i, j)
                            AddLeftBottom(points, i, j)
                            AddBottom(points, i, j, threshold)
                        Case 10
                            AddTop(points, i, j, threshold)
                            AddLeftTop(points, i, j)
                            AddLeft(points, i, j, threshold)
                            AddBottom(points, i, j, threshold)
                            AddRightBottom(points, i, j)
                            AddRight(points, i, j, threshold)
                        Case 11
                            AddTop(points, i, j, threshold)
                            AddLeftTop(points, i, j)
                            AddLeftBottom(points, i, j)
                            AddRightBottom(points, i, j)
                            AddRight(points, i, j, threshold)
                        Case 12
                            AddLeft(points, i, j, threshold)
                            AddRight(points, i, j, threshold)
                            AddRightTop(points, i, j)
                            AddLeftTop(points, i, j)
                        Case 13
                            AddRight(points, i, j, threshold)
                            AddRightTop(points, i, j)
                            AddLeftTop(points, i, j)
                            AddLeftBottom(points, i, j)
                            AddBottom(points, i, j, threshold)
                        Case 14
                            AddLeft(points, i, j, threshold)
                            AddBottom(points, i, j, threshold)
                            AddRightBottom(points, i, j)
                            AddRightTop(points, i, j)
                            AddLeftTop(points, i, j)
                        Case 15
                            AddLeftTop(points, i, j)
                            AddRightTop(points, i, j)
                            AddRightBottom(points, i, j)
                            AddLeftBottom(points, i, j)
                    End Select

                    If num <> 0 Then
                        shapes.Add(points)
                    End If
                Next
            Next

            Return shapes
        End Function

        Private Sub AddLeft(ByVal list As List(Of PointF), ByVal x As Integer, ByVal y As Integer, ByVal threshold As Single)
            list.Add(New PointF((x - 1) * grid_w, (y - 1 + V(data(x - 1, y - 1), data(x - 1, y), threshold)) * grid_h))
        End Sub

        Private Sub AddRight(ByVal list As List(Of PointF), ByVal x As Integer, ByVal y As Integer, ByVal threshold As Single)
            list.Add(New PointF(x * grid_w, (y - 1 + V(data(x, y - 1), data(x, y), threshold)) * grid_h))
        End Sub

        Private Sub AddTop(ByVal list As List(Of PointF), ByVal x As Integer, ByVal y As Integer, ByVal threshold As Single)
            list.Add(New PointF((x - 1 + V(data(x - 1, y - 1), data(x, y - 1), threshold)) * grid_w, (y - 1) * grid_h))
        End Sub

        Private Sub AddBottom(ByVal list As List(Of PointF), ByVal x As Integer, ByVal y As Integer, ByVal threshold As Single)
            list.Add(New PointF((x - 1 + V(data(x - 1, y), data(x, y), threshold)) * grid_w, y * grid_h))
        End Sub

        Private Sub AddLeftTop(ByVal list As List(Of PointF), ByVal x As Integer, ByVal y As Integer)
            list.Add(New PointF((x - 1) * grid_w, (y - 1) * grid_h))
        End Sub

        Private Sub AddRightTop(ByVal list As List(Of PointF), ByVal x As Integer, ByVal y As Integer)
            list.Add(New PointF(x * grid_w, (y - 1) * grid_h))
        End Sub

        Private Sub AddLeftBottom(ByVal list As List(Of PointF), ByVal x As Integer, ByVal y As Integer)
            list.Add(New PointF((x - 1) * grid_w, y * grid_h))
        End Sub

        Private Sub AddRightBottom(ByVal list As List(Of PointF), ByVal x As Integer, ByVal y As Integer)
            list.Add(New PointF(x * grid_w, y * grid_h))
        End Sub

        Private Function V(ByVal min As Single, ByVal max As Single, ByVal threshold As Single) As Single
            Return (threshold - min) / (max - min)
        End Function
    End Class
End Namespace
