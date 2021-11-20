Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values

Namespace LDA

    ''' <summary>
    ''' a set of documents
    ''' 语料库，也就是文档集合
    ''' 
    ''' @author hankcs
    ''' </summary>
    Public Class Corpus
        Friend documentList As IList(Of Integer())
        Friend vocabularyField As Vocabulary

        Public Sub New()
            documentList = New List(Of Integer())()
            vocabularyField = New Vocabulary()
        End Sub

        Public Overridable Function addDocument(ByVal document As IList(Of String)) As Integer()
            Dim doc = New Integer(document.Count - 1) {}
            Dim i = 0

            For Each word In document
                doc(System.Math.Min(Threading.Interlocked.Increment(i), i - 1)) = vocabularyField.getId(word, True)
            Next

            documentList.Add(doc)
            Return doc
        End Function

        Public Overridable Function toArray() As Integer()()
            Return documentList.ToArray()
        End Function

        Public Overridable ReadOnly Property VocabularySize As Integer
            Get
                Return vocabularyField.size()
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()

            For Each doc In documentList
                sb.Append(doc.JoinBy(", ")).Append(vbLf)
            Next

            sb.Append(vocabularyField)
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Load documents from disk
        ''' </summary>
        ''' <param name="folderPath"> is a folder, which contains text documents. </param>
        ''' <returns> a corpus </returns>
        ''' <exception cref="IOException"> </exception>
        Public Shared Function load(ByVal folderPath As String) As Corpus
            Dim corpus As Corpus = New Corpus()

            For Each filepath In folderPath.ListFiles()
                Dim file = filepath.Open(doClear:=False, [readOnly]:=True)
                Dim br As StreamReader = New StreamReader(file, Encoding.UTF8)
                Dim line As New Value(Of String)
                Dim wordList As New List(Of String)()

                While Not line = br.ReadLine() Is Nothing
                    Dim words = line.Split(" ")

                    For Each word In words

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

        Public Overridable ReadOnly Property Vocabulary As Vocabulary
            Get
                Return vocabularyField
            End Get
        End Property

        Public Overridable ReadOnly Property Document As Integer()()
            Get
                Return toArray()
            End Get
        End Property

        Public Shared Function loadDocument(ByVal path As String, ByVal vocabulary As Vocabulary) As Integer()
            Dim br As StreamReader = New StreamReader(path)
            Dim line As New Value(Of String)
            Dim wordList As New List(Of Integer)()

            While Not line = br.ReadLine() Is Nothing
                Dim words = line.Split(" ")

                For Each word In words

                    If word.Trim().Length < 2 Then
                        Continue For
                    End If

                    Dim id = vocabulary.getId(word)

                    If id IsNot Nothing Then
                        wordList.Add(id)
                    End If
                Next
            End While

            br.Close()
            Dim result = New Integer(wordList.Count - 1) {}
            Dim i = 0

            For Each [integer] As Integer In wordList
                result(System.Math.Min(Threading.Interlocked.Increment(i), i - 1)) = [integer]
            Next

            Return result
        End Function
    End Class
End Namespace
