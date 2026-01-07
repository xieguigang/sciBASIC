Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Public Class TFIDF

    ''' <summary>
    ''' 存储 序列ID -> {K-mer: 出现次数}
    ''' </summary>
    ReadOnly vecs As New Dictionary(Of String, Dictionary(Of String, Integer))

    Dim m_words As String()

    ''' <summary>
    ''' get N sequence
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property N As Integer
        Get
            Return vecs.Count
        End Get
    End Property

    ''' <summary>
    ''' get all unique words inside the sequence collection
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Words As String()
        Get
            If m_words Is Nothing Then
                m_words = WordsFromDocument.ToArray
            End If

            Return m_words
        End Get
    End Property

    Sub New()
    End Sub

    ''' <summary>
    ''' One example: processing sequence embedding with kmers, set all alphabet combination of the kmer words for sequence
    ''' (the input sequence kmer collection may be just a part subset of the input <paramref name="externalWords"/>). 
    ''' so the generated vector could be comparable with the result of another batch of the sequence processing result.
    ''' </summary>
    ''' <param name="externalWords"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' each time the <see cref="Add"/> method is call in this module, the words cache will be cleared. 
    ''' this function should be called after all sequence has already been added into this generator.
    ''' </remarks>
    Public Function SetWords(externalWords As IEnumerable(Of String)) As TFIDF
        m_words = externalWords.ToArray
        Return Me
    End Function

    Private Function WordsFromDocument() As IEnumerable(Of String)
        Dim pull = From seq As Dictionary(Of String, Integer)
                   In vecs.Values
                   Select seq.Keys

        Return From word As String
               In pull.IteratesALL
               Distinct
               Order By word
    End Function

    Public Sub Add(id As String, seq As IEnumerable(Of String))
        Dim counter As Dictionary(Of String, Integer) = seq _
            .GroupBy(Function(a) a) _
            .ToDictionary(Function(a) a.Key,
                          Function(a)
                              Return a.Count
                          End Function)

        m_words = Nothing
        vecs.Add(id, counter)
    End Sub

    ''' <summary>
    ''' generates the TF-IDF matrix
    ''' </summary>
    ''' <returns>
    ''' A dataframe object with row is sequence and column is word data
    ''' </returns>
    Public Function TfidfVectorizer() As DataFrame
        Dim m As New DataFrame With {
            .rownames = vecs.Keys.ToArray
        }
        Dim seqs As Dictionary(Of String, Integer)() = m.rownames _
            .Select(Function(id) vecs(id)) _
            .ToArray

        For Each v As String In Words
            ' add column
            ' row is sequence id
            Call m.add(v, From seq As Dictionary(Of String, Integer)
                          In seqs
                          Let tf As Integer = seq.TryGetValue(v, [default]:=0)
                          Select tf * IDF(v))
        Next

        Return m
    End Function

    ''' <summary>
    ''' n-gram One-hot(Bag-of-n-grams)
    ''' </summary>
    ''' <returns></returns>
    Public Function OneHotVectorizer() As DataFrame
        Dim m As New DataFrame With {
            .rownames = vecs.Keys.ToArray
        }
        Dim seqs As Dictionary(Of String, Integer)() = m.rownames _
            .Select(Function(id) vecs(id)) _
            .ToArray

        For Each v As String In Words
            ' add column
            ' row is sequence id
            Call m.add(v, From seq As Dictionary(Of String, Integer)
                          In seqs
                          Select If(seq.ContainsKey(v), 1, 0))
        Next

        Return m
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function DF(v As String) As Integer
        Return Aggregate seq As Dictionary(Of String, Integer)
               In vecs.Values
               Into Count(seq.ContainsKey(v))
    End Function

    ''' <summary>
    ''' IDF (Inverse Document Frequency)
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    Public Function IDF(v As String) As Double
        Return std.Log((N + 1) / (DF(v) + 1)) + 1
    End Function

End Class
