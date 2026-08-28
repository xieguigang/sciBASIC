#Region "Microsoft.VisualBasic::6ccfe34724db09a0a58f51953e355bb1, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Repository\TextIndex\QGramFullText.vb"

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

    '   Total Lines: 78
    '    Code Lines: 62 (79.49%)
    ' Comment Lines: 3 (3.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (16.67%)
    '     File Size: 3.33 KB


    '     Class QGramFullText
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Add, (+2 Overloads) Search, Tokenize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Trinity.NLP
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel.Repository

    ''' <summary>
    ''' 基于Qgram的全文索引
    ''' </summary>
    Public Class QGramFullText

        ReadOnly docs As Dictionary(Of String, String)
        ReadOnly word2Docs As Dictionary(Of String, HashSet(Of String))
        ReadOnly wordIndex As QGramIndex
        ReadOnly tokenlicer As Func(Of String, IEnumerable(Of String))

        Sub New(Optional q As Integer = 3, Optional tokenlicer As Func(Of String, IEnumerable(Of String)) = Nothing)
            Me.docs = New Dictionary(Of String, String)
            Me.word2Docs = New Dictionary(Of String, HashSet(Of String))
            Me.wordIndex = New QGramIndex(q)
            Me.tokenlicer = If(tokenlicer, AddressOf TextSplit.MakeWords)
        End Sub

        Public Function Add(doc As String) As QGramFullText
            Dim docId As String = doc.GetHashCode.ToString & "-" & (docs.Count + 1)

            Call docs.Add(docId, doc)

            For Each word As String In tokenlicer(Strings.Trim(doc).ToLower).Distinct
                If Not word2Docs.ContainsKey(word) Then
                    Call wordIndex.AddString(word)
                    Call word2Docs.Add(word, New HashSet(Of String))
                End If

                Call word2Docs(word).Add(docId)
            Next

            Return Me
        End Function

        Public Iterator Function Search(queryWords As IEnumerable(Of String), Optional top As Integer = 3, Optional threshold As Double = 0) As IEnumerable(Of FindResult)
            Dim findDocs = queryWords.Select(Function(i) wordIndex.FindSimilar(i, threshold)) _
                .IteratesALL _
                .Select(Function(wi)
                            Return (wi, word2Docs(wi.text))
                        End Function) _
                .ToArray
            Dim resultDocs = findDocs.Select(Function(a)
                                                 Return a.Item2.Select(Function(docId) (docId, a.wi))
                                             End Function) _
                .IteratesALL _
                .GroupBy(Function(a) a.docId) _
                .ToArray
            Dim rankDocs = From doc In resultDocs
                           Let rank As Double = doc.Sum(Function(a) a.wi.similarity)
                           Let docId = doc.Key
                           Select docId, rank
                           Order By rank Descending
                           Take top

            For Each hit In rankDocs
                Yield New FindResult With {
                    .index = -1,
                    .levenshtein = 0,
                    .similarity = hit.rank,
                    .text = docs(hit.docId)
                }
            Next
        End Function

        Public Function Tokenize(q As String) As IEnumerable(Of String)
            Return tokenlicer(q)
        End Function

        Public Function Search(q As String, Optional top As Integer = 3, Optional threshold As Double = 0) As IEnumerable(Of FindResult)
            Return Search(queryWords:=tokenlicer(Strings.Trim(q).ToLower).Distinct, top, threshold)
        End Function
    End Class
End Namespace
