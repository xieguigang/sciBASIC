#Region "Microsoft.VisualBasic::3def3fd0d523592ef6bb9c8d0327ed2a, nlp\NLP\Tokenizer\src\HuggingFace\WordPieceModel.vb"

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

    '   Total Lines: 145
    '    Code Lines: 95 (65.52%)
    ' Comment Lines: 22 (15.17%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 28 (19.31%)
    '     File Size: 5.18 KB


    '     Class WordPieceModel
    ' 
    '         Properties: VocabSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: IdToToken, Tokenize, TokenToId
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' the sciBASIC framework exposes its own Min/Max extension methods in the
' global namespace, so an explicit alias is required here.
Imports std = System.Math

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' WordPiece 子词模型（BERT 系列）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 切分策略为<b>贪心最长匹配</b>：从单词的左端出发，每次在词表中查找尽可能长的
    ''' 匹配子串；从第二个子词开始，候选子串需要先拼接上续接前缀（默认为 <c>##</c>）
    ''' 再去查表。
    ''' </para>
    ''' <para>
    ''' 只要在任意一个位置上找不到匹配，或者单词的长度超过了
    ''' <c>max_input_chars_per_word</c>，则<b>整个单词</b>都会被判定为未知词，
    ''' 这一点与 BPE 的逐符号回退存在本质区别。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class WordPieceModel : Implements ITokenizerModel

        Private ReadOnly _vocab As Dictionary(Of String, Integer)
        Private ReadOnly _idToToken As String()
        Private ReadOnly _prefix As String
        Private ReadOnly _unkToken As String
        Private ReadOnly _unkId As Integer = -1
        Private ReadOnly _maxInputCharsPerWord As Integer
        ''' <summary>
        ''' 词表中最长 token 的字符数，用于收缩贪心匹配的搜索窗口。
        ''' </summary>
        Private ReadOnly _maxTokenLength As Integer

        Public ReadOnly Property VocabSize As Integer Implements ITokenizerModel.VocabSize
            Get
                Return _vocab.Count
            End Get
        End Property

        Public Sub New(model As ModelSection)
            _vocab = model.Vocab
            _idToToken = model.IdToToken
            _prefix = If(model.ContinuingSubwordPrefix, "##")
            _unkToken = If(model.UnkToken, "[UNK]")
            _maxInputCharsPerWord = If(model.MaxInputCharsPerWord > 0, model.MaxInputCharsPerWord, 100)

            Dim id As Integer

            If _vocab.TryGetValue(_unkToken, id) Then
                _unkId = id
            End If

            For Each token As String In _vocab.Keys
                If token.Length > _maxTokenLength Then
                    _maxTokenLength = token.Length
                End If
            Next
        End Sub

        Public Function TokenToId(token As String) As Integer? Implements ITokenizerModel.TokenToId
            Dim id As Integer

            If token IsNot Nothing AndAlso _vocab.TryGetValue(token, id) Then
                Return id
            Else
                Return Nothing
            End If
        End Function

        Public Function IdToToken(id As Integer) As String Implements ITokenizerModel.IdToToken
            If id >= 0 AndAlso id < _idToToken.Length Then
                Return _idToToken(id)
            Else
                Return Nothing
            End If
        End Function

        Public Iterator Function Tokenize(fragment As String) As IEnumerable(Of Token) Implements ITokenizerModel.Tokenize
            If String.IsNullOrEmpty(fragment) Then
                Return
            End If

            If fragment.Length > _maxInputCharsPerWord Then
                If _unkId >= 0 Then
                    Yield New Token(_unkId, _unkToken, 0, fragment.Length)
                End If

                Return
            End If

            Dim tokens As New List(Of Token)
            Dim start As Integer = 0
            Dim failed As Boolean = False

            Do While start < fragment.Length
                ' greedily search for the longest sub string which exists in the vocabulary
                Dim [end] As Integer = std.Min(fragment.Length, start + _maxTokenLength)
                Dim matched As String = Nothing
                Dim matchedId As Integer = -1

                Do While [end] > start
                    Dim piece As String = fragment.Substring(start, [end] - start)

                    If start > 0 Then
                        piece = _prefix & piece
                    End If

                    Dim id As Integer

                    If _vocab.TryGetValue(piece, id) Then
                        matched = piece
                        matchedId = id
                        Exit Do
                    End If

                    [end] -= 1
                Loop

                If matched Is Nothing Then
                    failed = True
                    Exit Do
                End If

                tokens.Add(New Token(matchedId, matched, start, [end]))
                start = [end]
            Loop

            ' any failure makes the whole word an unknown token
            If failed Then
                If _unkId >= 0 Then
                    Yield New Token(_unkId, _unkToken, 0, fragment.Length)
                End If

                Return
            End If

            For Each token As Token In tokens
                Yield token
            Next
        End Function

    End Class

End Namespace
