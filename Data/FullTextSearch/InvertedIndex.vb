Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' The core of FTS is a data structure called Inverted Index. 
''' The Inverted Index associates every word in documents with documents that contain the word.
''' </summary>
''' <remarks>
''' https://artem.krylysov.com/blog/2020/07/28/lets-build-a-full-text-search-engine/
''' </remarks>
Public Class InvertedIndex

    ReadOnly index As New Dictionary(Of String, List(Of Integer))
    ReadOnly rawDocs As New List(Of String)

    Dim id As i32 = 0

    Public Sub Add(docs As IEnumerable(Of String))
        For Each doc As String In docs
            Call add(doc)
        Next
    End Sub

    Public Sub Add(doc As String)
        Dim id As Integer
        Dim tokens As String() = split(doc)

        If tokens.IsNullOrEmpty Then
            Return
        Else
            id = ++Me.id
        End If

        For Each str As String In tokens
            If Not index.ContainsKey(str) Then
                Call index.Add(str, New List(Of Integer))
            End If

            Call index(str).Add(id)
        Next

        Call rawDocs.Add(doc)
    End Sub

    Private Function split(doc As String) As String()
        doc = Strings.Trim(doc).ToLower

        If doc.StringEmpty Then
            Return Nothing
        Else
            Return doc.Split({"+"c, "-"c, "*"c, "/"c, "."c, ","c})
        End If
    End Function

    ''' <summary>
    ''' Boolean queries
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Iterator Function Search(text As String) As IEnumerable(Of SeqValue(Of String))
        Dim tokens As String() = split(text)

        If tokens.IsNullOrEmpty Then
            Return
        End If

        Dim r As List(Of Integer) = Nothing

        For Each token As String In tokens
            If index.ContainsKey(token) Then
                If r Is Nothing Then
                    r = New List(Of Integer)(index(token))
                Else
                    r = intersection(r, index(token))
                End If
            Else
                ' Token doesn't exist.
                Return
            End If
        Next

        For Each i As Integer In r
            Yield New SeqValue(Of String)(i, rawDocs(i))
        Next
    End Function

    Private Shared Function intersection(a As List(Of Integer), b As List(Of Integer)) As List(Of Integer)
        Dim maxLen = a.Count

        If b.Count > maxLen Then
            maxLen = b.Count
        End If

        Dim r As New List(Of Integer)
        Dim i, j As Integer

        Do While i < a.Count AndAlso j < b.Count
            If a(i) < b(j) Then
                i += 1
            ElseIf a(i) > b(j) Then
                j += 1
            Else
                r.Add(a(i))
                i += 1
                j += 1
            End If
        Loop

        Return r
    End Function

End Class
