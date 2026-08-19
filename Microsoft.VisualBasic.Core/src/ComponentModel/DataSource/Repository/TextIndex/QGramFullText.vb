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

        Public Function Search(q As String, Optional top As Integer = 3, Optional threshold As Double = 0) As IEnumerable(Of FindResult)
            Return Search(queryWords:=tokenlicer(Strings.Trim(q).ToLower).Distinct, top, threshold)
        End Function
    End Class
End Namespace