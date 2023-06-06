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

    Public ReadOnly Property alphabets As Char()
    Public ReadOnly Property feature_names As String()

    Dim kappa As Double
    Dim lengthsensitive As Boolean

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
            Optional lengthsensitive As Boolean = False)

        Me.alphabets = alphabets

        If Not alphabets.IsNullOrEmpty Then
            feature_names = __set_feature_name(alphabets)
        End If

        Me.kappa = kappa
        Me.lengthsensitive = lengthsensitive
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

    Private Function estimate_alphabets(ParamArray corpus As String()) As Char()
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
    Public Function fit(sequence As String) As Dictionary(Of String, Double)
        If alphabets.IsNullOrEmpty Then
            _alphabets = estimate_alphabets(sequence)
            _feature_names = __set_feature_name(alphabets)
        End If

        Dim size = alphabets.Length
        Dim l = 0
        Dim W0 As NumericMatrix = NumericMatrix.Zero(size, size)
        Dim Wk As NumericMatrix = NumericMatrix.Zero(size, size)
        Dim positions = get_positions(sequence, alphabets)
        Dim alphabets_in_sequence = sequence.Distinct.ToArray

        For Each char_i In alphabets_in_sequence.SeqIterator
            Dim i As Integer = char_i.i
            Dim u As Char = char_i.value
            Dim Upos As Integer() = positions(u)

            For Each char_j In alphabets_in_sequence.SeqIterator
                Dim j As Integer = char_j.i
                Dim v As Char = char_j.value
                Dim V2 As Integer() = positions(v)
                Dim C = (From ai In Upos From bj In V2 Where bj > ai Select (i:=ai, j:=bj)).ToArray
                Dim cu As Vector = C.Select(Function(ic) ic.i).ToArray
                Dim cv As Vector = C.Select(Function(ic) ic.j).ToArray
                Dim pos_i = _alphabets.IndexOf(u)
                Dim pos_j = _alphabets.IndexOf(v)

                W0(pos_i, pos_j) = C.Length
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
        Dim map As New Dictionary(Of String, Double)

        For i As Integer = 0 To feature_names.Length - 1
            Call map.Add(feature_names(i), sgtv(i))
        Next

        Return map
    End Function
End Class
