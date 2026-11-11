Imports Microsoft.VisualBasic.Data
Imports std = System.Math

''' <summary>
''' 统一的 glmnet 拟合结果（对应 R 端 <c>"glmnetfit"</c> 对象）。
''' </summary>
''' <remarks>
''' 六个 family 都返回同一形态：截距 <c>a0(step)(response)</c>、
''' 系数 <c>beta(step)(feature)(response)</c>、lambda 网格、df、dev.ratio、
''' nulldev 与收敛状态。系数一律为<b>原始量纲</b>，可直接与 R 的 <c>coef()</c> 对拍。
''' </remarks>
Public NotInheritable Class GlmnetFit

    ''' <summary>内核输出的原始路径（供适配器与 exact 重拟合使用）</summary>
    Friend Property Path As FamilyPath

    ''' <summary><c>relax = TRUE</c> 时的 relaxed lasso 结果；未开启时为空。</summary>
    Public Property RelaxedModel As RelaxedFit

    ''' <summary>模型家族</summary>
    Public ReadOnly Property Family As FamilyKind
        Get
            Return Path.Family
        End Get
    End Property

    ''' <summary>每一步的截距，<c>a0(step)(response)</c></summary>
    Public ReadOnly Property a0 As Double()()
        Get
            Return Path.A0
        End Get
    End Property

    ''' <summary>每一步的系数，<c>beta(step)(feature)(response)</c></summary>
    Public ReadOnly Property beta As Double()()()
        Get
            Return Path.Beta
        End Get
    End Property

    ''' <summary>lambda 网格（严格递减）</summary>
    Public ReadOnly Property lambda As Double()
        Get
            Return Path.Lambda
        End Get
    End Property

    ''' <summary>每一步的非零变量数</summary>
    Public ReadOnly Property df As Integer()
        Get
            Return Path.Df
        End Get
    End Property

    ''' <summary>每一步的 <c>1 - deviance/nulldev</c></summary>
    Public ReadOnly Property devRatio As Double()
        Get
            Return Path.DevRatio
        End Get
    End Property

    ''' <summary>零模型偏差</summary>
    Public ReadOnly Property nulldev As Double
        Get
            Return Path.NullDev
        End Get
    End Property

    ''' <summary>每一步的模型偏差</summary>
    Public ReadOnly Property deviance As Double()
        Get
            Return Path.Deviance
        End Get
    End Property

    ''' <summary>每一步的非零系数个数</summary>
    Public ReadOnly Property nzero As Integer()
        Get
            Return Path.NZero
        End Get
    End Property

    ''' <summary>每一步是否收敛</summary>
    Public ReadOnly Property converged As Boolean()
        Get
            Return Path.Converged
        End Get
    End Property

    ''' <summary>每一步实际消耗的坐标下降轮数</summary>
    Public ReadOnly Property iterations As Integer()
        Get
            Return Path.Iterations
        End Get
    End Property

    ''' <summary>R 端 <c>jerr</c> 对应的错误码，0 表示正常</summary>
    Public ReadOnly Property jerr As GlmnetError
        Get
            Return Path.Jerr
        End Get
    End Property

    ''' <summary>样本数</summary>
    Public ReadOnly Property nobs As Integer
        Get
            Return Path.TrainingData.N
        End Get
    End Property

    ''' <summary>特征数</summary>
    Public ReadOnly Property nvars As Integer
        Get
            Return Path.TrainingData.P
        End Get
    End Property

    ''' <summary>
    ''' 系数矩阵的响应维。Cox 的响应是 <c>(time, status)</c> 两列，
    ''' 但系数只有 1 列（风险得分），因此必须以 <c>a0</c> 的宽度为准。
    ''' </summary>
    Public ReadOnly Property nresponses As Integer
        Get
            If Path.A0 Is Nothing OrElse Path.A0.Length = 0 OrElse Path.A0(0) Is Nothing Then
                Return 1
            End If
            Return Path.A0(0).Length
        End Get
    End Property

    ''' <summary>路径步数</summary>
    Public ReadOnly Property Count As Integer
        Get
            Return Path.Count
        End Get
    End Property

    ''' <summary>特征列名</summary>
    Public ReadOnly Property featureNames As String()
        Get
            Return Path.FeatureNames
        End Get
    End Property

    ''' <summary>响应列名</summary>
    Public ReadOnly Property responseNames As String()
        Get
            Return Path.ResponseNames
        End Get
    End Property

    ''' <summary>多分类类别标签</summary>
    Public ReadOnly Property classLabels As String()
        Get
            Return Path.ClassLabels
        End Get
    End Property

    Friend Sub New(path As FamilyPath)
        Me.Path = path
    End Sub

    ''' <summary>
    ''' 取指定 lambda 步骤的 <c>(p+1) x K</c> 系数矩阵（第 0 行为截距）。
    ''' </summary>
    Public Function GetCoefficients(lambdaIndex As Integer) As Double()()
        If lambdaIndex < 0 OrElse lambdaIndex >= Count Then
            Throw New ArgumentOutOfRangeException(NameOf(lambdaIndex))
        End If

        Dim p As Integer = nvars
        Dim k As Integer = nresponses
        Dim result As Double()() = New Double(p)() {}

        For r As Integer = 0 To p
            result(r) = New Double(k - 1) {}
        Next
        For c As Integer = 0 To k - 1
            result(0)(c) = Path.A0(lambdaIndex)(c)
        Next
        For j As Integer = 0 To p - 1
            For c As Integer = 0 To k - 1
                result(j + 1)(c) = Path.Beta(lambdaIndex)(j)(c)
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 在给定 lambda 处取系数；与 R 一样默认在 lambda 尺度上线性插值，
    ''' <c>exact = True</c> 时重新在该 lambda 上拟合并返回精确解。
    ''' </summary>
    ''' <param name="s">lambda 值；为空表示路径最后一步（惩罚最弱）</param>
    ''' <param name="exact">是否重新拟合而不是插值</param>
    Public Function GetCoefficients(Optional s As Double? = Nothing, Optional exact As Boolean = False) As Double()()
        Dim target As Double = If(s.HasValue, s.Value, lambda(Count - 1))

        If exact Then
            Dim options As GlmnetOptions = Path.Options.Clone()
            options.Lambda = New Double() {target}
            options.NLambda = 1
            Dim refit As FamilyPath = FamilyRegistry.Create(Path.Family).Fit(Path.TrainingData, options)

            If refit.Count = 0 Then
                Throw New InvalidOperationException("exact refit produced an empty path")
            End If

            Dim p As Integer = nvars
            Dim k As Integer = nresponses
            Dim result As Double()() = New Double(p)() {}
            For r As Integer = 0 To p
                result(r) = New Double(k - 1) {}
            Next
            For c As Integer = 0 To k - 1
                result(0)(c) = refit.A0(0)(c)
            Next
            For j As Integer = 0 To p - 1
                For c As Integer = 0 To k - 1
                    result(j + 1)(c) = refit.Beta(0)(j)(c)
                Next
            Next
            Return result
        End If

        Dim lowerIndex As Integer
        Dim fraction As Double
        Call ResolveLambda(target, lowerIndex, fraction)

        Dim left As Double()() = GetCoefficients(lowerIndex)
        If fraction <= 0.0R OrElse lowerIndex >= Count - 1 Then
            Return left
        End If

        Dim right As Double()() = GetCoefficients(lowerIndex + 1)
        Dim blended As Double()() = New Double(left.Length - 1)() {}
        For r As Integer = 0 To left.Length - 1
            blended(r) = New Double(left(r).Length - 1) {}
            For c As Integer = 0 To left(r).Length - 1
                blended(r)(c) = (1.0R - fraction) * left(r)(c) + fraction * right(r)(c)
            Next
        Next

        Return blended
    End Function

    ''' <summary>
    ''' 把 lambda 值定位到网格区间：<c>lambda(lower) &gt;= s &gt;= lambda(lower+1)</c>。
    ''' </summary>
    Friend Sub ResolveLambda(s As Double, ByRef lowerIndex As Integer, ByRef fraction As Double)
        If s >= lambda(0) Then
            lowerIndex = 0
            fraction = 0.0R
            Return
        End If
        If s <= lambda(Count - 1) Then
            lowerIndex = Count - 1
            fraction = 0.0R
            Return
        End If

        For k As Integer = 0 To Count - 2
            If lambda(k) >= s AndAlso s >= lambda(k + 1) Then
                lowerIndex = k
                Dim span As Double = lambda(k) - lambda(k + 1)
                fraction = If(span > 0.0R, (lambda(k) - s) / span, 0.0R)
                Return
            End If
        Next

        lowerIndex = Count - 1
        fraction = 0.0R
    End Sub

    ''' <summary>
    ''' 计算线性预测子 <c>eta = a0 + X*beta + offset</c>。
    ''' </summary>
    ''' <param name="newx">待预测特征矩阵，每行 p 列</param>
    ''' <param name="coefficients"><c>(p+1) x K</c> 系数矩阵</param>
    ''' <param name="newoffset">预测期 offset，形状 <c>n x K</c> 或 <c>n x 1</c>；为空则用训练期 offset 的均值 0</param>
    Public Function LinearPredictor(newx As Double()(), coefficients As Double()(), Optional newoffset As Double()() = Nothing) As Double()()
        If newx Is Nothing Then Throw New ArgumentNullException(NameOf(newx))

        Dim p As Integer = nvars
        Dim k As Integer = nresponses
        Dim n As Integer = newx.Length
        Dim result As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            If newx(i) Is Nothing OrElse newx(i).Length < p Then
                Throw New ArgumentException($"prediction row {i} has fewer than {p} features!", NameOf(newx))
            End If

            Dim row As Double() = New Double(k - 1) {}
            For c As Integer = 0 To k - 1
                row(c) = coefficients(0)(c)
            Next
            For j As Integer = 0 To p - 1
                Dim b As Double = newx(i)(j)
                For c As Integer = 0 To k - 1
                    row(c) += coefficients(j + 1)(c) * b
                Next
            Next
            If newoffset IsNot Nothing Then
                Dim src As Double() = newoffset(i)
                For c As Integer = 0 To k - 1
                    row(c) += src(If(src.Length = 1, 0, std.Min(c, src.Length - 1)))
                Next
            End If
            result(i) = row
        Next

        Return result
    End Function

    ''' <summary>把整条路径导出为 <c>NumericTable</c>，每行一个 lambda 步骤。</summary>
    Public Function toNumericTable() As NumericTable
        Dim p As Integer = nvars
        Dim k As Integer = nresponses
        Dim steps As Integer = Count
        Dim header As New List(Of String) From {"(Intercept)"}

        If k = 1 Then
            header.AddRange(featureNames)
        Else
            For c As Integer = 0 To k - 1
                For j As Integer = 0 To p - 1
                    header.Add($"{featureNames(j)}.{responseNames(c)}")
                Next
            Next
        End If

        Dim matrix As Double()() = New Double(steps - 1)() {}
        Dim rowNames As String() = New String(steps - 1) {}
        Dim lambdaColumn As Double() = New Double(steps - 1) {}
        Dim dfColumn As Double() = New Double(steps - 1) {}
        Dim devColumn As Double() = New Double(steps - 1) {}
        Dim nzeroColumn As Double() = New Double(steps - 1) {}

        For s As Integer = 0 To steps - 1
            Dim row As New List(Of Double)()
            For c As Integer = 0 To k - 1
                row.Add(a0(s)(c))
            Next
            For j As Integer = 0 To p - 1
                For c As Integer = 0 To k - 1
                    row.Add(beta(s)(j)(c))
                Next
            Next
            matrix(s) = row.ToArray()
            rowNames(s) = $"s{s}"
            lambdaColumn(s) = lambda(s)
            dfColumn(s) = CDbl(df(s))
            devColumn(s) = devRatio(s)
            nzeroColumn(s) = CDbl(nzero(s))
        Next

        Dim table As New NumericTable(matrix, rowNames, header.ToArray())
        Call table.SetLabel("lambda", lambdaColumn)
        Call table.SetLabel("df", dfColumn)
        Call table.SetLabel("dev.ratio", devColumn)
        Call table.SetLabel("nzero", nzeroColumn)

        Return table
    End Function

    Public Overrides Function ToString() As String
        Return $"glmnet[{FamilyRegistry.Name(Path.Family)}]: {Count} lambda(s) from {lambda(0):G4} to {lambda(Count - 1):G4}, df {df(0)}..{df(Count - 1)}, dev.ratio max = {devRatio.Max():F4}, jerr = {CInt(jerr)}"
    End Function

End Class
