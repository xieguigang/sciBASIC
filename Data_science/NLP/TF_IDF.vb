#Region "Microsoft.VisualBasic::d92d70db4325f5c727344dc331e015e0, Data_science\NLP\TF_IDF.vb"

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

    '   Total Lines: 223
    '    Code Lines: 111 (49.78%)
    ' Comment Lines: 86 (38.57%)
    '    - Xml Docs: 95.35%
    ' 
    '   Blank Lines: 26 (11.66%)
    '     File Size: 9.54 KB


    ' Class TF_IDF
    ' 
    '     Properties: docSize
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: countTermOccurrenceInOneDoc, generateTerms, getDocumentVector, getIDFMeasure, getSimilarity
    '               getTFMeasure, parseDocuments
    ' 
    '     Sub: countTermOccurrence, generateTermWeight
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math

''' <summary>
''' TF-IDF algorithm in Java. An explanation of this algorithm can be found at
''' 
'''      http://en.wikipedia.org/wiki/Tf*idf
''' 
''' @author Bodong Chen &lt;bodong.chen>
''' 
''' > https://github.com/meefen/npl-lab/tree/master
''' </summary>
''' <remarks>
''' **TF-IDF (Term Frequency-Inverse Document Frequency)** is a statistical measure that evaluates how relevant
''' a word is to a document in a collection of documents. It is one of the most popular methods used for feature 
''' extraction in text data. The TF-IDF score for a word increases proportionally to the number of times a word 
''' appears in the document but is offset by the frequency of the word in the collection of documents.
''' **Components of TF-IDF:**
''' 1. **Term Frequency (TF):** 
'''   - Measures how frequently a term occurs in a document. 
'''   - Calculated as the number of times a term appears in a document divided by the total number of terms in the document.
'''   - Formula: \( \text{TF}(t,d) = \frac{\text{Number of times term t appears in document d}}{\text{Total number of terms in document d}} \)
''' 2. **Inverse Document Frequency (IDF):**
'''   - Measures how important a term is to a collection of documents.
'''  - Calculated as the logarithm of the total number of documents divided by the number of documents containing the term.
'''   - Formula: \( \text{IDF}(t,D) = \log\left(\frac{\text{Total number of documents}}{\text{Number of documents containing term t}}\right) \)
''' 3. **TF-IDF Score:**
'''  - Combines TF and IDF to compute the TF-IDF score for a term in a document.
'''   - Formula: \( \text{TF-IDF}(t,d,D) = \text{TF}(t,d) \times \text{IDF}(t,D) \)
''' **Interpretation:**
''' - A high TF-IDF score indicates that the term is highly relevant to the document and rare across all documents.
''' - A low TF-IDF score indicates that the term is either not very relevant to the document or very common across all documents.
''' **Applications:**
''' - **Information Retrieval:** Used to rank documents based on the relevance of search queries.
''' - **Text Mining:** Helps in identifying important terms and features in text data.
''' - **Machine Learning:** Used as a feature vector for text classification, clustering, and other NLP tasks.
''' **Advantages:**
''' - Simple and intuitive.
''' - Effective in identifying important terms in a document.
''' - Widely used and well-understood in the field of text analysis.
''' **Disadvantages:**
''' - Does not consider the semantic meaning of words.
''' - Can be biased towards longer documents.
''' - May not perform well with very rare or very common terms.
''' **Example:**
''' Consider a collection of documents about animals. The term "animal" might appear frequently in all documents, 
''' resulting in a high TF but a low IDF, and thus a lower TF-IDF score. Conversely, a specific term like "giraffe"
''' might appear less frequently but only in a few documents, resulting in a higher TF-IDF score, indicating its 
''' relevance to those specific documents.
''' 
''' In summary, TF-IDF is a powerful and widely-used technique for text analysis that helps in identifying the 
''' importance of terms within a document relative to a collection of documents.
''' </remarks>
Public Class TF_IDF

    Private stopwords As ISet(Of String) ' a common set of English stopwords

    Private docs As IList(Of IList(Of String)) ' documents as bags of words, with stopwords removed
    Private numDocs As Integer

    Private terms As List(Of String) ' unique terms
    Private numTerms As Integer

    Private termFreq As Integer()() ' tf matrix
    Private termWeight As Double()() ' tf-idf matrix
    Private docFreq As Integer() ' terms' frequency in all documents

    Public ReadOnly Property docSize As Integer
        Get
            Return docs.Count
        End Get
    End Property

    ''' <summary>
    ''' Constructor </summary>
    ''' <param name="documents"> documents represented as strings </param>
    ''' <remarks>
    ''' document similarity measurement
    ''' </remarks>
    Public Sub New(documents As String(), listOfstopwords As IEnumerable(Of String))
        stopwords = New HashSet(Of String)(listOfstopwords)
        docs = parseDocuments(documents)
        numDocs = docs.Count

        terms = generateTerms(docs)
        numTerms = terms.Count

        docFreq = New Integer(numTerms - 1) {}
        termFreq = RectangularArray.Matrix(Of Integer)(numTerms, numDocs)
        termWeight = RectangularArray.Matrix(Of Double)(numTerms, numDocs)

        countTermOccurrence()
        generateTermWeight()
    End Sub

    ''' <summary>
    ''' Parse documents into bags of words </summary>
    ''' <param name="docs"> documents in strings </param>
    ''' <returns> a list of documents represented by bags of words </returns>
    Private Function parseDocuments(docs As String()) As IList(Of IList(Of String))
        Dim parsedDocs As IList(Of IList(Of String)) = New List(Of IList(Of String))()

        For Each doc In docs
            Dim words As String() = doc.StringReplace("\p{Punct}", "").ToLower().StringSplit("\s", True)
            Dim wordList As IList(Of String) = New List(Of String)()
            For Each wordi In words
                Dim word = wordi.Trim()
                If word.Length > 0 AndAlso Not stopwords.Contains(word) Then
                    wordList.Add(word)
                End If
            Next
            parsedDocs.Add(wordList)
        Next

        Return parsedDocs
    End Function

    ''' <summary>
    ''' Generate terms from a list of documents </summary>
    ''' <param name="docs"> </param>
    ''' <returns>  </returns>
    Private Function generateTerms(docs As IList(Of IList(Of String))) As List(Of String)
        Dim uniqueTerms As List(Of String) = New List(Of String)()
        For Each doc In docs
            For Each word In doc
                If Not uniqueTerms.Contains(word) Then
                    uniqueTerms.Add(word)
                End If
            Next
        Next
        Return uniqueTerms
    End Function

    ''' <summary>
    ''' Count term occurrence
    ''' and occurrence of each term in the whole corpus
    ''' </summary>
    Private Sub countTermOccurrence()
        For i = 0 To docs.Count - 1
            Dim doc = docs(i)
            Dim tfMap = countTermOccurrenceInOneDoc(doc)
            For Each entry In tfMap
                Dim word = entry.Key
                Dim wordFreq = entry.Value
                Dim termIndex = terms.IndexOf(word)

                termFreq(termIndex)(i) = wordFreq
                docFreq(termIndex) += 1
            Next
        Next
    End Sub

    ''' <summary>
    ''' Count term frequency in a document </summary>
    ''' <param name="doc"> a document as a bag of words </param>
    ''' <returns> a map of term occurrence; key - term; value - occurrence. </returns>
    Private Function countTermOccurrenceInOneDoc(doc As IList(Of String)) As Dictionary(Of String, Integer)

        Dim tfMap As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)()

        For Each word In doc
            Dim count = 0
            For Each str As String In doc
                If str.Equals(word) Then
                    count += 1
                End If
            Next

            tfMap(word) = count
        Next

        Return tfMap
    End Function

    ''' <summary>
    ''' Calculate term weight based on tf*idf algorithm
    ''' There are different choices in calculating tf and idf.
    ''' So you may want to change it to fit your own needs.
    ''' </summary>
    Private Sub generateTermWeight()
        For i = 0 To numTerms - 1
            For j = 0 To numDocs - 1
                Dim tf = getTFMeasure(i, j)
                Dim idf = getIDFMeasure(i)
                termWeight(i)(j) = tf * idf
            Next
        Next
    End Sub

    Private Function getTFMeasure(term As Integer, doc As Integer) As Double
        Dim freq = termFreq(term)(doc)
        Return System.Math.Sqrt(freq)
    End Function

    Private Function getIDFMeasure(term As Integer) As Double
        Dim df = docFreq(term)
        Return 1.0R + System.Math.Log(numDocs / (1.0R + df))
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
        Dim v = New Double(numTerms - 1) {}
        For i = 0 To numTerms - 1
            v(i) = termWeight(i)(docIndex)
        Next
        Return v
    End Function
End Class
