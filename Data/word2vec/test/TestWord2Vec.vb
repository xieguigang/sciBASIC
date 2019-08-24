Imports System
Imports System.Collections.Generic
Imports Tokenizer = org.nlp.util.Tokenizer
Imports VectorModel = org.nlp.vec.VectorModel
Imports Word2Vec = org.nlp.vec.Word2Vec

Namespace test


    ''' <summary>
    ''' @author siegfang
    ''' </summary>
    Public Class TestWord2Vec
        Public Shared Sub readByJava(ByVal textFilePath As String, ByVal modelFilePath As String)
            Dim wv As Word2Vec = (New Word2Vec.Factory()).setMethod(Word2Vec.Method.Skip_Gram).setNumOfThread(1).build()

            Try

                Using br As StreamReader = New StreamReader(textFilePath)
                    Dim lineCount = 0
                    Dim line As String = br.ReadLine()

                    While Not ReferenceEquals(line, Nothing)
                        wv.readTokens(New Tokenizer(line, " "))
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
            wv.saveModel(New File(modelFilePath))
        End Sub

        Public Shared Sub testVector(ByVal modelFilePath As String)
            Dim vm = VectorModel.loadFromFile(modelFilePath)
            Dim result1 As ISet(Of VectorModel.WordScore) = Collections.emptySet()
            result1 = vm.similar("亲")

            For Each we In result1
                Console.WriteLine(we.name & " :" & vbTab & we.score)
            Next
        End Sub

        Public Shared Sub Main(ByVal args As String())
            Dim textFilePath = "D:/data/corpus.dat"
            Dim modelFilePath = "D:/data/corpus.nn"
            readByJava(textFilePath, modelFilePath)
            testVector(modelFilePath)
        End Sub
    End Class
End Namespace
