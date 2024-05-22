#Region "Microsoft.VisualBasic::c09491188de558a0464fa3efa9246e93, Data_science\DataMining\DataMining\Clustering\KMeans\Models\Cluster.vb"

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

    '   Total Lines: 93
    '    Code Lines: 60 (64.52%)
    ' Comment Lines: 16 (17.20%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 17 (18.28%)
    '     File Size: 3.18 KB


    '     Class Cluster
    ' 
    '         Properties: size
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: CompleteLinkageDistance
    ' 
    '         Sub: Add
    ' 
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

Namespace KMeans

    ''' <summary>
    ''' A collection of the target entity object will be a cluster
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Cluster(Of T As EntityBase(Of Double))

        Protected Friend ReadOnly m_innerList As New List(Of T)

        ''' <summary>
        ''' get member count in current cluster
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
            Get
                Return m_innerList.Count
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(points As List(Of T))
            m_innerList = New List(Of T)(points)
        End Sub

        Sub New(points As IEnumerable(Of T))
            m_innerList = New List(Of T)(points.SafeQuery)
        End Sub

        Public Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(p As T)
            Call m_innerList.Add(p)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub Add(p As T)
            Call m_innerList.Add(p)
        End Sub

        Public Function CompleteLinkageDistance(c2 As Cluster(Of T)) As Double
            Dim c1 = Me
            Dim points1 As List(Of T) = c1.m_innerList
            Dim points2 As List(Of T) = c2.m_innerList
            Dim numPointsInC1 As Integer = points1.Count
            Dim numPointsInC2 As Integer = points2.Count
            Dim maxDistance As Double = Double.MinValue
            Dim dist As Double

            For i1 As Integer = 0 To numPointsInC1 - 1
                For i2 As Integer = 0 To numPointsInC2 - 1
                    dist = points1(i1).entityVector.EuclideanDistance(points2(i2).entityVector)
                    maxDistance = std.Max(dist, maxDistance)
                Next
            Next

            Return maxDistance
        End Function

        ''' <summary>
        ''' merge two cluster to populate a new cluster
        ''' </summary>
        ''' <param name="c1"></param>
        ''' <param name="c2"></param>
        ''' <returns></returns>
        Public Shared Operator +(c1 As Cluster(Of T), c2 As Cluster(Of T)) As Cluster(Of T)
            Dim mergedCluster As New Cluster(Of T)
            Dim pointsC1 As List(Of T) = c1.m_innerList

            ' due to the reason of add method is overridable
            ' do we can not use the inner list to add directly
            For i As Integer = 0 To pointsC1.Count - 1
                mergedCluster.Add(pointsC1(i))
            Next

            Dim pointsC2 As List(Of T) = c2.m_innerList

            For i As Integer = 0 To pointsC2.Count - 1
                mergedCluster.Add(pointsC2(i))
            Next

            Return mergedCluster
        End Operator
    End Class
End Namespace
