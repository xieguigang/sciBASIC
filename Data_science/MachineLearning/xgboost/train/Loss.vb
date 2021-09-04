Namespace train
    Public Class Loss
        Public Overridable Function grad(ByVal pred As Double(), ByVal label As Double()) As Double()
            Throw New NotImplementedException()
        End Function

        Public Overridable Function hess(ByVal pred As Double(), ByVal label As Double()) As Double()
            Throw New NotImplementedException()
        End Function

        Public Overridable Function transform(ByVal pred As Double()) As Double()
            Throw New NotImplementedException()
        End Function
    End Class

    Friend Class SquareLoss
        Inherits Loss

        Public Overrides Function transform(ByVal pred As Double()) As Double()
            Return pred
        End Function

        Public Overrides Function grad(ByVal pred As Double(), ByVal label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i = 0 To ret.Length - 1
                ret(i) = pred(i) - label(i)
            Next

            Return ret
        End Function

        Public Overrides Function hess(ByVal pred As Double(), ByVal label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}
            Arrays.fill(ret, 1.0)
            Return ret
        End Function
    End Class

    Friend Class LogisticLoss
        Inherits Loss

        Private Function clip(ByVal val As Double) As Double
            If val < 0.00001 Then
                Return 0.00001
            ElseIf val > 0.99999 Then
                Return 0.99999
            Else
                Return val
            End If
        End Function

        Public Overrides Function transform(ByVal pred As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i = 0 To ret.Length - 1
                ret(i) = clip(1.0 / (1.0 + Math.Exp(-pred(i))))
            Next

            Return ret
        End Function

        Public Overrides Function grad(ByVal pred As Double(), ByVal label As Double()) As Double()
            Dim pred1 = transform(pred)
            Dim ret = New Double(pred1.Length - 1) {}

            For i = 0 To ret.Length - 1
                ret(i) = pred1(i) - label(i)
            Next

            Return ret
        End Function

        Public Overrides Function hess(ByVal pred As Double(), ByVal label As Double()) As Double()
            Dim pred1 = transform(pred)
            Dim ret = New Double(pred.Length - 1) {}

            For i = 0 To ret.Length - 1
                ret(i) = pred1(i) * (1.0 - pred1(i))
            Next

            Return ret
        End Function
    End Class
End Namespace
