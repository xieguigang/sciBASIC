Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' 测量数据
    ''' </summary>
    Public Structure MeasureData

        ''' <summary>
        ''' 坐标X
        ''' </summary>
        Public X As Single

        ''' <summary>
        ''' 坐标Y
        ''' </summary>
        Public Y As Single

        ''' <summary>
        ''' 高度
        ''' </summary>
        Public Z As Double

        ''' <summary>
        ''' 初始化测量数据
        ''' </summary>
        ''' <param name="x">坐标x</param>
        ''' <param name="y">坐标y</param>
        ''' <param name="z">高度</param>
        Public Sub New(ByVal x As Single, ByVal y As Single, ByVal z As Double)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub
    End Structure

End Namespace