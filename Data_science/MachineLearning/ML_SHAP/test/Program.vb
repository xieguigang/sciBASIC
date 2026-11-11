#Region "Microsoft.VisualBasic::50905a22c590f97e98f4536308e0426f, Data_science\MachineLearning\ML_SHAP\test\Program.vb"

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

    '   Total Lines: 208
    '    Code Lines: 129 (62.02%)
    ' Comment Lines: 31 (14.90%)
    '    - Xml Docs: 61.29%
    ' 
    '   Blank Lines: 48 (23.08%)
    '     File Size: 9.03 KB


    ' Module Program
    ' 
    '     Function: FindDataDirectory, ResolveDataDirectory
    ' 
    '     Sub: Main, RunDataset, RunShap, WritePredictions
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.MachineLearning.GLMNet
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports Microsoft.VisualBasic.MachineLearning.SHAP
Imports NumericTable = Microsoft.VisualBasic.Data.NumericTable
Imports INumericMatrix = Microsoft.VisualBasic.Math.INumericMatrix
Imports ILabeledMatrix = Microsoft.VisualBasic.ComponentModel.DataSourceModel.ILabeledMatrix

''' <summary>
''' 四种模型（随机森林 / XGBoost / 线性-逻辑回归 / glmnet 正则化路径）在
''' ``training_categorical.txt`` 与 ``training_regression.txt`` 之上的
''' 训练、预测评估以及 SHAP 可解释性分析的演示程序。
''' 
''' 运行过程中通过 <see cref="DebugAudit"/> 记录各阶段的耗时与断言结果，
''' 并在输出目录生成 ``shap-test-report.json`` 审计报告。
''' 
''' 用法：
''' 
''' ```
''' dotnet run [dataDirectory] [outputDirectory]
''' ```
''' </summary>
Module Program

    Private ReadOnly invariant As CultureInfo = CultureInfo.InvariantCulture

    ''' <summary>
    ''' 参与 SHAP 分析的样本数量上限（0 表示全部）。RanFog 的 TreeSHAP
    ''' 计算量较大，因此演示程序默认只解释前若干个样本。
    ''' </summary>
    Private ReadOnly shapSampleLimit As Integer = 200

    Private outputDir As String

    Sub Main(args As String())
        Try
            Console.OutputEncoding = Text.Encoding.UTF8
        Catch
            ' 控制台重定向的场景下设置编码可能失败，忽略即可
        End Try

        Dim dataDir As String = ResolveDataDirectory(args)

        If dataDir Is Nothing Then
            Console.WriteLine("[ERROR] 无法自动定位 training_categorical.txt / training_regression.txt，请在命令行参数之中显式指定数据目录。")
            Return
        End If

        outputDir = If(args.Length > 1, args(1), Path.Combine(AppContext.BaseDirectory, "shap_output"))
        Call Directory.CreateDirectory(outputDir)

        Console.WriteLine($"数据目录: {dataDir}")
        Console.WriteLine($"输出目录: {outputDir}")
        Console.WriteLine()

        Call RunDataset("categorical", Path.Combine(dataDir, "training_categorical.txt"), True)
        Call RunDataset("regression", Path.Combine(dataDir, "training_regression.txt"), False)

        Call WriteAuditReport()

        Console.WriteLine()
        Console.WriteLine("全部任务已完成。")
    End Sub

    ''' <summary>
    ''' 把 <see cref="DebugAudit"/> 收集到的耗时、断言与变量快照导出为 JSON 报告。
    ''' </summary>
    Private Sub WriteAuditReport()
        Dim report As String = Path.Combine(outputDir, "shap-test-report.json")

        Call DebugAudit.SaveReport(report)

        Console.WriteLine()
        Console.WriteLine($"调试审计报告: {report}")
        Console.WriteLine($"断言失败次数: {DebugAudit.FailureCount}")
        Console.WriteLine(DebugAudit.Dump(includeRecords:=False))
    End Sub

    Private Sub RunDataset(name As String, file As String, isClassification As Boolean)
        Dim line As String = New String("="c, 88)

        Console.WriteLine(line)
        Console.WriteLine($"数据集: {name}  ({If(isClassification, "分类任务", "回归任务")})  <- {file}")
        Console.WriteLine(line)

        Dim dataset As ShapDataset = ShapDataset.LoadFile(file, isClassification, name)

        Call AuditDatasetView(dataset)

        Console.WriteLine($"样本数量: {dataset.Size}, 特征维度: {dataset.Width}")
        Console.WriteLine()

        Dim evaluations As New List(Of ModelPrediction)()

        ' ---------------------------------------------------------------
        ' 1. 随机森林
        ' ---------------------------------------------------------------
        Try
            Console.WriteLine("[1/4] 训练随机森林 (RanFog) ...")
            Dim rf As RanFogShapModel = RanFogShapModel.Train(dataset, isClassification, maxTrees:=20, maxBranch:=128)
            Dim rfPrediction As ModelPrediction = rf.PredictResult()

            evaluations.Add(rfPrediction)
            Console.WriteLine($"  预测: {rfPrediction}")
            Call RunShap(name, rf.Name, rf.BuildExplainer(), dataset, AddressOf rf.Output)
        Catch ex As Exception
            Console.WriteLine($"  [ERROR] 随机森林分析失败: {ex.Message}")
        End Try

        ' ---------------------------------------------------------------
        ' 2. XGBoost
        ' ---------------------------------------------------------------
        Try
            Console.WriteLine("[2/4] 训练 XGBoost (TGBoost) ...")
            Dim xgb As XGBoostShapModel = XGBoostShapModel.Train(dataset, isClassification, rounds:=20, maxDepth:=4, eta:=0.3)
            Dim xgbPrediction As ModelPrediction = xgb.PredictResult()

            evaluations.Add(xgbPrediction)
            Console.WriteLine($"  预测: {xgbPrediction}")
            Call RunShap(name, xgb.Name, xgb.BuildExplainer(), dataset, AddressOf xgb.Output)
        Catch ex As Exception
            Console.WriteLine($"  [ERROR] XGBoost 分析失败: {ex.Message}")
        End Try

        ' ---------------------------------------------------------------
        ' 3. 线性 / 逻辑回归
        ' ---------------------------------------------------------------
        Try
            Console.WriteLine("[3/4] 训练线性模型 (LinearFitting) ...")
            Dim linear As LinearShapModel = LinearShapModel.Train(dataset, isClassification, iterations:=800, rate:=0.1)
            Dim linearPrediction As ModelPrediction = linear.PredictResult()

            evaluations.Add(linearPrediction)
            Console.WriteLine($"  预测: {linearPrediction}")
            Call RunShap(name, linear.Name, linear.BuildExplainer(), dataset, AddressOf linear.Output)
        Catch ex As Exception
            Console.WriteLine($"  [ERROR] 线性模型分析失败: {ex.Message}")
        End Try

        ' ---------------------------------------------------------------
        ' 4. glmnet（正则化路径 + 交叉验证选优）
        ' ---------------------------------------------------------------
        Try
            Console.WriteLine("[4/4] 训练 glmnet (GLMNet) ...")
            Call RunGlmnet(name, dataset, isClassification, evaluations)
        Catch ex As Exception
            Console.WriteLine($"  [ERROR] glmnet 分析失败: {ex.Message}")
        End Try

        Call WritePredictions(name, evaluations)

        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' 对一个模型执行 SHAP 分析：打印控制台摘要并导出 CSV。
    ''' </summary>
    Private Sub RunShap(datasetName As String,
                        modelName As String,
                        explainer As IShapExplainer,
                        dataset As ShapDataset,
                        output As Func(Of Double(), Double))

        Dim result As ShapAnalysisResult

        Using timer = DebugAudit.Watch($"shap:{datasetName}/{modelName}", logOnExit:=False)
            result = ShapAnalyzer.Analyze(explainer, dataset, modelName, shapSampleLimit, output)
            timer.Capture($"{modelName}.baseline", result.Baseline)
            timer.Capture($"{modelName}.explained", result.Explanations.Count)
        End Using

        Call DebugAudit.Check(result.Explanations.Count > 0, $"{modelName}: no sample was explained!")
        Call DebugAudit.Check(result.Explanations.Count <= dataset.Size,
                              $"{modelName}: explained {result.Explanations.Count} samples but the dataset holds only {dataset.Size}!")
        Call DebugAudit.Check(explainer.Size = dataset.Width,
                              $"{modelName}: explainer size {explainer.Size} != dataset width {dataset.Width}!")
        Call DebugAudit.Check(result.MeanAbsShap.Length = dataset.Width,
                              $"{modelName}: importance vector has {result.MeanAbsShap.Length} items but the dataset has {dataset.Width} features!")

        Call CheckAdditivity(modelName, explainer, dataset, output)

        Dim prefix As String = $"{datasetName}_{modelName}"

        Call ShapCsvWriter.WriteExplanations(result, Path.Combine(outputDir, $"{prefix}_shap.csv"))
        Call ShapCsvWriter.WriteGlobalImportance(result, Path.Combine(outputDir, $"{prefix}_importance.csv"))

        Console.WriteLine($"  SHAP: baseline={result.Baseline.ToString("G6", invariant)}, 解释样本数={result.Explanations.Count}")

        Console.WriteLine("  Top-5 全局重要性 (平均 |SHAP|):")

        For Each item In result.GlobalImportance().Take(5)
            Console.WriteLine($"    {item.name,-8} {item.importance.ToString("G6", invariant)}")
        Next

        Dim first As ShapExplanation = result.Explanations(0)
        Dim reconstructed As Double = first.Baseline + first.Contributions.Sum()

        Console.WriteLine($"  单样本解释 [{first.ID}]: baseline+sum(phi)={reconstructed.ToString("G6", invariant)}, 模型输出={first.Output.ToString("G6", invariant)}")

        If dataset.IsCategorical AndAlso System.Math.Abs(reconstructed - first.Output) > 0.000001 Then
            Console.WriteLine("    (注: 加性分解成立于 link scale, 响应尺度还需要经过 sigmoid / softmax 等变换)")
        End If

        Console.WriteLine("    贡献 Top-3:")

        For Each item In first.Ranked().Take(3)
            Console.WriteLine($"      {item.name,-8} {item.value.ToString("G6", invariant)}")
        Next
    End Sub

    ''' <summary>
    ''' 在 identity 连接的线性解释器上校验加性一致性：
    ''' ``baseline + sum(phi)`` 必须等于模型的原始输出。
    ''' </summary>
    ''' <remarks>
    ''' 分类任务（sigmoid / softmax）与计数模型（exp）的加性分解成立于 link scale，
    ''' 与响应尺度之间存在非线性变换，因此这里只在回归任务上做严格比较。
    ''' </remarks>
    Private Sub CheckAdditivity(modelName As String,
                                explainer As IShapExplainer,
                                dataset As ShapDataset,
                                output As Func(Of Double(), Double))
        Dim linear As LinearShapExplainer = TryCast(explainer, LinearShapExplainer)

        If linear Is Nothing OrElse linear.Link IsNot Nothing OrElse dataset.IsCategorical Then
            Return
        End If

        Dim x As Double() = dataset.Features(0)
        Dim additive As Double = linear.AdditiveValue(x)
        Dim actual As Double = output(x)
        Dim tolerance As Double = 0.000001 * System.Math.Max(1.0, System.Math.Abs(actual))

        Call DebugAudit.Check(System.Math.Abs(additive - actual) <= tolerance,
                              $"{modelName}: baseline+sum(phi) = {additive:G6} but the model output is {actual:G6}!")
    End Sub

    ''' <summary>
    ''' GLMNet（glmnet 正则化路径）的 SHAP 分析：路径末端模型、交叉验证选优模型，
    ''' 以及（分类任务下）多分类模型的多响应列解释。
    ''' </summary>
    ''' <param name="datasetName">数据集名字（用于 CSV 前缀）</param>
    ''' <param name="dataset">训练数据集</param>
    ''' <param name="isClassification">是否分类任务（决定 family）</param>
    ''' <param name="evaluations">预测评估结果的收集容器</param>
    Private Sub RunGlmnet(datasetName As String,
                          dataset As ShapDataset,
                          isClassification As Boolean,
                          evaluations As List(Of ModelPrediction))
        Dim family As String = If(isClassification, "binomial", "gaussian")
        ' 演示数据为 1500 x 500，较粗的 lambda 网格与较少的折数可以在覆盖全部代码路径的同时
        ' 把 glmnet 的求解耗时控制在可接受范围内
        Dim options As New GlmnetOptions With {
            .Alpha = 1.0R,
            .NLambda = 30
        }

        ' ---------------------------------------------------------------
        ' 4.1 正则化路径：缺省解释路径末端（惩罚最弱）的模型
        ' ---------------------------------------------------------------
        Dim path As GlmnetShapModel

        Using timer = DebugAudit.Watch($"glmnet.fit[{family}]", logOnExit:=False)
            path = GlmnetShapModel.Train(dataset, family, options)
            timer.Capture($"{family}.lambda", path.Lambda)
            timer.Capture($"{family}.nzero", path.Fit.nzero(path.LambdaIndex))
        End Using

        Call DebugAudit.Check(path.Coefficients.Length = dataset.Width,
                              $"glmnet: {path.Coefficients.Length} coefficients but the dataset has {dataset.Width} features!")
        Call DebugAudit.Check(path.IsClassification = isClassification,
                              $"glmnet: family '{family}' reports classification = {path.IsClassification}!")
        Call DebugAudit.Check(path.LambdaIndex >= 0 AndAlso path.LambdaIndex < path.Fit.Count,
                              $"glmnet: lambda index {path.LambdaIndex} is outside the path of {path.Fit.Count} step(s)!")

        Dim pathPrediction As ModelPrediction = path.PredictResult()

        evaluations.Add(pathPrediction)
        Console.WriteLine($"  glmnet({family}) 路径模型: lambda={path.Lambda.ToString("G6", invariant)}, 非零系数={path.Fit.nzero(path.LambdaIndex)}")
        Console.WriteLine($"  预测: {pathPrediction}")
        Call RunShap(datasetName, path.Name, path.BuildExplainer(), dataset, AddressOf path.Output)

        ' ---------------------------------------------------------------
        ' 4.2 cv.glmnet：使用 lambda.min 选出的模型
        ' ---------------------------------------------------------------
        Dim cvOptions As GlmnetOptions = options.Clone()

        cvOptions.NFolds = System.Math.Max(2, System.Math.Min(3, dataset.Size))
        cvOptions.TypeMeasure = If(isClassification, CVMeasure.Deviance, CVMeasure.Mse)

        Dim cv As GlmnetShapModel

        Using timer = DebugAudit.Watch($"cv.glmnet[{family}]", logOnExit:=False)
            cv = GlmnetShapModel.TrainCV(dataset, family, cvOptions, "lambda.min")
            timer.Capture($"{family}.lambda.min", cv.Lambda)
        End Using

        Call DebugAudit.Check(cv.Lambda > 0 AndAlso Double.IsFinite(cv.Lambda),
                              $"cv.glmnet: lambda.min = {cv.Lambda} is not a valid regularization strength!")
        Call DebugAudit.Check(cv.LambdaIndex >= 0 AndAlso cv.LambdaIndex < cv.Fit.Count,
                              $"cv.glmnet: lambda index {cv.LambdaIndex} is outside the path of {cv.Fit.Count} step(s)!")

        Dim cvPrediction As ModelPrediction = cv.PredictResult()

        evaluations.Add(cvPrediction)
        Console.WriteLine($"  glmnet({family}) CV 模型: lambda.min={cv.Lambda.ToString("G6", invariant)}, 非零系数={cv.Fit.nzero(cv.LambdaIndex)}")
        Console.WriteLine($"  预测: {cvPrediction}")
        Call RunShap(datasetName, $"{cv.Name}-cv", cv.BuildExplainer(), dataset, AddressOf cv.Output)

        If Not isClassification Then
            Return
        End If

        ' ---------------------------------------------------------------
        ' 4.3 multinomial：演示多响应（多分类）模型的 ResponseIndex
        ' ---------------------------------------------------------------
        Dim multi As GlmnetShapModel

        Using timer = DebugAudit.Watch("glmnet.fit[multinomial]", logOnExit:=False)
            ' 0/1 两类标签同样可以走 softmax 路径；这里解释第 2 类（class_1）的概率
            multi = GlmnetShapModel.Train(dataset, "multinomial", options, responseIndex:=1)
            timer.Capture("multinomial.response", multi.ResponseName)
        End Using

        Call DebugAudit.Check(multi.ResponseIndex = 1, "glmnet: the multinomial response index was not kept!")
        Call DebugAudit.Check(multi.Coefficients.Length = dataset.Width,
                              $"glmnet(multinomial): {multi.Coefficients.Length} coefficients but the dataset has {dataset.Width} features!")

        Dim multiPrediction As ModelPrediction = multi.PredictResult()

        evaluations.Add(multiPrediction)
        Console.WriteLine($"  glmnet(multinomial) 多分类模型: 响应={multi.ResponseName}, lambda={multi.Lambda.ToString("G6", invariant)}")
        Console.WriteLine($"  预测: {multiPrediction}")
        Call RunShap(datasetName, $"{multi.Name}-{multi.ResponseName}", multi.BuildExplainer(), dataset, AddressOf multi.Output)
    End Sub

    ''' <summary>
    ''' 校验 <see cref="ShapDataset"/> 的只读转发视图与其内部持有的
    ''' <see cref="NumericTable"/> 完全一致（组合改造之后的等价性断言）。
    ''' </summary>
    ''' <param name="dataset"></param>
    Private Sub AuditDatasetView(dataset As ShapDataset)
        Dim table As NumericTable = dataset.Table
        Dim matrix As INumericMatrix = dataset
        Dim rows As ILabeledMatrix = dataset

        Call DebugAudit.Check(table IsNot Nothing, "数据集没有持有底层数值表!")
        Call DebugAudit.Check(dataset.Size = table.nsamples, $"{dataset.Name}: Size={dataset.Size} 与 Table.nsamples={table.nsamples} 不一致!")
        Call DebugAudit.Check(dataset.Width = table.nfeatures, $"{dataset.Name}: Width={dataset.Width} 与 Table.nfeatures={table.nfeatures} 不一致!")
        Call DebugAudit.Check(table.nlabels = 1, $"{dataset.Name}: 响应列数量应当为 1，实际为 {table.nlabels}!")
        Call DebugAudit.Check(table.labelNames IsNot Nothing AndAlso table.labelNames.Length = 1 AndAlso table.labelNames(0) = "Y",
                              $"{dataset.Name}: 响应列名应当为 'Y'（回归与 glmnet 依赖缺省的第一个标签列）!")
        Call DebugAudit.Check(dataset.IDs Is table.rowNames, $"{dataset.Name}: IDs 未指向 Table.rowNames!")
        Call DebugAudit.Check(dataset.Features Is table.features, $"{dataset.Name}: Features 未指向 Table.features!")
        Call DebugAudit.Check(dataset.FeatureNames Is table.featureNames, $"{dataset.Name}: FeatureNames 未指向 Table.featureNames!")
        Call DebugAudit.Check(dataset.Name = table.name, $"{dataset.Name}: Name 与 Table.name 不一致!")
        Call DebugAudit.Check(dataset.Labels.SequenceEqual(table.GetLabel(0)), $"{dataset.Name}: Labels 与标签矩阵第 0 列不一致!")
        Call DebugAudit.Check(dataset.ClassLabels.SequenceEqual(table.GetClassLabel(0)), $"{dataset.Name}: ClassLabels 与标签矩阵第 0 列不一致!")
        Call DebugAudit.Check(matrix.ArrayPack() Is table.features, $"{dataset.Name}: INumericMatrix.ArrayPack 未转发到 Table.features!")
        Call DebugAudit.Check(rows.GetLabels().SequenceEqual(table.RowNamesOrDefault()), $"{dataset.Name}: ILabeledMatrix.GetLabels 未转发到 Table 的行名!")

        ' 维度一致性由基类校验，加载阶段已经调用过一次，这里确认对象可以再次通过校验
        Call table.Validate()
    End Sub

    Private Sub WritePredictions(datasetName As String, evaluations As List(Of ModelPrediction))
        Dim filePath As String = Path.Combine(outputDir, $"{datasetName}_predictions.csv")

        Using writer As New StreamWriter(filePath, False, Text.Encoding.UTF8)
            writer.WriteLine("model,id,label,prediction")

            For Each evaluation As ModelPrediction In evaluations
                For i As Integer = 0 To evaluation.Size - 1
                    writer.WriteLine($"{evaluation.ModelName},{evaluation.IDs(i)},{evaluation.Labels(i).ToString("G17", invariant)},{evaluation.Predictions(i).ToString("G17", invariant)}")
                Next
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 定位包含 ``training_categorical.txt`` 的数据目录。
    ''' </summary>
    Private Function ResolveDataDirectory(args As String()) As String
        If args IsNot Nothing AndAlso args.Length > 0 AndAlso Directory.Exists(args(0)) Then
            Return args(0)
        End If

        Dim location As String = FindDataDirectory(New DirectoryInfo(AppContext.BaseDirectory))

        If location IsNot Nothing Then
            Return location
        End If

        Return FindDataDirectory(New DirectoryInfo(Environment.CurrentDirectory))
    End Function

    Private Function FindDataDirectory(start As DirectoryInfo) As String
        Dim dir As DirectoryInfo = start

        While dir IsNot Nothing
            Dim candidate As String = Path.Combine(dir.FullName, "MachineLearning", "MachineLearning", "RandomForests")

            If File.Exists(Path.Combine(candidate, "training_categorical.txt")) Then
                Return candidate
            End If

            dir = dir.Parent
        End While

        Return Nothing
    End Function

End Module
