Namespace Ensemble.Persistence

    ''' <summary>
    ''' 单个算法族的模型编解码器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 编解码器把「具体适配器/算法模型」与「清单 + CSV」之间做双向映射：
    ''' <see cref="SaveModel"/> 负责把模型的全部预测状态摊平到清单与附表，
    ''' <see cref="LoadModel"/> 负责用底层库的恢复入口重建模型对象（零重训）。
    ''' </para>
    ''' <para>
    ''' 注册表以 <see cref="Algorithm"/> 为键，加载时按清单里的算法标识精确查表；
    ''' 因此清单中的算法名是稳定的外部契约，重命名会破坏旧模型包的兼容性。
    ''' </para>
    ''' </remarks>
    Public Interface IModelCodec

        ''' <summary>算法标识（写入清单，用于加载期分派）</summary>
        ReadOnly Property Algorithm As String

        ''' <summary>判断该编解码器是否负责给定的模型对象</summary>
        ''' <param name="model">待保存的模型对象</param>
        Function CanHandle(model As Object) As Boolean

        ''' <summary>
        ''' 把模型写入模型包。
        ''' </summary>
        ''' <param name="model">待保存的模型对象</param>
        ''' <param name="bundle">目标模型包</param>
        ''' <param name="folder">包内的相对目录（用于放置 CSV 附表），空值表示包根目录</param>
        ''' <returns>该模型的清单条目（由调用方决定是写为根清单还是挂到父清单的 children）</returns>
        Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest

        ''' <summary>按清单与附表重建模型对象</summary>
        ''' <param name="manifest">该模型的清单条目</param>
        ''' <param name="bundle">模型包（用于读取 CSV 附表）</param>
        Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object

    End Interface

    ''' <summary>
    ''' 可持久化的集成模型。
    ''' </summary>
    ''' <remarks>
    ''' 集成器的保存逻辑必须知道「基模型 + 权重 + 元学习器」的结构，
    ''' 因此由集成器自身实现 <see cref="SaveModel"/>；加载侧则集中在
    ''' <c>EnsembleModelCodec</c> 里按 <see cref="ModelKind"/> 统一分派。
    ''' </remarks>
    Public Interface IPersistableModel

        ''' <summary>模型类型标识：<c>voting</c> / <c>bagging</c> / <c>stacking</c></summary>
        ReadOnly Property ModelKind As String

        ''' <summary>模型类别：<c>classifier</c> 或 <c>regressor</c></summary>
        ReadOnly Property ModelCategory As String

        ''' <summary>把本模型（含全部基学习器）写入模型包</summary>
        ''' <param name="bundle">目标模型包</param>
        ''' <param name="folder">包内的相对目录，空值表示包根目录</param>
        Function SaveModel(bundle As ModelBundle, folder As String) As ModelManifest

        ''' <summary>把本模型写入指定目录（会创建目录并写入 <c>model.json</c>）</summary>
        ''' <param name="directory">目标目录</param>
        ''' <returns>写出的清单</returns>
        Function Save(directory As String) As ModelManifest

    End Interface

End Namespace
