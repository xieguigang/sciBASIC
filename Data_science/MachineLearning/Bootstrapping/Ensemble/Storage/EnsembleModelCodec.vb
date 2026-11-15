Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 三类集成器（Voting / Bagging / Stacking）的模型重建。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 保存侧由集成器自身实现（<see cref="IPersistableModel.SaveModel"/>），因为只有它
    ''' 知道「基模型顺序 + 权重 + 元学习器」的结构；这里负责恢复侧：
    ''' 按 <c>modelType</c> 与 <c>kind</c> 分派，逐个把 <c>children</c> 还原成基模型，
    ''' 再交给集成器的恢复构造完成装配。
    ''' </para>
    ''' <para>
    ''' 子模型顺序是硬契约：
    ''' <list type="bullet">
    ''' <item>Voting / Bagging：<c>children</c> 顺序 == 基模型下标；</item>
    ''' <item>Stacking：前 <c>modelCount</c> 个为基模型，最后一个固定为元学习器。</item>
    ''' </list>
    ''' 数量或顺序不符时直接抛异常，避免恢复出「能跑但错位」的模型。
    ''' </para>
    ''' </remarks>
    Friend NotInheritable Class EnsembleModelCodec

        Private Sub New()
        End Sub

        ''' <summary>按清单重建集成器</summary>
        ''' <param name="manifest">集成模型的清单</param>
        ''' <param name="bundle">模型包</param>
        Public Shared Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object
            If manifest Is Nothing Then
                Throw New ArgumentNullException(NameOf(manifest))
            End If
            If bundle Is Nothing Then
                Throw New ArgumentNullException(NameOf(bundle))
            End If

            Select Case If(manifest.modelType, "")
                Case "voting"
                    Return LoadVoting(manifest, bundle)
                Case "bagging"
                    Return LoadBagging(manifest, bundle)
                Case "stacking"
                    Return LoadStacking(manifest, bundle)
                Case Else
                    Throw New InvalidDataException(
                        $"the ensemble model type '{manifest.modelType}' is not supported!")
            End Select
        End Function

        Private Shared Function LoadVoting(manifest As ModelManifest, bundle As ModelBundle) As Object
            Dim baseCount As Integer = RequireBaseCount(manifest)
            Dim weights As Double() = GetOptionalVector(manifest, "weights")
            Dim featureWidth As Integer = manifest.featureWidth

            If String.Equals(manifest.kind, "classifier", StringComparison.OrdinalIgnoreCase) Then
                Dim models As IClassifier() = LoadClassifiers(manifest, bundle, 0, baseCount)
                Dim mode As VotingMode

                If Not [Enum].TryParse(manifest.GetParameter("votingMode"), mode) Then
                    mode = VotingMode.Soft
                End If

                Return New VotingClassifier(models, manifest.classes, weights, mode, featureWidth)
            End If

            Dim regressors As IRegressor() = LoadRegressors(manifest, bundle, 0, baseCount)

            Return New VotingRegressor(regressors, weights, featureWidth)
        End Function

        Private Shared Function LoadBagging(manifest As ModelManifest, bundle As ModelBundle) As Object
            Dim baseCount As Integer = RequireBaseCount(manifest)
            Dim featureWidth As Integer = manifest.featureWidth
            Dim ratio As Double = GetOptionalScalar(manifest, "sampleRatio", 1.0R)
            Dim seed As Integer = GetOptionalInteger(manifest, "seed", 42)

            If String.Equals(manifest.kind, "classifier", StringComparison.OrdinalIgnoreCase) Then
                Dim models As IClassifier() = LoadClassifiers(manifest, bundle, 0, baseCount)

                Return New BaggingClassifier(models, manifest.classes, ratio, seed, featureWidth)
            End If

            Dim regressors As IRegressor() = LoadRegressors(manifest, bundle, 0, baseCount)
            Dim aggregation As BaggingAggregation

            If Not [Enum].TryParse(manifest.GetParameter("aggregation"), aggregation) Then
                aggregation = BaggingAggregation.Mean
            End If

            Return New BaggingRegressor(regressors, aggregation, ratio, seed, featureWidth)
        End Function

        Private Shared Function LoadStacking(manifest As ModelManifest, bundle As ModelBundle) As Object
            Dim baseCount As Integer = RequireBaseCount(manifest)
            Dim total As Integer = If(manifest.children Is Nothing, 0, manifest.children.Count)

            If total <> baseCount + 1 Then
                Throw New InvalidDataException(
                    $"the stacking model expects {baseCount + 1} child model(s) " &
                    $"(base + meta), but the manifest holds {total}!")
            End If

            Dim featureWidth As Integer = manifest.featureWidth
            Dim folds As Integer = GetOptionalInteger(manifest, "folds", 5)
            Dim seed As Integer = GetOptionalInteger(manifest, "seed", 42)

            If String.Equals(manifest.kind, "classifier", StringComparison.OrdinalIgnoreCase) Then
                Dim models As IClassifier() = LoadClassifiers(manifest, bundle, 0, baseCount)
                Dim meta As IClassifier = DirectCast(
                    EnsembleModelStore.LoadChild(manifest.children(baseCount), bundle), IClassifier)

                Return New StackingClassifier(models, meta, manifest.classes, folds, seed,
                                              GetOptionalScalar(manifest, "oofAccuracy", Double.NaN),
                                              featureWidth)
            End If

            Dim regressors As IRegressor() = LoadRegressors(manifest, bundle, 0, baseCount)
            Dim metaRegressor As IRegressor = DirectCast(
                EnsembleModelStore.LoadChild(manifest.children(baseCount), bundle), IRegressor)

            Return New StackingRegressor(regressors, metaRegressor, folds, seed,
                                         GetOptionalScalar(manifest, "oofRmse", Double.NaN),
                                         featureWidth)
        End Function

        Private Shared Function RequireBaseCount(manifest As ModelManifest) As Integer
            Dim baseCount As Integer = manifest.GetInteger("modelCount")
            Dim total As Integer = If(manifest.children Is Nothing, 0, manifest.children.Count)

            If baseCount < 1 Then
                Throw New InvalidDataException(
                    $"the ensemble manifest declares an invalid base model count ({baseCount})!")
            End If
            If String.Equals(manifest.modelType, "stacking", StringComparison.OrdinalIgnoreCase) Then
                ' stacking 的 children 额外包含一个元学习器
                If total <> baseCount + 1 Then
                    Throw New InvalidDataException(
                        $"the stacking manifest declares {baseCount} base model(s) + 1 meta model, " &
                        $"but holds {total} child manifest(s)!")
                End If
            ElseIf total <> baseCount Then
                Throw New InvalidDataException(
                    $"the {manifest.modelType} manifest declares {baseCount} base model(s), " &
                    $"but holds {total} child manifest(s)!")
            End If

            Return baseCount
        End Function

        Private Shared Function LoadClassifiers(manifest As ModelManifest, bundle As ModelBundle,
                                                offset As Integer, count As Integer) As IClassifier()
            Dim models As IClassifier() = New IClassifier(count - 1) {}

            For i As Integer = 0 To count - 1
                Dim loaded As Object = EnsembleModelStore.LoadChild(manifest.children(offset + i), bundle)

                If Not (TypeOf loaded Is IClassifier) Then
                    Throw New InvalidDataException(
                        $"the child model[{offset + i}] of '{manifest.modelType}' " &
                        $"is a '{If(loaded Is Nothing, "null", loaded.GetType().FullName)}', not an IClassifier!")
                End If

                models(i) = DirectCast(loaded, IClassifier)
            Next

            Return models
        End Function

        Private Shared Function LoadRegressors(manifest As ModelManifest, bundle As ModelBundle,
                                               offset As Integer, count As Integer) As IRegressor()
            Dim models As IRegressor() = New IRegressor(count - 1) {}

            For i As Integer = 0 To count - 1
                Dim loaded As Object = EnsembleModelStore.LoadChild(manifest.children(offset + i), bundle)

                If Not (TypeOf loaded Is IRegressor) Then
                    Throw New InvalidDataException(
                        $"the child model[{offset + i}] of '{manifest.modelType}' " &
                        $"is a '{If(loaded Is Nothing, "null", loaded.GetType().FullName)}', not an IRegressor!")
                End If

                models(i) = DirectCast(loaded, IRegressor)
            Next

            Return models
        End Function

        Private Shared Function GetOptionalScalar(manifest As ModelManifest, name As String, fallback As Double) As Double
            Dim value As Double

            If manifest.scalars IsNot Nothing AndAlso manifest.scalars.TryGetValue(name, value) Then
                Return value
            End If

            Return fallback
        End Function

        Private Shared Function GetOptionalInteger(manifest As ModelManifest, name As String, fallback As Integer) As Integer
            Dim value As Integer

            If manifest.integers IsNot Nothing AndAlso manifest.integers.TryGetValue(name, value) Then
                Return value
            End If

            Return fallback
        End Function

        ''' <summary>读取可选的浮点向量；缺失时返回 <c>Nothing</c>（与「等权」语义一致）</summary>
        Private Shared Function GetOptionalVector(manifest As ModelManifest, name As String) As Double()
            Dim value As Double() = Nothing

            If manifest.vectors IsNot Nothing AndAlso manifest.vectors.TryGetValue(name, value) Then
                Return value
            End If

            Return Nothing
        End Function

    End Class

End Namespace
