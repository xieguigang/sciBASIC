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

        Public Iterator Function Search(q As String, Optional threshold As Double = 0) As IEnumerable(Of FindResult)
            Dim words As String() = tokenlicer(Strings.Trim(q).ToLower).Distinct.ToArray
            Dim findDocs = words.Select(Function(i) wordIndex.FindSimilar(i)) _
                .IteratesALL _
                .Select(Function(wi)
                            Return (wi, word2Docs(wi.text))
                        End Function) _
                .ToArray

        End Function
    End Class
End Namespace