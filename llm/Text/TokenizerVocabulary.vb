#Region "Microsoft.VisualBasic::4420d8377e5a4c0e4f0ca56ddf46a9be, llm\Text\TokenizerVocabulary.vb"

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

    '   Total Lines: 138
    '    Code Lines: 72 (52.17%)
    ' Comment Lines: 35 (25.36%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 31 (22.46%)
    '     File Size: 5.83 KB


    '     Class TokenizerVocabulary
    ' 
    '         Properties: FirstCharacters, Size, SpecialTokens, UsableTokens
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BucketOf, BucketSizeOf, IsSpecial, TextOf, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' TokenizerVocabulary —— 词表的"文本视图"与首字符分桶
'
' 约束解码需要在"logits 的 10 万维空间"里找出哪些 token 是合法的。逐个 token 去跑
' 状态机是 O(V) 次字符串操作，代价不可接受；而只靠首字符判断又太粗（例如 schema 要求
' 输出 <c>"celsius"</c>，首字符为引号的 token 有几万个）。
'
' 本类提供两级索引，把"候选集"从 10 万压到几十个量级：
'
'   1. 首字符分桶  ——"以字符 c 开头"的 token 集合，O(1) 取桶；
'   2. 精确状态机  ——桶内每个 token 再用状态机逐个字符验证。
'
' 由于约束解码的状态机每步只允许极少数首字符（键名首字符、数字、引号…），
' 绝大多数桶会被整体跳过，于是单步代价 ≈ O(合法 token 数 × token 平均长度)。
' ---------------------------------------------------------------------------

Namespace Text

    ''' <summary>
    ''' 词表的文本视图：把 token id 映射回它实际贡献的文本，并建立首字符倒排索引。
    ''' </summary>
    Public Class TokenizerVocabulary

        Private ReadOnly _idToText As String()
        Private ReadOnly _special As Boolean()
        Private ReadOnly _buckets As New Dictionary(Of Char, List(Of Integer))()
        Private ReadOnly _firstCharacters As Char()

        ''' <summary>词表大小。</summary>
        Public ReadOnly Property Size As Integer
            Get
                Return _idToText.Length
            End Get
        End Property

        ''' <summary>词表中出现过的不重复首字符（约束解码按它遍历，避免遍历 65536 个字符）。</summary>
        Public ReadOnly Property FirstCharacters As Char()
            Get
                Return _firstCharacters
            End Get
        End Property

        ''' <summary>文本非空、可用于约束解码的 token 个数。</summary>
        Public ReadOnly Property UsableTokens As Integer

        ''' <summary>特殊 token（角色标记、工具调用标记等）的个数；它们永远不参与约束解码。</summary>
        Public ReadOnly Property SpecialTokens As Integer

        ''' <param name="idToText">
        ''' 下标为 token id、值为该 token 单独解码后贡献的文本。空字符串表示该 token
        ''' 无法在约束解码中使用（特殊 token 或上下文相关 token）。
        ''' </param>
        ''' <param name="special">
        ''' 与 <paramref name="idToText"/> 等长的标记数组，True 表示该 id 是保留的特殊 token。
        ''' </param>
        Public Sub New(idToText As String(), Optional special As Boolean() = Nothing)
            If idToText Is Nothing Then Throw New ArgumentNullException(NameOf(idToText))

            _idToText = idToText
            _special = special

            Dim usable As Integer = 0
            Dim specialCount As Integer = 0
            Dim seen As New HashSet(Of Char)

            For id As Integer = 0 To idToText.Length - 1
                If _special IsNot Nothing AndAlso id < _special.Length AndAlso _special(id) Then
                    specialCount += 1
                    Continue For
                End If

                Dim text = idToText(id)

                If String.IsNullOrEmpty(text) Then Continue For

                Dim c = text(0)

                If Char.IsControl(c) Then Continue For

                Dim bucket As List(Of Integer) = Nothing

                If Not _buckets.TryGetValue(c, bucket) Then
                    bucket = New List(Of Integer)()
                    _buckets.Add(c, bucket)
                    Call seen.Add(c)
                End If

                bucket.Add(id)
                usable += 1
            Next

            UsableTokens = usable
            SpecialTokens = specialCount

            Dim chars(seen.Count - 1) As Char
            Call seen.CopyTo(chars)

            _firstCharacters = chars
        End Sub

        ''' <summary>取某个 token 贡献的文本；非法 id 返回空串。</summary>
        Public Function TextOf(id As Integer) As String
            If id < 0 OrElse id >= _idToText.Length Then Return String.Empty
            Return _idToText(id)
        End Function

        ''' <summary>该 id 是否为保留的特殊 token。</summary>
        Public Function IsSpecial(id As Integer) As Boolean
            If _special Is Nothing Then Return False
            If id < 0 OrElse id >= _special.Length Then Return False
            Return _special(id)
        End Function

        ''' <summary>取"以字符 <paramref name="c"/> 开头"的 token 列表；不存在时返回空列表。</summary>
        Public Function BucketOf(c As Char) As List(Of Integer)
            Dim bucket As List(Of Integer) = Nothing

            If _buckets.TryGetValue(c, bucket) Then Return bucket

            Return EmptyBucket
        End Function

        Private Shared ReadOnly EmptyBucket As New List(Of Integer)()

        ''' <summary>词表中以给定字符开头的 token 个数（诊断用）。</summary>
        Public Function BucketSizeOf(c As Char) As Integer
            Return BucketOf(c).Count
        End Function

        ''' <summary>Returns a short summary of the vocabulary.</summary>
        ''' <returns>A text that reports the total, usable and special token counts.</returns>
        Public Overrides Function ToString() As String
            Return $"vocab[{Size}] usable={UsableTokens}, special={SpecialTokens}, first_chars={_firstCharacters.Length}"
        End Function

    End Class

End Namespace

