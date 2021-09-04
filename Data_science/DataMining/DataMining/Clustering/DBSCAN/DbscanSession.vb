﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace DBSCAN

    Public Class DbscanSession(Of T)

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

            Me.maxStackSize = stdNum.Min(allPoints.Length / 2, 1024)
            Me.allPoints = allPoints
            Me.epsilon = epsilon
            Me.minPts = minPts
            Me.dbscan = dbscan

            Call Console.WriteLine($"max stack size for expands cluster is {maxStackSize}")
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

                Call Console.WriteLine($"Density cutoff for dbscan is: {densityCut}!")
                Call Console.WriteLine($"There are {orderDensity.Where(Function(d) d < densityCut).Count}/{densityList.Count} lower than this threshold value.")
            Else
                Call Console.WriteLine("No density cutoff of your sample data.")
            End If

            Return Me
        End Function

        Private Function CheckNeighborPts(pn As DbscanPoint(Of T)) As DbscanPoint(Of T)()
            If dbscan._full OrElse Not pn.IsVisited Then
                Dim neighborPts2 = RegionQuerySingle(pn.ClusterPoint).ToArray

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
                    neighborPts2 = RegionQuery(pn.ClusterPoint)

                    If densityCut > 0 AndAlso densityList(pn.ID) < densityCut Then
                        pn.ClusterId = ClusterIDs.Noise
                    ElseIf neighborPts2.Length >= minPts Then
                        neighborPts = neighborPts _
                            .Union(neighborPts2) _
                            .ToArray()

                        If stackDepth < maxStackSize Then
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
        ''' Checks and searchs neighbor points for given point
        ''' </summary>
        ''' <param name="point">centered point to be searched neighbors</param>
        ''' <returns>result neighbors</returns>
        ''' 
        Private Function RegionQuerySingle(point As T) As IEnumerable(Of DbscanPoint(Of T))
            Return From x As DbscanPoint(Of T)
                   In allPoints
                   Where dbscan._metricFunc(point, x.ClusterPoint) <= epsilon
        End Function

        ReadOnly queryCache As New Dictionary(Of T, DbscanPoint(Of T)())

        ''' <summary>
        ''' Checks and searchs neighbor points for given point
        ''' </summary>
        ''' <param name="point">centered point to be searched neighbors</param>
        ''' <returns>result neighbors</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RegionQuery(point As T) As DbscanPoint(Of T)()
            If Not queryCache.ContainsKey(point) Then
                queryCache(point) = allPoints _
                    .Split(allPoints.Length / 8) _
                    .AsParallel _
                    .SelectMany(Function(block)
                                    Return From x As DbscanPoint(Of T) In block Where dbscan._metricFunc(point, x.ClusterPoint) <= epsilon
                                End Function) _
                    .ToArray()
            End If

            Return queryCache(point)
        End Function
    End Class
End Namespace