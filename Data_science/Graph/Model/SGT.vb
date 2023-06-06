Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
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

    Public ReadOnly Property alphabets As Char()
    Public ReadOnly Property feature_names As String()

    Dim kappa As Double
    Dim lengthsensitive As Boolean
    Dim flatten As Boolean
    Dim mode As String = "default"
    Dim processors As Integer
    Dim lazy As Boolean

    Sub New(Optional alphabets As Char() = Nothing,
            Optional kappa As Double = 1,
            Optional lengthsensitive As Boolean = False,
            Optional flatten As Boolean = True,
            Optional mode As String = "default",
            Optional processors As Integer? = Nothing,
            Optional lazy As Boolean = False)

        Me.alphabets = alphabets

        If Not alphabets.IsNullOrEmpty Then
            feature_names = __set_feature_name(alphabets)
        End If

        Me.kappa = kappa
        Me.lengthsensitive = lengthsensitive
        Me.flatten = flatten
        Me.mode = mode
        Me.processors = processors
        Me.lazy = lazy

        If processors Is Nothing Then
            Me.processors = App.CPUCoreNumbers - 1
        End If
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
    Private Function getpositions(sequence As String, alphabets As Char()) As Dictionary(Of Char, Integer())
        Return alphabets _
            .ToDictionary(Function(c) c,
                          Function(c)
                              Return New Vector(Of Char)(sequence).Which(Function(ci) ci = c)
                          End Function)
    End Function

    ''' <summary>
    ''' Flatten one level of nesting
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="listOfLists"></param>
    ''' <returns></returns>
    Private Function __flatten(Of T)(listOfLists As IEnumerable(Of IEnumerable(Of T))) As IEnumerable(Of T)
        Return listOfLists.IteratesALL
    End Function

    Private Function estimate_alphabets(ParamArray corpus As String()) As Char()
        If Len(corpus) > 100000 Then
            Throw New Exception("Error: Too many sequences. Pass the alphabet list as an input. Exiting.")
        End If

        Return corpus _
            .Select(Function(si) si.AsEnumerable) _
            .IteratesALL _
            .Distinct _
            .OrderBy(Function(c) c) _
            .ToArray
    End Function

    Public Function set_alphabets(corpus As String()) As SequenceGraphTransform
        _alphabets = estimate_alphabets(corpus)
        _feature_names = __set_feature_name(alphabets)

        Return Me
    End Function

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
    Public Function fit(sequence As String)
        If Len(alphabets) = 0 Then
            _alphabets = estimate_alphabets(sequence)
            _feature_names = __set_feature_name(alphabets)
        End If

        Dim size = Len(alphabets)
        Dim l = 0
        Dim W0 As NumericMatrix = NumericMatrix.Zero(size, size)
        Dim Wk As NumericMatrix = NumericMatrix.Zero(size, size)
        Dim positions = getpositions(sequence, alphabets)
        Dim alphabets_in_sequence = sequence.Distinct.ToArray

        For Each char_i In alphabets_in_sequence.SeqIterator
            Dim i As Integer = char_i.i
            Dim u As Char = char_i.value
            Dim Upos As Integer() = positions(u)

            For Each char_j In alphabets_in_sequence.SeqIterator
                Dim j As Integer = char_j.i
                Dim v As Char = char_j.value
                Dim V2 As Integer() = positions(v)
                Dim C = Upos.Zip(V2, Function(a, b) (i:=a, j:=b)).Where(Function(t) t.j > t.i).ToArray
                Dim cu As Vector = C.Select(Function(ic) ic.i).ToArray
                Dim cv As Vector = C.Select(Function(ic) ic.j).ToArray
                Dim pos_i = _alphabets.IndexOf(u)
                Dim pos_j = _alphabets.IndexOf(v)

                W0(pos_i, pos_j) = Len(C)
                Wk(pos_i, pos_j) = (-kappa * (cu - cv).Abs).Exp().Sum
            Next

            l += Upos.Length
        Next

        If lengthsensitive Then
            W0 /= l
        End If

        W0(W0 = 0.0) = 10000000.0
    End Function
End Class
