Namespace GraphEmbedding

    Public MustInherit Class Algorithm

        Public m_NumFactor As Integer = 50
        Public m_Lambda As Double = 0.001
        Public m_Gamma As Double = 0.1
        Public m_NumNegative As Integer = 10
        Public m_NumBatch As Integer = 100
        Public m_NumIteration As Integer = 1000
        Public m_OutputIterSkip As Integer = 50

        Public MustOverride Sub initialization(strNumRelation As String, strNumEntity As String, fnTrainTriples As String, fnValidTriples As String, fnTestTriples As String, fnAllTriples As String, other As Dictionary(Of String, String))
        Public MustOverride Sub learn()

        Public Shared Function ComplEx(args As Arguments)
            Return learn(Of complex.ComplEx)(args)
        End Function

        Private Shared Function learn(Of alg As {New, Algorithm})(args As Arguments)
            Dim model As New alg With {
                .m_Gamma = args.gamma,
                .m_Lambda = args.lmbda,
                .m_NumFactor = args.k,
                .m_NumNegative = args.neg,
                .m_NumIteration = args.iterations,
                .m_OutputIterSkip = args.skip
            }
            model.initialization(args.strNumRelation, args.strNumEntity, args.fnTrainTriples, args.fnValidTriples, args.fnTestTriples, args.fnAllTriples, args.other)
            Console.WriteLine($"Start learning {model.Description} (triple only) model")
            model.learn()
        End Function

        Public Shared Function complex_NNE(args As Arguments)
            Return learn(Of complex_NNE.ComplEx)(args)
        End Function

        Public Shared Function complex_NNE_AER(args As Arguments, rules As String)
            args.other = New Dictionary(Of String, String) From {
                {"rules", rules}
            }
            Return learn(Of complex_NNE_AER.ComplEx)(args)
        End Function

        Public Shared Function complex_R(args As Arguments, rules As String)
            args.other = New Dictionary(Of String, String) From {
               {"rules", rules}
           }
            Return learn(Of complex_R.ComplEx)(args)
        End Function
    End Class
End Namespace