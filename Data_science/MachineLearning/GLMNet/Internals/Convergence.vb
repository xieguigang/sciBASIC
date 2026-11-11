Imports std = System.Math

''' <summary>收敛判据、数值守卫与错误码映射。</summary>
Friend Module Convergence

    ''' <summary>
    ''' glmnet 风格的收敛判据：目标函数的相对改进低于 <paramref name="thresh"/>。
    ''' </summary>
    Public Function HasConverged(previousObjective As Double, objective As Double, thresh As Double) As Boolean
        If Not Double.IsFinite(objective) Then
            Return False
        End If

        Dim scale As Double = std.Max(std.Abs(previousObjective), 1.0R)

        Return std.Abs(previousObjective - objective) / scale <= thresh
    End Function

    ''' <summary>检查一个向量是否全部为有限数。</summary>
    Public Function AllFinite(v As Double()) As Boolean
        For i As Integer = 0 To v.Length - 1
            If Not Double.IsFinite(v(i)) Then
                Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>把线性预测子钳制到 <c>[-MaxEta, MaxEta]</c>，避免指数溢出。</summary>
    Public Function ClampEta(eta As Double) As Double
        If Double.IsNaN(eta) Then
            Return -GlmnetControl.MaxEta
        End If
        If eta > GlmnetControl.MaxEta Then
            Return GlmnetControl.MaxEta
        End If
        If eta < -GlmnetControl.MaxEta Then
            Return -GlmnetControl.MaxEta
        End If

        Return eta
    End Function

    ''' <summary>数值安全的 <c>exp</c>。</summary>
    Public Function SafeExp(eta As Double) As Double
        Return std.Exp(ClampEta(eta))
    End Function

    ''' <summary>数值安全的 <c>log</c>（下界钳制）。</summary>
    Public Function SafeLog(v As Double) As Double
        Return std.Log(std.Max(v, GlmnetControl.MinProbability))
    End Function

    ''' <summary>把任意向量归一化到和为 <paramref name="target"/>。</summary>
    Public Sub NormalizeInplace(v As Double(), target As Double)
        Dim total As Double = 0.0R

        For i As Integer = 0 To v.Length - 1
            total += v(i)
        Next
        If total <= GlmnetControl.MinCurvature Then
            Throw New ArithmeticException("cannot normalize a zero-weight vector")
        End If

        Dim scale As Double = target / total
        For i As Integer = 0 To v.Length - 1
            v(i) *= scale
        Next
    End Sub

End Module
