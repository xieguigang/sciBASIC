Namespace GraphEmbedding

    ''' <summary>
    ''' "Improving Knowledge Graph Embedding Using Simple Constraints" (ACL-2018)
    ''' 
    ''' ### Parameters
    ''' 
    ''' You can changes parameter when training the model
    ''' 
    ''' ```
    ''' k = number of dimensions
    ''' lmbda = L2 regularization coffecient
    ''' neg = number of negative samples
    ''' mu = AER regularization coffecient
    ''' ```
    ''' 
    ''' ### Citation
    ''' 
    ''' ```
    ''' @inproceedings{boyang2018:aer,
    ''' 	author = {Ding, Boyang and Wang, Quan and Wang, Bin and Guo, Li},
    ''' 	booktitle = {56th Annual Meeting of the Association for Computational Linguistics},
    ''' 	title = {Improving Knowledge Graph Embedding Using Simple Constraints},
    ''' 	year = {2018}
    ''' }
    ''' ```
    ''' 
    ''' ### Contact
    ''' 
    ''' For all remarks or questions please contact Quan Wang: wangquan (at) iie (dot) ac (dot) cn .
    ''' </summary>
    ''' <remarks>
    ''' * [Improving Knowledge Graph Embedding Using Simple Constraints](https://arxiv.org/abs/1805.02408). Boyang Ding, Quan Wang, Bin Wang and Li Guo. ACL 2018.
    ''' * [Complex Embeddings for Simple Link Prediction](http://proceedings.mlr.press/v48/trouillon16.pdf). Théo Trouillon, Johannes Welbl, Sebastian Riedel, Éric Gaussier and Guillaume Bouchard. ICML 2016.
    ''' * [Regularizing Knowledge Graph Embeddings via Equivalence And Inversion Axioms](https://luca.costabello.info/docs/ECML_PKDD_2017_embeddings.pdf). Pasquale Minervini, Luca Costabello, Emir Muñoz, Vít Nováček And Pierre-Yves Vandenbussche. ECML 2017. 
    ''' </remarks>
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

        Public Shared Function learn(Of alg As {New, Algorithm})(args As Arguments)
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