'parse the csv file, get features and label. the format: feature1,feature2,...,label
'first scan, get the feature dimension, dataset size, count of missing value for each feature
'second scan, get each feature's (value,index) and missing value indexes
'if we use ArrayList,only one scanning is needed, but it is memory consumption

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

<Assembly: InternalsVisibleTo("Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet")>
<Assembly: InternalsVisibleTo("MLkit")>

Namespace train

    Public MustInherit Class Data

        'we use -Double.MAX_VALUE to represent missing value
        Public Const NA As Single = -Single.MaxValue

        ''' <summary>
        ''' <see cref="GBM.predict(Single()())"/>
        ''' </summary>
        Friend origin_feature As Single()()
        Friend feature_dim As Integer
        Friend dataset_size As Integer

    End Class

    Public Class TrainData : Inherits ValidationData

        Friend feature_value_index As Single()()()
        Friend missing_index As Integer()()
        Friend missing_count As New List(Of Integer)()
        Friend cat_features_names As List(Of String)
        Friend cat_features_cols As New List(Of Integer)()

        Public Sub New(categorical_features As IEnumerable(Of String))
            cat_features_names = categorical_features.AsList
        End Sub

        Public Overrides Function ToString() As String
            Return cat_features_names.ToArray.GetJson
        End Function
    End Class

    Public Class ValidationData : Inherits Data

        Friend label As Double()

    End Class

    Public Class TestData : Inherits Data

    End Class
End Namespace
