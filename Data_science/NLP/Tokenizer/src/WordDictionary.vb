#Region "Microsoft.VisualBasic::5060b5ebab396b262ea57ab225045c9f, Data_science\NLP\Tokenizer\src\WordDictionary.vb"

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

    '   Total Lines: 220
    '    Code Lines: 148 (67.27%)
    ' Comment Lines: 44 (20.00%)
    '    - Xml Docs: 93.18%
    ' 
    '   Blank Lines: 28 (12.73%)
    '     File Size: 9.09 KB


    '     Class WordDictionary
    ' 
    '         Properties: Count, MaxWordLength, TotalFrequency
    ' 
    '         Function: Contains, FindLongestMatch, FindNode, FromWords, GetFrequency
    '                   GetPosTag, IsPrefix, LoadFromFile
    ' 
    '         Sub: Add, Load
    '         Class TrieNode
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports std = System.Math

Namespace ChineseTokenizer

    ''' <summary>
    ''' 基于 Trie（前缀树）的中文词典实现。
    ''' 支持快速前缀查询、整词查询以及词频统计。
    ''' 该结构在最大匹配算法中用于 O(L) 复度的最长词查找，
    ''' 其中 L 为待匹配子串的最大长度。
    ''' </summary>
    Public NotInheritable Class WordDictionary

        ' ===== Trie 节点定义 =====
        Private NotInheritable Class TrieNode
            Friend ReadOnly Children As New Dictionary(Of Char, TrieNode)()
            Friend IsEnd As Boolean = False
            Friend Frequency As Long = 0L
            Friend PosTag As String = String.Empty
        End Class

        Private ReadOnly _root As New TrieNode()
        Private _maxWordLength As Integer = 0
        Private _totalWords As Integer = 0
        Private _totalFrequency As Long = 0L

        ''' <summary>词典中收录的最长词的字符数。</summary>
        Public ReadOnly Property MaxWordLength As Integer
            Get
                Return _maxWordLength
            End Get
        End Property

        ''' <summary>词典中词条总数。</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _totalWords
            End Get
        End Property

        ''' <summary>词典中所有词频之和（用于概率归一化）。</summary>
        Public ReadOnly Property TotalFrequency As Long
            Get
                Return _totalFrequency
            End Get
        End Property

        ''' <summary>
        ''' 向词典中添加一个词条。
        ''' </summary>
        ''' <param name="word">待添加的词（不能为空或空白）。</param>
        ''' <param name="frequency">词频，默认为 1。</param>
        ''' <param name="posTag">词性标注（可选）。</param>
        Public Sub Add(word As String, Optional frequency As Long = 1L, Optional posTag As String = "")
            If String.IsNullOrEmpty(word) Then Return
            word = word.Trim()
            If word.Length = 0 Then Return

            Dim node As TrieNode = _root
            For Each ch As Char In word
                Dim child As TrieNode = Nothing
                If Not node.Children.TryGetValue(ch, child) Then
                    child = New TrieNode()
                    node.Children(ch) = child
                End If
                node = child
            Next

            If Not node.IsEnd Then
                _totalWords += 1
                If word.Length > _maxWordLength Then _maxWordLength = word.Length
            End If
            node.IsEnd = True
            node.Frequency += std.Max(frequency, 1L)
            If Not String.IsNullOrEmpty(posTag) Then node.PosTag = posTag
            _totalFrequency += std.Max(frequency, 1L)
        End Sub

        ''' <summary>判断词典中是否包含指定词。</summary>
        Public Function Contains(word As String) As Boolean
            If String.IsNullOrEmpty(word) Then Return False
            Dim node As TrieNode = FindNode(word)
            Return node IsNot Nothing AndAlso node.IsEnd
        End Function

        ''' <summary>获取指定词的词频，不存在返回 0。</summary>
        Public Function GetFrequency(word As String) As Long
            If String.IsNullOrEmpty(word) Then Return 0L
            Dim node As TrieNode = FindNode(word)
            If node Is Nothing OrElse Not node.IsEnd Then Return 0L
            Return node.Frequency
        End Function

        ''' <summary>获取指定词的词性标注，不存在返回空字符串。</summary>
        Public Function GetPosTag(word As String) As String
            If String.IsNullOrEmpty(word) Then Return String.Empty
            Dim node As TrieNode = FindNode(word)
            If node Is Nothing OrElse Not node.IsEnd Then Return String.Empty
            Return node.PosTag
        End Function

        ''' <summary>
        ''' 判断指定前缀是否存在于词典中（即是否存在以该前缀开头的词）。
        ''' 用于最大匹配算法中判断是否可以继续向后扩展。
        ''' </summary>
        Public Function IsPrefix(prefix As String) As Boolean
            If String.IsNullOrEmpty(prefix) Then Return False
            Dim node As TrieNode = FindNode(prefix)
            Return node IsNot Nothing
        End Function

        ' 内部方法：沿 Trie 树查找指定字符串对应的节点
        Private Function FindNode(s As String) As TrieNode
            Dim node As TrieNode = _root
            For Each ch As Char In s
                Dim child As TrieNode = Nothing
                If Not node.Children.TryGetValue(ch, child) Then Return Nothing
                node = child
            Next
            Return node
        End Function

        ''' <summary>
        ''' 从词典文件加载词条。文件格式：每行一个词，可选词频和词性，以制表符或空格分隔。
        ''' 以 # 开头的行为注释。
        ''' </summary>
        ''' <param name="path">词典文件路径。</param>
        ''' <param name="encoding">文件编码，默认 UTF-8。</param>
        Public Sub Load(path As String, Optional encoding As Encoding = Nothing)
            If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then Return
            If encoding Is Nothing Then encoding = Encoding.UTF8

            Using reader As New StreamReader(path, encoding)
                Dim line As String = reader.ReadLine()
                Do While line IsNot Nothing
                    If String.IsNullOrWhiteSpace(line) Then
                        line = reader.ReadLine()
                        Continue Do
                    End If
                    line = line.Trim()
                    If line.StartsWith("#"c) Then
                        line = reader.ReadLine()
                        Continue Do
                    End If

                    Dim parts() As String = line.Split(New Char() {ControlChars.Tab, " "c}, StringSplitOptions.RemoveEmptyEntries)
                    If parts.Length = 0 Then
                        line = reader.ReadLine()
                        Continue Do
                    End If

                    Dim word As String = parts(0)
                    Dim freq As Long = 1L
                    Dim pos As String = ""
                    If parts.Length >= 2 AndAlso Long.TryParse(parts(1), freq) Then
                        If parts.Length >= 3 Then pos = parts(2)
                    ElseIf parts.Length >= 2 Then
                        pos = parts(1)
                    End If
                    Me.Add(word, freq, pos)

                    ' 读取下一行
                    line = reader.ReadLine()
                Loop
            End Using
        End Sub

        ''' <summary>从内联字符串集合构造词典（便于单元测试）。</summary>
        Public Shared Function FromWords(ParamArray words() As String) As WordDictionary
            Dim dict As New WordDictionary()
            If words Is Nothing Then Return dict
            For Each w As String In words
                dict.Add(w)
            Next
            Return dict
        End Function

        ''' <summary>
        ''' 从词典文件加载的便捷静态工厂方法。
        ''' </summary>
        Public Shared Function LoadFromFile(path As String, Optional encoding As Encoding = Nothing) As WordDictionary
            Dim dict As New WordDictionary()
            dict.Load(path, encoding)
            Return dict
        End Function

        ''' <summary>
        ''' 从文本 <paramref name="text"/> 的位置 <paramref name="startIdx"/> 开始，
        ''' 查找词典中存在的最长词，返回其长度；未找到返回 0。
        ''' 该方法是最大匹配算法的核心查询接口。
        ''' </summary>
        ''' <param name="text">待匹配文本。</param>
        ''' <param name="startIdx">起始位置。</param>
        ''' <param name="maxLen">允许的最大匹配长度（字符数）。</param>
        ''' <returns>最长匹配词的长度；未匹配返回 0。</returns>
        Public Function FindLongestMatch(text As String, startIdx As Integer, maxLen As Integer) As Integer
            If String.IsNullOrEmpty(text) OrElse startIdx < 0 OrElse startIdx >= text.Length Then Return 0
            If maxLen <= 0 Then Return 0

            Dim node As TrieNode = _root
            Dim bestLen As Integer = 0
            Dim limit As Integer = std.Min(startIdx + maxLen, text.Length)

            For i As Integer = startIdx To limit - 1
                Dim ch As Char = text(i)
                Dim child As TrieNode = Nothing
                If Not node.Children.TryGetValue(ch, child) Then Exit For
                node = child
                If node.IsEnd Then
                    bestLen = i - startIdx + 1
                End If
            Next

            Return bestLen
        End Function

    End Class

End Namespace

