#Region "Microsoft.VisualBasic::bdae276f17a3554ff5f123a9d226d8dd, Data_science\NLP\test\Program.vb"

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

    '   Total Lines: 47
    '    Code Lines: 35 (74.47%)
    ' Comment Lines: 4 (8.51%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (17.02%)
    '     File Size: 1.75 KB


    ' Module Program
    ' 
    '     Sub: Main, test1, test2
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.NLP.LDA
Imports Microsoft.VisualBasic.Parallel

Module Program
    Sub Main(args As String())
        ' 1. Load corpus from disk
        Dim corpus As Corpus = Corpus.load("\GCModeller\src\R-sharp\test\demo\machineLearning\NLP\data\mini")

        Call test2(corpus)
        Call test1(corpus)

        Pause()
    End Sub

    Sub test2(corpus As Corpus)
        VectorTask.n_threads = 12
        Dim ldaGibbsSampler As New LdaGibbsSampler(corpus.Document, corpus.VocabularySize)
        ldaGibbsSampler.gibbs(5)
        Dim phi = ldaGibbsSampler.phi
        Dim topicMap = LdaInterpreter.translate(phi, corpus.Vocabulary(), 10)
        Console.WriteLine("parallel result:")
        LdaInterpreter.explain(topicMap)

        Using s As Stream = "./gibbslda_parallel.txt".Open(FileMode.OpenOrCreate, doClear:=True)
            LdaInterpreter.explain(topicMap, New StreamWriter(s))
        End Using
    End Sub

    Sub test1(corpus As Corpus)
        VectorTask.n_threads = 1

        ' 2. Create a LDA sampler
        Dim ldaGibbsSampler As New LdaGibbsSampler(corpus.Document(), corpus.VocabularySize())
        ' 3. Train it
        ldaGibbsSampler.gibbs(5)
        ' 4. The phi matrix Is a LDA model, you can use LdaUtil to explain it.
        Dim phi = ldaGibbsSampler.Phi()
        Dim topicMap = LdaInterpreter.translate(phi, corpus.Vocabulary(), 10)
        Console.WriteLine("sequential result:")
        LdaInterpreter.explain(topicMap)

        Using s As Stream = "./gibbslda_sequential.txt".Open(FileMode.OpenOrCreate, doClear:=True)
            LdaInterpreter.explain(topicMap, New StreamWriter(s))
        End Using
    End Sub
End Module
