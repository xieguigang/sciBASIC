#Region "Microsoft.VisualBasic::53cb1fafebc3cce7257b0c2c163050dc, nlp\NLP\Tokenizer\src\ChineseTokenizer.vb"

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

    '   Total Lines: 197
    '    Code Lines: 128 (64.97%)
    ' Comment Lines: 45 (22.84%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 24 (12.18%)
    '     File Size: 8.09 KB


    '     Enum SegmentAlgorithm
    ' 
    '         BackwardMaxMatch, BidirectionalMaxMatch, ForwardMaxMatch, Hybrid
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class Tokenizer
    ' 
    '         Properties: Algorithm, Dictionary, Hmm
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CreateDefault, HybridSegment, Segment, SegmentToString
    ' 
    '         Sub: TrainHmm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace ChineseTokenizer

    ''' <summary>
    ''' 分词算法枚举，供 <see cref="Tokenizer"/> 选择内部策略。
    ''' </summary>
    Public Enum SegmentAlgorithm
        ''' <summary>正向最大匹配</summary>
        ForwardMaxMatch
        ''' <summary>逆向最大匹配</summary>
        BackwardMaxMatch
        ''' <summary>双向最大匹配（默认）</summary>
        BidirectionalMaxMatch
        ''' <summary>词典 + HMM 混合分词（推荐）</summary>
        Hybrid
    End Enum

    ''' <summary>
    ''' 中文分词器主入口，整合词典、最大匹配算法与 HMM 模型。
    ''' 工作流程（Hybrid 模式）：
    ''' 1. 使用双向最大匹配对文本进行初步切分；
    ''' 2. 对未登录的连续中文字符段，调用 HMM 进行二次切分；
    ''' 3. 合并结果输出。
    ''' 该类线程安全：内部状态为只读，可在多线程环境中共享实例。
    ''' </summary>
    Public NotInheritable Class Tokenizer

        Private ReadOnly _dict As WordDictionary
        Private ReadOnly _maxMatch As MaxMatchTokenizer
        Private ReadOnly _hmm As HmmModel
        Private _algorithm As SegmentAlgorithm = SegmentAlgorithm.Hybrid

        ''' <summary>当前使用的分词算法。</summary>
        Public Property Algorithm As SegmentAlgorithm
            Get
                Return _algorithm
            End Get
            Set(value As SegmentAlgorithm)
                _algorithm = value
            End Set
        End Property

        ''' <summary>底层词典实例（只读访问）。</summary>
        Public ReadOnly Property Dictionary As WordDictionary
            Get
                Return _dict
            End Get
        End Property

        ''' <summary>底层 HMM 模型实例。</summary>
        Public ReadOnly Property Hmm As HmmModel
            Get
                Return _hmm
            End Get
        End Property

        ''' <summary>
        ''' 使用指定词典构造分词器。HMM 模型使用默认参数。
        ''' </summary>
        Public Sub New(dictionary As WordDictionary)
            _dict = If(dictionary, New WordDictionary())
            _maxMatch = New MaxMatchTokenizer(_dict)
            _hmm = New HmmModel()
        End Sub

        ''' <summary>
        ''' 使用词典文件路径构造分词器。
        ''' </summary>
        Public Sub New(dictPath As String)
            _dict = WordDictionary.LoadFromFile(dictPath)
            _maxMatch = New MaxMatchTokenizer(_dict)
            _hmm = New HmmModel()
        End Sub

        ''' <summary>
        ''' 加载 HMM 训练语料，提升未登录词识别能力。
        ''' </summary>
        Public Sub TrainHmm(corpusPath As String)
            _hmm.Train(corpusPath)
        End Sub

        ''' <summary>
        ''' 对输入文本进行分词，返回词列表。
        ''' </summary>
        Public Function Segment(text As String) As List(Of String)
            If String.IsNullOrEmpty(text) Then Return New List(Of String)()

            Select Case _algorithm
                Case SegmentAlgorithm.ForwardMaxMatch
                    Return _maxMatch.ForwardMaxMatch(text)
                Case SegmentAlgorithm.BackwardMaxMatch
                    Return _maxMatch.BackwardMaxMatch(text)
                Case SegmentAlgorithm.BidirectionalMaxMatch
                    Return _maxMatch.BidirectionalMaxMatch(text)
                Case SegmentAlgorithm.Hybrid
                    Return HybridSegment(text)
                Case Else
                    Return _maxMatch.BidirectionalMaxMatch(text)
            End Select
        End Function

        ''' <summary>
        ''' 对输入文本进行分词，返回以指定分隔符连接的字符串。
        ''' </summary>
        Public Function SegmentToString(text As String, Optional separator As String = " / ") As String
            Dim words As List(Of String) = Segment(text)
            Return String.Join(separator, words)
        End Function

        ''' <summary>
        ''' 混合分词：词典优先，未登录中文段使用 HMM 切分。
        ''' </summary>
        Private Function HybridSegment(text As String) As List(Of String)
            Dim result As New List(Of String)()
            If String.IsNullOrEmpty(text) Then Return result

            Dim i As Integer = 0
            Dim n As Integer = text.Length

            Do While i < n
                Dim ch As Char = text(i)

                ' 非中文字符聚合输出
                If Not MaxMatchTokenizer.IsChineseChar(ch) Then
                    Dim buffer As New StringBuilder()
                    Do While i < n AndAlso Not MaxMatchTokenizer.IsChineseChar(text(i))
                        buffer.Append(text(i))
                        i += 1
                    Loop
                    result.Add(buffer.ToString())
                    Continue Do
                End If

                ' 中文段：先尝试词典最长匹配
                Dim matchedLen As Integer = _dict.FindLongestMatch(text, i, _dict.MaxWordLength)
                If matchedLen > 0 Then
                    result.Add(text.Substring(i, matchedLen))
                    i += matchedLen
                    Continue Do
                End If

                ' 未登录词：收集连续未匹配的中文字符段，交给 HMM 处理
                Dim start As Integer = i
                Dim buf As New StringBuilder()
                buf.Append(text(i))
                i += 1
                Do While i < n AndAlso MaxMatchTokenizer.IsChineseChar(text(i))
                    ' 若当前位置能匹配词典词，则停止累积
                    If _dict.FindLongestMatch(text, i, _dict.MaxWordLength) > 0 Then Exit Do
                    buf.Append(text(i))
                    i += 1
                Loop

                Dim oovSegment As String = buf.ToString()
                If oovSegment.Length = 1 Then
                    result.Add(oovSegment)
                Else
                    ' HMM 解码
                    Dim tags As List(Of String) = _hmm.Decode(oovSegment)
                    Dim words As List(Of String) = HmmModel.TagsToWords(oovSegment, tags)
                    result.AddRange(words)
                End If
            Loop

            Return result
        End Function

        ''' <summary>
        ''' 便捷工厂方法：使用内置默认词典创建分词器。
        ''' </summary>
        Public Shared Function CreateDefault() As Tokenizer
            Dim dict As New WordDictionary()
            ' 内置少量常用词，便于无外部词典时使用
            Dim commonWords() As String = {
                "我", "你", "他", "她", "我们", "你们", "他们",
                "是", "的", "了", "在", "和", "与", "或", "也",
                "中国", "中文", "分词", "自然", "语言", "处理",
                "计算机", "科学", "技术", "研究", "开发", "应用",
                "北京", "上海", "广州", "深圳", "学习", "算法",
                "数据", "结构", "程序", "设计", "系统", "网络",
                "今天", "明天", "昨天", "现在", "未来", "时间",
                "因为", "所以", "虽然", "但是", "如果", "那么",
                "学生", "老师", "朋友", "工作", "生活", "世界",
                "喜欢", "热爱", "希望", "梦想", "努力", "成功",
                "中文分词", "自然语言", "自然语言处理", "人工智能",
                "机器学习", "深度学习", "神经网络", "信息检索"
            }
            For Each w As String In commonWords
                dict.Add(w)
            Next
            Return New Tokenizer(dict) With {.Algorithm = SegmentAlgorithm.Hybrid}
        End Function

    End Class

End Namespace
