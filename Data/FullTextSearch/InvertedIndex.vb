
Imports Microsoft.VisualBasic.Data.NLP.Model
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
        Dim tokens As String()

        doc = Strings.Trim(doc).ToLower

        If doc.StringEmpty Then
            Return
        Else
            tokens = doc.Split({"+"c, "-"c, "*"c, "/"c, "."c, ","c})
        End If

        If tokens.Length = 0 Then
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

End Class
