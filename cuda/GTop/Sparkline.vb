#Region "Microsoft.VisualBasic::7723ac1af31d7e626de1a133b54de9ef, cuda\GTop\Sparkline.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 38
    '    Code Lines: 22 (57.89%)
    ' Comment Lines: 8 (21.05%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 8 (21.05%)
    '     File Size: 1.40 KB


    ' Module Sparkline
    ' 
    '     Function: LevelChar, Render
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text

''' <summary>
''' 用 Unicode 方块字符（▁▂▃▄▅▆▇█）把一串 0~100 的数值渲染成终端迷你曲线（sparkline）。
''' 每个字符只占 1 个显示列，便于在表格列里对齐。
''' </summary>
Public Module Sparkline

    Private ReadOnly Levels As Char() = {"▁"c, "▂"c, "▃"c, "▄"c, "▅"c, "▆"c, "▇"c, "█"c}

    ''' <summary>
    ''' 取序列末尾 <paramref name="width"/> 个值渲染；样本不足时在左侧补空格。
    ''' </summary>
    Public Function Render(values As IReadOnlyList(Of Integer), width As Integer) As String
        If width <= 0 Then Return String.Empty

        Dim n = If(values Is Nothing, 0, values.Count)
        Dim start = System.Math.Max(0, n - width)
        Dim sb As New StringBuilder()

        ' 样本不足：左补空格，使曲线始终右对齐且等宽
        sb.Append(New String(" "c, width - (n - start)))
        For i As Integer = start To n - 1
            sb.Append(LevelChar(values(i)))
        Next

        Return sb.ToString()
    End Function

    Private Function LevelChar(value As Integer) As Char
        If value < 0 Then Return " "c

        Dim v = System.Math.Min(100, value)
        Dim idx = v * (Levels.Length - 1) \ 100   ' 0..7
        Return Levels(idx)
    End Function
End Module
