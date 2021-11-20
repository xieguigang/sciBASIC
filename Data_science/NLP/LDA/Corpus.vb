Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language

Namespace LDA

    ''' <summary>
    ''' a set of documents
    ''' 语料库，也就是文档集合
    ''' 
    ''' @author hankcs
    ''' </summary>
    Public Class Corpus

        ReadOnly documentList As IList(Of Integer())
        ReadOnly vocabularyField As Vocabulary

        Public Overridable ReadOnly Property VocabularySize As Integer
            Get
                Return vocabularyField.size()
            End Get
        End Property

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

        Public Sub New()
            documentList = New List(Of Integer())()
            vocabularyField = New Vocabulary()
        End Sub

        Public Overridable Function addDocument(document As IEnumerable(Of String)) As Integer()
            Dim doc As New List(Of Integer)

            For Each word As String In document
                Call doc.Add(vocabularyField.getId(word, True))
            Next

            Call documentList.Add(doc.ToArray)

            Return documentList.Last
        End Function

        Public Overridable Function toArray() As Integer()()
            Return documentList.ToArray()
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder()

            For Each doc In documentList
                sb.Append(doc.JoinBy(", ")).Append(vbLf)
            Next

            Call sb.Append(vocabularyField)

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Load documents from disk
        ''' </summary>
        ''' <param name="folderPath"> is a folder, which contains text documents. </param>
        ''' <returns> a corpus </returns>
        ''' <exception cref="IOException"> </exception>
        Public Shared Function load(folderPath As String) As Corpus
            Return DocumentLoader.load(folderPath)
        End Function

        Public Shared Function loadDocument(path As String, vocabulary As Vocabulary) As Integer()
            Return DocumentLoader.loadDocument(path, vocabulary)
        End Function
    End Class
End Namespace
