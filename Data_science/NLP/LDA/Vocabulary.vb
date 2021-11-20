Imports System.Runtime.CompilerServices
Imports System.Text

Namespace LDA

    ''' <summary>
    ''' a word set
    ''' 
    ''' @author hankcs
    ''' </summary>
    Public Class Vocabulary

        Dim word2idMap As IDictionary(Of String, Integer?)
        Dim id2wordMap As String()

        Public ReadOnly Property size() As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return word2idMap.Count
            End Get
        End Property

        Public Sub New()
            word2idMap = New SortedDictionary(Of String, Integer?)()
            id2wordMap = New String(1023) {}
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function getId(word As String) As Integer?
            Return getId(word, False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function getWord(id As Integer) As String
            Return id2wordMap(id)
        End Function

        Public Overridable Function getId(word As String, create As Boolean) As Integer?
            Dim id = word2idMap.GetValueOrNull(word)

            If Not create Then
                Return id
            End If

            If id Is Nothing Then
                id = word2idMap.Count
            End If

            word2idMap(word) = id

            If id2wordMap.Length - 1 < id Then
                Call resize(word2idMap.Count * 2)
            End If

            id2wordMap(id) = word

            Return id
        End Function

        Private Sub resize(n As Integer)
            Dim nArray = New String(n - 1) {}
            Array.Copy(id2wordMap, 0, nArray, 0, id2wordMap.Length)
            id2wordMap = nArray
        End Sub

        Public Sub loseWeight()
            If size() = id2wordMap.Length Then
                Return
            End If

            Call resize(word2idMap.Count)
        End Sub

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder()

            For i As Integer = 0 To id2wordMap.Length - 1
                If id2wordMap(i) Is Nothing Then
                    Exit For
                End If

                sb.Append(i).Append("=").Append(id2wordMap(i)).Append(vbLf)
            Next

            Return sb.ToString()
        End Function
    End Class
End Namespace
