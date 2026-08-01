#Region "Microsoft.VisualBasic::c6c0849c19d91412ce689407eae38998, Data_science\NLP\TF-IDF\DocumentTFIDF.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 138
    '    Code Lines: 39 (28.26%)
    ' Comment Lines: 86 (62.32%)
    '    - Xml Docs: 79.07%
    ' 
    '   Blank Lines: 13 (9.42%)
    '     File Size: 5.97 KB


    ' Class DocumentTFIDF
    ' 
    '     Properties: docSize
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: getDocumentVector, getSimilarity, parseDocuments
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math

''' <summary>
''' TF-IDF algorithm: http://en.wikipedia.org/wiki/Tf*idf
''' </summary>
''' <remarks>
''' **TF-IDF (Term Frequency-Inverse Document Frequency)** is a statistical measure that evaluates how relevant
''' a word is to a document in a collection of documents. It is one of the most popular methods used for feature 
''' extraction in text data. The TF-IDF score for a word increases proportionally to the number of times a word 
''' appears in the document but is offset by the frequency of the word in the collection of documents.
''' 
''' **Components of TF-IDF:**
''' 
''' 1. **Term Frequency (TF):** 
''' 
'''   - Measures how frequently a term occurs in a document. 
'''   - Calculated as the number of times a term appears in a document divided by the total number of terms in the document.
'''   - Formula: \( \text{TF}(t,d) = \frac{\text{Number of times term t appears in document d}}{\text{Total number of terms in document d}} \)
'''   
''' 2. **Inverse Document Frequency (IDF):**
''' 
'''   - Measures how important a term is to a collection of documents.
'''  - Calculated as the logarithm of the total number of documents divided by the number of documents containing the term.
'''   - Formula: \( \text{IDF}(t,D) = \log\left(\frac{\text{Total number of documents}}{\text{Number of documents containing term t}}\right) \)
'''   
''' 3. **TF-IDF Score:**
''' 
'''  - Combines TF and IDF to compute the TF-IDF score for a term in a document.
'''   - Formula: \( \text{TF-IDF}(t,d,D) = \text{TF}(t,d) \times \text{IDF}(t,D) \)
'''   
''' **Interpretation:**
''' 
''' - A high TF-IDF score indicates that the term is highly relevant to the document and rare across all documents.
''' - A low TF-IDF score indicates that the term is either not very relevant to the document or very common across all documents.
''' 
''' **Applications:**
''' 
''' - **Information Retrieval:** Used to rank documents based on the relevance of search queries.
''' - **Text Mining:** Helps in identifying important terms and features in text data.
''' - **Machine Learning:** Used as a feature vector for text classification, clustering, and other NLP tasks.
''' 
''' **Advantages:**
''' 
''' - Simple and intuitive.
''' - Effective in identifying important terms in a document.
''' - Widely used and well-understood in the field of text analysis.
''' 
''' **Disadvantages:**
''' 
''' - Does not consider the semantic meaning of words.
''' - Can be biased towards longer documents.
''' - May not perform well with very rare or very common terms.
''' 
''' **Example:**
''' 
''' Consider a collection of documents about animals. The term "animal" might appear frequently in all documents, 
''' resulting in a high TF but a low IDF, and thus a lower TF-IDF score. Conversely, a specific term like "giraffe"
''' might appear less frequently but only in a few documents, resulting in a higher TF-IDF score, indicating its 
''' relevance to those specific documents.
''' 
''' In summary, TF-IDF is a powerful and widely-used technique for text analysis that helps in identifying the 
''' importance of terms within a document relative to a collection of documents.
''' </remarks>
Public Class DocumentTFIDF

    ''' <summary>
    ''' a common set of English stopwords
    ''' </summary>
    ReadOnly stopwords As ISet(Of String)
    ''' <summary>
    ''' the tf-idf algorithm engine
    ''' </summary>
    ReadOnly tfidf As New TFIDF

    Public ReadOnly Property docSize As Integer
        Get
            Return tfidf.N
        End Get
    End Property

    ''' <summary>
    ''' Constructor </summary>
    ''' <param name="documents"> documents represented as strings </param>
    ''' <remarks>
    ''' document similarity measurement
    ''' </remarks>
    Public Sub New(documents As String(), listOfstopwords As IEnumerable(Of String))
        Dim id As i32 = 0

        stopwords = New HashSet(Of String)(listOfstopwords)

        For Each doc As IList(Of String) In parseDocuments(documents)
            Call tfidf.Add($"doc_{++id}", doc)
        Next
    End Sub

    ''' <summary>
    ''' Parse documents into bags of words </summary>
    ''' <param name="docs"> documents in strings </param>
    ''' <returns> a list of documents represented by bags of words </returns>
    Private Iterator Function parseDocuments(docs As String()) As IEnumerable(Of IList(Of String))
        For Each doc As String In docs
            Dim words As String() = doc.StringReplace("\p{Punct}", "").ToLower().StringSplit("\s", True)
            Dim wordList As IList(Of String) = New List(Of String)()

            For Each word_str As String In words
                Dim word As String = word_str.Trim()

                If word.Length > 0 AndAlso Not stopwords.Contains(word) Then
                    Call wordList.Add(word)
                End If
            Next

            Yield wordList
        Next
    End Function

    ''' <summary>
    ''' Get similarity score between two documents </summary>
    ''' <param name="doc_i"> index of one document </param>
    ''' <param name="doc_j"> index of another document </param>
    ''' <returns> similarity score </returns>
    Public Overridable Function getSimilarity(doc_i As Integer, doc_j As Integer) As Double
        Dim vector1 = getDocumentVector(doc_i)
        Dim vector2 = getDocumentVector(doc_j)

        Return SSM_SIMD(vector1, vector2)
    End Function

    ''' <summary>
    ''' Compile a vector for a document </summary>
    ''' <param name="docIndex"> index of a document </param>
    ''' <returns> the vector representation of the document </returns>
    Private Function getDocumentVector(docIndex As Integer) As Double()
        Return tfidf.TfidfVectorizer($"doc_{docIndex}")
    End Function
End Class
