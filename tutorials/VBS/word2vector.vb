#include "Microsoft.VisualBasic.Data.NLP.Word2Vec.dll"
#include "Microsoft.VisualBasic.Data.NLP.dll"

Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Data.NLP.Model

dim textfile = "G:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Rapunzel.txt"
dim wv As Word2Vec = (New Word2VecFactory()).setVectorSize(100).setMethod(TrainMethod.Skip_Gram).setNumOfThread(1).setFreqThresold(1).build()
Dim data As Paragraph() = Paragraph.Segmentation(textFile.ReadAllText).ToArray

For Each p As Paragraph In data
    For Each line In p.sentences
        Call wv.readTokens(line)
    Next
Next

call wv.training()

dim result = wv.outputVector