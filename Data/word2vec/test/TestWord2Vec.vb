#Region "Microsoft.VisualBasic::47091a61152c0fd0a678546a386fd839, sciBASIC#\Data\word2vec\test\TestWord2Vec.vb"

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

    '   Total Lines: 54
    '    Code Lines: 39
    ' Comment Lines: 4
    '   Blank Lines: 11
    '     File Size: 1.95 KB


    '     Class TestWord2Vec
    ' 
    '         Sub: Main, readByJava, testVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.NlpVec
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils

Namespace test


    ''' <summary>
    ''' @author siegfang
    ''' </summary>
    Public Class TestWord2Vec
        Public Shared Sub readByJava(ByVal textFilePath As String, ByVal modelFilePath As String)
            Dim wv As Word2Vec = (New Word2VecFactory()).setMethod(TrainMethod.Skip_Gram).setNumOfThread(1).build()

            Try

                Using br As StreamReader = New StreamReader(textFilePath)
                    Dim lineCount = 0
                    Dim line As String = br.ReadLine()

                    While Not line Is Nothing
                        wv.readTokens(New Tokenizer(line, vbTab))
                        '                System.out.println(line);
                        lineCount += 1
                        line = br.ReadLine()
                    End While
                End Using

            Catch ioe As IOException
                Console.WriteLine(ioe.ToString())
                Console.Write(ioe.StackTrace)
            End Try

            wv.training()
            wv.saveModel(New FileStream(modelFilePath, FileMode.OpenOrCreate))
        End Sub

        Public Shared Sub testVector(ByVal modelFilePath As String)
            Dim vm = VectorModel.loadFromFile(modelFilePath)
            Dim result1 As New SortedSet(Of WordScore)(vm.similar("亲"))

            For Each we In result1
                Console.WriteLine(we.name & " :" & vbTab & we.score)
            Next
        End Sub

        Public Shared Sub Main(ByVal args As String())
            Dim textFilePath = "C:\Users\Administrator\Downloads\swresult_withoutnature.txt"
            Dim modelFilePath = "C:\Users\Administrator\Downloads\swresult_withoutnature.vec"
            readByJava(textFilePath, modelFilePath)
            testVector(modelFilePath)
        End Sub
    End Class
End Namespace
