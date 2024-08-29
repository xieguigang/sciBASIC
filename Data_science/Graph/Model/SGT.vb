#Region "Microsoft.VisualBasic::64d240d0a094e25c80dfe7fe159f8ff1, Data_science\Graph\Model\SGT.vb"

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

    '   Total Lines: 330
    '    Code Lines: 176 (53.33%)
    ' Comment Lines: 105 (31.82%)
    '    - Xml Docs: 84.76%
    ' 
    '   Blank Lines: 49 (14.85%)
    '     File Size: 12.11 KB


    ' Class SequenceGraphTransform
    ' 
    ' 
    '     Enum Modes
    ' 
    '         [Partial], Fast, Full
    ' 
    ' 
    ' 
    '     Delegate Function
    ' 
    '         Properties: alphabets, feature_names
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __set_feature_name, CombineFast, CombineFull, CombinePartial, estimate_alphabets
    '                   fit, fitInternal, fitVector, get_positions, set_alphabets
    '                   TranslateMatrix
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

''' <summary>
''' Sequence Graph Transform (SGT) — Sequence Embedding for Clustering, Classification, and Search
''' 
''' Sequence Graph Transform (SGT) is a sequence embedding function. SGT extracts 
''' the short- and long-term sequence features and embeds them in a finite-dimensional 
''' feature space. The long and short term patterns embedded in SGT can be tuned 
''' without any increase in the computation.
''' 
''' > https://github.com/cran2367/sgt/blob/25bf28097788fbbf9727abad91ec6e59873947cc/python/sgt-package/sgt/sgt.py
''' </summary>
''' <remarks>
''' Compute embedding of a single or a collection of discrete item
''' sequences. A discrete item sequence is a sequence made from a set
''' discrete elements, also known as alphabet set. For example,
''' suppose the alphabet set is the set of roman letters,
''' {A, B, ..., Z}. This set is made of discrete elements. Examples of
''' sequences from such a set are AABADDSA, UADSFJPFFFOIHOUGD, etc.
''' Such sequence datasets are commonly found in online industry,
''' for example, item purchase history, where the alphabet set is
''' the set of all product items. Sequence datasets are abundant in
''' bioinformatics as protein sequences.
''' Using the embeddings created here, classification and clustering
''' models can be built for sequence datasets.
''' Read more in https://arxiv.org/pdf/1608.03533.pdf
''' </remarks>
Public Class SequenceGraphTransform

    Public Enum Modes
        Full
        [Partial]
        Fast
    End Enum

    Public ReadOnly Property alphabets As Char()
        Get
            Return _alphabets
        End Get
    End Property

    ''' <summary>
    ''' the feature name is the combination of <see cref="alphabets"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property feature_names As String()

    Dim kappa As Double
    Dim lengthsensitive As Boolean
    Dim graph_matrix As String()()
    Dim _alphabetsIndex As Index(Of Char)
    Dim _alphabets As Char() = Nothing

    ''' <summary>
    ''' algorithm applied for check position
    ''' </summary>
    Dim mode As Modes = Modes.Full

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="alphabets">Optional, except if mode is Spark.
    ''' The set of alphabets that make up all
    ''' the sequences in the dataset. If not passed, the
    ''' alphabet set is automatically computed as the
    ''' unique set of elements that make all the sequences.
    ''' A list or 1d-array of the set of elements that make up the
    ''' sequences. For example, np.array(["A", "B", "C"].
    ''' If mode is 'spark', the alphabets are necessary.
    ''' </param>
    ''' <param name="kappa">
    ''' Tuning parameter, kappa > 0, to change the extraction of
    ''' long-term dependency. Higher the value the lesser
    ''' the long-term dependency captured in the embedding.
    ''' Typical values for kappa are 1, 5, 10.</param>
    ''' <param name="lengthsensitive">Default False. This is set to true if the embedding of
    ''' should have the information of the length of the sequence.
    ''' If set to false then the embedding of two sequences with
    ''' similar pattern but different lengths will be the same.
    ''' lengthsensitive = false is similar to length-normalization.</param>
    Sub New(Optional alphabets As Char() = Nothing,
            Optional kappa As Double = 1,
            Optional lengthsensitive As Boolean = False,
            Optional mode As Modes = Modes.Full)

        If Not alphabets.IsNullOrEmpty Then
            _alphabets = alphabets
            _alphabetsIndex = alphabets.Indexing

            feature_names = __set_feature_name(alphabets)
        End If

        Me.kappa = kappa
        Me.lengthsensitive = lengthsensitive
        Me.mode = mode
    End Sub

    ''' <summary>
    ''' Compute index position elements in the sequence
    ''' given alphabets Set.        
    ''' </summary>
    ''' <param name="sequence"></param>
    ''' <param name="alphabets"></param>
    ''' <returns>
    ''' Return list Of tuples [(value, position)]
    ''' </returns>
    Private Shared Function get_positions(sequence As String, alphabets As Char()) As Dictionary(Of Char, Integer())
        Return alphabets _
            .ToDictionary(Function(c) c,
                          Function(c)
                              Return New Vector(Of Char)(sequence).Which(Function(ci) ci = c)
                          End Function)
    End Function

    Public Shared Function estimate_alphabets(ParamArray corpus As String()) As Char()
        If corpus.Length > 100000 Then
            Throw New Exception("Error: Too many sequences. Pass the alphabet list as an input. Exiting.")
        End If

        Return corpus _
            .Select(Function(si) si.AsEnumerable) _
            .IteratesALL _
            .Distinct _
            .OrderBy(Function(c) c) _
            .ToArray
    End Function

    ''' <summary>
    ''' set the alphabet data
    ''' </summary>
    ''' <param name="corpus"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 1. set the alphabet vector
    ''' 2. then set the feature names for the transformation output
    ''' 3. finally create the graph matrix index in this function
    ''' </remarks>
    Public Function set_alphabets(corpus As String()) As SequenceGraphTransform
        _alphabets = estimate_alphabets(corpus)
        _alphabetsIndex = _alphabets.Indexing
        _feature_names = __set_feature_name(alphabets)

        graph_matrix = alphabets _
            .Select(Function(c)
                        Return alphabets _
                            .Select(Function(c2) New String({c, ","c, c2})) _
                            .ToArray
                    End Function) _
            .ToArray

        Return Me
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="alphabets"></param>
    ''' <returns>
    ''' returns an array of x,y combination result
    ''' </returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function __set_feature_name(alphabets As Char()) As String()
        Return CombinationExtensions.FullCombination(alphabets) _
            .Select(Function(t) $"{t.a},{t.b}") _
            .ToArray
    End Function

    ''' <summary>
    ''' Extract Sequence Graph Transform features using Algorithm-2.
    ''' </summary>
    ''' <param name="sequence"></param>
    ''' <returns>
    ''' sgt matrix or vector (depending on Flatten==False or True)
    ''' </returns>
    Public Function fit(sequence As String) As Dictionary(Of String, Double)
        If alphabets.IsNullOrEmpty Then
            _alphabets = estimate_alphabets(sequence)
            _alphabetsIndex = _alphabets.Indexing
            _feature_names = __set_feature_name(alphabets)
        End If

        Dim sgtv As Double() = fitInternal(sequence)
        Dim map As New Dictionary(Of String, Double)

        For i As Integer = 0 To feature_names.Length - 1
            Call map.Add(feature_names(i), sgtv(i))
        Next

        Return map
    End Function

    ''' <summary>
    ''' Strip of the unexpected alphabets that not existed in current embedding index
    ''' </summary>
    ''' <param name="seq"></param>
    ''' <returns></returns>
    Public Function SafeStrip(seq As String) As String
        If _alphabetsIndex Is Nothing Then
            Return seq
        End If

        seq = seq _
            .Where(Function(c) c Like _alphabetsIndex) _
            .CharString

        Return seq
    End Function

    Public Function fitVector(sequence As String) As Double()
        If alphabets.IsNullOrEmpty Then
            _alphabets = estimate_alphabets(sequence)
            _alphabetsIndex = _alphabets.Indexing
            _feature_names = __set_feature_name(alphabets)
        End If

        Return fitInternal(sequence)
    End Function

    Private Shared Function CombineFull(U As Integer(), V As Integer()) As IEnumerable(Of (i As Integer, j As Integer))
        Return From ai In U
               From bj In V
               Where bj > ai
               Select (i:=ai, j:=bj)
    End Function

    ''' <summary>
    ''' for save the memory and make the algorithm faster when deal with a long sequence data
    ''' </summary>
    ''' <param name="U"></param>
    ''' <param name="V"></param>
    ''' <returns></returns>
    Private Shared Function CombinePartial(U As Integer(), V As Integer()) As IEnumerable(Of (i As Integer, j As Integer))
        If U.Length = 0 Then
            Return {}
        End If

        'Dim minU As Integer = U.First

        '' offset the V vector
        '' for makes j always greater than i
        'For i As Integer = 0 To V.Length - 1
        '    If V(i) > minU Then
        '        V = V.Skip(i).ToArray
        '        Exit For
        '    End If
        'Next

        'Return U.Zip(V, Function(i, j) (i, j)).Where(Function(ij) ij.j > ij.i)

        ' just find for pattern AB in current tuple graph
        Return From ai In U
               From bj In V
               Where bj = ai + 1
               Select (i:=ai, j:=bj)
    End Function

    Private Shared Iterator Function CombineFast(seq As String, u As Char, v As Char) As IEnumerable(Of (i As Integer, j As Integer))
        Dim t As String = New String({u, v})
        Dim i As Integer = 1

        Do While True
            i = InStr(i, seq, t)

            If i > 0 Then
                Yield (i, i + 1)
                i += 1
            Else
                Exit Do
            End If
        Loop
    End Function

    Private Delegate Function Combine(U As Integer(), V As Integer()) As IEnumerable(Of (i As Integer, j As Integer))

    Public Function TranslateMatrix(v As Dictionary(Of String, Double)) As Double()()
        Dim m As Double()() = New Double(alphabets.Length - 1)() {}

        For i As Integer = 0 To graph_matrix.Length - 1
            Dim row_i = graph_matrix(i)
            Dim vr As Double() = row_i _
                .Select(Function(c) v.TryGetValue(c)) _
                .ToArray

            m(i) = vr
        Next

        Return m
    End Function

    Private Function fitInternal(sequence As String) As Double()
        Dim size = alphabets.Length
        Dim l = 0
        Dim W0 As NumericMatrix = NumericMatrix.Zero(size, size)
        Dim Wk As NumericMatrix = NumericMatrix.Zero(size, size)
        Dim positions = get_positions(sequence, alphabets)
        Dim alphabets_in_sequence = sequence.Distinct.ToArray
        Dim combine As Combine = If(
            mode = Modes.Full,
            New Combine(AddressOf CombineFull),
            New Combine(AddressOf CombinePartial)
        )
        Dim cu, cv As Vector
        Dim c As (i As Integer, j As Integer)()
        Dim V2 As Integer()

        For Each char_i As SeqValue(Of Char) In alphabets_in_sequence.SeqIterator
            Dim i As Integer = char_i.i
            Dim u As Char = char_i.value
            Dim Upos As Integer() = positions(u)

            For Each char_j As SeqValue(Of Char) In alphabets_in_sequence.SeqIterator
                Dim j As Integer = char_j.i
                Dim v As Char = char_j.value
                Dim pos_i = _alphabets.IndexOf(u)
                Dim pos_j = _alphabets.IndexOf(v)

                If positions.ContainsKey(v) Then
                    V2 = positions(v)
                Else
                    Call $"KeyNotFound: The given key '{v}' was not present in the dictionary.".Warning
                    V2 = {}
                End If

                If mode = Modes.Fast Then
                    c = CombineFast(sequence, u, v).ToArray
                Else
                    c = combine(Upos, V2).ToArray
                End If

                cu = c.Select(Function(ic) ic.i).ToArray
                cv = c.Select(Function(ic) ic.j).ToArray

                W0(pos_i, pos_j) = c.Length
                Wk(pos_i, pos_j) = (-kappa * (cu - cv).Abs).Exp().Sum
            Next

            l += Upos.Length
        Next

        If lengthsensitive Then
            W0 /= l
        End If

        ' avoid divide by 0
        W0(W0 = 0.0) = 10000000.0

        Dim sgt = (Wk / W0) ^ (1 / kappa)
        Dim sgtv As Double() = sgt.ArrayPack.IteratesALL.ToArray

        Return sgtv
    End Function
End Class
