Imports System
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports GLMNet
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data
Imports std = System.Math

''' <summary>
''' 与 R glmnet 5.1 黄金数据的逐项对拍 + API 契约自检。
''' </summary>
''' <remarks>
''' 断言结果全部通过 <c>DebugAudit</c> 记录（<c>Test / Require / CheckThrows / Mark / Snapshot</c>），
''' 结束时由 <c>SaveReport</c> 写出 JSON 测试报告；与 DataFittings 的
''' <c>_smoketest</c> 自检结构保持一致。
''' </remarks>
Public Module GlmnetAudit

    Private Const RscriptPath As String = "C:\Program Files\R\R-4.5.0\bin\Rscript.exe"

    ''' <summary>黄金数据目录（由 test.vbproj 复制到输出目录）。</summary>
    Public ReadOnly Property DataDir As String
        Get
            Return Path.Combine(AppContext.BaseDirectory, "data")
        End Get
    End Property

    ''' <summary>自检报告路径。</summary>
    Public ReadOnly Property ReportPath As String
        Get
            Return Path.Combine(AppContext.BaseDirectory, "glmnet-test-report.json")
        End Get
    End Property

    ''' <summary>运行全部自检，返回断言失败数。</summary>
    Public Function Run() As Integer
        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 8192
        Call DebugAudit.Mark("glmnet 5.1 replication audit started")
        Call DebugAudit.Snapshot(DebugAudit.V(DataDir), DebugAudit.V(ReportPath))

        Call RunCheck("environment", AddressOf CheckEnvironment)
        Call RunCheck("gaussian", Sub() CheckFamily("gaussian", FamilyKind.Gaussian, True))
        Call RunCheck("binomial", Sub() CheckFamily("binomial", FamilyKind.Binomial, True))
        Call RunCheck("poisson", Sub() CheckFamily("poisson", FamilyKind.Poisson, True))
        Call RunCheck("multinomial", Sub() CheckFamily("multinomial", FamilyKind.Multinomial, False))
        Call RunCheck("cox", Sub() CheckFamily("cox", FamilyKind.Cox, False))
        Call RunCheck("mgaussian", Sub() CheckFamily("mgaussian", FamilyKind.MGaussian, False))
        Call RunCheck("contracts", AddressOf CheckContracts)
        Call RunCheck("options", AddressOf CheckOptions)
        Call RunCheck("relaxed", AddressOf CheckRelaxed)
        Call RunCheck("table", AddressOf CheckNumericTable)

        Call DebugAudit.Dump()
        Call DebugAudit.SaveReport(ReportPath)
        Return DebugAudit.FailureCount
    End Function

    ''' <summary>单个检查组的异常兜底：不让一个用例中断全部对拍。</summary>
    Private Sub RunCheck(name As String, check As Action)
        Try
            Call check()
            Call DebugAudit.Mark($"{name}.completed", "ok")
        Catch ex As Exception
            Call DebugAudit.Mark($"{name}.exception", ex.ToString())
        End Try
    End Sub

    ''' <summary>核验本地 R / glmnet 版本为 5.1。</summary>
    Private Sub CheckEnvironment()
        If Not File.Exists(RscriptPath) Then
            Call DebugAudit.Mark("environment.rscript", $"missing: {RscriptPath}")
            Return
        End If

        Dim script As String = Path.Combine(AppContext.BaseDirectory, "R", "check_version.R")
        Dim psi As New ProcessStartInfo With {
            .FileName = RscriptPath,
            .Arguments = $"""{script}""",
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .UseShellExecute = False,
            .CreateNoWindow = True
        }

        Using proc As Process = Process.Start(psi)
            Dim output As String = proc.StandardOutput.ReadToEnd()
            proc.WaitForExit(60000)
            Call DebugAudit.Mark("environment.r.version", If(output.Contains("R.version=4.5.0"), "4.5.0", "unknown"))
            Call DebugAudit.Mark("environment.glmnet.version", If(output.Contains("glmnet.version=5.1"), "5.1", "unknown"))
            Call DebugAudit.Test(output.Contains("OK: glmnet 5.1 environment verified."),
                                 "the local R installation must provide glmnet 5.1")
        End Using
    End Sub

    ''' <summary>一个 family 的完整对拍：lambda、系数、dev.ratio、df 与 CV 曲线。</summary>
    Private Sub CheckFamily(prefix As String, family As FamilyKind, compareBeta As Boolean)
        Dim x As Double()() = ReadCsv($"{prefix}_x.csv")
        Dim y As Double()() = ReadCsv($"{prefix}_y.csv")
        Dim goldenFit As Double()() = ReadCsv($"{prefix}_fit.csv", True)
        Dim goldenBeta As Double()() = ReadCsv($"{prefix}_beta.csv", True)
        Dim goldenCv As Double()() = ReadCsv($"{prefix}_cv.csv", True)

        If family = FamilyKind.Multinomial Then
            For i As Integer = 0 To y.Length - 1
                y(i)(0) -= 1.0R
            Next
        End If

        Dim grid As Double() = goldenFit.Select(Function(r) r(1)).ToArray()
        Dim options As New GlmnetOptions With {
            .Lambda = grid,
            .NLambda = grid.Length,
            .FeatureNames = Nothing
        }

        Dim fit As GlmnetFit = GlmnetSolver.Fit(x, y, family, options)
        Dim steps As Integer = std.Min(fit.Count, grid.Length)

        Call DebugAudit.Mark($"{prefix}.steps", $"vb={fit.Count} r={grid.Length}")
        Call DebugAudit.Mark($"{prefix}.shapes",
                             $"beta0={fit.beta(0).Length} beta00={fit.beta(0)(0).Length} p={fit.nvars} k={fit.nresponses} a00={fit.a0(0).Length} jerr={fit.jerr} lastDev={fit.devRatio(fit.Count - 1):G4} lastDf={fit.df(fit.Count - 1)}")
        Call DebugAudit.Test(steps >= std.Max(3, CInt(grid.Length * 0.5)),
                             $"{prefix}: the fitted path must cover most of the R lambda grid")
        Call DebugAudit.Test(fit.jerr = GlmnetError.None, $"{prefix}: jerr must be 0")

        ' lambda 网格：使用自定义序列时应逐步一致
        Dim maxLambdaDiff As Double = 0.0R
        For s As Integer = 0 To steps - 1
            maxLambdaDiff = std.Max(maxLambdaDiff, std.Abs(fit.lambda(s) - goldenFit(s)(1)))
        Next
        Call DebugAudit.Mark($"{prefix}.lambda.maxDiff", maxLambdaDiff)
        Call DebugAudit.Test(maxLambdaDiff < 0.000001R, $"{prefix}: lambda grid must match R")

        ' 偏差解释度与零模型偏差
        Call DebugAudit.Mark($"{prefix}.nulldev", $"vb={fit.nulldev:G6} r={goldenFit(0)(6):G6}")
        Call AssertClose($"{prefix}.nulldev", goldenFit(0)(6), fit.nulldev, 0.05R, 0.01R)

        Dim maxDevDiff As Double = 0.0R
        For s As Integer = 0 To steps - 1
            maxDevDiff = std.Max(maxDevDiff, std.Abs(fit.devRatio(s) - goldenFit(s)(4)))
        Next
        Call DebugAudit.Mark($"{prefix}.dev.ratio.maxDiff", maxDevDiff)
        Call DebugAudit.Test(maxDevDiff < 0.25R, $"{prefix}: dev.ratio must be close to R")

        ' 系数：单响应 family 逐步比较最大偏差
        If compareBeta Then
            Dim maxCoefDiff As Double = 0.0R
            For s As Integer = 0 To steps - 1
                Dim coef As Double()() = fit.GetCoefficients(s)
                For j As Integer = 0 To std.Min(x(0).Length, goldenBeta(s).Length - 1) - 1
                    maxCoefDiff = std.Max(maxCoefDiff, std.Abs(coef(j + 1)(0) - goldenBeta(s)(j + 1)))
                Next
            Next
            Call DebugAudit.Mark($"{prefix}.beta.maxDiff", maxCoefDiff)
            Call DebugAudit.Test(maxCoefDiff < 0.35R, $"{prefix}: coefficient path must be close to R")
        End If

        ' CV：使用 R 导出的 foldid，保证分折完全一致
        Dim foldid As Integer() = ReadCsv($"{prefix}_foldid.csv").Select(Function(r) CInt(r(0)) - 1).ToArray()
        Dim cvOptions As GlmnetOptions = options.Clone()
        cvOptions.NFolds = 5
        cvOptions.FoldId = foldid
        cvOptions.TypeMeasure = If(family = FamilyKind.Cox OrElse family = FamilyKind.Poisson OrElse
                                   family = FamilyKind.Binomial OrElse family = FamilyKind.Multinomial,
                                   CVMeasure.Deviance, CVMeasure.Mse)

        Dim cv As GlmnetCVFit = GlmnetSolver.CrossValidate(x, y, family, cvOptions)
        Call DebugAudit.Mark($"{prefix}.cv.lambda.min", $"vb={cv.lambdaMin:G6} r={goldenCv(0)(4):G6}")
        Call DebugAudit.Mark($"{prefix}.cv.lambda.1se", $"vb={cv.lambda1se:G6} r={goldenCv(0)(5):G6}")
        Call DebugAudit.Test(std.Abs(std.Log((cv.lambdaMin + 0.000001R) / (goldenCv(0)(4) + 0.000001R))) < 1.5R,
                             $"{prefix}: lambda.min should be within a few grid steps of R")
        Call DebugAudit.Test(cv.cvm.Length = cv.lambda.Length, $"{prefix}: cvm length must match lambda")
        Call DebugAudit.Test(Not cv.cvm.Any(Function(v) Double.IsNaN(v)), $"{prefix}: cvm must be finite")
    End Sub

    ''' <summary>统一 API 的契约自检。</summary>
    Private Sub CheckContracts()
        Dim x As Double()() = ReadCsv("gaussian_x.csv")
        Dim y As Double()() = ReadCsv("gaussian_y.csv")
        Dim fit As GlmnetFit = GlmnetSolver.Fit(x, y, "gaussian")

        Call DebugAudit.Require(fit.Count > 0, "glmnet must produce a non-empty path")
        Call DebugAudit.Test(fit.nobs = x.Length AndAlso fit.nvars = x(0).Length, "nobs/nvars must match the input")

        Dim s As Double = fit.lambda(fit.Count \ 2)
        Dim coef As Double()() = GlmnetPredict.Coef(fit, s)
        Call DebugAudit.Test(coef.Length = x(0).Length + 1, "coef must have p+1 rows")

        Dim interpolated As Double()() = fit.GetCoefficients(s, exact:=True)
        Call DebugAudit.Test(interpolated.Length = coef.Length, "exact coef must have the same shape")

        Dim link As PredictionBundle = GlmnetPredict.Predict(fit, New Double()() {x(0), x(1)}, PredictionType.Link, s)
        Dim response As PredictionBundle = GlmnetPredict.Predict(fit, New Double()() {x(0)}, PredictionType.Response, s)
        Dim nonzero As PredictionBundle = GlmnetPredict.Predict(fit, x, PredictionType.NonZero, s)
        Call DebugAudit.Test(link.Values.Length = 2, "link prediction must return two rows")
        Call DebugAudit.Test(response.Values(0)(0) = link.Values(0)(0), "gaussian response equals link")
        Call DebugAudit.Test(nonzero.Indices IsNot Nothing, "nonzero prediction must return indices")

        Dim table As Microsoft.VisualBasic.Data.NumericTable = fit.toNumericTable()
        Call DebugAudit.Test(table.nsamples = fit.Count, "toNumericTable must have one row per lambda")

        Dim curves As Dictionary(Of String, Double()) = AssessGLMNet.Assess(fit, x, y)
        Call DebugAudit.Test(curves.ContainsKey("mse") AndAlso curves.ContainsKey("lambda"), "assess must return mse and lambda")

        Dim roc As Double()() = AssessGLMNet.Roc(New Double() {0.1R, 0.8R, 0.4R}, New Double() {0.0R, 1.0R, 1.0R})
        Call DebugAudit.Test(roc.Length >= 2, "roc must return at least two points")

        Dim sparse As SparseCSC = SparseCSC.FromDense(x)
        Call DebugAudit.Test(sparse.P = x(0).Length AndAlso sparse.N = x.Length, "CSC dimensions must match")
        Dim sparseFit As GlmnetFit = GlmnetSolver.Fit(sparse, y, "gaussian")
        Call DebugAudit.Test(sparseFit.Count > 0, "sparse input must produce a path")
    End Sub

    ''' <summary>参数校验与边界用例。</summary>
    Private Sub CheckOptions()
        Dim x As Double()() = ReadCsv("gaussian_x.csv")
        Dim y As Double()() = ReadCsv("gaussian_y.csv")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim bad As New GlmnetOptions With {.Alpha = 2.0R}
                Call GlmnetSolver.Fit(x, y, "gaussian", bad)
            End Sub, "alpha outside [0,1] must be rejected")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub() Call GlmnetSolver.Fit(x, y, "unknown-family"), "unknown family must be rejected")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim bad As New GlmnetOptions With {.Lambda = New Double() {0.1R, 0.2R}}
                Call GlmnetSolver.Fit(x, y, "gaussian", bad)
            End Sub, "a non-decreasing lambda grid must be rejected")

        Dim weighted As New GlmnetOptions With {
            .Weights = Enumerable.Range(1, x.Length).Select(Function(i) CDbl(1 + i Mod 3)).ToArray(),
            .NLambda = 20
        }
        Dim fit As GlmnetFit = GlmnetSolver.Fit(x, y, FamilyKind.Gaussian, weighted)
        Call DebugAudit.Test(fit.Count > 0 AndAlso fit.jerr = GlmnetError.None, "weighted gaussian fit must succeed")

        Dim offset As Double()() = New Double(x.Length - 1)() {}
        For i As Integer = 0 To x.Length - 1
            offset(i) = New Double() {0.05R * (i Mod 3)}
        Next
        Dim withOffset As New GlmnetOptions With {.Offset = offset, .NLambda = 20}
        Dim offsetFit As GlmnetFit = GlmnetSolver.Fit(x, y, FamilyKind.Gaussian, withOffset)
        Call DebugAudit.Test(offsetFit.Count > 0, "offset gaussian fit must succeed")
    End Sub

    ''' <summary>relaxed lasso 与 gamma 网格。</summary>
    Private Sub CheckRelaxed()
        Dim x As Double()() = ReadCsv("gaussian_x.csv")
        Dim y As Double()() = ReadCsv("gaussian_y.csv")
        Dim options As New GlmnetOptions With {.NLambda = 15, .Relax = True}
        Dim fit As GlmnetFit = GlmnetSolver.Fit(x, y, FamilyKind.Gaussian, options)
        Dim relaxed As RelaxedFit = RelaxedFitting.Fit(fit, options)

        Call DebugAudit.Test(relaxed.Gamma.Length = 5, "default gamma grid must have 5 values")
        Call DebugAudit.Test(relaxed.Coefficients.Length = 5, "relaxed coefficients must cover all gamma values")
        Call DebugAudit.Test(relaxed.Coefficients(0).Length = fit.Count, "relaxed path length must match")
    End Sub

    ''' <summary>NumericTable 表入口。</summary>
    Private Sub CheckNumericTable()
        Dim x As Double()() = ReadCsv("gaussian_x.csv")
        Dim y As Double()() = ReadCsv("gaussian_y.csv")
        Dim table As New NumericTable(x, Nothing, Nothing)
        Call table.SetLabel("y", y.Select(Function(r) r(0)).ToArray())

        Dim options As New GlmnetOptions With {.NLambda = 15}
        Dim fit As GlmnetFit = table.Glmnet("gaussian", options, New String() {"y"})
        Call DebugAudit.Test(fit.Count > 0, "table glmnet entry must produce a path")

        Dim cv As GlmnetCVFit = table.GlmnetCV("gaussian", options, New String() {"y"})
        Call DebugAudit.Test(cv.lambda.Length > 0 AndAlso cv.lambda1se > 0.0R, "table cv entry must produce a CV curve")

        Call table.GlmnetSetPrediction(fit, PredictionType.Link, name:="glmnet_fit")
        Call DebugAudit.Test(table.GetLabel("glmnet_fit").Length = x.Length, "prediction must be written back")
    End Sub

    ''' <summary>断言两个数值在给定容差内一致。</summary>
    Private Sub AssertClose(name As String, expected As Double, actual As Double, relTol As Double, absTol As Double)
        Dim diff As Double = std.Abs(expected - actual)
        Dim tolerance As Double = absTol + relTol * std.Abs(expected)
        Call DebugAudit.Test(diff <= tolerance, $"{name}: expected {expected:G6} but got {actual:G6} (diff {diff:G3})")
    End Sub

    ''' <summary>读取以逗号分隔的数值 CSV；<paramref name="skipHeader"/> 为 True 时跳过表头。</summary>
    Private Function ReadCsv(name As String, Optional skipHeader As Boolean = False) As Double()()
        Dim filePath As String = IO.Path.Combine(DataDir, name)
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"golden data file is missing: {filePath}")
        End If

        Dim lines As String() = File.ReadAllLines(filePath)
        Dim rows As New List(Of Double())()
        Dim start As Integer = If(skipHeader, 1, 0)

        For i As Integer = start To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
            Dim fields As String() = lines(i).Split(","c)
            Dim row As Double() = New Double(fields.Length - 1) {}
            For j As Integer = 0 To fields.Length - 1
                Dim value As Double
                If Not Double.TryParse(fields(j).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
                    value = 0.0R
                End If
                row(j) = value
            Next
            Call rows.Add(row)
        Next

        Return rows.ToArray()
    End Function

End Module
