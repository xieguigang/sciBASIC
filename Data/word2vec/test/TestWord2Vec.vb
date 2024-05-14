#Region "Microsoft.VisualBasic::cdded4fc2e4567a7432a554eea2f35d3, Data\word2vec\test\TestWord2Vec.vb"

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

    '   Total Lines: 41
    '    Code Lines: 29
    ' Comment Lines: 3
    '   Blank Lines: 9
    '     File Size: 1.46 KB


    '     Class TestWord2Vec
    ' 
    '         Function: readByJava
    ' 
    '         Sub: Main, testVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec

Namespace test


    ''' <summary>
    ''' @author siegfang
    ''' </summary>
    Public Class TestWord2Vec
        Public Shared Function readByJava(ByVal textFilePath As String, ByVal modelFilePath As String) As VectorModel
            Dim wv As Word2Vec = (New Word2VecFactory()).setMethod(TrainMethod.Skip_Gram).setNumOfThread(1).setFreqThresold(1).build()
            Dim data As Paragraph() = Paragraph.Segmentation(textFilePath.ReadAllText).ToArray

            For Each p As Paragraph In data
                For Each line In p.sentences
                    Call wv.readTokens(line)
                Next
            Next

            wv.training()
            Return wv.outputVector
        End Function

        Public Shared Sub testVector(vm As VectorModel)
            Dim result1 As New SortedSet(Of WordScore)(vm.similar("house"))

            For Each we In result1
                Console.WriteLine(we.name & " :" & vbTab & we.score)
            Next
        End Sub

        Public Shared Sub Main(ByVal args As String())
            Dim textFilePath = "E:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Rapunzel.txt"
            Dim modelFilePath = "./swresult_withoutnature.vec"

            testVector(readByJava(textFilePath, modelFilePath))
            Pause()
        End Sub
    End Class
End Namespace
