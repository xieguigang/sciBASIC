#Region "Microsoft.VisualBasic::d3c8ed45ab177e044f1aad4770ed679a, Data_science\NLP\Tokenizer\src\MaxMatchTokenizer.vb"

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

    '   Total Lines: 168
    '    Code Lines: 104 (61.90%)
    ' Comment Lines: 38 (22.62%)
    '    - Xml Docs: 71.05%
    ' 
    '   Blank Lines: 26 (15.48%)
    '     File Size: 6.67 KB


    '     Class MaxMatchTokenizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BackwardMaxMatch, BidirectionalMaxMatch, CountSingleCharWords, ForwardMaxMatch, IsChineseChar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports std = System.Math

Namespace ChineseTokenizer

    ''' <summary>
    ''' 基于词典的最大匹配分词算法集合，包含：
    ''' <list type="bullet">
    ''' <item><term>FMM</term><description>正向最大匹配（Forward Maximum Matching）</description></item>
    ''' <item><term>BMM</term><description>逆向最大匹配（Backward Maximum Matching）</description></item>
    ''' <item><term>BiMM</term><description>双向最大匹配（Bidirectional Maximum Matching），取切分歧义较少者</description></item>
    ''' </list>
    ''' 这类算法实现简单、效率高，适合作为基线分词器或与统计模型结合使用。
    ''' </summary>
    Public NotInheritable Class MaxMatchTokenizer

        Private ReadOnly _dict As WordDictionary

        Public Sub New(dictionary As WordDictionary)
            _dict = If(dictionary, New WordDictionary())
        End Sub

        ''' <summary>
        ''' 正向最大匹配：从左到右扫描文本，每次取词典中存在的最长词。
        ''' 时间复杂度 O(n * L)，其中 n 为文本长度，L 为词典最长词长度。
        ''' </summary>
        Public Function ForwardMaxMatch(text As String) As List(Of String)
            Dim result As New List(Of String)()
            If String.IsNullOrEmpty(text) Then Return result

            Dim i As Integer = 0
            Dim n As Integer = text.Length
            Dim maxLen As Integer = std.Min(_dict.MaxWordLength, n)

            Do While i < n
                Dim ch As Char = text(i)

                ' 非中文字符（标点、空格、字母、数字等）按字符类型聚合输出
                If Not IsChineseChar(ch) Then
                    Dim buffer As New StringBuilder()
                    Do While i < n AndAlso Not IsChineseChar(text(i))
                        buffer.Append(text(i))
                        i += 1
                    Loop
                    result.Add(buffer.ToString())
                    Continue Do
                End If

                ' 中文段：尝试最长匹配
                Dim matchedLen As Integer = _dict.FindLongestMatch(text, i, maxLen)
                If matchedLen = 0 Then
                    ' 未登录词：单字切分
                    result.Add(ch.ToString())
                    i += 1
                Else
                    result.Add(text.Substring(i, matchedLen))
                    i += matchedLen
                End If
            Loop

            Return result
        End Function

        ''' <summary>
        ''' 逆向最大匹配：从右到左扫描文本，每次取词典中存在的最长词。
        ''' 在中文中通常比 FMM 略优，因为中文重心常后置。
        ''' </summary>
        Public Function BackwardMaxMatch(text As String) As List(Of String)
            Dim result As New List(Of String)()
            If String.IsNullOrEmpty(text) Then Return result

            Dim n As Integer = text.Length
            Dim j As Integer = n - 1

            ' 临时从右向左收集，最后反转
            Dim reversed As New List(Of String)()

            Do While j >= 0
                Dim ch As Char = text(j)

                If Not IsChineseChar(ch) Then
                    Dim buffer As New StringBuilder()
                    Do While j >= 0 AndAlso Not IsChineseChar(text(j))
                        buffer.Insert(0, text(j))
                        j -= 1
                    Loop
                    reversed.Add(buffer.ToString())
                    Continue Do
                End If

                Dim maxLen As Integer = std.Min(_dict.MaxWordLength, j + 1)
                Dim matchedLen As Integer = 0
                ' 从最长开始尝试
                For L As Integer = maxLen To 1 Step -1
                    Dim candidate As String = text.Substring(j - L + 1, L)
                    If _dict.Contains(candidate) Then
                        matchedLen = L
                        Exit For
                    End If
                Next

                If matchedLen = 0 Then
                    reversed.Add(ch.ToString())
                    j -= 1
                Else
                    reversed.Add(text.Substring(j - matchedLen + 1, matchedLen))
                    j -= matchedLen
                End If
            Loop

            ' 反转结果
            For k As Integer = reversed.Count - 1 To 0 Step -1
                result.Add(reversed(k))
            Next
            Return result
        End Function

        ''' <summary>
        ''' 双向最大匹配：同时执行 FMM 与 BMM，按以下规则选择结果：
        ''' 1. 切分出的词数较少者优先；
        ''' 2. 词数相同时，单字词数量较少者优先；
        ''' 3. 仍相同时，默认采用 BMM（中文重心后置经验）。
        ''' 该方法能有效减少切分歧义。
        ''' </summary>
        Public Function BidirectionalMaxMatch(text As String) As List(Of String)
            Dim fmm As List(Of String) = ForwardMaxMatch(text)
            Dim bmm As List(Of String) = BackwardMaxMatch(text)

            If fmm.Count <> bmm.Count Then
                Return If(fmm.Count < bmm.Count, fmm, bmm)
            End If

            Dim fmmSingle As Integer = CountSingleCharWords(fmm)
            Dim bmmSingle As Integer = CountSingleCharWords(bmm)
            If fmmSingle <> bmmSingle Then
                Return If(fmmSingle < bmmSingle, fmm, bmm)
            End If

            ' 默认偏好 BMM
            Return bmm
        End Function

        ' 统计单字词数量
        Private Shared Function CountSingleCharWords(words As List(Of String)) As Integer
            Dim cnt As Integer = 0
            For Each w As String In words
                If w.Length = 1 AndAlso IsChineseChar(w(0)) Then cnt += 1
            Next
            Return cnt
        End Function

        ''' <summary>
        ''' 判断字符是否为中文字符（CJK 统一表意文字基本区 + 扩展 A 区）。
        ''' </summary>
        Public Shared Function IsChineseChar(ch As Char) As Boolean
            Dim code As Integer = Convert.ToInt32(ch)
            ' CJK 统一表意文字基本区：U+4E00 ~ U+9FFF
            If code >= &H4E00 AndAlso code <= &H9FFF Then Return True
            ' CJK 扩展 A 区：U+3400 ~ U+4DBF
            If code >= &H3400 AndAlso code <= &H4DBF Then Return True
            ' CJK 兼容表意文字：U+F900 ~ U+FAFF
            If code >= &HF900 AndAlso code <= &HFAFF Then Return True
            Return False
        End Function

    End Class

End Namespace

