Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans.Bisecting

	''' <summary>
	''' Created by touhid on 12/21/15.
	''' 
	''' @author touhid
	''' </summary>
	Public Class Cluster : Implements IVector, IClusterPoint, IEnumerable(Of ClusterEntity)

		''' <summary>
		''' the data point vector of the centroid node in current cluster
		''' </summary>
		''' <returns></returns>
		Public Property centroid As Double() Implements IVector.Data
		''' <summary>
		''' a collection of the data members inside current cluster
		''' </summary>
		''' <returns></returns>
		Public Overridable Property DataPoints As List(Of ClusterEntity)

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

		''' <summary>
		''' current cluster class label
		''' </summary>
		''' <returns></returns>
		Public Property Cluster As Integer Implements IClusterPoint.Cluster

		Public ReadOnly Property Size As Integer
			Get
				Return DataPoints.TryCount
			End Get
		End Property

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

		Sub New(centroid As Double(), dataPoints As ClusterEntity())
			Call Me.New(centroid, dataPoints.ToList)
		End Sub

		<MethodImpl(MethodImplOptions.AggressiveInlining)>
		Public Overridable Sub addPoint(p As ClusterEntity)
			Me.DataPoints.Add(p)
		End Sub

		Public Overrides Function ToString() As String
			Return "Cluster{" & centroid.GetJson & ", dataPoints=" & DataPoints.JoinBy(", ") & "}"c
		End Function

		Public Iterator Function GetEnumerator() As IEnumerator(Of ClusterEntity) Implements IEnumerable(Of ClusterEntity).GetEnumerator
			For Each point As ClusterEntity In DataPoints
				Yield point
			Next
		End Function

		Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
			Yield GetEnumerator()
		End Function
	End Class

End Namespace