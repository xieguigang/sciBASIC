Imports System.Drawing

Namespace Filters

    Public Class GaussianSmooth

        ''' <summary>
        ''' 声明私有的高斯模糊卷积核函数
        ''' </summary>
        ReadOnly GaussianBlur As Double(,)

        ''' <summary>
        ''' 初始化高斯模糊卷积核
        ''' </summary>
        ''' <param name="k"></param>
        Sub New(Optional k As Integer = 273)
            GaussianBlur = New Double(4, 4) {
        {1 / k, 4 / k, 7 / k, 4 / k, 1 / k},
        {4 / k, 16 / k, 26 / k, 16 / k, 4 / k},
        {7 / k, 26 / k, 41 / k, 26 / k, 7 / k},
        {4 / k, 16 / k, 26 / k, 16 / k, 4 / k},
        {1 / k, 4 / k, 7 / k, 4 / k, 1 / k}}
        End Sub

        ''' <summary>
        ''' 对图像进行平滑处理（利用高斯平滑Gaussian Blur）
        ''' </summary>
        ''' <param name="bitmap">要处理的位图</param>
        ''' <returns>返回平滑处理后的位图</returns>
        Public Function Smooth(ByVal bitmap As Bitmap) As Bitmap
            Dim InputPicture = New Integer(2, bitmap.Width - 1, bitmap.Height - 1) {} '以GRB以及位图的长宽建立整数输入的位图的数组
            Dim color As Color = New Color() '储存某一像素的颜色
            '循环使得InputPicture数组得到位图的RGB
            For i As Integer = 0 To bitmap.Width - 1

                For j As Integer = 0 To bitmap.Height - 1
                    color = bitmap.GetPixel(i, j)
                    InputPicture(0, i, j) = color.R
                    InputPicture(1, i, j) = color.G
                    InputPicture(2, i, j) = color.B
                Next
            Next

            Dim OutputPicture = New Integer(2, bitmap.Width - 1, bitmap.Height - 1) {} '以GRB以及位图的长宽建立整数输出的位图的数组
            Dim lSmooth As Bitmap = New Bitmap(bitmap.Width, bitmap.Height) '创建新位图
            '循环计算使得OutputPicture数组得到计算后位图的RGB
            For i As Integer = 0 To bitmap.Width - 1

                For j As Integer = 0 To bitmap.Height - 1
                    Dim lR = 0
                    Dim G = 0
                    Dim B = 0

                    '每一个像素计算使用高斯模糊卷积核进行计算
                    For r = 0 To 5 - 1 '循环卷积核的每一行

                        For f = 0 To 5 - 1 '循环卷积核的每一列
                            '控制与卷积核相乘的元素
                            Dim row = i - 2 + r
                            Dim index = j - 2 + f

                            '当超出位图的大小范围时，选择最边缘的像素值作为该点的像素值
                            row = If(row < 0, 0, row)
                            index = If(index < 0, 0, index)
                            row = If(row >= bitmap.Width, bitmap.Width - 1, row)
                            index = If(index >= bitmap.Height, bitmap.Height - 1, index)

                            '输出得到像素的RGB值
                            lR += CInt(GaussianBlur(r, f) * InputPicture(0, row, index))
                            G += CInt(GaussianBlur(r, f) * InputPicture(1, row, index))
                            B += CInt(GaussianBlur(r, f) * InputPicture(2, row, index))
                        Next
                    Next

                    If lR > 255 Then
                        lR = 255
                    ElseIf lR < 0 Then
                        lR = 0
                    End If
                    If G > 255 Then
                        G = 255
                    ElseIf G < 0 Then
                        G = 0
                    End If
                    If B > 255 Then
                        B = 255
                    ElseIf B < 0 Then
                        B = 0
                    End If

                    color = Color.FromArgb(lR, G, B) '颜色结构储存该点RGB
                    lSmooth.SetPixel(i, j, color) '位图存储该点像素值
                Next
            Next

            Return lSmooth
        End Function
    End Class
End Namespace