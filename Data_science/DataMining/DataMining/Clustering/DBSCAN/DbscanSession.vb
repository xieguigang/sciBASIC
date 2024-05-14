#Region "Microsoft.VisualBasic::de47e5c4a6f902ae3af011a558346470, Data_science\DataMining\DataMining\Clustering\DBSCAN\DbscanSession.vb"

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

    '   Total Lines: 252
    '    Code Lines: 171
    ' Comment Lines: 40
    '   Blank Lines: 41
    '     File Size: 9.85 KB


    '     Class DbscanSession
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CheckNeighborPts, isNoise, LoadDensityVector, RegionQuery
    ' 
    '         Sub: ExpandCluster, ExpandClusterParallel
    '         Class QueryTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace DBSCAN

    Public Class DbscanSession(Of T As IReadOnlyId)

        ReadOnly dbscan As DbscanAlgorithm(Of T)
        ''' <summary>
        ''' Dataset
        ''' </summary>
        Friend ReadOnly allPoints As DbscanPoint(Of T)()
        ''' <summary>
        ''' radius of center point
        ''' </summary>
        ReadOnly epsilon As Double
        Friend ReadOnly minPts As Integer
        ReadOnly densityCut As Double = -1
        ReadOnly maxStackSize As Integer

        Dim densityList As Dictionary(Of String, Double)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dbscan"></param>
        ''' <param name="allPoints"></param>
        ''' <param name="epsilon">Desired region ball range</param>
        ''' <param name="minPts">Minimum number of points to be in a region, density cutoff</param>
        Sub New(dbscan As DbscanAlgorithm(Of T),
                allPoints As DbscanPoint(Of T)(),
                epsilon As Double,
                minPts As Integer)

            Me.maxStackSize = std.Min(allPoints.Length / 2, 1024)
            Me.allPoints = allPoints
            Me.epsilon = epsilon
            Me.minPts = minPts
            Me.dbscan = dbscan

            Call dbscan.println($"max stack size for expands cluster is {maxStackSize}")
        End Sub

        Public Function isNoise(id As String) As Boolean
            If densityCut > 0 AndAlso densityList(id) < densityCut Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Function LoadDensityVector(densityCut As Double) As DbscanSession(Of T)
            Dim metric As Func(Of DbscanPoint(Of T), DbscanPoint(Of T), Double) =
                Function(a, b)
                    Return dbscan._metricFunc(a.ClusterPoint, b.ClusterPoint)
                End Function

            If densityCut > 0 Then
                Dim allDensity = Density _
                    .GetDensity(Of DbscanPoint(Of T))(allPoints, metric, k:=minPts) _
                    .ToArray
                Dim orderDensity As Double() = allDensity _
                    .Select(Function(d) d.Value) _
                    .OrderBy(Function(d) d) _
                    .ToArray

                densityCut = orderDensity(densityCut * allDensity.Length)
                densityList = allDensity _
                    .ToDictionary(Function(i) i.Name,
                                  Function(i)
                                      Return i.Value
                                  End Function)

                Call dbscan.println($"Density cutoff for dbscan is: {densityCut}!")
                Call dbscan.println($"There are {orderDensity.Where(Function(d) d < densityCut).Count}/{densityList.Count} lower than this threshold value.")
            Else
                Call dbscan.println("No density cutoff of your sample data.")
            End If

            Return Me
        End Function

        ''' <summary>
        ''' this function was called by a parallel linq
        ''' </summary>
        ''' <param name="pn"></param>
        ''' <returns></returns>
        Private Function CheckNeighborPts(pn As DbscanPoint(Of T)) As DbscanPoint(Of T)()
            If dbscan._full OrElse Not pn.IsVisited Then
                Dim neighborPts2 = RegionQuery(pn.ClusterPoint, parallel:=False).ToArray

                SyncLock pn
                    pn.IsVisited = True
                    pn.ClusterId = If(isNoise(pn.ID), ClusterIDs.Noise, pn.ClusterId)
                End SyncLock

                If neighborPts2.Length >= minPts Then
                    Return neighborPts2
                Else
                    Return {}
                End If
            Else
                Return {}
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="neighborPts">other points in same region with point parameter</param>
        ''' <param name="clusterId">given clusterId</param>
        Public Sub ExpandClusterParallel(neighborPts As DbscanPoint(Of T)(), clusterId As Integer, stackDepth As Integer)
            Dim queryNeighborPts = From pn As DbscanPoint(Of T)
                                   In neighborPts.AsParallel
                                   Where Not pn.IsVisited
                                   Select CheckNeighborPts(pn)
            Dim neighborPts2 As DbscanPoint(Of T)() = queryNeighborPts _
                .IteratesALL _
                .Distinct _
                .ToArray

            For Each pn As DbscanPoint(Of T) In neighborPts
                pn.IsVisited = True

                If pn.ClusterId = ClusterIDs.Unclassified OrElse pn.ClusterId = ClusterIDs.Noise Then
                    pn.ClusterId = clusterId
                End If
            Next

            If (stackDepth + 1) < maxStackSize Then
                For Each pn As DbscanPoint(Of T) In neighborPts2
                    If pn.IsVisited Then
                        Continue For
                    Else
                        Call ExpandClusterParallel(neighborPts2, clusterId, stackDepth + 1)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="neighborPts">other points in same region with point parameter</param>
        ''' <param name="clusterId">given clusterId</param>
        Public Sub ExpandCluster(neighborPts As DbscanPoint(Of T)(), clusterId As Integer, stackDepth As Integer)
            Dim neighborPts2 As DbscanPoint(Of T)() = Nothing

            Do While neighborPts.Length > 0
                Dim pn As DbscanPoint(Of T) = (From p As DbscanPoint(Of T) In neighborPts Where Not p.IsVisited).FirstOrDefault

                If pn Is Nothing Then
                    Exit Do
                ElseIf dbscan._full OrElse Not pn.IsVisited Then
                    pn.IsVisited = True
                    neighborPts2 = RegionQuery(pn.ClusterPoint, parallel:=True)

                    If densityCut > 0 AndAlso densityList(pn.ID) < densityCut Then
                        pn.ClusterId = ClusterIDs.Noise
                    ElseIf neighborPts2.Length >= minPts Then
                        neighborPts = neighborPts _
                            .Union(neighborPts2) _
                            .ToArray()

                        If stackDepth < maxStackSize Then
                            ' call in recursive
                            Call ExpandCluster(neighborPts2, clusterId, stackDepth + 1)
                        End If
                    End If
                End If

                If pn.ClusterId = ClusterIDs.Unclassified OrElse pn.ClusterId = ClusterIDs.Noise Then
                    pn.ClusterId = clusterId
                End If
            Loop
        End Sub

        ''' <summary>
        ''' the point set index
        ''' </summary>
        ReadOnly queryCache As New Dictionary(Of String, DbscanPoint(Of T)())

        ''' <summary>
        ''' Checks and searchs neighbor points for given point
        ''' </summary>
        ''' <param name="point">centered point to be searched neighbors</param>
        ''' <returns>result neighbors</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RegionQuery(point As T, parallel As Boolean) As DbscanPoint(Of T)()
            Dim key As String = point.Identity

            If Not queryCache.ContainsKey(key) Then
                Dim q As New QueryTask(Me, target:=point)

                ' run multiple dimension spatial query
                ' of the nearby points
                If parallel Then
                    Call q.Run()
                Else
                    Call q.Solve()
                End If

                SyncLock queryCache
                    queryCache(key) = q.out.ToArray
                End SyncLock
            End If

            Return queryCache(key)
        End Function

        Private Class QueryTask : Inherits VectorTask

            ReadOnly allPoints As DbscanPoint(Of T)(), target As T
            ReadOnly eval As Func(Of T, T, Double)
            ReadOnly epsilon As Double

            Friend out As New List(Of DbscanPoint(Of T))

            Sub New(session As DbscanSession(Of T), target As T)
                Call MyBase.New(session.allPoints.Length, workers:=std.Min(App.CPUCoreNumbers, 12))

                Me.allPoints = session.allPoints
                Me.target = target
                Me.epsilon = session.epsilon
                Me.eval = session.dbscan._metricFunc
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim out As New List(Of DbscanPoint(Of T))

                For i As Integer = start To ends
                    Dim x As DbscanPoint(Of T) = allPoints(i)

                    If eval(target, x.ClusterPoint) <= epsilon Then
                        Call out.Add(x)
                    End If
                Next

                If out.Count > 0 Then
                    SyncLock Me.out
                        Call Me.out.AddRange(out)
                    End SyncLock
                End If
            End Sub
        End Class
    End Class
End Namespace
