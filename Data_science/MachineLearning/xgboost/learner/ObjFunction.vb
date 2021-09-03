Imports stdNum = System.Math

Namespace learner


    ''' <summary>
    ''' Objective function implementations.
    ''' </summary>
    <Serializable>
    Public Class ObjFunction

        Private Shared ReadOnly FUNCTIONS As New Dictionary(Of String, ObjFunction)()

        Shared Sub New()
            Call register("rank:pairwise", New ObjFunction())
            Call register("binary:logistic", New RegLossObjLogistic())
            Call register("binary:logitraw", New ObjFunction())
            Call register("multi:softmax", New SoftmaxMultiClassObjClassify())
            Call register("multi:softprob", New SoftmaxMultiClassObjProb())
            Call register("reg:linear", New ObjFunction())
            Call register("reg:squarederror", New ObjFunction())
            Call register("reg:logistic", New RegLossObjLogistic())
        End Sub

        ''' <summary>
        ''' Gets <seealsocref="ObjFunction"/> from given name.
        ''' </summary>
        ''' <param name="name"> name of objective function </param>
        ''' <returns> objective function </returns>
        Public Shared Function fromName(name As String) As ObjFunction
            Dim result = FUNCTIONS.GetValueOrNull(name)

            If result Is Nothing Then
                Throw New ArgumentException(name & " is not supported objective function.")
            End If

            Return result
        End Function

        ''' <summary>
        ''' Register an <seealsocref="ObjFunction"/> for a given name.
        ''' </summary>
        ''' <param name="name"> name of objective function </param>
        ''' <param name="objFunction"> objective function </param>
        ''' @deprecated This method will be made private. Please use <seealsocref="PredictorConfiguration.BuilderType"/> instead. 
        Public Shared Sub register(name As String, objFunction As ObjFunction)
            FUNCTIONS(name) = objFunction
        End Sub

        ''' <summary>
        ''' Uses Jafama's <seealsocref="FastMath"/> instead of <seealsocref="Math"/>.
        ''' </summary>
        ''' <param name="useJafama"> {@code true} if you want to use Jafama's <seealsocref="FastMath"/>,
        '''                  or {@code false} if you don't want to use it but JDK's <seealsocref="Math"/>. </param>
        Public Shared Sub useFastMathExp()
            Call register("binary:logistic", New RegLossObjLogistic())
            Call register("reg:logistic", New RegLossObjLogistic())
            Call register("multi:softprob", New SoftmaxMultiClassObjProb())
        End Sub

        ''' <summary>
        ''' Transforms prediction values.
        ''' </summary>
        ''' <param name="preds"> prediction </param>
        ''' <returns> transformed values </returns>
        Public Overridable Function predTransform(preds As Double()) As Double()
            ' do nothing
            Return preds
        End Function

        ''' <summary>
        ''' Transforms a prediction value.
        ''' </summary>
        ''' <param name="pred"> prediction </param>
        ''' <returns> transformed value </returns>
        Public Overridable Function predTransform(pred As Double) As Double
            ' do nothing
            Return pred
        End Function

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
    End Class
End Namespace
