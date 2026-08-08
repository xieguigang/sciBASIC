#Region "Microsoft.VisualBasic::a3964328e6d50a79370d724857bc0df0, nlp\NLP\Tokenizer\src\HuggingFace\Decoders.vb"

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

    '   Total Lines: 283
    '    Code Lines: 176 (62.19%)
    ' Comment Lines: 40 (14.13%)
    '    - Xml Docs: 92.50%
    ' 
    '   Blank Lines: 67 (23.67%)
    '     File Size: 9.77 KB


    '     Class NullDecoder
    ' 
    '         Properties: Instance
    ' 
    '         Function: Decode
    ' 
    '     Class ByteLevelDecoder
    ' 
    '         Function: Decode
    ' 
    '     Class WordPieceDecoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CleanUp, Decode
    ' 
    '     Class MetaspaceDecoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Decode
    ' 
    '     Class ReplaceDecoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Decode
    ' 
    '     Class StripDecoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Decode
    ' 
    '     Class FuseDecoder
    ' 
    '         Function: Decode
    ' 
    '     Class ByteFallbackDecoder
    ' 
    '         Function: Decode
    ' 
    '     Class SequenceDecoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Decode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
' the sciBASIC framework declares its own 'Encoding' symbol in the global
' namespace, so an explicit alias is required here to reference the BCL type.
Imports TextEncoding = System.Text.Encoding

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' 空解码器：直接把 token 首尾相接。
    ''' </summary>
    Public NotInheritable Class NullDecoder : Implements IDecoder

        Public Shared ReadOnly Property Instance As New NullDecoder

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            Return String.Concat(tokens)
        End Function

    End Class

    ''' <summary>
    ''' ByteLevel 解码器：把映射字符还原为字节序列之后按 UTF-8 解码。
    ''' </summary>
    ''' <remarks>
    ''' 必须先把所有 token 拼接起来<b>再统一还原</b>，因为一个多字节的 UTF-8 字符
    ''' 完全有可能被拆分到相邻的两个 token 之中，逐个 token 解码会产生乱码。
    ''' </remarks>
    Public NotInheritable Class ByteLevelDecoder : Implements IDecoder

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            Return ByteLevelAlphabet.DecodeToString(String.Concat(tokens))
        End Function

    End Class

    ''' <summary>
    ''' WordPiece 解码器：去除续接前缀并以空格连接。
    ''' </summary>
    Public NotInheritable Class WordPieceDecoder : Implements IDecoder

        Private ReadOnly _prefix As String
        Private ReadOnly _cleanup As Boolean

        Public Sub New(prefix As String, cleanup As Boolean)
            _prefix = If(prefix, "##")
            _cleanup = cleanup
        End Sub

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            Dim sb As New StringBuilder
            Dim first As Boolean = True

            For Each token As String In tokens
                If token Is Nothing Then
                    Continue For
                End If

                If token.StartsWith(_prefix, StringComparison.Ordinal) Then
                    sb.Append(token, _prefix.Length, token.Length - _prefix.Length)
                Else
                    If Not first Then
                        sb.Append(" "c)
                    End If

                    sb.Append(token)
                End If

                first = False
            Next

            Dim result As String = sb.ToString()

            If _cleanup Then
                result = CleanUp(result)
            End If

            Return result
        End Function

        ''' <summary>
        ''' 清理由分词过程引入的多余空格，与 huggingface 的实现保持一致。
        ''' </summary>
        Friend Shared Function CleanUp(text As String) As String
            Return text _
                .Replace(" .", ".") _
                .Replace(" ?", "?") _
                .Replace(" !", "!") _
                .Replace(" ,", ",") _
                .Replace(" ' ", "'") _
                .Replace(" n't", "n't") _
                .Replace(" 'm", "'m") _
                .Replace(" 's", "'s") _
                .Replace(" 've", "'ve") _
                .Replace(" 're", "'re")
        End Function

    End Class

    ''' <summary>
    ''' Metaspace 解码器：把 <c>▁</c> 还原为空格。
    ''' </summary>
    Public NotInheritable Class MetaspaceDecoder : Implements IDecoder

        Private ReadOnly _replacement As String
        Private ReadOnly _prependScheme As String

        Public Sub New(replacement As String, prependScheme As String)
            _replacement = If(String.IsNullOrEmpty(replacement), MetaspacePreTokenizer.DefaultReplacement.ToString(), replacement)
            _prependScheme = If(prependScheme, "always").ToLowerInvariant()
        End Sub

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            Dim text As String = String.Concat(tokens).Replace(_replacement, " ")

            ' the leading whitespace is introduced by the pre tokenizer itself
            If _prependScheme <> "never" AndAlso text.StartsWith(" "c) Then
                text = text.Substring(1)
            End If

            Return text
        End Function

    End Class

    ''' <summary>
    ''' 替换解码器。
    ''' </summary>
    Public NotInheritable Class ReplaceDecoder : Implements IDecoder

        Private ReadOnly _pattern As String
        Private ReadOnly _regex As Regex
        Private ReadOnly _content As String

        Public Sub New(pattern As String, isRegex As Boolean, content As String)
            _pattern = If(pattern, String.Empty)
            _content = If(content, String.Empty)

            If isRegex Then
                _regex = New Regex(_pattern, RegexOptions.Compiled Or RegexOptions.CultureInvariant)
            End If
        End Sub

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            Dim text As String = String.Concat(tokens)

            If _pattern.Length = 0 Then
                Return text
            ElseIf _regex Is Nothing Then
                Return text.Replace(_pattern, _content)
            Else
                Return _regex.Replace(text, _content)
            End If
        End Function

    End Class

    ''' <summary>
    ''' 首尾裁剪解码器。
    ''' </summary>
    Public NotInheritable Class StripDecoder : Implements IDecoder

        Private ReadOnly _content As Char
        Private ReadOnly _start As Integer
        Private ReadOnly _stop As Integer

        Public Sub New(content As Char, start As Integer, [stop] As Integer)
            _content = content
            _start = start
            _stop = [stop]
        End Sub

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            Dim sb As New StringBuilder

            For Each token As String In tokens
                If token Is Nothing Then
                    Continue For
                End If

                Dim from As Integer = 0
                Dim [to] As Integer = token.Length

                For i As Integer = 1 To _start
                    If from < [to] AndAlso token(from) = _content Then
                        from += 1
                    End If
                Next
                For i As Integer = 1 To _stop
                    If [to] > from AndAlso token([to] - 1) = _content Then
                        [to] -= 1
                    End If
                Next

                sb.Append(token, from, [to] - from)
            Next

            Return sb.ToString()
        End Function

    End Class

    ''' <summary>
    ''' 熔合解码器：直接拼接全部 token。
    ''' </summary>
    Public NotInheritable Class FuseDecoder : Implements IDecoder

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            Return String.Concat(tokens)
        End Function

    End Class

    ''' <summary>
    ''' 字节回退解码器：把 <c>&lt;0xXX&gt;</c> 形式的 token 还原为原始字节。
    ''' </summary>
    Public NotInheritable Class ByteFallbackDecoder : Implements IDecoder

        Private Shared ReadOnly bytePattern As New Regex("^<0x([0-9A-Fa-f]{2})>$", RegexOptions.Compiled)

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            Dim sb As New StringBuilder
            Dim pending As New List(Of Byte)

            For Each token As String In tokens
                If token Is Nothing Then
                    Continue For
                End If

                Dim m As Match = bytePattern.Match(token)

                If m.Success Then
                    pending.Add(Convert.ToByte(m.Groups(1).Value, 16))
                Else
                    If pending.Count > 0 Then
                        sb.Append(TextEncoding.UTF8.GetString(pending.ToArray()))
                        pending.Clear()
                    End If

                    sb.Append(token)
                End If
            Next

            If pending.Count > 0 Then
                sb.Append(TextEncoding.UTF8.GetString(pending.ToArray()))
            End If

            Return sb.ToString()
        End Function

    End Class

    ''' <summary>
    ''' 组合解码器：按顺序依次应用子解码器。
    ''' </summary>
    ''' <remarks>
    ''' 第一个子解码器接收 token 序列，其输出会作为单元素序列继续传递给后续的解码器。
    ''' </remarks>
    Public NotInheritable Class SequenceDecoder : Implements IDecoder

        Private ReadOnly _items As IDecoder()

        Public Sub New(items As IEnumerable(Of IDecoder))
            _items = If(items Is Nothing, New IDecoder() {}, items.Where(Function(i) i IsNot Nothing).ToArray())
        End Sub

        Public Function Decode(tokens As IEnumerable(Of String)) As String Implements IDecoder.Decode
            If _items.Length = 0 Then
                Return String.Concat(tokens)
            End If

            Dim text As String = _items(0).Decode(tokens)

            For i As Integer = 1 To _items.Length - 1
                text = _items(i).Decode({text})
            Next

            Return text
        End Function

    End Class

End Namespace
