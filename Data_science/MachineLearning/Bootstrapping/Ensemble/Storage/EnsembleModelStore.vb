Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 模型持久化的统一入口：编解码器注册表 + 保存/加载分派。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 分派规则：
    ''' <list type="number">
    ''' <item>保存时，若对象实现了 <see cref="IPersistableModel"/>（三类集成器），
    '''       走其自身的结构感知保存逻辑；否则按算法标识匹配 <see cref="IModelCodec"/>。</item>
    ''' <item>加载时按清单的 <c>modelType</c> 分派：<c>base</c> 走算法编解码器，
    '''       <c>voting</c> / <c>bagging</c> / <c>stacking</c> 走集成模型重建。</item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' 未知算法一律抛出列有全部已注册标识的异常，不做静默降级——
    ''' 「加载成另一个能跑但语义不同的模型」比直接失败危险得多。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class EnsembleModelStore

        Private Shared ReadOnly _codecs As New Dictionary(Of String, IModelCodec)(StringComparer.OrdinalIgnoreCase)
        Private Shared ReadOnly _sync As New Object()

        Shared Sub New()
            ' 显式注册内置编解码器：顺序无关，算法标识必须唯一
            ' 朴素贝叶斯族
            Call Register(New NaiveBayesCodec())
            Call Register(New BayesianRegressionCodec())
            ' 广义线性 / 正则化回归族
            Call Register(New LogisticCodec())
            Call Register(New RidgeCodec())
            Call Register(New MlrCodec())
            Call Register(New LassoCodec())
            ' 核方法与树模型
            Call Register(New SvmCodec())
            Call Register(New RandomForestCodec())
            Call Register(New XGBoostCodec())
        End Sub

        ''' <summary>注册一个编解码器；同标识重复注册会覆盖旧实现</summary>
        ''' <param name="codec">编解码器</param>
        Public Shared Sub Register(codec As IModelCodec)
            If codec Is Nothing Then
                Throw New ArgumentNullException(NameOf(codec))
            End If
            If String.IsNullOrEmpty(codec.Algorithm) Then
                Throw New ArgumentException("the codec algorithm identifier can not be empty!", NameOf(codec))
            End If

            SyncLock _sync
                _codecs(codec.Algorithm) = codec
            End SyncLock
        End Sub

        ''' <summary>已注册的全部算法标识（升序）</summary>
        Public Shared Function Algorithms() As String()
            SyncLock _sync
                Return _codecs.Keys.OrderBy(Function(k) k, StringComparer.OrdinalIgnoreCase).ToArray()
            End SyncLock
        End Function

        ''' <summary>按算法标识查找编解码器</summary>
        ''' <param name="algorithm">算法标识</param>
        Public Shared Function GetCodec(algorithm As String) As IModelCodec
            Dim codec As IModelCodec = Nothing

            If Not String.IsNullOrEmpty(algorithm) Then
                SyncLock _sync
                    Call _codecs.TryGetValue(algorithm, codec)
                End SyncLock
            End If

            If codec Is Nothing Then
                Throw New NotSupportedException(
                    $"the model algorithm '{If(algorithm, "")}' is not supported; " &
                    $"registered algorithms: {String.Join(", ", Algorithms())}")
            End If

            Return codec
        End Function

        ''' <summary>按对象类型查找编解码器</summary>
        ''' <param name="model">模型对象</param>
        Public Shared Function FindCodec(model As Object) As IModelCodec
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If

            Dim found As IModelCodec() = Nothing

            SyncLock _sync
                found = _codecs.Values.Where(Function(c) c.CanHandle(model)).ToArray()
            End SyncLock

            If found.Length = 0 Then
                Throw New NotSupportedException(
                    $"the model type '{model.GetType().FullName}' can not be persisted; " &
                    $"registered algorithms: {String.Join(", ", Algorithms())}")
            End If
            If found.Length > 1 Then
                Throw New InvalidOperationException(
                    $"the model type '{model.GetType().FullName}' is claimed by multiple codecs: " &
                    $"{String.Join(", ", found.Select(Function(c) c.Algorithm))}!")
            End If

            Return found(0)
        End Function

        ''' <summary>
        ''' 把模型保存为模型包（目录）。
        ''' </summary>
        ''' <param name="model">集成器或任意受支持的基学习器</param>
        ''' <param name="directory">目标目录</param>
        ''' <returns>写出的清单</returns>
        Public Shared Function Save(model As Object, directory As String) As ModelManifest
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If

            Dim bundle As ModelBundle = ModelBundle.Create(directory)
            Dim manifest As ModelManifest

            If TypeOf model Is IPersistableModel Then
                manifest = DirectCast(model, IPersistableModel).SaveModel(bundle, "")
            Else
                manifest = SaveChild(model, bundle, "base")
            End If

            Call bundle.WriteManifest(manifest)

            Return manifest
        End Function

        ''' <summary>读取模型包的清单（不做模型重建）</summary>
        ''' <param name="directory">模型包目录</param>
        Public Shared Function LoadManifest(directory As String) As ModelManifest
            Return ModelBundle.Open(directory).ReadManifest()
        End Function

        ''' <summary>从模型包恢复模型对象（零重训）</summary>
        ''' <param name="directory">模型包目录</param>
        Public Shared Function LoadModel(directory As String) As Object
            Dim bundle As ModelBundle = ModelBundle.Open(directory)

            Return LoadChild(bundle.ReadManifest(), bundle)
        End Function

        ''' <summary>从模型包恢复分类器</summary>
        ''' <param name="directory">模型包目录</param>
        Public Shared Function LoadClassifier(directory As String) As IClassifier
            Dim model As Object = LoadModel(directory)

            If Not (TypeOf model Is IClassifier) Then
                Throw New InvalidDataException(
                    $"the model bundle '{directory}' holds a '{If(model Is Nothing, "null", model.GetType().FullName)}', " &
                    "which is not an IClassifier!")
            End If

            Return DirectCast(model, IClassifier)
        End Function

        ''' <summary>从模型包恢复回归器</summary>
        ''' <param name="directory">模型包目录</param>
        Public Shared Function LoadRegressor(directory As String) As IRegressor
            Dim model As Object = LoadModel(directory)

            If Not (TypeOf model Is IRegressor) Then
                Throw New InvalidDataException(
                    $"the model bundle '{directory}' holds a '{If(model Is Nothing, "null", model.GetType().FullName)}', " &
                    "which is not an IRegressor!")
            End If

            Return DirectCast(model, IRegressor)
        End Function

        ''' <summary>
        ''' 保存子模型（集成器内部使用）：既支持集成器，也支持任意基学习器。
        ''' </summary>
        ''' <param name="model">模型对象</param>
        ''' <param name="bundle">目标模型包</param>
        ''' <param name="folder">包内的相对目录</param>
        Friend Shared Function SaveChild(model As Object, bundle As ModelBundle, folder As String) As ModelManifest
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If
            If bundle Is Nothing Then
                Throw New ArgumentNullException(NameOf(bundle))
            End If

            If TypeOf model Is IPersistableModel Then
                Return DirectCast(model, IPersistableModel).SaveModel(bundle, folder)
            End If

            Return FindCodec(model).SaveModel(model, bundle, folder)
        End Function

        ''' <summary>按清单条目恢复模型对象（集成器内部使用）</summary>
        ''' <param name="manifest">模型清单条目</param>
        ''' <param name="bundle">模型包</param>
        Friend Shared Function LoadChild(manifest As ModelManifest, bundle As ModelBundle) As Object
            If manifest Is Nothing Then
                Throw New ArgumentNullException(NameOf(manifest))
            End If
            If bundle Is Nothing Then
                Throw New ArgumentNullException(NameOf(bundle))
            End If

            Select Case If(manifest.modelType, "")
                Case "base"
                    Return GetCodec(manifest.algorithm).LoadModel(manifest, bundle)
                Case "voting", "bagging", "stacking"
                    Return EnsembleModelCodec.LoadModel(manifest, bundle)
                Case Else
                    Throw New InvalidDataException(
                        $"the model manifest has an unsupported 'modelType' ('{manifest.modelType}')!")
            End Select
        End Function

    End Class

End Namespace
