Imports System.Text

Namespace LDA

    ''' <summary>
    ''' a word set
    ''' 
    ''' @author hankcs
    ''' </summary>
    Public Class Vocabulary
        Friend word2idMap As IDictionary(Of String, Integer?)
        Friend id2wordMap As String()

        Public Sub New()
            word2idMap = New SortedDictionary(Of String, Integer?)()
            id2wordMap = New String(1023) {}
        End Sub

        Public Overridable Function getId(ByVal word As String) As Integer?
            Return getId(word, False)
        End Function

        Public Overridable Function getWord(ByVal id As Integer) As String
            Return id2wordMap(id)
        End Function

        Public Overridable Function getId(ByVal word As String, ByVal create As Boolean) As Integer?
            Dim id = word2idMap.GetValueOrNull(word)

            If Not create Then
                Return id
            End If

            If id Is Nothing Then
                id = word2idMap.Count
            End If

            word2idMap(word) = id

            If id2wordMap.Length - 1 < id Then
                resize(word2idMap.Count * 2)
            End If

            id2wordMap(id) = word
            Return id
        End Function

        Private Sub resize(ByVal n As Integer)
            Dim nArray = New String(n - 1) {}
            Array.Copy(id2wordMap, 0, nArray, 0, id2wordMap.Length)
            id2wordMap = nArray
        End Sub

        Private Sub loseWeight()
            If size() = id2wordMap.Length Then
                Return
            End If

            resize(word2idMap.Count)
        End Sub

        Public Overridable Function size() As Integer
            Return word2idMap.Count
        End Function

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()

            For i = 0 To id2wordMap.Length - 1

                If ReferenceEquals(id2wordMap(i), Nothing) Then
                    Exit For
                End If

                sb.Append(i).Append("=").Append(id2wordMap(i)).Append(vbLf)
            Next

            Return sb.ToString()
        End Function
    End Class
End Namespace
