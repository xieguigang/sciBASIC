Namespace DownSampling.LargestTriangleBucket


	''' <summary>
	''' Weight = Area of triangle (point A: the previous event, point B: this event; point C: the next event)
	''' </summary>
	Public Class LTOneBucketWeightCalculator
		Implements LTWeightCalculator

		Public Overridable Sub calcWeight(triangle As Triangle, buckets As IList(Of LTWeightedBucket)) Implements LTWeightCalculator.calcWeight
			For Each bucket As LTWeightedBucket In buckets
				For Each ITimeSignal As WeightedEvent In bucket
					triangle.calc(ITimeSignal)
				Next ITimeSignal
			Next bucket
		End Sub

	End Class

End Namespace