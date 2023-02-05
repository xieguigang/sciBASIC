Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder

Namespace SVM

    Public NotInheritable Class LibSVM

        Private Sub New()
        End Sub

        Public Shared Function getSvmModel(problem As Problem, par As Parameter) As SVMModel
            Dim transform As RangeTransform = RangeTransform.Compute(problem)
            Dim scale = transform.Scale(problem)
            Dim model As SVM.Model = Training.Train(scale, par)

            Call Logging.flush()

            Return New SVMModel With {
                .transform = transform,
                .model = model,
                .factors = New ClassEncoder(problem.Y)
            }
        End Function
    End Class
End Namespace