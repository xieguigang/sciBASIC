#Region "Microsoft.VisualBasic::b85e8e814df08e4dcc1633f211dab3aa, Data_science\MachineLearning\xgboost\test\Training.vb"

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

    '   Total Lines: 73
    '    Code Lines: 65 (89.04%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (10.96%)
    '     File Size: 3.02 KB


    '     Class Training
    ' 
    '         Sub: Main, testing, training
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train

Namespace train
    Public Class Training
        Public Shared Sub training()
            Dim file_training = "E:\GCModeller\src\R-sharp\Library\demo\machineLearning\XGBoost\train.csv"
            Dim file_validation = "E:\GCModeller\src\R-sharp\Library\demo\machineLearning\XGBoost\val.csv"
            Dim file_model = "E:\GCModeller\src\R-sharp\Library\demo\machineLearning\XGBoost\test.xgb"
            Dim early_stopping_round = 10
            Dim maximize = True
            Dim eval_metric As Metrics = Metrics.auc
            Dim loss = "logloss"
            Dim eta = 0.3
            Dim num_boost_round = 20
            Dim max_depth = 7
            Dim scale_pos_weight = 1.0
            Dim rowsample = 0.8
            Dim colample = 0.8
            Dim min_child_weight = 1
            Dim min_sample_split = 5
            Dim lambda = 1
            Dim gamma = 0
            Dim num_thread = 0
            Dim cat_features = {"PRI_jet_num"}
            Dim categorical_features As List(Of String) = New List(Of String)()

            For Each cat_feature In cat_features
                categorical_features.Add(cat_feature)
            Next

            Dim tgb As New GBM()
            Dim trainData As TrainData = Tabular.ReadTrainData(file_training, categorical_features)
            Dim valids As ValidationData = Tabular.ReadValidationData(file_validation)

            tgb.fit(trainData, valids,
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

        Public Shared Sub testing()
            Dim file_model = "E:\GCModeller\src\R-sharp\Library\demo\machineLearning\XGBoost\test.xgb"
            Dim file_testing = "E:\GCModeller\src\R-sharp\Library\demo\machineLearning\XGBoost\test.csv"
            Dim file_output = "E:\GCModeller\src\R-sharp\Library\demo\machineLearning\XGBoost\test_result.csv"
            Dim tgb As GBM = ModelSerializer.load_model(file_model)
            Dim test As TestData = Tabular.ReadTestData(file_testing)

            tgb.predict(test.origin_feature).Select(Function(s) s.ToString).SaveTo(file_output)
        End Sub

        Public Shared Sub Main(ByVal args As String())
            Call training()
            Call testing()
        End Sub
    End Class
End Namespace
