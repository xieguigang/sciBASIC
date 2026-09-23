''' <summary>
''' 整像素运动估计与运动补偿
''' </summary>
''' <remarks>
''' 首版只做整像素：有界搜索（半径 <see cref="searchRadius"/>）遍历候选位移，
''' 以 SAD 为代价函数取最优。参考平面越界时由 <see cref="H264Plane.at"/> 做边缘复制，
''' 这与解码器的边界外推语义一致，因此编码器的重建与解码器逐样点相同。
''' </remarks>
Friend Class H264MotionEstimation

    ''' <summary>整像素搜索半径</summary>
    Friend Const searchRadius As Integer = 16

    ''' <summary>
    ''' 为 (x0, y0) 处的方块做有界整像素搜索。
    ''' </summary>
    ''' <returns>最小 SAD</returns>
    Friend Shared Function searchInteger(reference As H264Plane, source As H264Plane,
                                         x0 As Integer, y0 As Integer, size As Integer,
                                         ByRef bestDx As Integer, ByRef bestDy As Integer) As Long
        Dim best As Long = Long.MaxValue

        bestDx = 0
        bestDy = 0

        For oy As Integer = -searchRadius To searchRadius
            For ox As Integer = -searchRadius To searchRadius
                Dim cost As Long = sad(reference, source, x0, y0, size, ox, oy)

                If cost < best Then
                    best = cost
                    bestDx = ox
                    bestDy = oy
                End If
            Next
        Next

        Return best
    End Function

    ''' <summary>
    ''' 带边缘复制的取样：越界时取最近的有效样点。
    ''' </summary>
    ''' <remarks>
    ''' 这一步必须与解码器一致——运动向量指向画面外时，规范要求做边缘外推（复制边界样点），
    ''' 而 <see cref="H264Plane.at"/> 对越界是直接抛异常，因此不能直接取用。
    ''' </remarks>
    Friend Shared Function sample(plane As H264Plane, x As Integer, y As Integer) As Integer
        Dim sx As Integer = If(x < 0, 0, If(x >= plane.width, plane.width - 1, x))
        Dim sy As Integer = If(y < 0, 0, If(y >= plane.height, plane.height - 1, y))

        Return plane.at(sx, sy)
    End Function

    ''' <summary>参考帧 (x0+ox, y0+oy) 处的方块与源帧 (x0, y0) 处方块的绝对差之和</summary>
    Friend Shared Function sad(reference As H264Plane, source As H264Plane,
                               x0 As Integer, y0 As Integer, size As Integer,
                               ox As Integer, oy As Integer) As Long
        Dim sum As Long = 0

        For y As Integer = 0 To size - 1
            For x As Integer = 0 To size - 1
                sum += Math.Abs(sample(source, x0 + x, y0 + y) - sample(reference, x0 + ox + x, y0 + oy + y))
            Next
        Next

        Return sum
    End Function

    ''' <summary>
    ''' 运动补偿：取参考平面 (x0+dx, y0+dy) 处的方块，写入 <paramref name="pred"/>（行优先）
    ''' </summary>
    Friend Shared Sub buildPrediction(reference As H264Plane, x0 As Integer, y0 As Integer,
                                      size As Integer, dx As Integer, dy As Integer,
                                      pred As Integer(), predStride As Integer)
        For y As Integer = 0 To size - 1
            For x As Integer = 0 To size - 1
                pred(y * predStride + x) = sample(reference, x0 + dx + x, y0 + dy + y)
            Next
        Next
    End Sub

End Class
