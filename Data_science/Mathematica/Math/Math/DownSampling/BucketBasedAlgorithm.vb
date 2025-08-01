Namespace DownSampling



	''' <summary>
	''' General algorithm using buckets to downsample events:<br />
	''' <ul>
	''' <li>Prepare data.</li>
	''' <li>Split events into buckets.</li>
	''' <li>Calculate weight of events.</li>
	''' <li>Select significant events from each bucket.</li>
	''' </ul>
	''' </summary>
	''' <typeparam name="B"> Bucket class </typeparam>
	''' <typeparam name="E"> Event class </typeparam>
	Public MustInherit Class BucketBasedAlgorithm(Of B As Bucket, E As [Event])
		Implements DownSamplingAlgorithm

		Protected Friend spliter_Conflict As BucketSplitter(Of B, E)

		Protected Friend factory As BucketFactory(Of B)

		''' <summary>
		''' initialize data for down sampling
		''' </summary>
		Protected Friend MustOverride Function prepare(data As IList(Of [Event])) As IList(Of E)

		''' <summary>
		''' calculating weight or something else
		''' </summary>
		Protected Friend MustOverride Sub beforeSelect(buckets As IList(Of B), threshold As Integer)

		Public Overridable Function process(events As IList(Of [Event]), threshold As Integer) As IList(Of [Event]) Implements DownSamplingAlgorithm.process

			Dim dataSize As Integer = events.Count
			If threshold >= dataSize OrElse dataSize < 3 Then
				Return events
			End If

			Dim preparedData As IList(Of E) = prepare(events)

			Dim buckets As IList(Of B) = spliter_Conflict.split(factory, preparedData, threshold)

			' calculating weight or something else
			beforeSelect(buckets, threshold)

			Dim result As IList(Of [Event]) = New List(Of [Event])(threshold)

			' select from every bucket
			For Each bucket As Bucket In buckets
				bucket.selectInto(result)
			Next bucket
			Return result
		End Function

		Public Sub Spliter(value As BucketSplitter(Of B, E))
			Me.spliter_Conflict = value
		End Sub

		Public Overridable WriteOnly Property BucketFactory As BucketFactory(Of B)
			Set(factory As BucketFactory(Of B))
				Me.factory = factory
			End Set
		End Property

	End Class

End Namespace