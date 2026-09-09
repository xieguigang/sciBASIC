#include "Microsoft.VisualBasic.Data.NLP.Word2Vec.dll"
#include "Microsoft.VisualBasic.Data.NLP.dll"
#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Math.Statistics.ANOVA.dll"

Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.Kmeans
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA

dim textfile = "G:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Rapunzel.txt"
dim wv As Word2Vec = Word2VecFactory().setVectorSize(100).setMethod(TrainMethod.Skip_Gram).setNumOfThread(1).setFreqThresold(1).build()
Dim data As Paragraph() = Paragraph.Segmentation(textFile.ReadAllText).ToArray

For Each p As Paragraph In data
    For Each line In p.sentences
        Call wv.readTokens(line)
    Next
Next

call wv.training()

dim result = wv.outputVector.AsEnumerable().AsDataSet().CommonDataSet().PrincipalComponentAnalysis(maxPC:= 20).GetPCAScore()
dim clusters = result.Kmeans(k:=6).toarray()