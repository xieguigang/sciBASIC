Imports std = System.Math

<HideModuleName> Public Module Extensions

    ''' <summary>
    ''' 对数求和指数 (log-sum-exp) - 数值稳定
    ''' </summary>
    Public Function LogSumExp(values As Double()) As Double
        Dim maxVal = Double.NegativeInfinity
        For Each v In values
            If v > maxVal Then maxVal = v
        Next

        If Double.IsNegativeInfinity(maxVal) Then Return Double.NegativeInfinity

        Dim sumExp As Double = 0
        For Each v In values
            sumExp += std.Exp(v - maxVal)
        Next
        Return maxVal + std.Log(sumExp)
    End Function

    ''' <summary>
    ''' Softmax 函数 (1D)
    ''' </summary>
    Public Function Softmax(logits As Double()) As Double()
        Dim n = logits.Length
        Dim maxVal = Double.NegativeInfinity
        For Each v In logits
            If v > maxVal Then maxVal = v
        Next

        Dim result = New Double(n - 1) {}
        Dim sumExp As Double = 0
        For i = 0 To n - 1
            result(i) = std.Exp(logits(i) - maxVal)
            sumExp += result(i)
        Next
        For i = 0 To n - 1
            result(i) /= sumExp
        Next
        Return result
    End Function

    ''' <summary>
    ''' Sigmoid 函数
    ''' </summary>
    Public Function Sigmoid(x As Double) As Double
        If x >= 0 Then
            Return 1.0 / (1.0 + std.Exp(-x))
        Else
            Dim ex = std.Exp(x)
            Return ex / (1.0 + ex)
        End If
    End Function
End Module
