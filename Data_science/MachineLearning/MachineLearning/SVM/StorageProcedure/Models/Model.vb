Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM.StorageProcedure

    Public Class RangeTransformModel

        Public Property inputStart As Double()
        Public Property inputScale As Double()
        Public Property outputStart As Double
        Public Property outputScale As Double
        Public Property length As Integer

        Sub New()
        End Sub

        Sub New(range As RangeTransform)
            inputStart = range._inputStart
            inputScale = range._inputScale
            outputStart = range._outputStart
            outputScale = range._outputScale
            length = range._length
        End Sub

        Public Function GetTransform() As IRangeTransform
            Return New RangeTransform(inputStart, inputScale, outputStart, outputScale, length)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

    Public Class GaussianTransformModel

        Public Property means As Double()
        Public Property stddevs As Double()

        Sub New()
        End Sub

        Sub New(gaussian As GaussianTransform)
            means = gaussian._means
            stddevs = gaussian._stddevs
        End Sub

        Public Function GetTransform() As IRangeTransform
            Return New GaussianTransform(means, stddevs)
        End Function

        Public Overrides Function ToString() As String
            Return means.GetJson
        End Function

    End Class
End Namespace