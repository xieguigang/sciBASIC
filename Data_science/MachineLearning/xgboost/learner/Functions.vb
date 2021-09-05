Imports stdNum = System.Math

Namespace learner

    ''' <summary>
    ''' Logistic regression.
    ''' </summary>
    <Serializable>
    Friend Class RegLossObjLogistic
        Inherits ObjFunction

        Public Overloads Overrides Function predTransform(preds As Double()) As Double()
            For i = 0 To preds.Length - 1
                preds(i) = sigmoid(preds(i))
            Next

            Return preds
        End Function

        Public Overloads Overrides Function predTransform(pred As Double) As Double
            Return sigmoid(pred)
        End Function

        Friend Overridable Function sigmoid(x As Double) As Double
            Return 1 / (1 + stdNum.Exp(-x))
        End Function
    End Class

    ''' <summary>
    ''' Multiclass classification.
    ''' </summary>
    <Serializable>
    Friend Class SoftmaxMultiClassObjClassify
        Inherits ObjFunction

        Public Overloads Overrides Function predTransform(preds As Double()) As Double()
            Dim maxIndex = 0
            Dim max = preds(0)

            For i = 1 To preds.Length - 1

                If max < preds(i) Then
                    maxIndex = i
                    max = preds(i)
                End If
            Next

            Return New Double() {maxIndex}
        End Function

        Public Overloads Overrides Function predTransform(pred As Double) As Double
            Throw New NotSupportedException()
        End Function
    End Class

    ''' <summary>
    ''' Multiclass classification (predicted probability).
    ''' </summary>
    <Serializable>
    Friend Class SoftmaxMultiClassObjProb
        Inherits ObjFunction

        Public Overloads Overrides Function predTransform(preds As Double()) As Double()
            Dim max = preds(0)

            For i = 1 To preds.Length - 1
                max = stdNum.Max(preds(i), max)
            Next

            Dim sum As Double = 0

            For i = 0 To preds.Length - 1
                preds(i) = exp(preds(i) - max)
                sum += preds(i)
            Next

            For i = 0 To preds.Length - 1
                preds(i) /= CSng(sum)
            Next

            Return preds
        End Function

        Public Overloads Overrides Function predTransform(pred As Double) As Double
            Throw New NotSupportedException()
        End Function

        Friend Overridable Function exp(x As Double) As Double
            Return stdNum.Exp(x)
        End Function
    End Class
End Namespace