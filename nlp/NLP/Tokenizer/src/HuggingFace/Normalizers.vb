#Region "Microsoft.VisualBasic::72479d8a10b875ea2eed1ad9f9f4430c, nlp\NLP\Tokenizer\src\HuggingFace\Normalizers.vb"

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

    '   Total Lines: 348
    '    Code Lines: 210 (60.34%)
    ' Comment Lines: 59 (16.95%)
    '    - Xml Docs: 98.31%
    ' 
    '   Blank Lines: 79 (22.70%)
    '     File Size: 11.97 KB


    '     Class NullNormalizer
    ' 
    '         Properties: Instance
    ' 
    '         Function: Normalize
    ' 
    '     Class SequenceNormalizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Normalize
    ' 
    '     Class UnicodeNormalizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Normalize
    ' 
    '     Class LowercaseNormalizer
    ' 
    '         Function: Normalize
    ' 
    '     Class StripAccentsNormalizer
    ' 
    '         Function: Normalize
    ' 
    '     Class StripNormalizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Normalize
    ' 
    '     Class ReplaceNormalizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Normalize
    ' 
    '     Class PrependNormalizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Normalize
    ' 
    '     Class BertNormalizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: IsChineseChar, Normalize
    ' 
    '     Class NmtNormalizer
    ' 
    '         Function: Normalize
    ' 
    '     Class PrecompiledNormalizer
    ' 
    '         Function: Normalize
    ' 
    '         Sub: WarnOnce
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' 空归一化器：原样返回输入文本。
    ''' </summary>
    ''' <remarks>
    ''' 当 tokenizer.json 中的 <c>normalizer</c> 为 <c>null</c> 或者是一个空的
    ''' <c>Sequence</c> 时使用（DeepSeek 的模型即属于后者）。
    ''' </remarks>
    Public NotInheritable Class NullNormalizer : Implements INormalizer

        Public Shared ReadOnly Property Instance As New NullNormalizer

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            Return text
        End Function

    End Class

    ''' <summary>
    ''' 组合归一化器：按顺序依次应用子归一化器。
    ''' </summary>
    Public NotInheritable Class SequenceNormalizer : Implements INormalizer

        Private ReadOnly _items As INormalizer()

        Public Sub New(items As IEnumerable(Of INormalizer))
            _items = If(items Is Nothing, New INormalizer() {}, items.Where(Function(i) i IsNot Nothing).ToArray())
        End Sub

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            For Each item As INormalizer In _items
                text = item.Normalize(text)
            Next

            Return text
        End Function

    End Class

    ''' <summary>
    ''' Unicode 标准化归一化器：NFC / NFD / NFKC / NFKD。
    ''' </summary>
    Public NotInheritable Class UnicodeNormalizer : Implements INormalizer

        Private ReadOnly _form As NormalizationForm

        Public Sub New(form As NormalizationForm)
            _form = form
        End Sub

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            Return text.Normalize(_form)
        End Function

    End Class

    ''' <summary>
    ''' 小写化归一化器。
    ''' </summary>
    Public NotInheritable Class LowercaseNormalizer : Implements INormalizer

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            Return text.ToLowerInvariant()
        End Function

    End Class

    ''' <summary>
    ''' 去除变音符号的归一化器。
    ''' </summary>
    ''' <remarks>
    ''' 先做 NFD 分解，再剔除所有 <c>NonSpacingMark</c> 类别的组合字符。
    ''' </remarks>
    Public NotInheritable Class StripAccentsNormalizer : Implements INormalizer

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            Dim decomposed As String = text.Normalize(NormalizationForm.FormD)
            Dim sb As New StringBuilder(decomposed.Length)

            For Each c As Char In decomposed
                If CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark Then
                    sb.Append(c)
                End If
            Next

            Return sb.ToString()
        End Function

    End Class

    ''' <summary>
    ''' 去除首尾空白字符的归一化器。
    ''' </summary>
    Public NotInheritable Class StripNormalizer : Implements INormalizer

        Private ReadOnly _left As Boolean
        Private ReadOnly _right As Boolean

        Public Sub New(left As Boolean, right As Boolean)
            _left = left
            _right = right
        End Sub

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            If _left Then
                text = text.TrimStart()
            End If
            If _right Then
                text = text.TrimEnd()
            End If

            Return text
        End Function

    End Class

    ''' <summary>
    ''' 字符串/正则替换归一化器。
    ''' </summary>
    Public NotInheritable Class ReplaceNormalizer : Implements INormalizer

        Private ReadOnly _pattern As String
        Private ReadOnly _regex As Regex
        Private ReadOnly _content As String

        ''' <summary>
        ''' </summary>
        ''' <param name="pattern">待替换的模式。</param>
        ''' <param name="isRegex">
        ''' 为真时 <paramref name="pattern"/> 是一个正则表达式，否则为字面字符串。
        ''' </param>
        ''' <param name="content">替换之后的内容。</param>
        Public Sub New(pattern As String, isRegex As Boolean, content As String)
            _pattern = If(pattern, String.Empty)
            _content = If(content, String.Empty)

            If isRegex Then
                _regex = New Regex(_pattern, RegexOptions.Compiled Or RegexOptions.CultureInvariant)
            End If
        End Sub

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            If String.IsNullOrEmpty(text) OrElse _pattern.Length = 0 Then
                Return text
            End If

            If _regex Is Nothing Then
                Return text.Replace(_pattern, _content)
            Else
                Return _regex.Replace(text, _content)
            End If
        End Function

    End Class

    ''' <summary>
    ''' 前缀追加归一化器，常见于 Llama 系列模型（在句首补一个 <c>▁</c>）。
    ''' </summary>
    Public NotInheritable Class PrependNormalizer : Implements INormalizer

        Private ReadOnly _prefix As String

        Public Sub New(prefix As String)
            _prefix = If(prefix, String.Empty)
        End Sub

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            Return _prefix & text
        End Function

    End Class

    ''' <summary>
    ''' BERT 的归一化器：清理控制字符、可选地在 CJK 字符两侧补空格、去变音符号与小写化。
    ''' </summary>
    Public NotInheritable Class BertNormalizer : Implements INormalizer

        Private ReadOnly _cleanText As Boolean
        Private ReadOnly _handleChineseChars As Boolean
        Private ReadOnly _stripAccents As Boolean
        Private ReadOnly _lowercase As Boolean

        Public Sub New(cleanText As Boolean, handleChineseChars As Boolean, stripAccents As Boolean, lowercase As Boolean)
            _cleanText = cleanText
            _handleChineseChars = handleChineseChars
            _stripAccents = stripAccents
            _lowercase = lowercase
        End Sub

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            Dim sb As New StringBuilder(text.Length)

            For Each c As Char In text
                If _cleanText Then
                    Dim code As Integer = AscW(c)

                    If code = 0 OrElse code = &HFFFD Then
                        Continue For
                    ElseIf Char.IsControl(c) AndAlso c <> ChrW(9) AndAlso c <> ChrW(10) AndAlso c <> ChrW(13) Then
                        Continue For
                    ElseIf c = ChrW(9) OrElse c = ChrW(10) OrElse c = ChrW(13) Then
                        sb.Append(" "c)
                        Continue For
                    End If
                End If

                If _handleChineseChars AndAlso IsChineseChar(c) Then
                    sb.Append(" "c).Append(c).Append(" "c)
                Else
                    sb.Append(c)
                End If
            Next

            Dim result As String = sb.ToString()

            If _stripAccents Then
                result = New StripAccentsNormalizer().Normalize(result)
            End If
            If _lowercase Then
                result = result.ToLowerInvariant()
            End If

            Return result
        End Function

        ''' <summary>
        ''' 判断是否为 CJK 统一表意文字区段内的字符。
        ''' </summary>
        Private Shared Function IsChineseChar(c As Char) As Boolean
            Dim cp As Integer = AscW(c)

            Return (cp >= &H4E00 AndAlso cp <= &H9FFF) OrElse
                   (cp >= &H3400 AndAlso cp <= &H4DBF) OrElse
                   (cp >= &HF900 AndAlso cp <= &HFAFF) OrElse
                   (cp >= &H2E80 AndAlso cp <= &H2EFF) OrElse
                   (cp >= &H2F00 AndAlso cp <= &H2FDF)
        End Function

    End Class

    ''' <summary>
    ''' NMT 归一化器：把若干不可见的控制字符剔除，并把各类空白统一为普通空格。
    ''' </summary>
    Public NotInheritable Class NmtNormalizer : Implements INormalizer

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            Dim sb As New StringBuilder(text.Length)

            For Each c As Char In text
                Dim cp As Integer = AscW(c)

                Select Case cp
                    Case &H1, &H2, &H3, &H4, &H5, &H6, &H7, &H8,
                         &HB, &HE, &HF, &H10, &H11, &H12, &H13, &H14,
                         &H15, &H16, &H17, &H18, &H19, &H1A, &H1B, &H1C,
                         &H1D, &H1E, &H1F, &H7F, &H8F, &H9F
                        ' dropped
                    Case &HA0, &HAD, &H180E, &H200B, &H200C, &H200D, &H2028,
                         &H2029, &H2581, &HFEFF, &HFFFD
                        sb.Append(" "c)
                    Case Else
                        sb.Append(c)
                End Select
            Next

            Return sb.ToString()
        End Function

    End Class

    ''' <summary>
    ''' SentencePiece 的 <c>Precompiled</c> 归一化器。
    ''' </summary>
    ''' <remarks>
    ''' 完整实现需要解释 SentencePiece 内嵌的 <c>precompiled_charsmap</c> 有限状态机
    ''' 二进制数据，这里退化为语义上最接近的 NFKC 标准化，并在首次使用时打印一次
    ''' 警告，避免使用者在结果不一致时无从排查。
    ''' </remarks>
    Public NotInheritable Class PrecompiledNormalizer : Implements INormalizer

        Private Shared _warned As Boolean = False
        Private Shared ReadOnly _warnLock As New Object

        Public Function Normalize(text As String) As String Implements INormalizer.Normalize
            Call WarnOnce()

            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            Return text.Normalize(NormalizationForm.FormKC)
        End Function

        ''' <summary>
        ''' 仅在首次调用时输出一次降级警告，避免刷屏。
        ''' </summary>
        Private Shared Sub WarnOnce()
            If _warned Then
                Return
            End If

            SyncLock _warnLock
                If _warned Then
                    Return
                End If

                _warned = True
            End SyncLock

            Call Console.WriteLine("[warning] the 'Precompiled' normalizer is approximated by the NFKC normalization, the tokenization result may be slightly different from the sentencepiece implementation.")
        End Sub

    End Class

End Namespace
