
Imports Microsoft.VisualBasic.Data.NLP.LDA

Module Program
    Sub Main(args As String())
        ' 1. Load corpus from disk
        Dim corpus As Corpus = Corpus.load("D:\GCModeller\src\R-sharp\Library\demo\machineLearning\NLP\data\mini")
        ' 2. Create a LDA sampler
        Dim ldaGibbsSampler As New LdaGibbsSampler(corpus.Document(), corpus.VocabularySize())
        ' 3. Train it
        ldaGibbsSampler.gibbs(10)
        ' 4. The phi matrix Is a LDA model, you can use LdaUtil to explain it.
        Dim phi = ldaGibbsSampler.Phi()
        Dim topicMap = LdaUtil.translate(phi, corpus.Vocabulary(), 10)
        LdaUtil.explain(topicMap)

        Pause()
    End Sub
End Module
