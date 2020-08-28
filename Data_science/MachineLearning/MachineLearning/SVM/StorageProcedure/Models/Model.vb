Namespace SVM.StorageProcedure

    Public Class RangeTransformModel

        Public Property inputStart As Double()
        Public Property inputScale As Double()
        Public Property outputStart As Double
        Public Property outputScale As Double
        Public Property length As Integer

        Public Function GetTransform() As IRangeTransform
            Return New RangeTransform(inputStart, inputScale, outputStart, outputScale, length)
        End Function

    End Class

    Public Class GaussianTransformModel

        Public Property means As Double()
        Public Property stddevs As Double()

        Public Function GetTransform() As IRangeTransform
            Return New GaussianTransform(means, stddevs)
        End Function

    End Class
End Namespace