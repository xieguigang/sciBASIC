Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow

Public Module nn

    Public Function sigmoid_cross_entropy_with_logits(Optional _sentinel As Object = Nothing,
                                                      Optional labels As Vector = Nothing,
                                                      Optional logits As Vector = Nothing,
                                                      Optional name As String = Nothing) As Vector

        Dim x = logits, z = labels
        Dim logistic_loss = x - x * z + tf.log(1 + tf.exp(-x))

        Return logistic_loss
    End Function

    Public Function log_softmax(
    logits As Vector,
  Optional axis As Integer = Nothing,
  Optional name As String = Nothing,
  Optional [dim] As Object = Nothing
)
        Return logits - std.Log(tf.reduce_sum(tf.exp(logits)))
    End Function
End Module
