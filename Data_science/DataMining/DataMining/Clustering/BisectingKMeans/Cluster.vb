Imports System
Imports System.Collections.Generic

Namespace BisectingKMeans


	''' <summary>
	''' Created by touhid on 12/21/15.
	''' 
	''' @author touhid
	''' </summary>
	Public Class Cluster
		Private Const CENTROID_THRESHOLD As Double = 0.005


		Private cx_Conflict, cy_Conflict As Double
		Private dataPoints_Conflict As List(Of DataPoint)

		Public Sub New(ByVal cx As Double, ByVal cy As Double)
			Me.cx_Conflict = cx
			Me.cy_Conflict = cy
			dataPoints_Conflict = New List(Of DataPoint)()
		End Sub

		Public Sub New(ByVal centroid As DataPoint)
			Me.cx_Conflict = centroid.Dx
			Me.cy_Conflict = centroid.Dy
			dataPoints_Conflict = New List(Of DataPoint)()
		End Sub

		Public Sub New(ByVal cx As Double, ByVal cy As Double, ByVal dataPoints As List(Of DataPoint))
			Me.cx_Conflict = cx
			Me.cy_Conflict = cy
			Me.dataPoints_Conflict = dataPoints
		End Sub

		Public Overridable Property Cx As Double
			Get
				Return cx_Conflict
			End Get
			Set(ByVal cx As Double)
				Me.cx_Conflict = cx
			End Set
		End Property


		Public Overridable Property Cy As Double
			Get
				Return cy_Conflict
			End Get
			Set(ByVal cy As Double)
				Me.cy_Conflict = cy
			End Set
		End Property


		Public Overridable Property DataPoints As List(Of DataPoint)
			Get
				Return dataPoints_Conflict
			End Get
			Set(ByVal dataPoints As List(Of DataPoint))
				Me.dataPoints_Conflict = dataPoints
			End Set
		End Property


		Public Overridable Sub addPoint(ByVal p As DataPoint)
			Me.dataPoints_Conflict.Add(p)
		End Sub


		Public Overridable ReadOnly Property SSE As Double
			Get
				Dim sse_d As Double = 0.0
				For Each p As DataPoint In dataPoints_Conflict
					Dim dx As Double = cx_Conflict - p.Dx
					Dim dy As Double = cy_Conflict - p.Dy
					sse_d += (dx * dx + dy * dy)
				Next p
				Return sse_d
			End Get
		End Property

		Public Overridable Function getDistSq(ByVal p As DataPoint) As Double
			Dim dx As Double = p.Dx - cx_Conflict
			Dim dy As Double = p.Dy - cy_Conflict
			Return dx * dx + dy * dy
		End Function

		Public Overrides Function ToString() As String
			Return "Cluster{" & "cx=" & cx_Conflict & ", cy=" & cy_Conflict & ", dataPoints=" & dataPoints_Conflict.jointby(", ") & "}"c
		End Function

		Public Overridable Function updateCentroid() As Boolean
			Dim sumX As Double = 0.0
			Dim sumY As Double = 0.0
			For Each p As DataPoint In dataPoints_Conflict
				sumX += p.Dx
				sumY += p.Dy
			Next p
			Dim size As Integer = dataPoints_Conflict.Count
			If size = 0 Then
				size = 1
			End If
			Dim tcx As Double = cx_Conflict
			cx_Conflict = sumX / size
			Dim tcy As Double = cy_Conflict
			cy_Conflict = sumY / size

			Return Not (Math.Abs(tcx - cx_Conflict) < CENTROID_THRESHOLD AndAlso Math.Abs(tcy - cy_Conflict) < CENTROID_THRESHOLD)
		End Function
	End Class

End Namespace