Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data

''' <summary>
''' glmnet 的 <c>NumericTable</c> 二维表入口。
''' </summary>
''' <remarks>
''' 与 <c>NumericTableElasticNet.vb</c> 的约定一致：自变量取表的全部特征列，
''' 响应由 <c>y</c> 指定的标签列给出（缺省为第一个标签列）；
''' mgaussian 使用全部标签列，cox 使用 <c>time</c> / <c>status</c> 两个标签列。
''' 入口必须留在本程序集，避免 DataFittings 反向引用形成循环依赖。
''' </remarks>
Public Module NumericTableGLMNet

    ''' <summary>对二维表执行 glmnet 拟合。</summary>
    <Extension>
    Public Function Glmnet(table As NumericTable,
                           family As String,
                           Optional options As GlmnetOptions = Nothing,
                           Optional y As String() = Nothing) As GlmnetFit

        Dim kind As FamilyKind = FamilyRegistry.Parse(family)
        Return GlmnetSolver.Fit(FeatureValues(table, NameOf(Glmnet)),
                                ResponseValues(table, y, kind, NameOf(Glmnet)),
                                kind, PrepareOptions(options, table))
    End Function

    ''' <summary>对二维表执行 cv.glmnet。</summary>
    <Extension>
    Public Function GlmnetCV(table As NumericTable,
                             family As String,
                             Optional options As GlmnetOptions = Nothing,
                             Optional y As String() = Nothing) As GlmnetCVFit

        Dim kind As FamilyKind = FamilyRegistry.Parse(family)
        Return GlmnetSolver.CrossValidate(FeatureValues(table, NameOf(GlmnetCV)),
                                          ResponseValues(table, y, kind, NameOf(GlmnetCV)),
                                          kind, PrepareOptions(options, table))
    End Function

    ''' <summary>把拟合结果的预测写回二维表（新增一个标签列）。</summary>
    <Extension>
    Public Function GlmnetSetPrediction(table As NumericTable,
                                        fit As GlmnetFit,
                                        Optional type As PredictionType = PredictionType.Link,
                                        Optional s As Double? = Nothing,
                                        Optional name As String = "glmnet_fit") As NumericTable

        Dim bundle As PredictionBundle = GlmnetPredict.Predict(
            fit, FeatureValues(table, NameOf(GlmnetSetPrediction)), type, s)

        If bundle.Values Is Nothing Then
            Throw New ArgumentException("only link/response predictions can be written back to a table!", NameOf(type))
        End If

        Dim column As Double() = New Double(bundle.Values.Length - 1) {}
        For i As Integer = 0 To bundle.Values.Length - 1
            column(i) = bundle.Values(i)(0)
        Next

        Call table.SetLabel(name, column)
        Return table
    End Function

    ''' <summary>复制参数对象并补齐默认列名。</summary>
    Private Function PrepareOptions(options As GlmnetOptions, table As NumericTable) As GlmnetOptions
        Dim opts As GlmnetOptions = If(options, New GlmnetOptions()).Clone()

        If opts.FeatureNames Is Nothing AndAlso table.featureNames IsNot Nothing AndAlso table.featureNames.Length = table.nfeatures Then
            opts.FeatureNames = table.featureNames
        End If

        Return opts
    End Function

    ''' <summary>读取全部特征列。</summary>
    Private Function FeatureValues(table As NumericTable, caller As String) As Double()()
        If table Is Nothing Then Throw New ArgumentNullException(NameOf(table))
        If table.nsamples = 0 OrElse table.nfeatures = 0 Then
            Throw New ArgumentException($"the '{caller}' requires a non-empty feature matrix!", NameOf(table))
        End If
        Return table.NumericRows
    End Function

    ''' <summary>按 family 读取响应矩阵。</summary>
    Private Function ResponseValues(table As NumericTable, y As String(),
                                     family As FamilyKind, caller As String) As Double()()
        If table.labelNames Is Nothing OrElse table.labelNames.Length = 0 Then
            Throw New ArgumentException($"the '{caller}' requires at least one label column!", NameOf(table))
        End If

        Dim names As String()

        Select Case family
            Case FamilyKind.Cox
                If y IsNot Nothing AndAlso y.Length >= 2 Then
                    names = New String() {y(0), y(1)}
                Else
                    names = New String() {"time", "status"}
                End If
            Case FamilyKind.MGaussian
                names = If(y, table.labelNames)
            Case Else
                names = New String() {If(y IsNot Nothing AndAlso y.Length > 0, y(0), table.labelNames(0))}
        End Select

        Dim n As Integer = table.nsamples
        Dim k As Integer = names.Length
        Dim matrix As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            matrix(i) = New Double(k - 1) {}
        Next
        For c As Integer = 0 To k - 1
            Dim column As Double() = table.GetLabel(names(c))
            For i As Integer = 0 To n - 1
                matrix(i)(c) = column(i)
            Next
        Next

        Return matrix
    End Function

End Module
