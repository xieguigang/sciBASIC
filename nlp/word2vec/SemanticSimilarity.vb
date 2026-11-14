Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports std = System.Math

' NOTE: no Namespace declaration here. The project RootNamespace is
' Microsoft.VisualBasic.Data.NLP.Word2Vec, so these types sit next to
' VectorModel / WordScore.

''' <summary>
''' Pluggable distributional (semantic) similarity used as an optional signal
''' in entity equivalence scoring.
''' </summary>
''' <remarks>
''' Keeping this an interface lets upper layers degrade gracefully: when no
''' word-vector model is available, <see cref="NullSemanticSimilarity"/> returns
''' 0 and the caller redistributes its weight to the graph-structure metrics.
''' </remarks>
Public Interface ISemanticSimilarity

    ''' <summary>Whether a usable model is loaded.</summary>
    ReadOnly Property IsAvailable As Boolean

    ''' <summary>Similarity in [0, 1]; 0 when either term is out of vocabulary.</summary>
    Function Similarity(termA As String, termB As String) As Double

    ''' <summary>The (normalized) vector of a term, or Nothing when missing.</summary>
    Function GetVector(term As String) As Single()

End Interface

''' <summary>
''' Null object used when no word-vector model was supplied. IsAvailable is
''' False so callers can drop the semantic metric entirely.
''' </summary>
Public NotInheritable Class NullSemanticSimilarity : Implements ISemanticSimilarity

    Public ReadOnly Property IsAvailable As Boolean Implements ISemanticSimilarity.IsAvailable
        Get
            Return False
        End Get
    End Property

    Public Function Similarity(termA As String, termB As String) As Double Implements ISemanticSimilarity.Similarity
        Return 0.0
    End Function

    Public Function GetVector(term As String) As Single() Implements ISemanticSimilarity.GetVector
        Return Nothing
    End Function

    Public Overrides Function ToString() As String
        Return "null_semantic_model"
    End Function

End Class

