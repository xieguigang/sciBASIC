Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports Xgb = Microsoft.VisualBasic.MachineLearning.XGBoost.train.GBM

Namespace Ensemble.Persistence

    ''' <summary>
    ''' XGBoost（TGBoost 后端）的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 树集合本身就是一棵棵二进制友好的浮点结构，重写成「一行一个节点」的长表
    ''' 既冗长又容易在浮点精度、缺失值方向等细节上出错。TGBoost 已经提供了
    ''' 经过测试的文本序列化器 <c>ModelSerializer.save_model/load_model</c>
    ''' （浮点使用 <c>"R"</c> + invariant culture），因此这里直接复用它，
    ''' 只是把输出**落到 CSV 的单列**里：
    ''' </para>
    ''' <code>
    ''' index,line
    ''' 0,"0 0.5 3 ..." 
    ''' 1,"1 leaf ..."
    ''' </code>
    ''' <para>
    ''' 额外加一列 <c>index</c> 而不是只写一列 <c>line</c>，是为了避免
    ''' 「空行被 CSV 读取器当作无效行跳过」导致模型缺行；行号同时也能在
    ''' 人工排查时直接定位。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class XGBoostCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "xgboost"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is XGBoostClassifier OrElse TypeOf model Is XGBoostRegressor
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim fitted As XGBoostModel
            Dim manifest As ModelManifest
            Dim classes As Double() = Nothing

            Dim classifier As XGBoostClassifier = TryCast(model, XGBoostClassifier)

            If classifier IsNot Nothing Then
                fitted = classifier.FittedModel
                classes = classifier.Classes
                manifest = ModelManifest.Create("classifier", "base", AlgorithmName)
            Else
                fitted = DirectCast(model, XGBoostRegressor).FittedModel
                manifest = ModelManifest.Create("regressor", "base", AlgorithmName)
            End If

            If fitted Is Nothing OrElse fitted.FittedModel Is Nothing Then
                Throw New InvalidOperationException("the xgboost adapter holds no fitted model!")
            End If

            manifest.featureWidth = fitted.FeatureWidth
            manifest.classes = classes

            Call bundle.WriteCsv(manifest, "booster", folder,
                                 New String() {"index", "line"},
                                 EnumerateBoosterLines(fitted.FittedModel))

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            Dim table As CsvTable = bundle.ReadCsv(manifest, "booster")
            Dim lines As String() = table.Texts("line")

            If lines.Length = 0 Then
                Throw New System.IO.InvalidDataException(
                    "the xgboost model bundle contains an empty booster table!")
            End If

            Dim booster As Xgb = ModelSerializer.load_model(lines)

            If booster Is Nothing Then
                Throw New System.IO.InvalidDataException("the xgboost booster can not be deserialized!")
            End If

            Dim restored As New XGBoostModel(booster, manifest.featureWidth)

            If String.Equals(manifest.kind, "classifier", StringComparison.OrdinalIgnoreCase) Then
                Return New XGBoostClassifier(restored, manifest.classes)
            End If

            Return New XGBoostRegressor(restored)
        End Function

        Private Shared Iterator Function EnumerateBoosterLines(booster As Xgb) As IEnumerable(Of String())
            Dim index As Integer = 0

            For Each line As String In ModelSerializer.save_model(booster)
                Yield New String() {CStr(index), If(line, "")}
                index += 1
            Next
        End Function

    End Class

End Namespace
