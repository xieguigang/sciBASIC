#Region "Microsoft.VisualBasic::ae913ef63f2786b66342d1b0b33314c8, nlp\NLP\Tokenizer\src\HuggingFace\PostProcessors.vb"

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

    '   Total Lines: 165
    '    Code Lines: 94 (56.97%)
    ' Comment Lines: 32 (19.39%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 39 (23.64%)
    '     File Size: 6.17 KB


    '     Class NullPostProcessor
    ' 
    '         Properties: Instance
    ' 
    '         Function: Process
    ' 
    '     Class ByteLevelPostProcessor
    ' 
    '         Function: Process
    ' 
    '     Class TemplatePostProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Process
    '         Structure TemplatePiece
    ' 
    ' 
    ' 
    ' 
    ' 
    '     Class BertPostProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Process
    ' 
    '     Class RobertaPostProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Process
    ' 
    '     Class SequencePostProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Process
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' 空后处理器：不对 token 序列做任何改动。
    ''' </summary>
    Public NotInheritable Class NullPostProcessor : Implements IPostProcessor

        Public Shared ReadOnly Property Instance As New NullPostProcessor

        Public Function Process(tokens As List(Of Token), addSpecialTokens As Boolean) As List(Of Token) Implements IPostProcessor.Process
            Return tokens
        End Function

    End Class

    ''' <summary>
    ''' ByteLevel 后处理器。
    ''' </summary>
    ''' <remarks>
    ''' <b>该后处理器不会增删任何 token</b>。其配置中的 <c>add_prefix_space</c> 与
    ''' <c>trim_offsets</c> 只影响偏移量的对齐语义，编码结果本身保持原样。这一点非常
    ''' 关键：如果在此处再次追加前导空格，整个 id 序列都会发生偏移，从而与 python 端的
    ''' 输出产生系统性差异。
    ''' </remarks>
    Public NotInheritable Class ByteLevelPostProcessor : Implements IPostProcessor

        Public Function Process(tokens As List(Of Token), addSpecialTokens As Boolean) As List(Of Token) Implements IPostProcessor.Process
            Return tokens
        End Function

    End Class

    ''' <summary>
    ''' 模板后处理器，对应 <c>TemplateProcessing</c>。
    ''' </summary>
    ''' <remarks>
    ''' 模板由若干片段组成，片段要么是序列占位符（<c>A</c> / <c>B</c>），要么是一个
    ''' 具体的特殊 token。这里只处理单序列（<c>single</c>）的情形，因为句对编码并不在
    ''' 当前的功能范围之内。
    ''' </remarks>
    Public NotInheritable Class TemplatePostProcessor : Implements IPostProcessor

        ''' <summary>
        ''' 模板片段：<see cref="IsSequence"/> 为真时表示序列占位符。
        ''' </summary>
        Public Structure TemplatePiece
            Public IsSequence As Boolean
            Public Value As String
            Public Id As Integer
        End Structure

        Private ReadOnly _single As TemplatePiece()

        Public Sub New(single__ As IEnumerable(Of TemplatePiece))
            _single = If(single__ Is Nothing, New TemplatePiece() {}, single__.ToArray())
        End Sub

        Public Function Process(tokens As List(Of Token), addSpecialTokens As Boolean) As List(Of Token) Implements IPostProcessor.Process
            If Not addSpecialTokens OrElse _single.Length = 0 Then
                Return tokens
            End If

            Dim result As New List(Of Token)(tokens.Count + _single.Length)

            For Each piece As TemplatePiece In _single
                If piece.IsSequence Then
                    result.AddRange(tokens)
                Else
                    result.Add(New Token(piece.Id, piece.Value, 0, 0))
                End If
            Next

            Return result
        End Function

    End Class

    ''' <summary>
    ''' BERT 的后处理器：在序列两端补上 <c>[CLS]</c> 与 <c>[SEP]</c>。
    ''' </summary>
    Public NotInheritable Class BertPostProcessor : Implements IPostProcessor

        Private ReadOnly _clsToken As String
        Private ReadOnly _clsId As Integer
        Private ReadOnly _sepToken As String
        Private ReadOnly _sepId As Integer

        Public Sub New(clsToken As String, clsId As Integer, sepToken As String, sepId As Integer)
            _clsToken = clsToken
            _clsId = clsId
            _sepToken = sepToken
            _sepId = sepId
        End Sub

        Public Function Process(tokens As List(Of Token), addSpecialTokens As Boolean) As List(Of Token) Implements IPostProcessor.Process
            If Not addSpecialTokens Then
                Return tokens
            End If

            Dim result As New List(Of Token)(tokens.Count + 2)

            result.Add(New Token(_clsId, _clsToken, 0, 0))
            result.AddRange(tokens)
            result.Add(New Token(_sepId, _sepToken, 0, 0))

            Return result
        End Function

    End Class

    ''' <summary>
    ''' RoBERTa 的后处理器：在序列两端补上 <c>&lt;s&gt;</c> 与 <c>&lt;/s&gt;</c>。
    ''' </summary>
    Public NotInheritable Class RobertaPostProcessor : Implements IPostProcessor

        Private ReadOnly _clsToken As String
        Private ReadOnly _clsId As Integer
        Private ReadOnly _sepToken As String
        Private ReadOnly _sepId As Integer

        Public Sub New(clsToken As String, clsId As Integer, sepToken As String, sepId As Integer)
            _clsToken = clsToken
            _clsId = clsId
            _sepToken = sepToken
            _sepId = sepId
        End Sub

        Public Function Process(tokens As List(Of Token), addSpecialTokens As Boolean) As List(Of Token) Implements IPostProcessor.Process
            If Not addSpecialTokens Then
                Return tokens
            End If

            Dim result As New List(Of Token)(tokens.Count + 2)

            result.Add(New Token(_clsId, _clsToken, 0, 0))
            result.AddRange(tokens)
            result.Add(New Token(_sepId, _sepToken, 0, 0))

            Return result
        End Function

    End Class

    ''' <summary>
    ''' 组合后处理器：按顺序依次应用子后处理器。
    ''' </summary>
    Public NotInheritable Class SequencePostProcessor : Implements IPostProcessor

        Private ReadOnly _items As IPostProcessor()

        Public Sub New(items As IEnumerable(Of IPostProcessor))
            _items = If(items Is Nothing, New IPostProcessor() {}, items.Where(Function(i) i IsNot Nothing).ToArray())
        End Sub

        Public Function Process(tokens As List(Of Token), addSpecialTokens As Boolean) As List(Of Token) Implements IPostProcessor.Process
            For Each item As IPostProcessor In _items
                tokens = item.Process(tokens, addSpecialTokens)
            Next

            Return tokens
        End Function

    End Class

End Namespace