''' <summary>
''' Cosine similarity over a <see cref="VectorModel"/>.
''' </summary>
''' <remarks>
''' VectorModel.similar() uses a raw dot product, which equals cosine only when
''' all vectors are L2-normalized (Word2Vec.outputVector does that for trained
''' models, but externally loaded vectors do not). This class normalizes once at
''' construction time, so similarity is well defined for every source.
''' </remarks>
Public NotInheritable Class WordVectorSimilarity : Implements ISemanticSimilarity

    Private ReadOnly _vectors As Dictionary(Of String, Single())
    Private ReadOnly _lookup As Func(Of String, String)
    Private ReadOnly _size As Integer

    ''' <summary>
    ''' </summary>
    ''' <param name="model">The word vectors.</param>
    ''' <param name="normalize">L2-normalize every vector once.</param>
    ''' <param name="termLookup">
    ''' Optional term canonicalizer (e.g. Unicode/lowercase normalization)
    ''' injected by the caller; keeps this assembly free of a concrete NLP dependency.
    ''' </param>
    Public Sub New(model As VectorModel,
                   Optional normalize As Boolean = True,
                   Optional termLookup As Func(Of String, String) = Nothing)

        _vectors = New Dictionary(Of String, Single())(StringComparer.OrdinalIgnoreCase)
        _lookup = termLookup

        If model Is Nothing OrElse model.wordMap Is Nothing Then
            _size = 0
            Return
        End If

        _size = model.vectorSize

        For Each item As KeyValuePair(Of String, Single()) In model.wordMap
            Dim vector As Single() = item.Value

            If vector Is Nothing OrElse vector.Length = 0 Then
                Continue For
            End If

            If normalize Then
                vector = NormalizeVector(vector)
            End If

            _vectors(item.Key) = vector
        Next

        If _size <= 0 Then
            For Each item As KeyValuePair(Of String, Single()) In _vectors
                _size = item.Value.Length
                Exit For
            Next
        End If
    End Sub

    ''' <summary>Number of loaded word vectors.</summary>
    Public ReadOnly Property WordCount As Integer
        Get
            Return _vectors.Count
        End Get
    End Property

    ''' <summary>Dimensionality of the loaded vectors.</summary>
    Public ReadOnly Property VectorSize As Integer
        Get
            Return _size
        End Get
    End Property

    Public ReadOnly Property IsAvailable As Boolean Implements ISemanticSimilarity.IsAvailable
        Get
            Return _vectors.Count > 0
        End Get
    End Property

    ''' <summary>L2 normalize a single-precision vector (returns a copy).</summary>
    Public Shared Function NormalizeVector(vector As Single()) As Single()
        If vector Is Nothing OrElse vector.Length = 0 Then
            Return New Single() {}
        End If

        Dim magnitude As Double = 0.0

        For Each value As Single In vector
            magnitude += CDbl(value) * CDbl(value)
        Next

        magnitude = std.Sqrt(magnitude)
        Dim result(vector.Length - 1) As Single

        If magnitude <= 0.0 Then
            Return result
        End If

        For i As Integer = 0 To vector.Length - 1
            result(i) = CSng(CDbl(vector(i)) / magnitude)
        Next

        Return result
    End Function

    Private Function Lookup(term As String) As String
        If term Is Nothing Then Return ""
        If _lookup IsNot Nothing Then
            Dim mapped As String = _lookup(term)
            If Not String.IsNullOrEmpty(mapped) Then
                Return mapped
            End If
        End If
        Return term
    End Function

    Public Function GetVector(term As String) As Single() Implements ISemanticSimilarity.GetVector
        If Not IsAvailable OrElse String.IsNullOrEmpty(term) Then
            Return Nothing
        End If

        Dim vector As Single() = Nothing

        If _vectors.TryGetValue(Lookup(term), vector) Then
            Return vector
        End If

        ' Fall back to the first whitespace-delimited token of a multiword name.
        Dim tokens As String() = Lookup(term).Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        If tokens.Length > 1 AndAlso _vectors.TryGetValue(tokens(0), vector) Then
            Return vector
        End If

        Return Nothing
    End Function

    Public Function Similarity(termA As String, termB As String) As Double Implements ISemanticSimilarity.Similarity
        If Not IsAvailable OrElse String.IsNullOrEmpty(termA) OrElse String.IsNullOrEmpty(termB) Then
            Return 0.0
        End If

        Dim a As Single() = GetVector(termA)
        Dim b As Single() = GetVector(termB)

        If a Is Nothing OrElse b Is Nothing Then
            Return 0.0
        End If

        Dim n As Integer = std.Min(a.Length, b.Length)
        Dim dot As Double = 0.0

        For i As Integer = 0 To n - 1
            dot += CDbl(a(i)) * CDbl(b(i))
        Next

        ' Negative cosine means "unrelated" for our purposes, not "half similar".
        If Double.IsNaN(dot) OrElse Double.IsInfinity(dot) OrElse dot <= 0.0 Then
            Return 0.0
        End If
        If dot > 1.0 Then dot = 1.0

        Return dot
    End Function

    Public Overrides Function ToString() As String
        Return $"word2vec[{_vectors.Count} x {_size}]"
    End Function

End Class

''' <summary>
''' Model persistence and two-term scoring helpers. The library previously had
''' neither a loader nor a "score these two words" API.
''' </summary>
Public Module SemanticModelIO

    ''' <summary>Direct cosine score between two words in a VectorModel.</summary>
    <Extension>
    Public Function CosineSimilarity(model As VectorModel, termA As String, termB As String) As Double
        Dim semantic As New WordVectorSimilarity(model)
        Return semantic.Similarity(termA, termB)
    End Function

    ''' <summary>Wrap a word-map into an ISemanticSimilarity implementation.</summary>
    Public Function AsSemanticModel(model As VectorModel,
                                    Optional termLookup As Func(Of String, String) = Nothing) As ISemanticSimilarity
        If model Is Nothing Then
            Return New NullSemanticSimilarity()
        End If
        Return New WordVectorSimilarity(model, True, termLookup)
    End Function

    ''' <summary>
    ''' Load a word2vec text vector file (the standard .vec / .txt format).
    ''' An optional first line "vocabSize dimension" is accepted and skipped.
    ''' Returns Nothing when the file is missing or contains no usable vectors.
    ''' </summary>
    Public Function LoadModel(path As String) As VectorModel
        If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then
            Return Nothing
        End If

        Dim map As New Dictionary(Of String, Single())(StringComparer.OrdinalIgnoreCase)
        Dim size As Integer = 0
        Dim isFirstLine As Boolean = True

        Using reader As New StreamReader(path, Encoding.UTF8, True)
            Dim line As String = reader.ReadLine()

            While line IsNot Nothing
                Dim trimmed As String = line.Trim()

                If trimmed.Length > 0 Then
                    Dim parts As String() = trimmed.Split(New Char() {" "c, Chr(9)},
                                                          StringSplitOptions.RemoveEmptyEntries)

                    If isFirstLine AndAlso parts.Length = 2 AndAlso
                       Integer.TryParse(parts(0), NumberStyles.Integer, CultureInfo.InvariantCulture, Nothing) AndAlso
                       Integer.TryParse(parts(1), NumberStyles.Integer, CultureInfo.InvariantCulture, Nothing) Then

                        size = Integer.Parse(parts(1), CultureInfo.InvariantCulture)
                    ElseIf parts.Length > 1 Then
                        Dim values(parts.Length - 2) As Single
                        Dim valid As Boolean = True

                        For i As Integer = 0 To values.Length - 1
                            Dim parsed As Single

                            If Not Single.TryParse(parts(i + 1), NumberStyles.Float,
                                                   CultureInfo.InvariantCulture, parsed) Then
                                valid = False
                                Exit For
                            End If
                            values(i) = parsed
                        Next

                        If valid Then
                            map(parts(0)) = values
                            If size = 0 Then size = values.Length
                        End If
                    End If
                End If

                isFirstLine = False
                line = reader.ReadLine()
            End While
        End Using

        If map.Count = 0 Then
            Return Nothing
        End If

        Return New VectorModel(map, size)
    End Function

    ''' <summary>Save a VectorModel in the same text format understood by LoadModel.</summary>
    Public Sub SaveModel(model As VectorModel, path As String)
        If model Is Nothing OrElse model.wordMap Is Nothing OrElse String.IsNullOrEmpty(path) Then
            Return
        End If

        Dim folder As String = System.IO.Path.GetDirectoryName(path)
        If Not String.IsNullOrEmpty(folder) AndAlso Not System.IO.Directory.Exists(folder) Then
            Call System.IO.Directory.CreateDirectory(folder)
        End If

        Using writer As New StreamWriter(path, False, New UTF8Encoding(False))
            writer.WriteLine($"{model.wordMap.Count} {model.vectorSize}")

            For Each item As KeyValuePair(Of String, Single()) In model.wordMap
                If item.Value Is Nothing Then Continue For

                writer.Write(item.Key)

                For Each value As Single In item.Value
                    writer.Write(" "c)
                    writer.Write(value.ToString("G7", CultureInfo.InvariantCulture))
                Next

                writer.WriteLine()
            Next
        End Using
    End Sub

End Module
