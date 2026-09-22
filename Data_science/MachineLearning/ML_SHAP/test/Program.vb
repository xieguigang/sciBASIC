Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports Microsoft.VisualBasic.MachineLearning.SHAP

''' <summary>
''' 三种模型（随机森林 / XGBoost / 线性-逻辑回归）在
''' ``training_categorical.txt`` 与 ``training_regression.txt`` 之上的
''' 训练、预测评估以及 SHAP 可解释性分析的演示程序。
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

        Console.WriteLine()
        Console.WriteLine("全部任务已完成。")
    End Sub

    Private Sub RunDataset(name As String, file As String, isClassification As Boolean)
        Dim line As String = New String("="c, 88)

        Console.WriteLine(line)
        Console.WriteLine($"数据集: {name}  ({If(isClassification, "分类任务", "回归任务")})  <- {file}")
        Console.WriteLine(line)

        Dim dataset As ShapDataset = ShapDataset.LoadFile(file, isClassification, name)

        Console.WriteLine($"样本数量: {dataset.Size}, 特征维度: {dataset.Width}")
        Console.WriteLine()

        Dim evaluations As New List(Of ModelPrediction)()

        ' ---------------------------------------------------------------
        ' 1. 随机森林
        ' ---------------------------------------------------------------
        Try
            Console.WriteLine("[1/3] 训练随机森林 (RanFog) ...")
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
            Console.WriteLine("[2/3] 训练 XGBoost (TGBoost) ...")
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
            Console.WriteLine("[3/3] 训练线性模型 (LinearFitting) ...")
            Dim linear As LinearShapModel = LinearShapModel.Train(dataset, isClassification, iterations:=800, rate:=0.1)
            Dim linearPrediction As ModelPrediction = linear.PredictResult()

            evaluations.Add(linearPrediction)
            Console.WriteLine($"  预测: {linearPrediction}")
            Call RunShap(name, linear.Name, linear.BuildExplainer(), dataset, AddressOf linear.Output)
        Catch ex As Exception
            Console.WriteLine($"  [ERROR] 线性模型分析失败: {ex.Message}")
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

        Dim result As ShapAnalysisResult = ShapAnalyzer.Analyze(explainer, dataset, modelName, shapSampleLimit, output)
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
            Console.WriteLine($"    (注: 分类模型输出经过 sigmoid 变换, sigmoid(baseline+sum(phi))={LinearShapExplainer.Sigmoid(reconstructed).ToString("G6", invariant)})")
        End If

        Console.WriteLine("    贡献 Top-3:")

        For Each item In first.Ranked().Take(3)
            Console.WriteLine($"      {item.name,-8} {item.value.ToString("G6", invariant)}")
        Next
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
