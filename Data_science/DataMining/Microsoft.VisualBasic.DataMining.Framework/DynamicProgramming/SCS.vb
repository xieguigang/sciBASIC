Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' shortest_common_superstring
''' </summary>
Public Module SCS

    ''' <summary>
    ''' https://github.com/feliperfmarques/challenge1_gnmk/blob/master/shortest_common_superstring.py
    ''' </summary>
    ''' <param name="originalSeqs$"></param>
    ''' <returns></returns>
    Public Function shortest_common_superstring(originalSeqs$()) As String
        Dim paths As New Dictionary(Of Integer, List(Of String))
        paths(0) = New List(Of String) From {""}

        Do While paths.Count > 0
            Dim minLength = paths.Keys.Min

            Do While paths(minLength).Count > 0
                Dim candidate = paths(minLength).Pop
                Dim seqAdded As Boolean = False

                For Each seq In originalSeqs
                    If candidate.Contains(seq) Then
                        Continue For
                    End If

                    seqAdded = True
                    For Each i In (seq.Length + 1).SeqIterator
                        If candidate.EndsWith(Mid(seq, 1, i)) Then
                            Dim newCandidate = candidate + Mid(seq, i)
                            paths(newCandidate.Length).Add(newCandidate)
                        End If
                    Next
                Next

                If Not seqAdded Then
                    Return candidate
                End If
            Loop

            paths.Remove(minLength)
        Loop

        ' 这些序列都没有共同的部分
        Return Nothing
    End Function
End Module
