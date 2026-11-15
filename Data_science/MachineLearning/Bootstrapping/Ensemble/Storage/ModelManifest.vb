Imports System
Imports System.Collections.Generic

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 模型包的清单（<c>model.json</c>）数据结构。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 清单刻意**只使用具体类型**（<see cref="String"/>、<see cref="Double"/>、
    ''' 数组、<see cref="Dictionary(Of String, T)"/> 与嵌套的 <see cref="ModelManifest"/>），
    ''' 不使用接口或抽象类字段。这样 System.Text.Json 可以直接往返，
    ''' 不需要 <c>knownTypes</c>，也不会出现多态反序列化的类型判别难题。
    ''' </para>
    ''' <para>
    ''' 数值存放位置的约定：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><b>小标量 / 小向量</b>（系数、超参数、类别数组）直接内联在
    '''       <see cref="Scalars"/> / <see cref="Integers"/> / <see cref="Vectors"/> 等字典里，
    '''       便于人工阅读；</item>
    ''' <item><b>大块数值</b>（条件概率表、树结构、支持向量、系数矩阵）写入同目录 CSV，
    '''       通过 <see cref="Data"/> 以逻辑名 → 相对路径的方式引用。</item>
    ''' </list>
    ''' </remarks>
    Public NotInheritable Class ModelManifest

        ''' <summary>当前支持的清单格式版本</summary>
        Public Const CurrentFormatVersion As String = "1.0"

        ''' <summary>模型包格式版本，加载时严格校验</summary>
        Public Property formatVersion As String = CurrentFormatVersion

        ''' <summary>产出该模型包的工具标识</summary>
        public Property producer As String = "sciBASIC#/Microsoft.VisualBasic.MachineLearning.Bootstrapping"

        ''' <summary>模型包创建时间（ISO 8601 UTC）</summary>
        public Property created As String = ModelFormat.Timestamp()

        ''' <summary>
        ''' 模型类别：<c>classifier</c> 或 <c>regressor</c>。
        ''' 用于在加载时选择 <see cref="IClassifier"/> / <see cref="IRegressor"/> 的强类型入口。
        ''' </summary>
        public Property kind As String

        ''' <summary>
        ''' 模型类型：<c>voting</c> / <c>bagging</c> / <c>stacking</c> / <c>base</c>
        ''' （<c>base</c> 表示直接持久化的单个基学习器）。
        ''' </summary>
        public Property modelType As String

        ''' <summary>
        ''' 算法标识；对集成器为空，对基学习器为适配器算法名
        ''' （例如 <c>naive-bayes</c> / <c>svm</c> / <c>random-forest</c>）。
        ''' </summary>
        public Property algorithm As String

        ''' <summary>训练期的特征维度，预测时用于维度守卫</summary>
        public Property featureWidth As Integer

        ''' <summary>全局类别顺序（分类模型）；回归模型为空</summary>
        public Property classes As Double()

        ''' <summary>字符串型超参数（枚举名、损失函数名、标注等）</summary>
        public Property parameters As New Dictionary(Of String, String)()

        ''' <summary>标量数值（系数、阈值、统计量）</summary>
        public Property scalars As New Dictionary(Of String, Double)()

        ''' <summary>标量整数（计数、索引、维度）</summary>
        public Property integers As New Dictionary(Of String, Integer)()

        ''' <summary>小型浮点向量（系数、代表值、先验）</summary>
        public Property vectors As New Dictionary(Of String, Double())()

        ''' <summary>小型整数向量（类别编号、压缩指针）</summary>
        public Property integerVectors As New Dictionary(Of String, Integer())()

        ''' <summary>小型字符串数组（特征名）</summary>
        public Property texts As New Dictionary(Of String, String())()

        ''' <summary>逻辑名 → 包内相对路径的 CSV 附表索引</summary>
        public Property data As New Dictionary(Of String, String)()

        ''' <summary>子模型清单（集成器的基学习器与元学习器）</summary>
        public Property children As New List(Of ModelManifest)()

        ''' <summary>按算法标识创建清单骨架</summary>
        ''' <param name="kind"><c>classifier</c> 或 <c>regressor</c></param>
        ''' <param name="modelType">模型类型</param>
        ''' <param name="algorithm">算法标识</param>
        Public Shared Function Create(kind As String, modelType As String, algorithm As String) As ModelManifest
            Return New ModelManifest With {
                .kind = kind,
                .modelType = modelType,
                .algorithm = algorithm
            }
        End Function

        ''' <summary>读取必须存在的字符串参数</summary>
        ''' <param name="name">参数名</param>
        Public Function GetParameter(name As String) As String
            Dim value As String = Nothing

            If parameters Is Nothing OrElse Not parameters.TryGetValue(name, value) Then
                Throw New InvalidOperationException(
                    $"the model manifest is missing the parameter '{name}' (algorithm='{algorithm}', modelType='{modelType}')!")
            End If

            Return value
        End Function

        ''' <summary>读取必须存在的标量</summary>
        ''' <param name="name">标量名</param>
        Public Function GetScalar(name As String) As Double
            Dim value As Double

            If scalars Is Nothing OrElse Not scalars.TryGetValue(name, value) Then
                Throw New InvalidOperationException(
                    $"the model manifest is missing the scalar '{name}' (algorithm='{algorithm}', modelType='{modelType}')!")
            End If

            Return value
        End Function

        ''' <summary>读取必须存在的整数标量</summary>
        ''' <param name="name">整数名</param>
        Public Function GetInteger(name As String) As Integer
            Dim value As Integer

            If integers Is Nothing OrElse Not integers.TryGetValue(name, value) Then
                Throw New InvalidOperationException(
                    $"the model manifest is missing the integer '{name}' (algorithm='{algorithm}', modelType='{modelType}')!")
            End If

            Return value
        End Function

        ''' <summary>读取必须存在的浮点向量</summary>
        ''' <param name="name">向量名</param>
        Public Function GetVector(name As String) As Double()
            Dim value As Double() = Nothing

            If vectors Is Nothing OrElse Not vectors.TryGetValue(name, value) Then
                Throw New InvalidOperationException(
                    $"the model manifest is missing the vector '{name}' (algorithm='{algorithm}', modelType='{modelType}')!")
            End If

            Return value
        End Function

        ''' <summary>读取必须存在的整数向量</summary>
        ''' <param name="name">向量名</param>
        Public Function GetIntegerVector(name As String) As Integer()
            Dim value As Integer() = Nothing

            If integerVectors Is Nothing OrElse Not integerVectors.TryGetValue(name, value) Then
                Throw New InvalidOperationException(
                    $"the model manifest is missing the integer vector '{name}' (algorithm='{algorithm}', modelType='{modelType}')!")
            End If

            Return value
        End Function

        ''' <summary>写入字符串参数（<c>Nothing</c> 时写入空字符串）</summary>
        Public Sub SetParameter(name As String, value As String)
            Call EnsureDictionaries()
            parameters(name) = If(value, "")
        End Sub

        ''' <summary>写入标量</summary>
        Public Sub SetScalar(name As String, value As Double)
            Call EnsureDictionaries()
            scalars(name) = value
        End Sub

        ''' <summary>写入整数标量</summary>
        Public Sub SetInteger(name As String, value As Integer)
            Call EnsureDictionaries()
            integers(name) = value
        End Sub

        ''' <summary>写入浮点向量</summary>
        Public Sub SetVector(name As String, value As Double())
            Call EnsureDictionaries()
            vectors(name) = value
        End Sub

        ''' <summary>写入整数向量</summary>
        Public Sub SetIntegerVector(name As String, value As Integer())
            Call EnsureDictionaries()
            integerVectors(name) = value
        End Sub

        ''' <summary>写入字符串数组</summary>
        Public Sub SetText(name As String, value As String())
            Call EnsureDictionaries()
            texts(name) = value
        End Sub

        ''' <summary>
        ''' 反序列化后的字典可能为 <c>Nothing</c>（JSON 中缺失该键），
        ''' 所有写入入口统一补齐，避免空引用。
        ''' </summary>
        Friend Sub EnsureDictionaries()
            If parameters Is Nothing Then parameters = New Dictionary(Of String, String)()
            If scalars Is Nothing Then scalars = New Dictionary(Of String, Double)()
            If integers Is Nothing Then integers = New Dictionary(Of String, Integer)()
            If vectors Is Nothing Then vectors = New Dictionary(Of String, Double())()
            If integerVectors Is Nothing Then integerVectors = New Dictionary(Of String, Integer())()
            If texts Is Nothing Then texts = New Dictionary(Of String, String())()
            If data Is Nothing Then data = New Dictionary(Of String, String)()
            If children Is Nothing Then children = New List(Of ModelManifest)()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{modelType}/{algorithm} ({kind}), featureWidth={featureWidth}, children={If(children, New List(Of ModelManifest)()).Count}"
        End Function

    End Class

End Namespace
