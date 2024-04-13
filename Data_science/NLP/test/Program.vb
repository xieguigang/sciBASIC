﻿#Region "Microsoft.VisualBasic::c4153c32fb43fdf439811abed6295fdd, sciBASIC#\Data_science\NLP\test\Program.vb"

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

'   Total Lines: 18
'    Code Lines: 12
' Comment Lines: 4
'   Blank Lines: 2
'     File Size: 719 B


' Module Program
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.NLP.LDA

Module Program
    Sub Main(args As String())
        ' 1. Load corpus from disk
        Dim corpus As Corpus = Corpus.load("\GCModeller\src\R-sharp\test\demo\machineLearning\NLP\data\mini")

        Call test1(corpus)
        Call test2(corpus)

        Pause()
    End Sub

    Sub test2(corpus As Corpus)
        Dim ldaGibbsSampler As New ParallelGibbsLda(corpus.Document, corpus.VocabularySize)
        ldaGibbsSampler.gibbsSampling(5, 2.0, 0.5, 1000, 8)
        Dim phi = ldaGibbsSampler.phi
        Dim topicMap = LdaInterpreter.translate(phi, corpus.Vocabulary(), 10)
        Console.WriteLine("parallel result:")
        LdaInterpreter.explain(topicMap)
    End Sub

    Sub test1(corpus As Corpus)
        ' 2. Create a LDA sampler
        Dim ldaGibbsSampler As New LdaGibbsSampler(corpus.Document(), corpus.VocabularySize())
        ' 3. Train it
        ldaGibbsSampler.gibbs(5)
        ' 4. The phi matrix Is a LDA model, you can use LdaUtil to explain it.
        Dim phi = ldaGibbsSampler.Phi()
        Dim topicMap = LdaInterpreter.translate(phi, corpus.Vocabulary(), 10)
        Console.WriteLine("sequential result:")
        LdaInterpreter.explain(topicMap)
    End Sub
End Module
