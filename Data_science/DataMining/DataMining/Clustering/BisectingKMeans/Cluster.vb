#Region "Microsoft.VisualBasic::9a5018789e744185b6c96c8c6217464b, Data_science\DataMining\DataMining\Clustering\BisectingKMeans\Cluster.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 96
    '    Code Lines: 62
    ' Comment Lines: 17
    '   Blank Lines: 17
    '     File Size: 2.78 KB


    ' 	Class Cluster
    ' 
    ' 	    Properties: centroid, Cluster, DataPoints, Size, SSE
    ' 
    ' 	    Constructor: (+5 Overloads) Sub New
    ' 
    ' 	    Function: GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    ' 	    Sub: addPoint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

		Sub New()
			DataPoints = New List(Of ClusterEntity)
		End Sub

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
