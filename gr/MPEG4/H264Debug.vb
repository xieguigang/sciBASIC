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
''' 【开发期诊断工具】编码器与解码器逐 bin 对账用的钩子。
''' </summary>
''' <remarks>
''' 它不参与编码/解码流程，也不是包对外 API 的一部分，只为把「编码器实际写出的 bin 序列」
''' 暴露给验证程序，用来定位算术编码与解码的分叉点。<see cref="Enabled"/> 默认为 False，
''' 关闭时不产生任何记录开销；码流通过验收后可连同 <see cref="H264CabacDebugReader"/> 一并删除。
''' </remarks>
Public NotInheritable Class H264Debug

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 置为 True 时，<see cref="H264Cabac"/> 会把每个 bin 记入 <see cref="Bins"/>
    ''' </summary>
    Public Shared Property Enabled As Boolean

    ''' <summary>
    ''' 已编码的 bin 序列：<c>c{ctx}={bin}</c> 为上下文位、<c>t{bin}</c> 为终止位、<c>b{bin}</c> 为旁路位
    ''' </summary>
    Public Shared ReadOnly Property Bins As List(Of String) = New List(Of String)

    ''' <summary>
    ''' 每个 bin 编码完成（含归一化）之后的区间宽度，与解码器侧完全同域，可逐 bin 对账
    ''' </summary>
    Public Shared ReadOnly Property Ranges As List(Of Integer) = New List(Of Integer)

    ''' <summary>
    ''' 供 DC 通路标定使用的数值记录（<c>名称=值</c>）
    ''' </summary>
    Public Shared ReadOnly Property Values As List(Of String) = New List(Of String)

    ''' <summary>记录一个标定数值</summary>
    Public Shared Sub note(name As String, value As Integer)
        If Enabled Then Values.Add($"{name}={value}")
    End Sub

    ''' <summary>
    ''' 【临时诊断】大于 0 时强制使用该 QP 编码，用于判定解码器实际使用的 QP
    ''' </summary>
    Public Shared ForceQp As Integer = 0

    ''' <summary>
    ''' 【临时诊断】大于 0 时把亮度 DC 块的 level 强制为「首系数 = 该值、其余 = 0」，
    ''' 用于把 DC 第二级增益单独测出来
    ''' </summary>
    Public Shared ForceDcLevel As Integer = 0

End Class
