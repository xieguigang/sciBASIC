Imports System
Imports System.Collections.Generic
Imports SvmModelType = Microsoft.VisualBasic.MachineLearning.SVM.Model
Imports SvmNode = Microsoft.VisualBasic.MachineLearning.SVM.Node
Imports SvmParameter = Microsoft.VisualBasic.MachineLearning.SVM.Parameter

Namespace Ensemble.Persistence

    ''' <summary>
    ''' SVM 分类/回归（LibSVM 语义）的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 一个编解码器同时负责 <see cref="SvmClassifier"/> 与 <see cref="SvmRegressor"/>：
    ''' 两者共用同一个底层 <c>Microsoft.VisualBasic.MachineLearning.SVM.Model</c>，
    ''' 差别只在包装方式与 <c>kind</c> 标记。
    ''' </para>
    ''' <para>
    ''' 支持向量天然稀疏，且数量可达数千，因此按
    ''' <c>(vector, index, value)</c> 三元组落 CSV，只写非零分量；
    ''' <c>supportVectorCoefficients</c>（类别数 × 支持向量数）同样落 CSV。
    ''' </para>
    ''' <para>
    ''' <b>超参数必须逐项落盘</b>：<c>svmType</c> 决定预测走 <c>svr_predict</c> 还是
    ''' <c>svc_predict</c>，<c>kernelType</c> / <c>gamma</c> / <c>degree</c> /
    ''' <c>coefficient0</c> 决定核函数取值——任何一个丢失都会让恢复后的模型
    ''' 「能跑但算错」。因此这里把预测相关的每一项都以显式字段写进清单，
    ''' 而不依赖对整个 <c>Parameter</c> 对象做通用序列化。
    ''' </para>
    ''' <para>
    ''' 训练期专用的 <c>weights</c>（类别惩罚权重）不参与预测，不予持久化。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SvmCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "svm"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is SvmClassifier OrElse TypeOf model Is SvmRegressor
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim fitted As SvmModelType
            Dim manifest As ModelManifest
            Dim labelOffset As Integer
            Dim featureWidth As Integer
            Dim classes As Double() = Nothing

            Dim classifier As SvmClassifier = TryCast(model, SvmClassifier)

            If classifier IsNot Nothing Then
                fitted = classifier.FittedModel
                labelOffset = classifier.IndexOffset
                featureWidth = classifier.FeatureWidth
                classes = classifier.Classes
                manifest = ModelManifest.Create("classifier", "base", AlgorithmName)
            Else
                Dim regressor As SvmRegressor = DirectCast(model, SvmRegressor)

                fitted = regressor.FittedModel
                labelOffset = regressor.IndexOffset
                featureWidth = regressor.FeatureWidth
                manifest = ModelManifest.Create("regressor", "base", AlgorithmName)
            End If

            If fitted Is Nothing Then
                Throw New InvalidOperationException("the svm adapter holds no fitted model!")
            End If

            manifest.featureWidth = featureWidth
            manifest.classes = classes
            manifest.SetInteger("labelOffset", labelOffset)
            manifest.SetInteger("numberOfClasses", fitted.numberOfClasses)
            manifest.SetInteger("supportVectorCount", fitted.supportVectorCount)
            manifest.SetInteger("trainingSize", fitted.trainingSize)
            manifest.SetIntegerVector("classLabels", fitted.classLabels)
            manifest.SetIntegerVector("numberOfSVPerClass", fitted.numberOfSVPerClass)
            manifest.SetIntegerVector("supportVectorIndices", fitted.supportVectorIndices)
            manifest.SetVector("rho", fitted.rho)
            manifest.SetVector("pairwiseProbabilityA", fitted.pairwiseProbabilityA)
            manifest.SetVector("pairwiseProbabilityB", fitted.pairwiseProbabilityB)

            If fitted.dimensionNames IsNot Nothing Then
                manifest.SetText("dimensionNames", fitted.dimensionNames)
            End If

            ' 预测相关的超参数必须逐项持久化，见类型备注
            Dim parameter As SvmParameter = fitted.parameter

            If parameter IsNot Nothing Then
                manifest.SetParameter("svmType", parameter.svmType.ToString())
                manifest.SetParameter("kernelType", parameter.kernelType.ToString())
                manifest.SetInteger("degree", parameter.degree)
                manifest.SetScalar("gamma", parameter.gamma)
                manifest.SetScalar("coefficient0", parameter.coefficient0)
                manifest.SetScalar("c", parameter.c)
                manifest.SetScalar("nu", parameter.nu)
                manifest.SetScalar("svrEpsilon", parameter.P)
                manifest.SetScalar("tolerance", parameter.EPS)
                manifest.SetScalar("cacheSize", parameter.cacheSize)
                manifest.SetInteger("shrinking", If(parameter.shrinking, 1, 0))
                manifest.SetInteger("probability", If(parameter.probability, 1, 0))
            End If

            Call bundle.WriteCsv(manifest, "support-vectors", folder,
                                 New String() {"vector", "index", "value"},
                                 EnumerateSupportVectors(fitted))
            Call bundle.WriteCsv(manifest, "coefficients", folder,
                                 New String() {"vector", "position", "value"},
                                 EnumerateCoefficients(fitted))

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            Dim parameter As SvmParameter = RestoreParameter(manifest)
            Dim names As String() = Nothing

            If manifest.texts IsNot Nothing Then
                Call manifest.texts.TryGetValue("dimensionNames", names)
            End If

            Dim restored As New SvmModelType With {
                .parameter = parameter,
                .numberOfClasses = manifest.GetInteger("numberOfClasses"),
                .supportVectorCount = manifest.GetInteger("supportVectorCount"),
                .trainingSize = manifest.GetInteger("trainingSize"),
                .classLabels = GetOptionalIntegerVector(manifest, "classLabels"),
                .numberOfSVPerClass = GetOptionalIntegerVector(manifest, "numberOfSVPerClass"),
                .supportVectorIndices = GetOptionalIntegerVector(manifest, "supportVectorIndices"),
                .rho = manifest.GetVector("rho"),
                .pairwiseProbabilityA = GetOptionalVector(manifest, "pairwiseProbabilityA"),
                .pairwiseProbabilityB = GetOptionalVector(manifest, "pairwiseProbabilityB"),
                .dimensionNames = names,
                .supportVectors = LoadSupportVectors(bundle, manifest),
                .supportVectorCoefficients = LoadCoefficients(bundle, manifest)
            }

            Dim labelOffset As Integer = manifest.GetInteger("labelOffset")
            Dim width As Integer = manifest.featureWidth

            If String.Equals(manifest.kind, "classifier", StringComparison.OrdinalIgnoreCase) Then
                If manifest.classes Is Nothing OrElse manifest.classes.Length = 0 Then
                    Throw New System.IO.InvalidDataException("the svm manifest carries no class labels!")
                End If

                Return New SvmClassifier(restored, manifest.classes, labelOffset, width)
            End If

            Return New SvmRegressor(restored, labelOffset, width)
        End Function

        Private Shared Iterator Function EnumerateSupportVectors(fitted As SvmModelType) As IEnumerable(Of String())
            Dim vectors As SvmNode()() = fitted.supportVectors

            If vectors Is Nothing Then
                Return
            End If

            For i As Integer = 0 To vectors.Length - 1
                Dim nodes As SvmNode() = vectors(i)

                If nodes Is Nothing Then
                    Continue For
                End If

                For j As Integer = 0 To nodes.Length - 1
                    If nodes(j) Is Nothing Then
                        Continue For
                    End If

                    Yield New String() {
                        CStr(i),
                        CStr(nodes(j).index),
                        ModelFormat.Number(nodes(j).value)
                    }
                Next
            Next
        End Function

        Private Shared Iterator Function EnumerateCoefficients(fitted As SvmModelType) As IEnumerable(Of String())
            Dim matrix As Double()() = fitted.supportVectorCoefficients

            If matrix Is Nothing Then
                Return
            End If

            For i As Integer = 0 To matrix.Length - 1
                Dim row As Double() = matrix(i)

                If row Is Nothing Then
                    Continue For
                End If

                For j As Integer = 0 To row.Length - 1
                    Yield New String() {CStr(i), CStr(j), ModelFormat.Number(row(j))}
                Next
            Next
        End Function

        Private Shared Function LoadSupportVectors(bundle As ModelBundle, manifest As ModelManifest) As SvmNode()()
            Dim table As CsvTable = bundle.ReadCsv(manifest, "support-vectors")
            Dim vectorCount As Integer = manifest.GetInteger("supportVectorCount")

            If vectorCount <= 0 Then
                vectorCount = 0

                For Each value As Integer In table.Integers("vector", "svm/support-vectors")
                    If value + 1 > vectorCount Then
                        vectorCount = value + 1
                    End If
                Next
            End If

            Dim vectors As New Dictionary(Of Integer, List(Of SvmNode))()
            Dim cells As Integer() = table.Integers("vector", "svm/support-vectors")
            Dim indexes As Integer() = table.Integers("index", "svm/support-vectors")
            Dim values As Double() = table.Doubles("value", "svm/support-vectors")

            For i As Integer = 0 To table.RowCount - 1
                Dim slot As Integer = cells(i)
                Dim nodes As List(Of SvmNode) = Nothing

                If Not vectors.TryGetValue(slot, nodes) Then
                    nodes = New List(Of SvmNode)()
                    vectors(slot) = nodes
                End If

                Call nodes.Add(New SvmNode(indexes(i), values(i)))
            Next

            Dim result As SvmNode()() = New SvmNode(vectorCount - 1)() {}

            For i As Integer = 0 To vectorCount - 1
                Dim nodes As List(Of SvmNode) = Nothing

                If vectors.TryGetValue(i, nodes) Then
                    result(i) = nodes.ToArray()
                Else
                    result(i) = Array.Empty(Of SvmNode)()
                End If
            Next

            Return result
        End Function

        Private Shared Function LoadCoefficients(bundle As ModelBundle, manifest As ModelManifest) As Double()()
            Dim table As CsvTable = bundle.ReadCsv(manifest, "coefficients")
            Dim rows As Integer() = table.Integers("vector", "svm/coefficients")

            If rows.Length = 0 Then
                Return Array.Empty(Of Double())()
            End If

            Dim rowCount As Integer = 0

            For Each value As Integer In rows
                If value + 1 > rowCount Then
                    rowCount = value + 1
                End If
            Next

            Dim positions As Integer() = table.Integers("position", "svm/coefficients")
            Dim cells As Double() = table.Doubles("value", "svm/coefficients")
            Dim width As Integer = 0

            For Each value As Integer In positions
                If value + 1 > width Then
                    width = value + 1
                End If
            Next

            Dim matrix As Double()() = New Double(rowCount - 1)() {}

            For i As Integer = 0 To rowCount - 1
                matrix(i) = New Double(width - 1) {}
            Next

            For i As Integer = 0 To table.RowCount - 1
                matrix(rows(i))(positions(i)) = cells(i)
            Next

            Return matrix
        End Function

        ''' <summary>
        ''' 从清单逐项还原 LibSVM 超参数；缺失项回退到 <see cref="SvmParameter"/> 的默认值。
        ''' </summary>
        ''' <param name="manifest">SVM 模型的清单</param>
        Private Shared Function RestoreParameter(manifest As ModelManifest) As SvmParameter
            Dim parameter As New SvmParameter()

            Dim svmType As Microsoft.VisualBasic.MachineLearning.SVM.SvmType
            Dim kernelType As Microsoft.VisualBasic.MachineLearning.SVM.KernelType
            Dim text As String = Nothing

            If manifest.parameters IsNot Nothing AndAlso manifest.parameters.TryGetValue("svmType", text) Then
                If Not [Enum].TryParse(text, svmType) Then
                    Throw New System.IO.InvalidDataException(
                        $"the svm manifest has an invalid svmType ('{text}')!")
                End If

                parameter.svmType = svmType
            End If

            If manifest.parameters IsNot Nothing AndAlso manifest.parameters.TryGetValue("kernelType", text) Then
                If Not [Enum].TryParse(text, kernelType) Then
                    Throw New System.IO.InvalidDataException(
                        $"the svm manifest has an invalid kernelType ('{text}')!")
                End If

                parameter.kernelType = kernelType
            End If

            parameter.degree = GetOptionalInteger(manifest, "degree", parameter.degree)
            parameter.gamma = GetOptionalScalar(manifest, "gamma", parameter.gamma)
            parameter.coefficient0 = GetOptionalScalar(manifest, "coefficient0", parameter.coefficient0)
            parameter.c = GetOptionalScalar(manifest, "c", parameter.c)
            parameter.nu = GetOptionalScalar(manifest, "nu", parameter.nu)
            parameter.P = GetOptionalScalar(manifest, "svrEpsilon", parameter.P)
            parameter.EPS = GetOptionalScalar(manifest, "tolerance", parameter.EPS)
            parameter.cacheSize = GetOptionalScalar(manifest, "cacheSize", parameter.cacheSize)
            parameter.shrinking = GetOptionalInteger(manifest, "shrinking", 1) <> 0
            parameter.probability = GetOptionalInteger(manifest, "probability", 0) <> 0

            Return parameter
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

        Private Shared Function GetOptionalVector(manifest As ModelManifest, name As String) As Double()
            Dim value As Double() = Nothing

            If manifest.vectors IsNot Nothing AndAlso manifest.vectors.TryGetValue(name, value) Then
                Return value
            End If

            Return Array.Empty(Of Double)()
        End Function

        Private Shared Function GetOptionalIntegerVector(manifest As ModelManifest, name As String) As Integer()
            Dim value As Integer() = Nothing

            If manifest.integerVectors IsNot Nothing AndAlso manifest.integerVectors.TryGetValue(name, value) Then
                Return value
            End If

            Return Array.Empty(Of Integer)()
        End Function

    End Class

End Namespace
