Namespace GraphEmbedding

    Public Class Arguments

        Const help =
            " Usagelala: java ComplEx -train train_triples -valid valid_triples -test test_triples -all all_triples -m number_of_relations -n number_of_entities [options]" & vbLf & vbLf &
            " Options: " & vbLf &
            "   -k        -> number of latent factors (default 50)" & vbLf &
            "   -lmbda    -> regularization parameter (default 0.001)" & vbLf &
            "   -gamma    -> initial learning rate (default 0.1)" & vbLf &
            "   -neg      -> number of negative instances (default 2)" & vbLf &
            "   -#        -> number of iterations (default 1000)" & vbLf &
            "   -skip     -> number of skipped iterations (default 50)"

        Public Property k As Integer = 50
        Public Property lmbda As Double = 0.001
        Public Property gamma As Double = 0.1
        Public Property neg As Integer = 2
        Public Property iterations As Integer = 1000
        Public Property skip As Integer = 50

        Public fnTrainTriples As String, fnValidTriples As String, fnTestTriples As String, fnAllTriples As String, strNumRelation As String, strNumEntity As String

        Public other As Dictionary(Of String, String)

    End Class
End Namespace