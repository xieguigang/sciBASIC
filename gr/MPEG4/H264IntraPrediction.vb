' Author:
' 
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

''' <summary>
''' 帧内预测的模式编号（与 H.264 规范一致）。
''' </summary>
''' <remarks>
''' 亮度 I_16x16 与色度的 4 种模式编号相同（<c>Vertical=0, Horizontal=1, DC=2, Plane=3</c>），
''' 写入码流时：I_16x16 的模式直接进 mb_type；色度的模式则用 unary 编码单独传输。
''' </remarks>
Friend Enum H264IntraMode
    Vertical = 0
    Horizontal = 1
    DC = 2
    Plane = 3
End Enum

''' <summary>
''' I_16x16 亮度与 8x8 色度的帧内预测。
''' </summary>
''' <remarks>
''' 公式与 ffmpeg libavcodec/h264pred_template.c 的对应实现一致：
''' 
''' + I_16x16 DC：<c>(Σleft + Σtop + 16) &gt;&gt; 5</c>；只有一侧可用时 <c>(Σ + 8) &gt;&gt; 4</c>；都不可用时为 128
''' + I_16x16 Plane：<c>H = (5*H + 32) &gt;&gt; 6</c>、<c>V = (5*V + 32) &gt;&gt; 6</c>，
'''   <c>a0 = 16*(left[15] + top[15] + 1) - 7*(V + H)</c>，<c>pred = (a0 + y*V + x*H) &gt;&gt; 5</c>
''' + 色度 DC 按 4 个 4x4 子块分别求均值；色度 Plane 的系数为 17/16 与 3
''' 
''' 相邻样点不可用时，规范要求把某些模式回退成"只用一侧求均值"的 DC 变体：
''' 顶部不可用时 Vertical/Plane 非法、左侧不可用时 Horizontal/Plane 非法，
''' 因此编码器在选模式时必须先按可用性收窄候选集合（<see cref="allowedModes"/>）。
''' </remarks>
Friend NotInheritable Class H264IntraPrediction

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 给定左、上相邻样点是否可用，返回合法的模式集合
    ''' </summary>
    Friend Shared Function allowedModes(leftAvailable As Boolean, topAvailable As Boolean) As Boolean()
        Dim allowed As Boolean() = New Boolean(3) {}

        allowed(CInt(H264IntraMode.DC)) = True
        allowed(CInt(H264IntraMode.Vertical)) = topAvailable
        allowed(CInt(H264IntraMode.Horizontal)) = leftAvailable
        allowed(CInt(H264IntraMode.Plane)) = leftAvailable AndAlso topAvailable

        Return allowed
    End Function

    ''' <summary>
    ''' 生成 I_16x16 的预测样点（16x16）
    ''' </summary>
    ''' <param name="plane">重建平面（左邻与上邻样点已就绪）</param>
    ''' <param name="x0">当前宏块左上角 x（像素）</param>
    ''' <param name="y0">当前宏块左上角 y（像素）</param>
    ''' <param name="mode">预测模式</param>
    ''' <param name="leftAvailable">左邻是否可用</param>
    ''' <param name="topAvailable">上邻是否可用</param>
    ''' <param name="pred">输出 16x16 预测样点，按行优先</param>
    Friend Shared Sub predict16x16(plane As H264Plane, x0 As Integer, y0 As Integer,
                                   mode As H264IntraMode,
                                   leftAvailable As Boolean, topAvailable As Boolean,
                                   pred As Integer())

        Select Case mode
            Case H264IntraMode.Vertical
                For x As Integer = 0 To 15
                    Dim v As Integer = plane.at(x0 + x, y0 - 1)

                    For y As Integer = 0 To 15
                        pred(y * 16 + x) = v
                    Next
                Next
            Case H264IntraMode.Horizontal
                For y As Integer = 0 To 15
                    Dim v As Integer = plane.at(x0 - 1, y0 + y)

                    For x As Integer = 0 To 15
                        pred(y * 16 + x) = v
                    Next
                Next
            Case H264IntraMode.DC
                Dim sum As Integer = 0

                For i As Integer = 0 To 15
                    If topAvailable Then sum += plane.at(x0 + i, y0 - 1)
                    If leftAvailable Then sum += plane.at(x0 - 1, y0 + i)
                Next

                Dim dc As Integer

                If topAvailable AndAlso leftAvailable Then
                    dc = (sum + 16) >> 5
                ElseIf topAvailable OrElse leftAvailable Then
                    dc = (sum + 8) >> 4
                Else
                    dc = 128
                End If

                For i As Integer = 0 To pred.Length - 1
                    pred(i) = dc
                Next
            Case Else
                Call predictPlane16x16(plane, x0, y0, pred)
        End Select
    End Sub

    ''' <summary>
    ''' I_16x16 的 plane 模式
    ''' </summary>
    ''' <remarks>
    ''' 按规范 8.3.2.2.1：<c>H = Σ(i+1)*(top[8+i] - top[6-i])</c>、<c>V = Σ(i+1)*(left[8+i] - left[6-i])</c>，
    ''' 其中 <c>i = 0..7</c>。注意 <c>6-i</c> 在 <c>i = 7</c> 时等于 -1，取的是<b>左上角样点</b>
    ''' <c>p[-1,-1]</c>，因此不能只准备 16 个上邻/左邻样点（这里通过 <see cref="topAt"/> 与
    ''' <see cref="leftAt"/> 统一处理越界到角落的情况）。
    ''' </remarks>
    Private Shared Sub predictPlane16x16(plane As H264Plane, x0 As Integer, y0 As Integer, pred As Integer())
        Dim h As Integer = 0
        Dim v As Integer = 0

        For i As Integer = 0 To 7
            h += (i + 1) * (topAt(plane, x0, y0, 8 + i) - topAt(plane, x0, y0, 6 - i))
            v += (i + 1) * (leftAt(plane, x0, y0, 8 + i) - leftAt(plane, x0, y0, 6 - i))
        Next

        h = (5 * h + 32) >> 6
        v = (5 * v + 32) >> 6

        Dim a0 As Integer = 16 * (plane.at(x0 - 1, y0 + 15) + plane.at(x0 + 15, y0 - 1) + 1) - 7 * (v + h)

        For y As Integer = 0 To 15
            For x As Integer = 0 To 15
                pred(y * 16 + x) = clip((a0 + y * v + x * h) >> 5)
            Next
        Next
    End Sub

    ''' <summary>取上邻样点；x 为 -1 时取左上角样点</summary>
    Private Shared Function topAt(plane As H264Plane, x0 As Integer, y0 As Integer, x As Integer) As Integer
        If x < 0 Then Return plane.at(x0 - 1, y0 - 1)

        Return plane.at(x0 + x, y0 - 1)
    End Function

    ''' <summary>取左邻样点；y 为 -1 时取左上角样点</summary>
    Private Shared Function leftAt(plane As H264Plane, x0 As Integer, y0 As Integer, y As Integer) As Integer
        If y < 0 Then Return plane.at(x0 - 1, y0 - 1)

        Return plane.at(x0 - 1, y0 + y)
    End Function

    ''' <summary>
    ''' 生成色度 8x8 的预测样点
    ''' </summary>
    Friend Shared Sub predict8x8(plane As H264Plane, x0 As Integer, y0 As Integer,
                                 mode As H264IntraMode,
                                 leftAvailable As Boolean, topAvailable As Boolean,
                                 pred As Integer())

        Select Case mode
            Case H264IntraMode.Vertical
                For x As Integer = 0 To 7
                    Dim v As Integer = plane.at(x0 + x, y0 - 1)

                    For y As Integer = 0 To 7
                        pred(y * 8 + x) = v
                    Next
                Next
            Case H264IntraMode.Horizontal
                For y As Integer = 0 To 7
                    Dim v As Integer = plane.at(x0 - 1, y0 + y)

                    For x As Integer = 0 To 7
                        pred(y * 8 + x) = v
                    Next
                Next
            Case H264IntraMode.DC
                Call predictDc8x8(plane, x0, y0, leftAvailable, topAvailable, pred)
            Case Else
                Call predictPlane8x8(plane, x0, y0, pred)
        End Select
    End Sub

    ''' <summary>
    ''' 色度 DC：8x8 拆成 4 个 4x4 子块，各自取左/上均值
    ''' </summary>
    Private Shared Sub predictDc8x8(plane As H264Plane, x0 As Integer, y0 As Integer,
                                    leftAvailable As Boolean, topAvailable As Boolean,
                                    pred As Integer())
        Dim dcs As Integer() = New Integer(3) {}

        If leftAvailable AndAlso topAvailable Then
            Dim sumTop%, sumLeft%

            For i As Integer = 0 To 3
                sumLeft += plane.at(x0 - 1, y0 + i)
                sumTop += plane.at(x0 + i, y0 - 1)
            Next

            dcs(0) = (sumLeft + sumTop + 4) >> 3

            Dim sumTopRight%, sumLeftBottom%

            For i As Integer = 0 To 3
                sumTopRight += plane.at(x0 + 4 + i, y0 - 1)
                sumLeftBottom += plane.at(x0 - 1, y0 + 4 + i)
            Next

            dcs(1) = (sumTopRight + 2) >> 2
            dcs(2) = (sumLeftBottom + 2) >> 2
            dcs(3) = (dcs(1) + dcs(2) + 4) >> 3
        ElseIf leftAvailable Then
            Dim sumLeftTop%, sumLeftBottom%

            For i As Integer = 0 To 3
                sumLeftTop += plane.at(x0 - 1, y0 + i)
                sumLeftBottom += plane.at(x0 - 1, y0 + 4 + i)
            Next

            dcs(0) = (sumLeftTop + 2) >> 2
            dcs(1) = dcs(0)
            dcs(2) = (sumLeftBottom + 2) >> 2
            dcs(3) = dcs(2)
        ElseIf topAvailable Then
            Dim sumTopLeft%, sumTopRight%

            For i As Integer = 0 To 3
                sumTopLeft += plane.at(x0 + i, y0 - 1)
                sumTopRight += plane.at(x0 + 4 + i, y0 - 1)
            Next

            dcs(0) = (sumTopLeft + 2) >> 2
            dcs(1) = (sumTopRight + 2) >> 2
            dcs(2) = dcs(0)
            dcs(3) = dcs(1)
        Else
            For i As Integer = 0 To 3
                dcs(i) = 128
            Next
        End If

        For y As Integer = 0 To 7
            For x As Integer = 0 To 7
                pred(y * 8 + x) = dcs(If(y < 4, 0, 2) + If(x < 4, 0, 1))
            Next
        Next
    End Sub

    ''' <summary>
    ''' 色度 plane 模式（系数为 17/16 与 3，与亮度的 5/6 与 7 不同）
    ''' </summary>
    Private Shared Sub predictPlane8x8(plane As H264Plane, x0 As Integer, y0 As Integer, pred As Integer())
        Dim h As Integer = 0
        Dim v As Integer = 0

        For i As Integer = 0 To 3
            h += (i + 1) * (topAt(plane, x0, y0, 4 + i) - topAt(plane, x0, y0, 2 - i))
            v += (i + 1) * (leftAt(plane, x0, y0, 4 + i) - leftAt(plane, x0, y0, 2 - i))
        Next

        h = (17 * h + 16) >> 5
        v = (17 * v + 16) >> 5

        Dim a0 As Integer = 16 * (plane.at(x0 - 1, y0 + 7) + plane.at(x0 + 7, y0 - 1) + 1) - 3 * (v + h)

        For y As Integer = 0 To 7
            For x As Integer = 0 To 7
                pred(y * 8 + x) = clip((a0 + y * v + x * h) >> 5)
            Next
        Next
    End Sub

    Private Shared Function clip(value As Integer) As Integer
        If value < 0 Then Return 0
        If value > 255 Then Return 255
        Return value
    End Function

End Class
