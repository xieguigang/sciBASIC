Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.Data.NLP.ChineseTokenizer.HuggingFace
Imports std = System.Math

' NOTE: this file deliberately declares no Namespace. The project RootNamespace
' is Microsoft.VisualBasic.Data.NLP, so the types below live next to Stemmer,
' Text and Extensions. Do NOT write a fully qualified Namespace here - VB would
' prepend the RootNamespace a second time.

''' <summary>
''' Fuzzy string similarity helpers used for entity name clues in alias
''' detection (case, accents, punctuation and spelling variants).
''' </summary>
''' <remarks>
''' The NLP toolkit previously had no edit-distance implementation at all.
''' These functions are pure string math and are shared with the knowledge
''' graph layer through a delegate, so that the knowledge graph project does
''' not have to take a dependency on this assembly.
''' </remarks>
Public Module StringSimilarity

    ''' <summary>Clamp a score into [0, 1].</summary>
    Public Function Clamp01(value As Double) As Double
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
            Return 0.0
        End If
        If value < 0.0 Then Return 0.0
        If value > 1.0 Then Return 1.0
        Return value
    End Function

    ''' <summary>Levenshtein edit distance with O(min(len)) working memory.</summary>
    Public Function LevenshteinDistance(a As String, b As String) As Integer
        If a Is Nothing Then a = ""
        If b Is Nothing Then b = ""

        If a.Length = 0 Then Return b.Length
        If b.Length = 0 Then Return a.Length
        If a = b Then Return 0

        ' Keep the shorter string on the inner loop axis.
        If a.Length < b.Length Then
            Dim swap As String = a
            a = b
            b = swap
        End If

        Dim previous(b.Length) As Integer
        Dim current(b.Length) As Integer
        Dim j As Integer

        For j = 0 To b.Length
            previous(j) = j
        Next

        For i As Integer = 1 To a.Length
            current(0) = i

            For j = 1 To b.Length
                Dim substitution As Integer = If(a(i - 1) = b(j - 1), 0, 1)
                Dim deletion As Integer = previous(j) + 1
                Dim insertion As Integer = current(j - 1) + 1
                Dim replace As Integer = previous(j - 1) + substitution

                current(j) = std.Min(std.Min(deletion, insertion), replace)
            Next

            Dim tmp As Integer() = previous
            previous = current
            current = tmp
        Next

        Return previous(b.Length)
    End Function

    ''' <summary>1 - editDistance / max(length), clamped into [0, 1].</summary>
    Public Function NormalizedLevenshtein(a As String, b As String) As Double
        If a Is Nothing Then a = ""
        If b Is Nothing Then b = ""

        Dim longest As Integer = std.Max(a.Length, b.Length)

        If longest = 0 Then
            Return 1.0
        End If

        Return Clamp01(1.0 - CDbl(LevenshteinDistance(a, b)) / CDbl(longest))
    End Function

    ''' <summary>Jaro similarity, the base of Jaro-Winkler.</summary>
    Public Function Jaro(a As String, b As String) As Double
        If a Is Nothing OrElse b Is Nothing Then Return 0.0
        If a.Length = 0 AndAlso b.Length = 0 Then Return 1.0
        If a.Length = 0 OrElse b.Length = 0 Then Return 0.0
        If a = b Then Return 1.0

        Dim matchDistance As Integer = std.Max(a.Length, b.Length) \ 2 - 1
        If matchDistance < 0 Then matchDistance = 0

        Dim aMatches(a.Length - 1) As Boolean
        Dim bMatches(b.Length - 1) As Boolean
        Dim matches As Integer = 0

        For i As Integer = 0 To a.Length - 1
            Dim startAt As Integer = std.Max(0, i - matchDistance)
            Dim endAt As Integer = std.Min(i + matchDistance + 1, b.Length)

            For j As Integer = startAt To endAt - 1
                If bMatches(j) Then Continue For
                If a(i) <> b(j) Then Continue For

                aMatches(i) = True
                bMatches(j) = True
                matches += 1
                Exit For
            Next
        Next

        If matches = 0 Then Return 0.0

        Dim transpositions As Double = 0.0
        Dim k As Integer = 0

        For i As Integer = 0 To a.Length - 1
            If Not aMatches(i) Then Continue For

            While Not bMatches(k)
                k += 1
            End While

            If a(i) <> b(k) Then
                transpositions += 1.0
            End If
            k += 1
        Next

        transpositions /= 2.0

        Dim m As Double = CDbl(matches)
        Dim value As Double = (m / CDbl(a.Length) +
                               m / CDbl(b.Length) +
                               (m - transpositions) / m) / 3.0

        Return Clamp01(value)
    End Function

    ''' <summary>Jaro-Winkler with the standard 0.1 prefix scaling and a 4 character cap.</summary>
    Public Function JaroWinkler(a As String, b As String) As Double
        Dim baseScore As Double = Jaro(a, b)
        If baseScore <= 0.0 Then Return 0.0

        If a Is Nothing OrElse b Is Nothing Then Return baseScore

        Dim prefix As Integer = 0
        Dim limit As Integer = std.Min(std.Min(a.Length, b.Length), 4)

        For i As Integer = 0 To limit - 1
            If a(i) <> b(i) Then Exit For
            prefix += 1
        Next

        Return Clamp01(baseScore + 0.1 * CDbl(prefix) * (1.0 - baseScore))
    End Function

    ''' <summary>
    ''' Blended name similarity. Jaro-Winkler rewards a shared prefix while
    ''' normalized Levenshtein is more robust for short chemical terms.
    ''' </summary>
    Public Function Blend(a As String, b As String) As Double
        Dim lev As Double = NormalizedLevenshtein(a, b)
        Dim jw As Double = JaroWinkler(a, b)
        Return Clamp01(0.45 * lev + 0.55 * jw)
    End Function

End Module

''' <summary>
''' Canonicalizes a knowledge term before it is used as a name clue or as a
''' lookup key in a semantic model.
''' </summary>
''' <remarks>
''' Pipeline: Unicode NFKC - lowercase - strip accents - collapse whitespace
''' and punctuation - optional Porter stemming. It reuses the HuggingFace
''' normalizers shipped with this assembly and explicitly calls Stemmer.stem()
''' (the existing StemmerNormalize extension only calls add + ToString).
''' </remarks>
Public NotInheritable Class TermNormalizer

    Private ReadOnly _pipeline As INormalizer
    Private ReadOnly _useStemmer As Boolean

    Private Shared ReadOnly _instance As New TermNormalizer(True)

    ''' <summary>A shared, stateless instance.</summary>
    Public Shared ReadOnly Property Instance As TermNormalizer
        Get
            Return _instance
        End Get
    End Property

    Public Sub New(Optional useStemmer As Boolean = True)
        Dim steps As New List(Of INormalizer)()
        steps.Add(New UnicodeNormalizer(NormalizationForm.FormKC))
        steps.Add(New LowercaseNormalizer())
        steps.Add(New StripAccentsNormalizer())

        _pipeline = New SequenceNormalizer(steps)
        _useStemmer = useStemmer
    End Sub

    ''' <summary>Normalize a term into a lower-case, accent-free, spaced form.</summary>
    Public Function Normalize(text As String) As String
        If text Is Nothing Then Return ""

        Dim value As String = _pipeline.Normalize(text.Trim())
        Dim buffer As New StringBuilder(value.Length)
        Dim lastWasSpace As Boolean = True

        For Each ch As Char In value
            Dim isSeparator As Boolean = Char.IsWhiteSpace(ch) OrElse
                                         Char.IsPunctuation(ch) OrElse
                                         Char.IsSymbol(ch)

            If isSeparator Then
                If Not lastWasSpace Then
                    buffer.Append(" "c)
                    lastWasSpace = True
                End If
            Else
                buffer.Append(ch)
                lastWasSpace = False
            End If
        Next

        Dim result As String = buffer.ToString().Trim()

        If Not _useStemmer OrElse result.Length = 0 Then
            Return result
        End If

        ' Stem each token, but keep multi-token names intact so that
        ' "dihydrogen oxide" and "hydrogen hydroxide" remain distinguishable.
        Dim tokens As String() = result.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim stemmed As New StringBuilder(result.Length)

        For i As Integer = 0 To tokens.Length - 1
            If i > 0 Then stemmed.Append(" "c)
            stemmed.Append(StemToken(tokens(i)))
        Next

        Return stemmed.ToString()
    End Function

    ''' <summary>A punctuation-free compact key, useful for exact alias matching.</summary>
    Public Function LooseKey(text As String) As String
        Return Normalize(text).Replace(" ", "")
    End Function

    ''' <summary>Porter-stem one token. Long/unusual tokens are returned unchanged.</summary>
    Public Function StemToken(word As String) As String
        If String.IsNullOrEmpty(word) OrElse word.Length > 64 Then
            Return word
        End If

        Dim lower As String = word.ToLowerInvariant()
        Dim stemmer As New Stemmer()

        stemmer.add(lower.ToCharArray(), lower.Length)
        stemmer.stem()

        Dim result As String = stemmer.ToString()

        Return If(String.IsNullOrEmpty(result), lower, result)
    End Function

    ''' <summary>Split a term into normalized tokens.</summary>
    Public Function NormalizeTerms(text As String) As String()
        Return Normalize(text).Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
    End Function

End Class
