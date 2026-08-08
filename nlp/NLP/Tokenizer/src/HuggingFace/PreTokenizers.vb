#Region "Microsoft.VisualBasic::HuggingFace/PreTokenizers.vb"

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

    '     Enum SplitBehavior
    ' 
    '     Class SequencePreTokenizer, SplitPreTokenizer, ByteLevelPreTokenizer
    '     Class MetaspacePreTokenizer, WhitespacePreTokenizer, WhitespaceSplitPreTokenizer
    '     Class PunctuationPreTokenizer, DigitsPreTokenizer, FixedLengthPreTokenizer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
' the sciBASIC framework exposes its own Min/Max extension methods in the
' global namespace, so an explicit alias is required here.
Imports std = System.Math

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' <c>Split</c> 预分词器对匹配片段的处理方式。
    ''' </summary>
    Public Enum SplitBehavior
        ''' <summary>匹配片段独立成片，未匹配片段同样保留。</summary>
        Isolated
        ''' <summary>丢弃匹配片段，仅保留未匹配片段。</summary>
        Removed
        ''' <summary>匹配片段并入前一个分片。</summary>
        MergedWithPrevious
        ''' <summary>匹配片段并入后一个分片。</summary>
        MergedWithNext
        ''' <summary>连续的匹配片段合并为一个分片。</summary>
        Contiguous
    End Enum

    ''' <summary>
    ''' 组合预分词器：把上一级产出的分片依次交给子预分词器继续细分。
    ''' </summary>
    ''' <remarks>
    ''' 这里采用逐级 map-flatten 的处理方式，即第 N 个子预分词器处理的是第 N-1 个
    ''' 子预分词器<b>产出的全部分片</b>，而不是重新处理原始输入串。
    ''' </remarks>
    Public NotInheritable Class SequencePreTokenizer : Implements IPreTokenizer

        Private ReadOnly _items As IPreTokenizer()

        Public Sub New(items As IEnumerable(Of IPreTokenizer))
            _items = If(items Is Nothing, New IPreTokenizer() {}, items.Where(Function(i) i IsNot Nothing).ToArray())
        End Sub

        Public Function PreTokenize(splits As List(Of Split)) As List(Of Split) Implements IPreTokenizer.PreTokenize
            For Each item As IPreTokenizer In _items
                splits = item.PreTokenize(splits)
            Next

            Return splits
        End Function

    End Class

    ''' <summary>
    ''' 预分词器的抽象基类，负责处理"逐分片应用 + 跳过特殊分片"这一通用逻辑。
    ''' </summary>
    Public MustInherit Class PreTokenizerBase : Implements IPreTokenizer

        Public Function PreTokenize(splits As List(Of Split)) As List(Of Split) Implements IPreTokenizer.PreTokenize
            Dim result As New List(Of Split)(splits.Count)

            For Each split As Split In splits
                If split.IsSpecial Then
                    ' the added tokens should never be splitted any more
                    result.Add(split)
                ElseIf split.Value.Length = 0 Then
                    Continue For
                Else
                    Call SplitOne(split, result)
                End If
            Next

            Return result
        End Function

        ''' <summary>
        ''' 对单个分片执行切分，并把结果追加到 <paramref name="output"/> 之中。
        ''' </summary>
        Protected MustOverride Sub SplitOne(split As Split, output As List(Of Split))

    End Class

    ''' <summary>
    ''' 基于正则表达式的 <c>Split</c> 预分词器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 正则模式直接取自 tokenizer.json 而不做任何硬编码：DeepSeek 模型所使用的模式串
    ''' 中含有真实的换行字符与大量转义，硬编码极易出错。
    ''' </para>
    ''' <para>
    ''' 需要注意 huggingface 的 rust 实现使用的是 <c>fancy-regex</c>，其 Unicode 字符
    ''' 类别（<c>\p{L}</c>、<c>\p{P}</c>、<c>\p{S}</c> 等）的边界定义与 .NET 的实现
    ''' 存在极少量差异，若出现结果不一致应优先排查此处。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SplitPreTokenizer : Inherits PreTokenizerBase

        Private ReadOnly _regex As Regex
        Private ReadOnly _behavior As SplitBehavior
        Private ReadOnly _invert As Boolean

        ''' <summary>
        ''' </summary>
        ''' <param name="pattern">切分所使用的模式。</param>
        ''' <param name="isRegex">
        ''' 为真时 <paramref name="pattern"/> 是正则表达式，否则会被当作字面字符串。
        ''' </param>
        ''' <param name="behavior">匹配片段的处理方式。</param>
        ''' <param name="invert">为真时把"匹配"与"未匹配"的语义对调。</param>
        Public Sub New(pattern As String, isRegex As Boolean, behavior As SplitBehavior, invert As Boolean)
            _behavior = behavior
            _invert = invert
            _regex = New Regex(
                If(isRegex, pattern, Regex.Escape(pattern)),
                RegexOptions.Compiled Or RegexOptions.CultureInvariant
            )
        End Sub

        Protected Overrides Sub SplitOne(split As Split, output As List(Of Split))
            Dim text As String = split.Value
            ' collect the matched / unmatched pieces along with a flag which
            ' indicates whether the piece is produced by the regex pattern
            Dim pieces As New List(Of (Start As Integer, [End] As Integer, Matched As Boolean))
            Dim last As Integer = 0

            For Each m As Match In _regex.Matches(text)
                ' a zero width match would never make any progress
                If m.Length = 0 Then
                    Continue For
                End If

                If m.Index > last Then
                    pieces.Add((last, m.Index, False))
                End If

                pieces.Add((m.Index, m.Index + m.Length, True))
                last = m.Index + m.Length
            Next

            If last < text.Length Then
                pieces.Add((last, text.Length, False))
            End If

            If pieces.Count = 0 Then
                Return
            End If

            If _invert Then
                For i As Integer = 0 To pieces.Count - 1
                    pieces(i) = (pieces(i).Start, pieces(i).End, Not pieces(i).Matched)
                Next
            End If

            Call Emit(split, text, pieces, output)
        End Sub

        ''' <summary>
        ''' 按照 <see cref="SplitBehavior"/> 的语义把切片输出为分片。
        ''' </summary>
        Private Sub Emit(split As Split,
                         text As String,
                         pieces As List(Of (Start As Integer, [End] As Integer, Matched As Boolean)),
                         output As List(Of Split))

            Select Case _behavior
                Case SplitBehavior.Removed
                    For Each p In pieces
                        If Not p.Matched Then
                            Call Append(split, text, p.Start, p.End, output)
                        End If
                    Next

                Case SplitBehavior.MergedWithPrevious
                    Dim start As Integer = -1
                    Dim [end] As Integer = -1

                    For Each p In pieces
                        If start < 0 Then
                            start = p.Start
                        End If

                        [end] = p.End

                        If p.Matched Then
                            Call Append(split, text, start, [end], output)
                            start = -1
                        End If
                    Next

                    If start >= 0 Then
                        Call Append(split, text, start, [end], output)
                    End If

                Case SplitBehavior.MergedWithNext
                    ' a matched piece is merged with the content that follows it,
                    ' so a new fragment always begins at a matched piece
                    Dim i As Integer = 0

                    Do While i < pieces.Count
                        Dim start As Integer = pieces(i).Start
                        Dim [end] As Integer = pieces(i).End

                        If pieces(i).Matched AndAlso i + 1 < pieces.Count Then
                            i += 1
                            [end] = pieces(i).End
                        End If

                        Call Append(split, text, start, [end], output)
                        i += 1
                    Loop

                Case SplitBehavior.Contiguous
                    Dim start As Integer = pieces(0).Start
                    Dim matched As Boolean = pieces(0).Matched
                    Dim [end] As Integer = pieces(0).End

                    For i As Integer = 1 To pieces.Count - 1
                        Dim p = pieces(i)

                        If p.Matched = matched Then
                            [end] = p.End
                        Else
                            Call Append(split, text, start, [end], output)
                            start = p.Start
                            [end] = p.End
                            matched = p.Matched
                        End If
                    Next

                    Call Append(split, text, start, [end], output)

                Case Else ' SplitBehavior.Isolated
                    For Each p In pieces
                        Call Append(split, text, p.Start, p.End, output)
                    Next
            End Select
        End Sub

        Private Shared Sub Append(split As Split, text As String, start As Integer, [end] As Integer, output As List(Of Split))
            If [end] <= start Then
                Return
            End If

            output.Add(New Split(text.Substring(start, [end] - start), split.Start + start, split.Start + [end]))
        End Sub

    End Class

    ''' <summary>
    ''' ByteLevel 预分词器：把分片按 UTF-8 编码后逐字节映射为可见字符。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' "useRegex" 为真时会先套用 GPT-2 的经典切分正则，
    ''' DeepSeek 的模型把它设置为 <c>false</c>，因为其上游已经串联了三个自定义的
    ''' <c>Split</c> 预分词器。
    ''' </para>
    ''' <para>
    ''' <b>注意</b>：<c>add_prefix_space</c> 只在这一处生效。
    ''' <c>post_processor</c> 与 <c>decoder</c> 段中同样存在该字段，但它们只影响
    ''' 偏移量与解码语义，编码阶段<b>绝不可</b>重复添加前导空格，否则整个 id 序列
    ''' 都会发生偏移。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class ByteLevelPreTokenizer : Inherits PreTokenizerBase

        ''' <summary>
        ''' GPT-2 的经典预分词正则表达式。
        ''' </summary>
        Private Shared ReadOnly GPT2Pattern As New Regex(
            "'s|'t|'re|'ve|'m|'ll|'d| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+",
            RegexOptions.Compiled Or RegexOptions.CultureInvariant
        )

        Private ReadOnly _addPrefixSpace As Boolean
        Private ReadOnly _useRegex As Boolean

        Public Sub New(addPrefixSpace As Boolean, useRegex As Boolean)
            _addPrefixSpace = addPrefixSpace
            _useRegex = useRegex
        End Sub

        Protected Overrides Sub SplitOne(split As Split, output As List(Of Split))
            Dim text As String = split.Value

            If _addPrefixSpace AndAlso Not text.StartsWith(" "c) Then
                text = " " & text
            End If

            If _useRegex Then
                For Each m As Match In GPT2Pattern.Matches(text)
                    If m.Length = 0 Then
                        Continue For
                    End If

                    output.Add(New Split(ByteLevelAlphabet.EncodeBytes(m.Value), split.Start, split.End))
                Next
            Else
                output.Add(New Split(ByteLevelAlphabet.EncodeBytes(text), split.Start, split.End))
            End If
        End Sub

    End Class

    ''' <summary>
    ''' Metaspace 预分词器：把空格替换为 <c>▁</c>（U+2581），SentencePiece 系列专用。
    ''' </summary>
    Public NotInheritable Class MetaspacePreTokenizer : Inherits PreTokenizerBase

        ''' <summary>
        ''' SentencePiece 用于表示空格的替代字符。
        ''' </summary>
        Public Const DefaultReplacement As Char = ChrW(&H2581)

        Private ReadOnly _replacement As String
        Private ReadOnly _prependScheme As String
        Private ReadOnly _split As Boolean

        ''' <summary>
        ''' </summary>
        ''' <param name="replacement">用于替换空格的字符。</param>
        ''' <param name="prependScheme">
        ''' <c>always</c> 表示总是在句首补一个替代字符，<c>first</c> 表示仅对第一个
        ''' 分片补，<c>never</c> 表示不补。
        ''' </param>
        ''' <param name="split">为真时按替代字符再切分为多个分片。</param>
        Public Sub New(replacement As String, prependScheme As String, split As Boolean)
            _replacement = If(String.IsNullOrEmpty(replacement), DefaultReplacement.ToString(), replacement)
            _prependScheme = If(prependScheme, "always").ToLowerInvariant()
            _split = split
        End Sub

        Protected Overrides Sub SplitOne(split As Split, output As List(Of Split))
            Dim text As String = split.Value.Replace(" ", _replacement)

            If _prependScheme <> "never" AndAlso Not text.StartsWith(_replacement, StringComparison.Ordinal) Then
                text = _replacement & text
            End If

            If Not _split Then
                output.Add(New Split(text, split.Start, split.End))
                Return
            End If

            ' split on the replacement char, and keep it as the leading char of
            ' each of the produced fragments
            Dim start As Integer = 0

            For i As Integer = 1 To text.Length - 1
                If text.Substring(i, 1) = _replacement Then
                    output.Add(New Split(text.Substring(start, i - start), split.Start, split.End))
                    start = i
                End If
            Next

            If start < text.Length Then
                output.Add(New Split(text.Substring(start), split.Start, split.End))
            End If
        End Sub

    End Class

    ''' <summary>
    ''' 按单词/标点边界切分的预分词器（BERT 的 <c>Whitespace</c>）。
    ''' </summary>
    Public NotInheritable Class WhitespacePreTokenizer : Inherits PreTokenizerBase

        Private Shared ReadOnly pattern As New Regex("\w+|[^\w\s]+", RegexOptions.Compiled Or RegexOptions.CultureInvariant)

        Protected Overrides Sub SplitOne(split As Split, output As List(Of Split))
            For Each m As Match In pattern.Matches(split.Value)
                If m.Length > 0 Then
                    output.Add(New Split(m.Value, split.Start + m.Index, split.Start + m.Index + m.Length))
                End If
            Next
        End Sub

    End Class

    ''' <summary>
    ''' 仅按空白字符切分的预分词器（<c>WhitespaceSplit</c>）。
    ''' </summary>
    Public NotInheritable Class WhitespaceSplitPreTokenizer : Inherits PreTokenizerBase

        Protected Overrides Sub SplitOne(split As Split, output As List(Of Split))
            Dim text As String = split.Value
            Dim start As Integer = -1

            For i As Integer = 0 To text.Length - 1
                If Char.IsWhiteSpace(text(i)) Then
                    If start >= 0 Then
                        output.Add(New Split(text.Substring(start, i - start), split.Start + start, split.Start + i))
                        start = -1
                    End If
                ElseIf start < 0 Then
                    start = i
                End If
            Next

            If start >= 0 Then
                output.Add(New Split(text.Substring(start), split.Start + start, split.End))
            End If
        End Sub

    End Class

    ''' <summary>
    ''' 按标点符号切分的预分词器。
    ''' </summary>
    Public NotInheritable Class PunctuationPreTokenizer : Inherits PreTokenizerBase

        Private ReadOnly _behavior As SplitBehavior

        Public Sub New(behavior As SplitBehavior)
            _behavior = behavior
        End Sub

        Protected Overrides Sub SplitOne(split As Split, output As List(Of Split))
            Dim text As String = split.Value
            Dim start As Integer = 0

            For i As Integer = 0 To text.Length - 1
                If Not IsPunctuation(text(i)) Then
                    Continue For
                End If

                If i > start Then
                    output.Add(New Split(text.Substring(start, i - start), split.Start + start, split.Start + i))
                End If

                If _behavior <> SplitBehavior.Removed Then
                    output.Add(New Split(text.Substring(i, 1), split.Start + i, split.Start + i + 1))
                End If

                start = i + 1
            Next

            If start < text.Length Then
                output.Add(New Split(text.Substring(start), split.Start + start, split.End))
            End If
        End Sub

        ''' <summary>
        ''' 与 huggingface 保持一致：ASCII 区间内的非字母数字字符也视为标点。
        ''' </summary>
        Private Shared Function IsPunctuation(c As Char) As Boolean
            Dim cp As Integer = AscW(c)

            If (cp >= 33 AndAlso cp <= 47) OrElse (cp >= 58 AndAlso cp <= 64) OrElse
               (cp >= 91 AndAlso cp <= 96) OrElse (cp >= 123 AndAlso cp <= 126) Then
                Return True
            End If

            Return Char.IsPunctuation(c) OrElse Char.IsSymbol(c)
        End Function

    End Class

    ''' <summary>
    ''' 数字切分预分词器。
    ''' </summary>
    Public NotInheritable Class DigitsPreTokenizer : Inherits PreTokenizerBase

        Private ReadOnly _individualDigits As Boolean

        ''' <summary>
        ''' </summary>
        ''' <param name="individualDigits">
        ''' 为真时每一位数字都独立成片，否则连续的数字合并为一个分片。
        ''' </param>
        Public Sub New(individualDigits As Boolean)
            _individualDigits = individualDigits
        End Sub

        Protected Overrides Sub SplitOne(split As Split, output As List(Of Split))
            Dim text As String = split.Value
            Dim i As Integer = 0

            Do While i < text.Length
                Dim start As Integer = i
                Dim digit As Boolean = Char.IsDigit(text(i))

                If digit AndAlso _individualDigits Then
                    i += 1
                Else
                    Do While i < text.Length AndAlso Char.IsDigit(text(i)) = digit
                        i += 1
                    Loop
                End If

                output.Add(New Split(text.Substring(start, i - start), split.Start + start, split.Start + i))
            Loop
        End Sub

    End Class

    ''' <summary>
    ''' 定长切分预分词器。
    ''' </summary>
    Public NotInheritable Class FixedLengthPreTokenizer : Inherits PreTokenizerBase

        Private ReadOnly _length As Integer

        Public Sub New(length As Integer)
            _length = std.Max(1, length)
        End Sub

        Protected Overrides Sub SplitOne(split As Split, output As List(Of Split))
            Dim text As String = split.Value
            Dim i As Integer = 0

            Do While i < text.Length
                Dim n As Integer = std.Min(_length, text.Length - i)

                output.Add(New Split(text.Substring(i, n), split.Start + i, split.Start + i + n))
                i += n
            Loop
        End Sub

    End Class

End Namespace
