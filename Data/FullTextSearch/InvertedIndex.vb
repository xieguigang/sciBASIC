Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

''' <summary>
''' The core of FTS is a data structure called Inverted Index. 
''' The Inverted Index associates every word in documents with documents that contain the word.
''' </summary>
''' <remarks>
''' https://artem.krylysov.com/blog/2020/07/28/lets-build-a-full-text-search-engine/
''' </remarks>
Public Class InvertedIndex : Implements Enumeration(Of NamedCollection(Of Integer))

    ReadOnly index As New Dictionary(Of String, List(Of Integer))

    Dim id As i32 = 0

    Public ReadOnly Property lastId As Integer
        Get
            Return CInt(id)
        End Get
    End Property

    Public ReadOnly Property size As Integer
        Get
            Return index.Count
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(index As Dictionary(Of String, List(Of Integer)), lastId As Integer)
        Me.id = lastId
        Me.index = index
    End Sub

    Public Sub Add(docs As IEnumerable(Of String))
        For Each doc As String In docs
            Call Add(doc)
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
            If str = "" Then
                Continue For
            End If

            If Not index.ContainsKey(str) Then
                Call index.Add(str, New List(Of Integer))
            End If

            If index(str).Count = 0 OrElse index(str).Last <> id Then
                Call index(str).Add(id)
            End If
        Next
    End Sub

    Private Function split(doc As String) As String()
        doc = Strings.Trim(doc).ToLower

        If doc.StringEmpty Then
            Return Nothing
        Else
            Return doc.Split({"+"c, "-"c, "*"c, "/"c, "."c, ","c, " "c, ASCII.TAB})
        End If
    End Function

    ''' <summary>
    ''' Boolean queries
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function Search(text As String) As IReadOnlyCollection(Of Integer)
        Dim tokens As String() = split(text)

        If tokens.IsNullOrEmpty Then
            Return Nothing
        End If

        Dim r As List(Of Integer) = Nothing

        For Each token As String In tokens
            If token = "" Then
                Continue For
            End If

            If index.ContainsKey(token) Then
                If r Is Nothing Then
                    r = New List(Of Integer)(index(token))
                Else
                    r = intersection(r, index(token))
                End If
            Else
                ' Token doesn't exist.
                Return Nothing
            End If
        Next

        Return r
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

    Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedCollection(Of Integer)) Implements Enumeration(Of NamedCollection(Of Integer)).GenericEnumerator
        For Each token In index
            Yield New NamedCollection(Of Integer)(token.Key, token.Value)
        Next
    End Function
End Class
