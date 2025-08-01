Namespace DownSampling.LargestTriangleBucket

    ''' <summary>
    ''' A builder class for LT Algorithms.
    ''' </summary>
    Public Class LTABuilder

        Public Shared ReadOnly S_FIXED As New FixedNumBucketSplitter(Of LTWeightedBucket, WeightedEvent)()
        Public Shared ReadOnly S_DYNAMIC As New LTDynamicBucketSplitter()
        Public Shared ReadOnly ONE_BUCKET As New LTOneBucketWeightCalculator()
        Public Shared ReadOnly THREE_BUCKET As New LTThreeBucketWeightCalculator()

        Private lta As LTAlgorithm

        Public Sub New()
            lta = New LTAlgorithm()
            lta.BucketFactory(New LTWeightedBucketFactory)
        End Sub

        Public Overridable Function fixed() As LTABuilder
            lta.SetSpliter(S_FIXED)
            Return Me
        End Function

        Public Overridable Function dynamic() As LTABuilder
            lta.SetSpliter(S_DYNAMIC)
            Return Me
        End Function

        Public Overridable Function oneBucket() As LTABuilder
            lta.Wcalc = ONE_BUCKET
            Return Me
        End Function

        Public Overridable Function threeBucket() As LTABuilder
            lta.Wcalc = THREE_BUCKET
            Return Me
        End Function

        Public Overridable Function build() As LTAlgorithm
            Return lta
        End Function

    End Class

End Namespace