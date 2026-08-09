#Region "Microsoft.VisualBasic::ed81ae097eb1b309a82151ab0e8e7eee, nlp\NLP\BM25\Document.vb"

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

    '   Total Lines: 31
    '    Code Lines: 17 (54.84%)
    ' Comment Lines: 7 (22.58%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (22.58%)
    '     File Size: 929 B


    '     Class Document
    ' 
    '         Properties: Id, Length, RawText, Tokens
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace BM25

    ''' <summary>
    ''' 文档表示：ID + 分词后的词元列表。
    ''' </summary>
    Public Class Document

        ''' <summary>文档唯一标识。</summary>
        Public Property Id As Integer

        ''' <summary>文档原始文本（可选，用于回显结果）。</summary>
        Public Property RawText As String

        ''' <summary>分词后的词元数组。</summary>
        Public Property Tokens As String()

        ''' <summary>文档长度 = Tokens.Length。</summary>
        Public ReadOnly Property Length As Integer
            Get
                Return If(Tokens?.Length, 0)
            End Get
        End Property

        Public Sub New(id As Integer, tokens As String(), Optional rawText As String = "")
            Me.Id = id
            Me.Tokens = tokens
            Me.RawText = rawText
        End Sub

    End Class
End Namespace
