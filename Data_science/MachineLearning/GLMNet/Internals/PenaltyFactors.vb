''' <summary>
''' 变量级惩罚因子（对应 R 的 <c>penalty.factor</c> 与 <c>exclude</c>）。
''' </summary>
''' <remarks>
''' glmnet 会把惩罚因子重缩放至和为 p；<c>pf(j) = 0</c> 表示该变量永不惩罚、
''' 始终保留在模型中；<c>exclude</c> 中的变量则完全不进入模型。
''' </remarks>
Friend NotInheritable Class PenaltyFactors

    ''' <summary>重缩放后的惩罚乘子（和为 p，0 保持为 0）</summary>
    Public ReadOnly Property Values As Double()
    ''' <summary>变量是否允许进入模型</summary>
    Public ReadOnly Property Active As Boolean()
    ''' <summary>变量是否永不惩罚（pf = 0）</summary>
    Public ReadOnly Property NeverPenalize As Boolean()
    ''' <summary>变量数</summary>
    Public ReadOnly Property P As Integer

    Public Sub New(raw As Double(), p As Integer, exclude As Integer())
        Me.P = p
        Values = New Double(p - 1) {}
        Active = New Boolean(p - 1) {}
        NeverPenalize = New Boolean(p - 1) {}

        Dim base As Double() = If(raw, Enumerable.Repeat(1.0R, p).ToArray())
        Dim total As Double = 0.0R

        For j As Integer = 0 To p - 1
            Values(j) = base(j)
            total += base(j)
            Active(j) = True
        Next

        If total > 0.0R Then
            Dim rescale As Double = CDbl(p) / total

            For j As Integer = 0 To p - 1
                Values(j) *= rescale
            Next
        End If

        For j As Integer = 0 To p - 1
            NeverPenalize(j) = (base(j) <= 0.0R)
            If NeverPenalize(j) Then
                Values(j) = 0.0R
            End If
        Next

        If exclude IsNot Nothing Then
            For Each j As Integer In exclude
                Active(j) = False
                Values(j) = 0.0R
            Next
        End If
    End Sub

    ''' <summary>该变量的有效 L1 惩罚系数（永不惩罚时返回 0）。</summary>
    Public Function L1(lambda As Double, alpha As Double, j As Integer) As Double
        Return lambda * alpha * Values(j)
    End Function

    ''' <summary>该变量是否可以进入坐标下降的活跃集。</summary>
    Public Function CanEnter(j As Integer) As Boolean
        Return Active(j)
    End Function

    ''' <summary>至少有一个变量参与 L1 惩罚。</summary>
    Public Function HasPenalty() As Boolean
        For j As Integer = 0 To P - 1
            If Active(j) AndAlso Values(j) > 0.0R Then
                Return True
            End If
        Next

        Return False
    End Function

End Class
