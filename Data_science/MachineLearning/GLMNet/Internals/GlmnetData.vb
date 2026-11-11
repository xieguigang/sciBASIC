Imports std = System.Math

''' <summary>
''' glmnet 的统一输入容器：设计矩阵、响应、权重、offset 与列名。
''' </summary>
''' <remarks>
''' <para>
''' 响应 <see cref="Y"/> 一律整理成 <c>n x k</c> 的数值矩阵：
''' gaussian / poisson / binomial 为 1 列；cox 为 2 列 <c>(time, status)</c>；
''' mgaussian 为 K 列；multinomial 会被展开成 K 列 one-hot。
''' </para>
''' <para>
''' 权重在构造时归一化到 <c>sum(w) = n</c>（glmnet 约定），
''' offset 统一补齐为 <c>n x k</c> 的零矩阵以便内核无分支访问。
''' </para>
''' </remarks>
Public NotInheritable Class GlmnetData

    ''' <summary>样本数</summary>
    Public Property N As Integer
    ''' <summary>特征数</summary>
    Public Property P As Integer
    ''' <summary>响应列数</summary>
    Public Property K As Integer
    ''' <summary>行主序设计矩阵 <c>X(i)(j)</c></summary>
    Public Property X As Double()()
    ''' <summary>响应矩阵 <c>Y(i)(c)</c></summary>
    Public Property Y As Double()()
    ''' <summary>归一化到和为 n 的观测权重</summary>
    Public Property Weights As Double()
    ''' <summary>link scale 上的偏移 <c>Offsets(i)(c)</c></summary>
    Public Property Offsets As Double()()
    ''' <summary>特征列名</summary>
    Public Property FeatureNames As String()
    ''' <summary>响应列名</summary>
    Public Property ResponseNames As String()
    ''' <summary>模型家族</summary>
    Public Property Family As FamilyKind
    ''' <summary>多分类的类别标签（仅 multinomial）</summary>
    Public Property ClassLabels As String()
    ''' <summary>多分类的类别数（仅 multinomial，等于 K）</summary>
    Public Property ClassCount As Integer

    ''' <summary>
    ''' 构造并规范化输入数据。
    ''' </summary>
    Public Sub New(x As Double()(), y As Double()(), options As GlmnetOptions, family As FamilyKind)
        If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
        If y Is Nothing Then Throw New ArgumentNullException(NameOf(y))
        If x.Length = 0 OrElse x(0) Is Nothing OrElse x(0).Length = 0 Then
            Throw New ArgumentException("the design matrix must be non-empty!", NameOf(x))
        End If
        If y.Length <> x.Length Then
            Throw New ArgumentException($"x has {x.Length} rows but y has {y.Length} rows!", NameOf(y))
        End If

        Dim n As Integer = x.Length
        Dim p As Integer = x(0).Length
        Dim k As Integer = y(0).Length

        If k = 0 Then
            Throw New ArgumentException("the response matrix must have at least one column!", NameOf(y))
        End If

        For i As Integer = 0 To n - 1
            If x(i) Is Nothing OrElse x(i).Length <> p Then
                Throw New ArgumentException($"row {i} of x has {If(x(i) Is Nothing, 0, x(i).Length)} features but p = {p}!", NameOf(x))
            End If
            If y(i) Is Nothing OrElse y(i).Length <> k Then
                Throw New ArgumentException($"row {i} of y has {If(y(i) Is Nothing, 0, y(i).Length)} columns but k = {k}!", NameOf(y))
            End If
            For j As Integer = 0 To p - 1
                If Not Double.IsFinite(x(i)(j)) Then
                    Throw New ArgumentException($"x({i})({j}) must be finite!", NameOf(x))
                End If
            Next
            For c As Integer = 0 To k - 1
                If Not Double.IsFinite(y(i)(c)) Then
                    Throw New ArgumentException($"y({i})({c}) must be finite!", NameOf(y))
                End If
            Next
        Next

        Me.N = n
        Me.P = p
        Me.Family = family
        Me.X = x

        ' 观测权重：默认全 1，归一化到和为 n
        Dim w As Double() = If(options.Weights Is Nothing, Nothing, CType(options.Weights.Clone(), Double()))
        If w Is Nothing Then
            w = New Double(n - 1) {}
            For i As Integer = 0 To n - 1
                w(i) = 1.0R
            Next
        End If

        ' binomial 的两列矩阵表示 (success, failure)：转成比例并把总次数并入权重
        If family = FamilyKind.Binomial AndAlso k = 2 Then
            Dim proportions As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                Dim total As Double = y(i)(0) + y(i)(1)

                If total <= 0.0R Then
                    Throw New ArgumentException($"the binomial response row {i} has no trials!", NameOf(y))
                End If

                proportions(i) = New Double() {y(i)(0) / total}
                w(i) *= total
            Next

            Me.Y = proportions
            k = 1
        Else
            Me.Y = y
        End If

        ' multinomial：单列类别编码展开成 one-hot
        If family = FamilyKind.Multinomial AndAlso k = 1 Then
            Dim labels As New List(Of Double)()

            For i As Integer = 0 To n - 1
                Dim label As Double = Me.Y(i)(0)
                If label <> std.Floor(label) OrElse label < 0.0R Then
                    Throw New ArgumentException($"the multinomial response row {i} must be a non-negative class index!", NameOf(y))
                End If
                If Not labels.Contains(label) Then
                    labels.Add(label)
                End If
            Next

            labels.Sort()
            Dim nclass As Integer = labels.Count
            If nclass < 2 Then
                Throw New ArgumentException("the multinomial family requires at least two classes!", NameOf(y))
            End If

            Dim onehot As Double()() = New Double(n - 1)() {}
            Dim names As String() = New String(nclass - 1) {}

            For c As Integer = 0 To nclass - 1
                names(c) = $"class_{labels(c):G}"
            Next
            For i As Integer = 0 To n - 1
                onehot(i) = New Double(nclass - 1) {}
                onehot(i)(labels.IndexOf(Me.Y(i)(0))) = 1.0R
            Next

            Me.Y = onehot
            Me.ClassLabels = names
            Me.ClassCount = nclass
            k = nclass
        ElseIf family = FamilyKind.Multinomial Then
            Me.ClassCount = k
            If options.ResponseNames IsNot Nothing Then
                Me.ClassLabels = options.ResponseNames
            Else
                Dim names As String() = New String(k - 1) {}
                For c As Integer = 0 To k - 1
                    names(c) = $"class_{c + 1}"
                Next
                Me.ClassLabels = names
            End If
        End If

        Me.K = k

        Dim sumW As Double = 0.0R
        For i As Integer = 0 To n - 1
            If Not Double.IsFinite(w(i)) OrElse w(i) <= 0.0R Then
                Throw New ArgumentException($"weight({i}) must be a finite positive number!", NameOf(options))
            End If
            sumW += w(i)
        Next
        If sumW <= 0.0R OrElse Not Double.IsFinite(sumW) Then
            Throw New ArgumentException("the sum of observation weights must be positive and finite!", NameOf(options))
        End If
        Dim scale As Double = CDbl(n) / sumW
        For i As Integer = 0 To n - 1
            w(i) *= scale
        Next
        Me.Weights = w

        ' offset 补齐为 n x k；单列 offset 广播到全部响应列
        Dim off As Double()() = New Double(n - 1)() {}
        If options.Offset Is Nothing Then
            For i As Integer = 0 To n - 1
                off(i) = New Double(k - 1) {}
            Next
        Else
            Dim srcK As Integer = options.Offset(0).Length
            For i As Integer = 0 To n - 1
                Dim row As Double() = New Double(k - 1) {}
                For c As Integer = 0 To k - 1
                    row(c) = options.Offset(i)(If(srcK = 1, 0, std.Min(c, srcK - 1)))
                Next
                off(i) = row
            Next
        End If
        Me.Offsets = off

        If options.FeatureNames IsNot Nothing Then
            Me.FeatureNames = options.FeatureNames
        Else
            Dim names As String() = New String(p - 1) {}
            For j As Integer = 0 To p - 1
                names(j) = $"V{j + 1}"
            Next
            Me.FeatureNames = names
        End If

        If options.ResponseNames IsNot Nothing AndAlso options.ResponseNames.Length = k Then
            Me.ResponseNames = options.ResponseNames
        ElseIf family = FamilyKind.Multinomial Then
            Me.ResponseNames = Me.ClassLabels
        Else
            Dim names As String() = New String(k - 1) {}
            For c As Integer = 0 To k - 1
                names(c) = If(family = FamilyKind.Cox AndAlso c = 0, "time", If(family = FamilyKind.Cox AndAlso c = 1, "status", $"y{c + 1}"))
            Next
            Me.ResponseNames = names
        End If
    End Sub

    Private Sub New()
    End Sub

    ''' <summary>取出指定行的子数据（CV 折内拟合用），权重会重新归一化。</summary>
    Public Function Subset(rows As Integer()) As GlmnetData
        Dim m As Integer = rows.Length
        Dim copy As New GlmnetData With {
            .N = m,
            .P = P,
            .K = K,
            .Family = Family,
            .FeatureNames = FeatureNames,
            .ResponseNames = ResponseNames,
            .ClassLabels = ClassLabels,
            .ClassCount = ClassCount,
            .X = New Double(m - 1)() {},
            .Y = New Double(m - 1)() {},
            .Weights = New Double(m - 1) {},
            .Offsets = New Double(m - 1)() {}
        }

        Dim sumW As Double = 0.0R
        For t As Integer = 0 To m - 1
            Dim i As Integer = rows(t)
            copy.X(t) = X(i)
            copy.Y(t) = Y(i)
            copy.Offsets(t) = Offsets(i)
            copy.Weights(t) = Weights(i)
            sumW += Weights(i)
        Next

        Dim scale As Double = CDbl(m) / sumW
        For t As Integer = 0 To m - 1
            copy.Weights(t) *= scale
        Next

        Return copy
    End Function

    ''' <summary>把特征矩阵转为列主序原始值（未标准化），供预测与稀疏转换使用。</summary>
    Public Function RawColumns() As Double()()
        Dim columns As Double()() = New Double(P - 1)() {}

        For j As Integer = 0 To P - 1
            Dim col As Double() = New Double(N - 1) {}
            For i As Integer = 0 To N - 1
                col(i) = X(i)(j)
            Next
            columns(j) = col
        Next

        Return columns
    End Function

End Class
