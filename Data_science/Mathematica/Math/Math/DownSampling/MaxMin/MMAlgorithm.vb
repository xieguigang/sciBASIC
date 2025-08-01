Namespace DownSampling.MaxMin


	''' <summary>
	''' Select events with maximum or minimum value in bucket
	''' </summary>
	Public Class MMAlgorithm
		Inherits BucketBasedAlgorithm(Of MMBucket, [Event])

		Public Sub New()
			BucketFactory = New MMBucketFactory()
			Spliter(New FixedTimeBucketSplitter(Of MMBucket, [Event]))
		End Sub

		Protected Friend Overrides Function prepare(data As IList(Of [Event])) As IList(Of [Event])
			Return data
		End Function

		Protected Friend Overrides Sub beforeSelect(buckets As IList(Of MMBucket), threshold As Integer)

		End Sub

		Public Overrides Function ToString() As String
			Return "MaxMin"
		End Function

	End Class

End Namespace