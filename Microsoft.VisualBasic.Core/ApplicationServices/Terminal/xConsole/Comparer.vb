Namespace Terminal.xConsole

#Region "COMPARER 💻"

    ''' <summary>
    ''' This can compute the input then return back the most appropriate word.
    ''' </summary>
    Public Class Comparer

        ''' <summary>
        ''' This is the word to find
        ''' </summary>
        Public Word As String = String.Empty

        ''' <summary>
        ''' Descrizione
        ''' </summary>
        Public Description As String = String.Empty

        ''' <summary>
        ''' Init to 0!
        ''' </summary>
        Private Points As Integer = 0

        ''' <summary>
        ''' Initliaze a new instance
        ''' </summary>
        ''' <param name="w">The word to find</param>
        ''' <param name="p">It's should be 0</param>
        Public Sub New(w As String, p As Integer)
            Word = w
            Points = p
        End Sub

        ''' <summary>
        ''' Initliaze a new instance
        ''' </summary>
        ''' <param name="w">The word to find</param>
        Public Sub New(w As String)
            Word = w
            Points = 0
        End Sub

        ''' <summary>
        ''' Initliaze a new instance
        ''' </summary>
        ''' <param name="w">The word to find</param>
        ''' <param name="desc">Description (do nothing)</param>
        Public Sub New(w As String, desc As String)
            Word = w
            Description = desc
        End Sub

        ''' <summary>
        ''' Find a word from an input abbreviation (es n > name)
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Shared Function Find(abbr As String, ByRef Words As List(Of Comparer)) As String
            Dim Result As String = String.Empty
            Dim Best As Integer = 0
            Dim c As Integer = 0

            While Words.Count > c AndAlso Words(c) IsNot Nothing
                Dim word = Words(c)

                If abbr = word.Word Then
                    Result = abbr
                    Exit While
                End If

                For i As Integer = 0 To abbr.Length - 1
                    If abbr.Length < word.Word.Length AndAlso abbr(i) = word.Word(i) Then
                        word.Points += 1
                    Else
                        word.Points = 0
                        Exit For
                    End If
                Next

                If word.Points > Best Then
                    Best = word.Points
                End If


                c += 1
            End While
            ' End while
            Dim n As Integer = 0
            For Each word As Comparer In Words
                If word.Points = Best AndAlso word.Points > 0 Then
                    Result = word.Word
                    If System.Threading.Interlocked.Increment(n) > 1 Then
                        Result = String.Empty
                    End If
                End If
            Next

            Return Result
        End Function
    End Class

    '////////////////////////////////////////////////////////////////////////////////////////////////
    '////////////////////////////////////////////////////////////////////////////////////////////////
    '////////////////////////////////////////////////////////////////////////////////////////////////
#End Region
End Namespace