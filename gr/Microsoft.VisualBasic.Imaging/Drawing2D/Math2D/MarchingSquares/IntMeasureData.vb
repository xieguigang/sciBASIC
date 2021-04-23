Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' 这个是对实际数据做映射为绘图数据之后的结果
    ''' </summary>
    Friend Structure IntMeasureData

        Public X As Integer
        Public Y As Integer
        Public Z As Single

        Public Sub New(md As MeasureData, x_num As Integer, y_num As Integer)
            X = CInt(md.X * x_num)

            If X >= x_num Then
                X = x_num - 1
            End If

            Y = CInt(md.Y * y_num)

            If Y >= y_num Then
                Y = y_num - 1
            End If

            Z = md.Z
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}] {Z}"
        End Function
    End Structure

End Namespace