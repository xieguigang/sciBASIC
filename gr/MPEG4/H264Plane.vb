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
''' 单个 8bit 图像平面（亮度或色度），带独立步长。
''' </summary>
''' <remarks>
''' 编码器需要频繁按 4x4 块读写样点，加上重建帧还要作为后续预测的参考，
''' 因此这里用连续的一维数组加步长来承载，避免二维数组的边界检查开销。
''' 平面尺寸已按宏块对齐，右侧/下方的多余样点用于预测时越界的相邻样点。
''' </remarks>
Friend Class H264Plane

    Friend ReadOnly Property width As Integer
    Friend ReadOnly Property height As Integer
    Friend ReadOnly Property stride As Integer
    Friend ReadOnly Property data As Byte()

    Friend Sub New(width As Integer, height As Integer)
        Me.width = width
        Me.height = height
        Me.stride = width
        Me.data = New Byte(width * height - 1) {}
    End Sub

    ''' <summary>
    ''' 把整幅平面填充为同一个值
    ''' </summary>
    Friend Sub fill(value As Integer)
        Dim v As Byte = CByte(value And &HFF)

        For i As Integer = 0 To data.Length - 1
            data(i) = v
        Next
    End Sub

    Friend Function at(x As Integer, y As Integer) As Integer
        Return data(y * stride + x)
    End Function

    Friend Sub setAt(x As Integer, y As Integer, value As Integer)
        If value < 0 Then value = 0
        If value > 255 Then value = 255

        data(y * stride + x) = CByte(value)
    End Sub

    ''' <summary>
    ''' 用左/上边界的样点复制来补齐到宏块对齐区域之外的部分
    ''' </summary>
    ''' <param name="visibleWidth">实际可见宽度</param>
    ''' <param name="visibleHeight">实际可见高度</param>
    Friend Sub padEdges(visibleWidth As Integer, visibleHeight As Integer)
        For y As Integer = 0 To height - 1
            Dim last As Integer = at(Math.Max(0, visibleWidth - 1), Math.Min(y, visibleHeight - 1))

            For x As Integer = visibleWidth To width - 1
                setAt(x, y, last)
            Next
        Next

        For y As Integer = visibleHeight To height - 1
            For x As Integer = 0 To width - 1
                setAt(x, y, at(x, Math.Max(0, visibleHeight - 1)))
            Next
        Next
    End Sub

End Class

''' <summary>
''' 一幅 4:2:0 YUV 图像（亮度 + 两个色度平面，均已按宏块对齐）。
''' </summary>
''' <remarks>
''' 由 RGBA 帧转换而来：BT.601 的 studio range 整数公式，色度用 2x2 平均下采样。
''' 转换结果同时作为「原始帧」参与残差计算与「重建帧」被逐块覆盖，
''' 因此 <see cref="clone"/> 用于保留原始样点。
''' </remarks>
Friend Class H264Yuv420

    Friend ReadOnly Property codedWidth As Integer
    Friend ReadOnly Property codedHeight As Integer

    ''' <summary>实际可见宽度（用于边界补齐与裁剪）</summary>
    Friend ReadOnly Property width As Integer

    ''' <summary>实际可见高度</summary>
    Friend ReadOnly Property height As Integer

    Friend ReadOnly Property y As H264Plane
    Friend ReadOnly Property u As H264Plane
    Friend ReadOnly Property v As H264Plane

    Friend Sub New(codedWidth As Integer, codedHeight As Integer, width As Integer, height As Integer)
        Me.codedWidth = codedWidth
        Me.codedHeight = codedHeight
        Me.width = width
        Me.height = height
        Me.y = New H264Plane(codedWidth, codedHeight)
        Me.u = New H264Plane(codedWidth \ 2, codedHeight \ 2)
        Me.v = New H264Plane(codedWidth \ 2, codedHeight \ 2)
    End Sub

    ''' <summary>
    ''' 复制一份（用于保留原始帧，编码过程中的重建会就地覆盖）
    ''' </summary>
    Friend Function clone() As H264Yuv420
        Dim copy As New H264Yuv420(codedWidth, codedHeight, width, height)

        Call Array.Copy(y.data, copy.y.data, y.data.Length)
        Call Array.Copy(u.data, copy.u.data, u.data.Length)
        Call Array.Copy(v.data, copy.v.data, v.data.Length)

        Return copy
    End Function

    ''' <summary>
    ''' 由扁平的 BGRA 字节数组（每像素 4 字节，b/g/r/a 顺序）载入画面。
    ''' </summary>
    Friend Sub loadBgra(bgra As Byte(), pixelWidth As Integer)
        For j As Integer = 0 To height - 1
            For i As Integer = 0 To width - 1
                Dim p As Integer = (j * pixelWidth + i) * 4
                Dim b As Integer = bgra(p)
                Dim g As Integer = bgra(p + 1)
                Dim r As Integer = bgra(p + 2)

                ' 注意：必须先加上偏移量再钳位。若写成 clip(delta) + 16，负的 delta 会被截断成 0，
                ' 整幅画面会被系统性抬高（色度同理，会直接毁掉色度）
                Call y.setAt(i, j, clip(((66 * r + 129 * g + 25 * b + 128) >> 8) + 16))
            Next
        Next

        For j As Integer = 0 To height \ 2 - 1
            For i As Integer = 0 To width \ 2 - 1
                Dim sumB%, sumG%, sumR%
                Dim n% = 0

                For dy As Integer = 0 To 1
                    For dx As Integer = 0 To 1
                        Dim x As Integer = i * 2 + dx
                        Dim yy As Integer = j * 2 + dy

                        If x < width AndAlso yy < height Then
                            Dim p As Integer = (yy * pixelWidth + x) * 4

                            sumB += bgra(p)
                            sumG += bgra(p + 1)
                            sumR += bgra(p + 2)
                            n += 1
                        End If
                    Next
                Next

                Dim bAvg As Integer = sumB \ n
                Dim gAvg As Integer = sumG \ n
                Dim rAvg As Integer = sumR \ n

                ' 同上：偏移量要参与钳位，否则所有负的色度差都会被截成 0
                Call u.setAt(i, j, clip(((-38 * rAvg - 74 * gAvg + 112 * bAvg + 128) >> 8) + 128))
                Call v.setAt(i, j, clip(((112 * rAvg - 94 * gAvg - 18 * bAvg + 128) >> 8) + 128))
            Next
        Next

        Call y.padEdges(width, height)
        Call u.padEdges(Math.Max(1, width \ 2), Math.Max(1, height \ 2))
        Call v.padEdges(Math.Max(1, width \ 2), Math.Max(1, height \ 2))
    End Sub

    ''' <summary>
    ''' 按规格把样点限制到 0 - 255
    ''' </summary>
    Private Shared Function clip(value As Integer) As Integer
        If value < 0 Then Return 0
        If value > 255 Then Return 255
        Return value
    End Function

End Class
