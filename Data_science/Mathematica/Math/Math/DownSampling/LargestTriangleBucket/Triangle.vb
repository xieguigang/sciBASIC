Imports std = System.Math

Namespace DownSampling.LargestTriangleBucket

	''' <summary>
	''' Calculate a triangle's area
	''' </summary>
	Public Class Triangle

		Private last As WeightedEvent
		Private curr As WeightedEvent
		Private [next] As WeightedEvent

		' S=(1/2)*|x1*(y2-y3) + x2*(y3-y1) + x3*(y1-y2)|
		' S=(1/2)*|y1*(x2-x3) + y2*(x3-x1) + y3*(x1-x2)|
		Private Sub updateWeight()
			If last Is Nothing OrElse curr Is Nothing OrElse [next] Is Nothing Then
				Return
			End If
			Dim dx1 As Double = curr.Time - last.Time
			Dim dx2 As Double = last.Time - [next].Time
			Dim dx3 As Double = [next].Time - curr.Time
			Dim y1 As Double = [next].Value
			Dim y2 As Double = curr.Value
			Dim y3 As Double = last.Value
			Dim s As Double = 0.5 * std.Abs(y1 * dx1 + y2 * dx2 + y3 * dx3)
			curr.Weight = s
		End Sub

		Public Overridable Sub calc(e As WeightedEvent)
			last = curr
			curr = [next]
			[next] = e
			updateWeight()
		End Sub

		Public Overridable Sub calc(last As WeightedEvent, curr As WeightedEvent, [next] As WeightedEvent)
			Me.last = last
			Me.curr = curr
			Me.next = [next]
			updateWeight()
		End Sub

	End Class

End Namespace