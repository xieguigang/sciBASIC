#Region "Microsoft.VisualBasic::761ba573236d8fe276c0b0ec412f848d, Data_science\NLP\TF-IDF\TFIDF.vb"

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

    '   Total Lines: 191
    '    Code Lines: 113 (59.16%)
    ' Comment Lines: 53 (27.75%)
    '    - Xml Docs: 86.79%
    ' 
    '   Blank Lines: 25 (13.09%)
    '     File Size: 6.82 KB


    ' Class TFIDF
    ' 
    '     Properties: N, Words
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: DF, IDF, OneHotVectorizer, SetWords, (+2 Overloads) TfidfVectorizer
    '               WordsFromDocument
    ' 
    '     Sub: (+2 Overloads) Add
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
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

    Public Sub Add(id As String, counter As Dictionary(Of String, Integer))
        m_words = Nothing
        vecs.Add(id, counter)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Add(id As String, seq As IEnumerable(Of String))
        Call Add(id, seq _
            .GroupBy(Function(a) a) _
            .ToDictionary(Function(a) a.Key,
                          Function(a)
                              Return a.Count
                          End Function))
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="id">the sequence id</param>
    ''' <returns></returns>
    Public Function TfidfVectorizer(id As String) As Double()
        Dim terms As Dictionary(Of String, Integer) = vecs(id)
        Dim vec As IEnumerable(Of Double) = From v As String
                                            In Words
                                            Let tf As Integer = terms.TryGetValue(v, [default]:=0)
                                            Select tf * IDF(v)
        Return vec.ToArray
    End Function

    ''' <summary>
    ''' generates the TF-IDF matrix
    ''' </summary>
    ''' <returns>
    ''' A dataframe object with row is sequence and column is word data
    ''' </returns>
    Public Function TfidfVectorizer(Optional L2normalized As Boolean = False) As DataFrame
        Dim m As New DataFrame With {
            .rownames = vecs.Keys.ToArray
        }
        Dim seqs As SeqValue(Of Dictionary(Of String, Integer))() = m.rownames _
            .Select(Function(id) vecs(id)) _
            .SeqIterator _
            .ToArray

        For Each v As String In TqdmWrapper.Wrap(Words)
            ' add column
            ' row is sequence id
            Call m.add(v, From seq As SeqValue(Of Dictionary(Of String, Integer))
                          In seqs.AsParallel
                          Let tf As Integer = seq.value.TryGetValue(v, [default]:=0)
                          Let ordinal As Integer = seq.i
                          Let embedding As Double = tf * IDF(v)
                          Order By ordinal Ascending
                          Select embedding)
        Next

        If L2normalized Then
            ' data populated by rows
            Dim rows As NamedCollection(Of Double)() = m _
                .foreachRow(Function(r)
                                Dim vec As New Vector(From xi As Object In r Select CDbl(xi))
                                Dim norm As Double = std.Sqrt((vec ^ 2).Sum)

                                ' get normalized sequence row data vector
                                Return New NamedCollection(Of Double)(r.name, vec / norm)
                            End Function) _
                .ToArray

            m = DataFrame.FromRows(rows, m.featureNames)
        End If

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
    ''' get number of document which contains term <paramref name="v"/>
    ''' </summary>
    ''' <param name="v">term for count documents</param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function DF(v As String) As Integer
        Return Aggregate seq As Dictionary(Of String, Integer)
               In vecs.Values
               Where seq.ContainsKey(v)
               Into Count
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

