Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Public Class TFIDF

    ReadOnly vecs As New Dictionary(Of String, Dictionary(Of String, Integer))

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
    Public ReadOnly Property Words As IEnumerable(Of String)
        Get
            Return (From seq As Dictionary(Of String, Integer)
                    In vecs.Values
                    Select seq.Keys).IteratesALL.Distinct
        End Get
    End Property

    Sub New()
    End Sub

    Public Sub Add(id As String, seq As IEnumerable(Of String))
        Dim counter As Dictionary(Of String, Integer) = seq _
            .GroupBy(Function(a) a) _
            .ToDictionary(Function(a) a.Key,
                          Function(a)
                              Return a.Count
                          End Function)

        Call vecs.Add(id, seq)
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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function DF(v As String) As Integer
        Return Aggregate seq As Dictionary(Of String, Integer)
               In vecs.Values
               Into Count(seq.ContainsKey(v))
    End Function

    Public Function IDF(v As String) As Double
        Return std.Log((N + 1) / (DF(v) + 1)) + 1
    End Function

End Class
