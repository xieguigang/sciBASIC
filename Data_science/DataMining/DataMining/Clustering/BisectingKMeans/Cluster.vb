Imports std = System.Math

Namespace BisectingKMeans

	''' <summary>
	''' Created by touhid on 12/21/15.
	''' 
	''' @author touhid
	''' </summary>
	Public Class Cluster
		Private Const CENTROID_THRESHOLD As Double = 0.005

		Public Sub New(cx As Double, cy As Double)
			Me.Cx = cx
			Me.Cy = cy
			DataPoints = New List(Of DataPoint)()
		End Sub

		Public Sub New(centroid As DataPoint)
			Me.Cx = centroid.Dx
			Me.Cy = centroid.Dy
			DataPoints = New List(Of DataPoint)()
		End Sub

		Public Sub New(cx As Double, cy As Double, dataPoints As List(Of DataPoint))
			Me.Cx = cx
			Me.Cy = cy
			Me.DataPoints = dataPoints
		End Sub

		Public Overridable Property Cx As Double
		Public Overridable Property Cy As Double
		Public Overridable Property DataPoints As List(Of DataPoint)

		Public Overridable Sub addPoint(p As DataPoint)
			Me.DataPoints.Add(p)
		End Sub


		Public Overridable ReadOnly Property SSE As Double
			Get
				Dim sse_d As Double = 0.0
				For Each p As DataPoint In DataPoints
					Dim dx As Double = Cx - p.Dx
					Dim dy As Double = Cy - p.Dy
					sse_d += (dx * dx + dy * dy)
				Next p
				Return sse_d
			End Get
		End Property

		Public Overridable Function getDistSq(p As DataPoint) As Double
			Dim dx As Double = p.Dx - Cx
			Dim dy As Double = p.Dy - Cy
			Return dx * dx + dy * dy
		End Function

		Public Overrides Function ToString() As String
			Return "Cluster{" & "cx=" & Cx & ", cy=" & Cy & ", dataPoints=" & DataPoints.JoinBy(", ") & "}"c
		End Function

		Public Overridable Function updateCentroid() As Boolean
			Dim sumX As Double = 0.0
			Dim sumY As Double = 0.0
			For Each p As DataPoint In DataPoints
				sumX += p.Dx
				sumY += p.Dy
			Next p
			Dim size As Integer = DataPoints.Count
			If size = 0 Then
				size = 1
			End If
			Dim tcx As Double = Cx
			Cx = sumX / size
			Dim tcy As Double = Cy
			Cy = sumY / size

			Return Not (std.Abs(tcx - Cx) < CENTROID_THRESHOLD AndAlso std.Abs(tcy - Cy) < CENTROID_THRESHOLD)
		End Function
	End Class

End Namespace