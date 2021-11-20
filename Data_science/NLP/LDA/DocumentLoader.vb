Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values

Namespace LDA

    Module DocumentLoader

        ''' <summary>
        ''' Load documents from disk
        ''' </summary>
        ''' <param name="folderPath"> is a folder, which contains text documents. </param>
        ''' <returns> a corpus </returns>
        ''' <exception cref="IOException"> </exception>
        Public Function load(folderPath As String) As Corpus
            Dim corpus As Corpus = New Corpus()

            For Each filepath As String In folderPath.ListFiles()
                Dim file = filepath.Open(doClear:=False, [readOnly]:=True)
                Dim br As New StreamReader(file, Encoding.UTF8)
                Dim line As New Value(Of String)
                Dim wordList As New List(Of String)()

                While Not line = br.ReadLine() Is Nothing
                    Dim words = line.Split(" ")

                    For Each word As String In words
                        If word.Trim().Length < 2 Then
                            Continue For
                        End If

                        wordList.Add(word)
                    Next
                End While

                br.Close()
                corpus.addDocument(wordList)
            Next

            If corpus.VocabularySize = 0 Then
                Return Nothing
            End If

            Return corpus
        End Function

        Public Function loadDocument(path As String, vocabulary As Vocabulary) As Integer()
            Dim br As New StreamReader(path)
            Dim line As New Value(Of String)
            Dim wordList As New List(Of Integer)()

            While Not line = br.ReadLine() Is Nothing
                Dim words = line.Split(" ")

                For Each word As String In words
                    If word.Trim().Length < 2 Then
                        Continue For
                    End If

                    Dim id = vocabulary.getId(word)

                    If id IsNot Nothing Then
                        wordList.Add(id)
                    End If
                Next
            End While

            Call br.Close()

            Dim result = New Integer(wordList.Count - 1) {}
            Dim i As i32 = 0

            For Each [integer] As Integer In wordList
                result(++i) = [integer]
            Next

            Return result
        End Function
    End Module
End Namespace