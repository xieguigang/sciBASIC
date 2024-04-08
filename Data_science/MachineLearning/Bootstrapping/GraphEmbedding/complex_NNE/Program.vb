Imports System
Imports ClassLibrary1.util

Namespace complex_NNE

    Public Class Program

        Public Shared Sub Main(args As String())
            Dim cmmdArg As Arguments = New Arguments(args)
            Dim model As ComplEx = New ComplEx()
            Dim fnTrainTriples = ""
            Dim fnValidTriples = ""
            Dim fnTestTriples = ""
            Dim fnAllTriples = ""
            Dim strNumRelation = ""
            Dim strNumEntity = ""

            Try
                fnTrainTriples = cmmdArg.getValue("train")
                If ReferenceEquals(fnTrainTriples, Nothing) OrElse fnTrainTriples.Equals("") Then
                    Call Usage()
                    Return
                End If
                fnValidTriples = cmmdArg.getValue("valid")
                If ReferenceEquals(fnValidTriples, Nothing) OrElse fnValidTriples.Equals("") Then
                    Call Usage()
                    Return
                End If
                fnTestTriples = cmmdArg.getValue("test")
                If ReferenceEquals(fnTestTriples, Nothing) OrElse fnTestTriples.Equals("") Then
                    Call Usage()
                    Return
                End If
                fnAllTriples = cmmdArg.getValue("all")
                If ReferenceEquals(fnAllTriples, Nothing) OrElse fnAllTriples.Equals("") Then
                    Call Usage()
                    Return
                End If
                strNumRelation = cmmdArg.getValue("m")
                If ReferenceEquals(strNumRelation, Nothing) OrElse strNumRelation.Equals("") Then
                    Call Usage()
                    Return
                End If
                strNumEntity = cmmdArg.getValue("n")
                If ReferenceEquals(strNumEntity, Nothing) OrElse strNumEntity.Equals("") Then
                    Call Usage()
                    Return
                End If
                If Not ReferenceEquals(cmmdArg.getValue("k"), Nothing) AndAlso Not cmmdArg.getValue("k").Equals("") Then
                    model.m_NumFactor = Integer.Parse(cmmdArg.getValue("k"))
                End If
                If Not ReferenceEquals(cmmdArg.getValue("lmbda"), Nothing) AndAlso Not cmmdArg.getValue("lmbda").Equals("") Then
                    model.m_Lambda = Double.Parse(cmmdArg.getValue("lmbda"))
                End If
                If Not ReferenceEquals(cmmdArg.getValue("gamma"), Nothing) AndAlso Not cmmdArg.getValue("gamma").Equals("") Then
                    model.m_Gamma = Double.Parse(cmmdArg.getValue("gamma"))
                End If
                If Not ReferenceEquals(cmmdArg.getValue("neg"), Nothing) AndAlso Not cmmdArg.getValue("neg").Equals("") Then
                    model.m_NumNegative = Integer.Parse(cmmdArg.getValue("neg"))
                End If
                If Not ReferenceEquals(cmmdArg.getValue("#"), Nothing) AndAlso Not cmmdArg.getValue("#").Equals("") Then
                    model.m_NumIteration = Integer.Parse(cmmdArg.getValue("#"))
                End If

                model.initialization(strNumRelation, strNumEntity, fnTrainTriples, fnValidTriples, fnTestTriples, fnAllTriples)
                Console.WriteLine(vbLf & "Start learning ComplEx (nonnegative E) model")
                model.learn()
                Console.WriteLine("Success.")
            Catch e As Exception
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
                Call Usage()
                Return
            End Try
        End Sub

        Friend Shared Sub Usage()
            Console.WriteLine("Usagelala: java ComplEx -train train_triples -valid valid_triples -test test_triples -all all_triples" & "-m number_of_relations -n number_of_entities [options]" & vbLf & vbLf & "Options: " & vbLf & "   -k        -> number of latent factors (default 50)" & vbLf & "   -lmbda    -> regularization parameter (default 0.001)" & vbLf & "   -gamma    -> initial learning rate (default 0.1)" & vbLf & "   -neg      -> number of negative instances (default 2)" & vbLf & "   -#        -> number of iterations (default 1000)" & vbLf & "   -skip     -> number of skipped iterations (default 50)" & vbLf & vbLf)
        End Sub
    End Class

End Namespace
