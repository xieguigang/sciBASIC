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
