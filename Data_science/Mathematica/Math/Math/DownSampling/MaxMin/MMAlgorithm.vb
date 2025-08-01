Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.MaxMin

    ''' <summary>
    ''' Select events with maximum or minimum value in bucket
    ''' </summary>
    Public Class MMAlgorithm : Inherits BucketBasedAlgorithm(Of MMBucket, ITimeSignal)

        Public Sub New()
            BucketFactory(New MMBucketFactory)
            SetSpliter(New FixedTimeBucketSplitter(Of MMBucket, ITimeSignal))
        End Sub

        Protected Friend Overrides Function prepare(data As IList(Of ITimeSignal)) As IList(Of ITimeSignal)
            Return data
        End Function

        Protected Friend Overrides Sub beforeSelect(buckets As IList(Of MMBucket), threshold As Integer)

        End Sub

        Public Overrides Function ToString() As String
            Return "MaxMin"
        End Function

    End Class
End Namespace
