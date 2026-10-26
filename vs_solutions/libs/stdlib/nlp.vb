Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Math.Correlations
' both NLP namespaces declare a module named 'Extensions', so the word2vec factory entry
' is aliased here
Imports Word2VecExtensions = Microsoft.VisualBasic.Data.NLP.Word2Vec.Extensions

''' <summary>
''' 文本分析模块：分词、TF-IDF 文档向量与文档距离矩阵、word2vec 词向量训练。
''' </summary>
Public Module nlp

    ' ======================================================== 分词

    ''' <summary>按正则表达式切分文本（默认按空白符切分）。</summary>
    Public Function tokenize(text As String, Optional pattern As String = "\s+") As String()
        Return If(text, "").StringSplit(pattern)
    End Function

    ' ======================================================== TF-IDF

    ''' <summary>
    ''' 计算文档集合两两之间的 TF-IDF 平方欧氏距离矩阵（n×n，对称，对角为 0）。
    ''' </summary>
    Public Function tfidfMatrix(docs As String(), Optional pattern As String = "\s+") As Double()()
        Dim n As Integer = docs.Length

        Dim index As TFIDF = BuildTfidf(docs, pattern)
        Dim vectors(n - 1)() As Double

        For i As Integer = 0 To n - 1
            vectors(i) = index.TfidfVectorizer((i + 1).ToString)
        Next

        Dim out(n - 1)() As Double

        For i As Integer = 0 To n - 1
            Dim row(n - 1) As Double

            For j As Integer = 0 To n - 1
                row(j) = DistanceMethods.SquareDistance(vectors(i), vectors(j))
            Next

            out(i) = row
        Next

        Return out
    End Function

    ''' <summary>
    ''' 计算文档集合的 TF-IDF 距离矩阵并包装成表（行/列名为 ``doc_i``）。
    ''' </summary>
    Public Function tfidfTable(docs As String(), Optional pattern As String = "\s+") As NumericTable
        Dim matrix As Double()() = tfidfMatrix(docs, pattern)
        Dim names As String() = New String(matrix.Length - 1) {}

        For i As Integer = 0 To names.Length - 1
            names(i) = "doc_" & (i + 1)
        Next

        Return NumericTable.FromRows(names, matrix, names)
    End Function

    ''' <summary>文档集合的词表（按首次出现顺序）。</summary>
    Public Function vocabulary(docs As String(), Optional pattern As String = "\s+") As String()
        Return BuildTfidf(docs, pattern).Words
    End Function

    ' ======================================================== word2vec

    ''' <summary>
    ''' 用给定语料训练 word2vec 词向量模型，返回行名为词、特征列为 ``v_1 .. v_n`` 的表格。
    ''' </summary>
    ''' <param name="method">``cbow``（默认）或 ``skipgram``</param>
    Public Function word2vec(text As String,
                             Optional vectorSize As Integer = 30,
                             Optional method As String = "cbow",
                             Optional freqThreshold As Integer = 1,
                             Optional window As Integer = 5,
                             Optional threads As Integer = 1) As NumericTable

        Dim factory As Word2VecFactory = Word2VecExtensions.BuildWord2VecFactory() _
            .setVectorSize(vectorSize) _
            .setWindow(window) _
            .setMethod(If("skipgram".Equals(If(method, "").Trim, StringComparison.OrdinalIgnoreCase), TrainMethod.Skip_Gram, TrainMethod.CBow)) _
            .setNumOfThread(If(threads <= 0, 1, threads)) _
            .setFreqThresold(freqThreshold)

        Dim model As Word2Vec = factory.build()

        For Each paragraph As Paragraph In Paragraph.Segmentation(text).ToArray
            For Each sentence As Sentence In paragraph.sentences
                Call model.readTokens(sentence)
            Next
        Next

        Call model.training()

        Dim vector As VectorModel = model.outputVector()
        Dim tokens As String() = vector.tokens
        Dim rows(tokens.Length - 1)() As Double

        For i As Integer = 0 To tokens.Length - 1
            Dim raw As Single() = vector.wordMap(tokens(i))
            Dim values(raw.Length - 1) As Double

            For j As Integer = 0 To raw.Length - 1
                values(j) = CDbl(raw(j))
            Next

            rows(i) = values
        Next

        Dim features As String() = New String(vector.vectorSize - 1) {}

        For i As Integer = 0 To features.Length - 1
            features(i) = "v_" & (i + 1)
        Next

        Return NumericTable.FromRows(tokens, rows, features)
    End Function

    ''' <summary>
    ''' 词向量表中与给定词最相似的若干个词（返回 "word: score" 形式的文本行）。
    ''' </summary>
    Public Function similarWords(text As String, query As String,
                                 Optional vectorSize As Integer = 30,
                                 Optional method As String = "cbow",
                                 Optional topN As Integer = 10) As String()

        Dim factory As Word2VecFactory = Word2VecExtensions.BuildWord2VecFactory() _
            .setVectorSize(vectorSize) _
            .setMethod(If("skipgram".Equals(If(method, "").Trim, StringComparison.OrdinalIgnoreCase), TrainMethod.Skip_Gram, TrainMethod.CBow)) _
            .setNumOfThread(1) _
            .setFreqThresold(1)

        Dim model As Word2Vec = factory.build()

        For Each paragraph As Paragraph In Paragraph.Segmentation(text).ToArray
            For Each sentence As Sentence In paragraph.sentences
                Call model.readTokens(sentence)
            Next
        Next

        Call model.training()

        Dim vector As VectorModel = model.outputVector()

        If vector.wordMap Is Nothing OrElse Not vector.wordMap.ContainsKey(query) Then
            Return New String() {}
        End If

        Return vector.similar(query, topN) _
            .Select(Function(w) $"{w.name}: {w.score.ToString("F4")}") _
            .ToArray
    End Function

    Private Function BuildTfidf(docs As String(), pattern As String) As TFIDF
        Dim index As New TFIDF

        For i As Integer = 0 To docs.Length - 1
            Call index.Add((i + 1).ToString, docs(i).StringSplit(pattern))
        Next

        Return index
    End Function
End Module
