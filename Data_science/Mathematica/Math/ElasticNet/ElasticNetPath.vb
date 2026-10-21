''' <summary>
''' 弹性网络在一条 alpha 正则化路径上面的全部解（正则化路径 / 系数路径）
''' 
''' 路径上面的每一步都由 <see cref="Alphas"/> 之中的一个 alpha 值、
''' <see cref="Intercepts"/> 之中的一个截距与 <see cref="Coefficients"/>
''' 之中的一组回归系数组成；alpha 由大到小排列，因此路径的第一步是最稀疏的
''' （最大 alpha 之下所有系数为零），最后一步则最接近无惩罚的最小二乘解。
''' 
''' 路径求解使用了 warm start（以上一步的解作为下一步的初值），
''' 这是坐标下降能够快速扫完整条路径的关键。
''' </summary>
Public Class ElasticNetPath

    ''' <summary>
    ''' 整条路径所使用的 L1 混合比例
    ''' </summary>
    ''' <returns></returns>
    Public Property L1Ratio As Double

    ''' <summary>
    ''' 路径上面每一个步骤的 alpha 值（由大到小）
    ''' </summary>
    ''' <returns></returns>
    Public Property Alphas As Double()

    ''' <summary>
    ''' 路径上面每一个步骤的截距（已经还原到原始量纲）
    ''' </summary>
    ''' <returns></returns>
    Public Property Intercepts As Double()

    ''' <summary>
    ''' 路径上面每一个步骤的回归系数（已经还原到原始量纲），
    ''' 每一行对应一个 alpha、每一列对应一个特征
    ''' </summary>
    ''' <returns></returns>
    Public Property Coefficients As Double()()

    ''' <summary>
    ''' 路径上面每一个步骤的 R2
    ''' </summary>
    ''' <returns></returns>
    Public Property RSquared As Double()

    ''' <summary>
    ''' 路径上面每一个步骤的非零系数个数
    ''' </summary>
    ''' <returns></returns>
    Public Property NonZeroCounts As Integer()

    ''' <summary>
    ''' 特征列的名称
    ''' </summary>
    ''' <returns></returns>
    Public Property FeatureNames As String()

    ''' <summary>
    ''' 是否因为非零系数个数超出上限而提前结束了路径求解。
    ''' 
    ''' 为 <c>True</c> 的时候 <see cref="Count"/> 会小于请求的步数，
    ''' 但 alpha 网格与系数数组是同步截断的，因此仍然一一对应。
    ''' </summary>
    ''' <returns></returns>
    Public Property StoppedEarly As Boolean

    ''' <summary>
    ''' 路径的步数
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Count As Integer
        Get
            If Alphas Is Nothing Then
                Return 0
            End If

            Return Alphas.Length
        End Get
    End Property

    ''' <summary>
    ''' 取出路径上面指定步骤的回归系数
    ''' </summary>
    ''' <param name="idx">步骤下标，即 <see cref="Alphas"/> 的下标</param>
    ''' <returns>该 alpha 之下的回归系数向量（原始量纲）</returns>
    Public Function getCoefficients(idx As Integer) As Double()
        If Coefficients Is Nothing OrElse Coefficients.Length = 0 Then
            Return New Double() {}
        End If
        If idx < 0 OrElse idx >= Coefficients.Length Then
            Throw New ArgumentOutOfRangeException(NameOf(idx))
        End If

        Return Coefficients(idx)
    End Function

    ''' <summary>
    ''' 将正则化路径导出为统一的二维表对象 <see cref="NumericTable"/>：
    ''' 
    ''' 1. 每一行对应路径上面的一个步骤
    ''' 2. 表的第一列是模型的截距，其余的特征列是各个变量的回归系数
    ''' 3. 标签矩阵之中附带 ``alphas``、``rsquared`` 与 ``nonzero`` 这几个路径指标
    ''' </summary>
    ''' <param name="featureNames">
    ''' 特征列的名称。为空或者数量不匹配的时候自动生成 ``[1]..[n]`` 序号
    ''' </param>
    ''' <returns></returns>
    Public Function toNumericTable(Optional featureNames As String() = Nothing) As NumericTable
        Dim names As String() = If(featureNames, Me.FeatureNames)
        Dim p As Integer = If(Coefficients Is Nothing OrElse Coefficients.Length = 0, 0, Coefficients(0).Length)

        If names Is Nothing OrElse names.Length <> p Then
            names = Enumerable.Range(1, p).Select(Function(i) $"[{i}]").ToArray
        End If

        Dim steps As Integer = Count
        Dim header As String() = New String(names.Length) {}
        Dim matrix As Double()() = New Double(steps - 1)() {}
        ' 注意：局部变量不能叫 alphas —— VB 大小写不敏感，会与属性 Alphas 撞名
        Dim alphaColumn As Double() = New Double(steps - 1) {}
        Dim rsq As Double() = New Double(steps - 1) {}
        Dim nonzero As Double() = New Double(steps - 1) {}

        header(0) = "intercept"

        For j As Integer = 0 To names.Length - 1
            header(j + 1) = names(j)
        Next

        For i As Integer = 0 To steps - 1
            Dim w As Double() = getCoefficients(i)
            Dim row As Double() = New Double(names.Length) {}

            row(0) = Intercepts(i)

            For j As Integer = 0 To names.Length - 1
                row(j + 1) = If(j < w.Length, w(j), 0.0)
            Next

            matrix(i) = row
            alphaColumn(i) = Alphas(i)
            rsq(i) = RSquared(i)
            nonzero(i) = CDbl(NonZeroCounts(i))
        Next

        Dim rowNames As String() = Enumerable.Range(1, steps).Select(Function(i) $"alpha_{i}").ToArray
        Dim table As New NumericTable(matrix, rowNames, header)

        Call table.SetLabel("alphas", alphaColumn)
        Call table.SetLabel("rsquared", rsq)
        Call table.SetLabel("nonzero", nonzero)

        Return table
    End Function

    Public Overrides Function ToString() As String
        Dim summary As String = $"elastic net path: l1_ratio = {L1Ratio:G4}, {Count} step(s)"

        If Count = 0 Then
            Return summary
        End If

        Return summary & $", alpha from {Alphas(0):G4} to {Alphas(Count - 1):G4}"
    End Function
End Class
