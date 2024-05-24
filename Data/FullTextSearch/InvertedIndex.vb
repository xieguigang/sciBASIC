Imports Microsoft.VisualBasic.Language

''' <summary>
''' The core of FTS is a data structure called Inverted Index. 
''' The Inverted Index associates every word in documents with documents that contain the word.
''' </summary>
Public Class InvertedIndex

    ReadOnly index As Dictionary(Of String, List(Of Integer))

    Dim id As i32 = 0

    Public Sub Add(docs As IEnumerable(Of String))
        For Each doc As String In docs
            Call add(doc)
        Next
    End Sub

    Private Sub add(doc As String)
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
    ''' Querying
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Iterator Function Search(text As String) As IEnumerable(Of IReadOnlyCollection(Of Integer))
        Dim tokens As String() = split(text)

        If tokens.IsNullOrEmpty Then
            Return
        End If

        For Each token As String In tokens
            If index.ContainsKey(token) Then
                Yield DirectCast(index(token), IReadOnlyCollection(Of Integer))
            End If
        Next
    End Function

End Class
