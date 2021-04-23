Imports System.Drawing
Imports System.Runtime.CompilerServices

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
        Public Iterator Function CreateMapData(threshold As Single) As IEnumerable(Of PointF())
            Dim binary_data = New Byte(x_num - 1, y_num - 1) {}

            For i As Integer = 0 To x_num - 1
                For j As Integer = 0 To y_num - 1
                    binary_data(i, j) = CByte(If(data(i, j) >= threshold, 1, 0))
                Next
            Next

            For i As Integer = 1 To x_num - 1
                For Each block As PointF() In ScanRows(binary_data, i, threshold)
                    Yield block
                Next
            Next
        End Function

        Private Iterator Function ScanRows(binary_data As Byte(,), i%, threshold As Single) As IEnumerable(Of PointF())
            Dim points As New List(Of PointF)()

            For j As Integer = 1 To y_num - 1
                Dim num As Integer = (binary_data(i - 1, j - 1) << 3) + (binary_data(i, j - 1) << 2) + (binary_data(i, j) << 1) + binary_data(i - 1, j)

                Call points.Clear()

                Select Case num
                    Case 0
                    Case 1
                        Call points.AddLeft(points, i, j, threshold)
                        Call points.AddLeftBottom(points, i, j)
                        Call points.AddBottom(points, i, j, threshold)
                    Case 2
                        Call points.AddRight(points, i, j, threshold)
                        Call points.AddRightBottom(points, i, j)
                        Call points.AddBottom(points, i, j, threshold)
                    Case 3
                        Call points.AddLeft(points, i, j, threshold)
                        Call points.AddLeftBottom(points, i, j)
                        Call points.AddRightBottom(points, i, j)
                        Call points.AddRight(points, i, j, threshold)
                    Case 4
                        Call points.AddTop(points, i, j, threshold)
                        Call points.AddRightTop(points, i, j)
                        Call points.AddRight(points, i, j, threshold)
                    Case 5
                        Call points.AddLeft(points, i, j, threshold)
                        Call points.AddRightTop(points, i, j)
                        Call points.AddRight(points, i, j, threshold)
                        Call points.AddBottom(points, i, j, threshold)
                        Call points.AddLeftBottom(points, i, j)
                        Call points.AddLeft(points, i, j, threshold)
                    Case 6
                        Call points.AddTop(points, i, j, threshold)
                        Call points.AddRightTop(points, i, j)
                        Call points.AddRightBottom(points, i, j)
                        Call points.AddBottom(points, i, j, threshold)
                    Case 7
                        Call points.AddTop(points, i, j, threshold)
                        Call points.AddRightTop(points, i, j)
                        Call points.AddRightBottom(points, i, j)
                        Call points.AddLeftBottom(points, i, j)
                        Call points.AddLeft(points, i, j, threshold)
                    Case 8
                        Call points.AddTop(points, i, j, threshold)
                        Call points.AddLeftTop(points, i, j)
                        Call points.AddLeft(points, i, j, threshold)
                    Case 9
                        Call points.AddTop(points, i, j, threshold)
                        Call points.AddLeftTop(points, i, j)
                        Call points.AddLeftBottom(points, i, j)
                        Call points.AddBottom(points, i, j, threshold)
                    Case 10
                        Call points.AddTop(points, i, j, threshold)
                        Call points.AddLeftTop(points, i, j)
                        Call points.AddLeft(points, i, j, threshold)
                        Call points.AddBottom(points, i, j, threshold)
                        Call points.AddRightBottom(points, i, j)
                        Call points.AddRight(points, i, j, threshold)
                    Case 11
                        Call points.AddTop(points, i, j, threshold)
                        Call points.AddLeftTop(points, i, j)
                        Call points.AddLeftBottom(points, i, j)
                        Call points.AddRightBottom(points, i, j)
                        Call points.AddRight(points, i, j, threshold)
                    Case 12
                        Call points.AddLeft(points, i, j, threshold)
                        Call points.AddRight(points, i, j, threshold)
                        Call points.AddRightTop(points, i, j)
                        Call points.AddLeftTop(points, i, j)
                    Case 13
                        Call points.AddRight(points, i, j, threshold)
                        Call points.AddRightTop(points, i, j)
                        Call points.AddLeftTop(points, i, j)
                        Call points.AddLeftBottom(points, i, j)
                        Call points.AddBottom(points, i, j, threshold)
                    Case 14
                        Call points.AddLeft(points, i, j, threshold)
                        Call points.AddBottom(points, i, j, threshold)
                        Call points.AddRightBottom(points, i, j)
                        Call points.AddRightTop(points, i, j)
                        Call points.AddLeftTop(points, i, j)
                    Case 15
                        Call points.AddLeftTop(points, i, j)
                        Call points.AddRightTop(points, i, j)
                        Call points.AddRightBottom(points, i, j)
                        Call points.AddLeftBottom(points, i, j)
                End Select

                If num <> 0 Then
                    Yield points.ToArray
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Left(x As Integer, y As Integer, threshold As Single)
            Return New PointF((x - 1) * grid_w, (y - 1 + V(data(x - 1, y - 1), data(x - 1, y), threshold)) * grid_h)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Right(x As Integer, y As Integer, threshold As Single)
            Return New PointF(x * grid_w, (y - 1 + V(data(x, y - 1), data(x, y), threshold)) * grid_h)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Top(x As Integer, y As Integer, threshold As Single)
            Return New PointF((x - 1 + V(data(x - 1, y - 1), data(x, y - 1), threshold)) * grid_w, (y - 1) * grid_h)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Bottom(x As Integer, y As Integer, threshold As Single)
            Return New PointF((x - 1 + V(data(x - 1, y), data(x, y), threshold)) * grid_w, y * grid_h)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function LeftTop(x As Integer, y As Integer)
            Return New PointF((x - 1) * grid_w, (y - 1) * grid_h)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function RightTop(x As Integer, y As Integer)
            Return New PointF(x * grid_w, (y - 1) * grid_h)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function LeftBottom(x As Integer, y As Integer)
            Return New PointF((x - 1) * grid_w, y * grid_h)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function RightBottom(x As Integer, y As Integer)
            Return New PointF(x * grid_w, y * grid_h)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function V(min As Single, max As Single, threshold As Single) As Single
            Return (threshold - min) / (max - min)
        End Function
    End Class
End Namespace
