Namespace DownSampling


	Public Interface BucketFactory(Of B As Bucket)

		Function newBucket() As B

		Function newBucket(size As Integer) As B

		Function newBucket(e As [Event]) As B

	End Interface

End Namespace