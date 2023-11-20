Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BisectingKMeans

	''' <summary>
	''' Created by touhid on 12/21/15.
	''' 
	''' @author touhid
	''' </summary>
	Public Class Cluster : Implements IVector

		Public Property centroid As Double() Implements IVector.Data
		Public Overridable Property DataPoints As List(Of ClusterEntity)

		Public Sub New(c As Double())
			Me.centroid = c
			Me.DataPoints = New List(Of ClusterEntity)()
		End Sub

		Public Sub New(centroid As ClusterEntity)
			Me.centroid = centroid.entityVector.ToArray
			Me.DataPoints = New List(Of ClusterEntity)()
		End Sub

		Public Sub New(centroid As Double(), dataPoints As List(Of ClusterEntity))
			Me.centroid = centroid
			Me.DataPoints = dataPoints
		End Sub

		Public Overridable Sub addPoint(p As ClusterEntity)
			Me.DataPoints.Add(p)
		End Sub

		Public Overridable ReadOnly Property SSE As Double
			Get
				Dim sse_d As Double = 0.0
				Dim c As New Vector(centroid)
				Dim dx As Vector

				For Each p As ClusterEntity In DataPoints
					dx = (c - New Vector(p.entityVector)) ^ 2
					sse_d += dx.Sum
				Next p
				Return sse_d
			End Get
		End Property

		Public Overrides Function ToString() As String
			Return "Cluster{" & centroid.GetJson & ", dataPoints=" & DataPoints.JoinBy(", ") & "}"c
		End Function

		Friend Overridable Function updateCentroid(Optional CENTROID_THRESHOLD As Double = 0.005) As Boolean
			Dim sum As Vector = Vector.Zero(centroid.Length)

			For Each p As ClusterEntity In DataPoints
				sum = sum + p.entityVector
			Next

			Dim size As Integer = DataPoints.Count

			If size = 0 Then
				size = 1
			End If

			Dim m As Vector = sum / size
			Dim e As Vector = (m - New Vector(centroid)).Abs

			Return Not (e < CENTROID_THRESHOLD).All
		End Function
	End Class

End Namespace