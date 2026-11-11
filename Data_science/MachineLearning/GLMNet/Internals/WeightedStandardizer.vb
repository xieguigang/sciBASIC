Imports std = System.Math

''' <summary>
''' 加权标准化：把设计矩阵列主序化、中心化并（可选）按加权总体标准差缩放。
''' </summary>
''' <remarks>
''' <para>
''' glmnet 使用 <b>加权总体方差</b>（除以 <c>sum(w) = n</c>，而不是 n−1），
''' 因此标准化后每列满足 <c>sum_i w_i*x_ij^2/n = 1</c>。既有
''' <c>ElasticNet</c> 用总体标准差、<c>Ridge</c> 用样本标准差，
''' 二者差异由本类统一消除。
''' </para>
''' <para>
''' 系数在标准化空间求解，输出前用
''' <c>beta_orig = beta_std / scale</c>、
''' <c>a0_orig = a0_std - sum(beta_orig * mean)</c> 还原到原始量纲。
''' </para>
''' </remarks>
Friend NotInheritable Class WeightedStandardizer

    ''' <summary>特征数</summary>
    Public ReadOnly Property P As Integer
    ''' <summary>每一列的加权均值</summary>
    Public ReadOnly Property Means As Double()
    ''' <summary>每一列的加权标准差（不标准化时为 1，常数列为 1）</summary>
    Public ReadOnly Property Scales As Double()
    ''' <summary>标准化后的列主序矩阵，<c>Columns(j)(i)</c> 为第 j 列第 i 个样本</summary>
    Public ReadOnly Property Columns As Double()()
    ''' <summary>每列的加权曲率 <c>sum_i w_i*x_ij^2/n</c></summary>
    Public ReadOnly Property Curvature As Double()
    ''' <summary>是否真的执行了标准化</summary>
    Public ReadOnly Property Applied As Boolean

    ''' <param name="x">行主序设计矩阵 <c>x(i)(j)</c></param>
    ''' <param name="weights">已归一化到和为样本数的观测权重</param>
    ''' <param name="standardize">是否缩放；False 时只做中心化</param>
    Public Sub New(x As Double()(), weights As Double(), standardize As Boolean)
        Dim n As Integer = x.Length
        Dim p As Integer = x(0).Length

        P = p
        Applied = standardize
        Means = New Double(p - 1) {}
        Scales = New Double(p - 1) {}
        Curvature = New Double(p - 1) {}
        Columns = New Double(p - 1)() {}

        For j As Integer = 0 To p - 1
            Dim col As Double() = New Double(n - 1) {}
            Dim sum As Double = 0.0R

            For i As Integer = 0 To n - 1
                col(i) = x(i)(j)
                sum += weights(i) * col(i)
            Next

            Dim mean As Double = sum / CDbl(n)
            Dim scale As Double = 1.0R

            If standardize Then
                Dim variance As Double = 0.0R

                For i As Integer = 0 To n - 1
                    Dim d As Double = col(i) - mean
                    variance += weights(i) * d * d
                Next

                variance /= CDbl(n)
                If variance > GlmnetControl.MinCurvature Then
                    scale = std.Sqrt(variance)
                End If
            End If

            Dim inv As Double = 1.0R / scale
            Dim curvature As Double = 0.0R

            For i As Integer = 0 To n - 1
                col(i) = (col(i) - mean) * inv
                curvature += weights(i) * col(i) * col(i)
            Next

            Means(j) = mean
            Scales(j) = scale
            Curvature(j) = curvature / CDbl(n)
            Columns(j) = col
        Next
    End Sub

    ''' <summary>把标准化空间的系数还原到原始量纲。</summary>
    Public Function RestoreCoefficients(betaStd As Double()) As Double()
        Dim beta As Double() = New Double(P - 1) {}

        For j As Integer = 0 To P - 1
            beta(j) = betaStd(j) / Scales(j)
        Next

        Return beta
    End Function

    ''' <summary>把标准化空间的截距还原到原始量纲。</summary>
    Public Function RestoreIntercept(a0Std As Double, betaStd As Double()) As Double
        Dim a0 As Double = a0Std

        For j As Integer = 0 To P - 1
            a0 -= (betaStd(j) / Scales(j)) * Means(j)
        Next

        Return a0
    End Function

End Class
