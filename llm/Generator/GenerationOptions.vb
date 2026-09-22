#Region "Microsoft.VisualBasic::b2a7d5d833ee9dc06591db7612a51339, llm\Generator\GenerationOptions.vb"

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

    '   Total Lines: 29
    '    Code Lines: 9 (31.03%)
    ' Comment Lines: 12 (41.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (27.59%)
    '     File Size: 1.14 KB


    '     Class GenerationOptions
    ' 
    '         Properties: LogitsProcessor, MaxNewTokens, StopTokenIds, TrackTiming, UseCache
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Generator

    ''' <summary>生成超参。</summary>
    Public Class GenerationOptions

        ''' <summary>最多新生成的 token 个数。</summary>
        Public Property MaxNewTokens As Integer = 32

        ''' <summary>是否启用 KV Cache（关闭即退化为每步重算全部前缀）。</summary>
        Public Property UseCache As Boolean = True

        ''' <summary>遇到这些 token 就停止生成（通常是 EOS / 工具调用结束标记）。</summary>
        Public Property StopTokenIds As Integer()

        ''' <summary>
        ''' 采样前的 logits 处理器：<c>(logits, stepIndex) → logits</c>。
        ''' </summary>
        ''' <remarks>
        ''' 约束解码在这里把非法 token 的 logits 置为 <c>-∞</c>。
        ''' 传入的数组会被原地修改，返回同一个引用即可。
        ''' </remarks>
        Public Property LogitsProcessor As Func(Of Double(), Integer, Double())

        ''' <summary>是否记录每一步的耗时明细。</summary>
        Public Property TrackTiming As Boolean = True

    End Class

End Namespace
