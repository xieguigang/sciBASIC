Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train

Namespace train
    Public Class Training
        Public Shared Sub training(ByVal args As String())
            Dim file_training = args(1)
            Dim file_validation = args(2)
            Dim file_model = args(3)
            Dim early_stopping_round = Integer.Parse(args(4))
            Dim maximize = args(5).Equals("true")
            Dim eval_metric = args(6)
            Dim loss = args(7)
            Dim eta = Double.Parse(args(8))
            Dim num_boost_round = Integer.Parse(args(9))
            Dim max_depth = Integer.Parse(args(10))
            Dim scale_pos_weight = Double.Parse(args(11))
            Dim rowsample = Double.Parse(args(12))
            Dim colample = Double.Parse(args(13))
            Dim min_child_weight = Double.Parse(args(14))
            Dim min_sample_split = Integer.Parse(args(15))
            Dim lambda = Double.Parse(args(16))
            Dim gamma = Double.Parse(args(17))
            Dim num_thread = Integer.Parse(args(18))
            Dim cat_features = args(19).Split(","c)
            Dim categorical_features As List(Of String) = New List(Of String)()

            For Each cat_feature In cat_features
                categorical_features.Add(cat_feature)
            Next

            Dim tgb As GBM = New GBM()
            tgb.fit(file_training,
                    file_validation,
                    categorical_features,
                    early_stopping_round,
                    maximize,
                    eval_metric,
                    loss,
                    eta,
                    num_boost_round,
                    max_depth,
                    scale_pos_weight,
                    rowsample,
                    colample,
                    min_child_weight,
                    min_sample_split,
                    lambda,
                    gamma,
                    num_thread
                    )
            ModelSerializer.save_model(tgb, file_model)
        End Sub

        Public Shared Sub testing(ByVal args As String())
            Dim file_model = args(1)
            Dim file_testing = args(2)
            Dim file_output = args(3)
            Dim tgb As GBM = ModelSerializer.load_model(file_model)
            tgb.predict(file_testing, file_output)
        End Sub

        Public Shared Sub Main(ByVal args As String())
            If args(0).Equals("training") Then
                training(args)
            ElseIf args(0).Equals("testing") Then
                testing(args)
            End If
        End Sub
    End Class
End Namespace
