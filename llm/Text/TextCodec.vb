#Region "Microsoft.VisualBasic::6d79453a0122e289b8e43175b67e2ac1, llm\Text\TextCodec.vb"

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
    '    Code Lines: 8 (21.05%)
    ' Comment Lines: 22 (57.89%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 8 (21.05%)
    '     File Size: 1.72 KB


    '     Interface ITextCodec
    ' 
    '         Properties: Vocabulary
    ' 
    '         Function: Decode, Encode, TokenIdOf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' ITextCodec —— 文本 ↔ token 的编解码契约
'
' LLM 算法层（本命名空间）刻意<b>不依赖</b>具体的分词器实现：它只要求外部提供
' "文本 → token id"、"token id → 文本"、以及"标记 → token id"三件事。
'
' 这样做有两个好处：
'   * 算法层与 HuggingFace 分词器解耦，可以直接面向任何词表复现；
'   * 约束解码需要的"词表文本视图"（<see cref="Vocabulary"/>）由分词器一侧一次性
'     构建好后注入，算法层不必知道 BPE / WordPiece / byte-level 的差别。
' ---------------------------------------------------------------------------

Namespace Text

    ''' <summary>文本与 token 之间的编解码接口。</summary>
    Public Interface ITextCodec

        ''' <summary>
        ''' 把文本编码为 token 序列。
        ''' </summary>
        ''' <remarks>
        ''' 实现方<b>不应</b>自动追加 BOS / EOS：prompt 里的角色标记与工具调用标记都是
        ''' 显式写出来的，自动追加会破坏协议。
        ''' </remarks>
        Function Encode(text As String) As Integer()

        ''' <summary>把 token 序列解码为文本。</summary>
        Function Decode(ids As IEnumerable(Of Integer)) As String

        ''' <summary>查询标记对应的 token id；词表中不存在时返回 -1。</summary>
        Function TokenIdOf(marker As String) As Integer

        ''' <summary>词表的文本视图（约束解码使用）。</summary>
        ReadOnly Property Vocabulary As TokenizerVocabulary

    End Interface

End Namespace

