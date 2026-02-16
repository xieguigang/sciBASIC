' <copyright file="TravelingCosts.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Imports System.Collections.Concurrent

Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' Implementation of distance calculation from an arbitrary point to the given destination.
    ''' </summary>
    ''' <typeparam name="TItem">Type of the points.</typeparam>
    ''' <typeparam name="TDistance">Type of the diatnce.</typeparam>
    Public Class TravelingCosts(Of TItem, TDistance)
        Implements IComparer(Of TItem)
        ''' <summary>
        ''' Default distance comaprer.
        ''' </summary>
        Private Shared ReadOnly DistanceComparer As Comparer(Of TDistance) = Comparer(Of TDistance).Default

        ''' <summary>
        ''' The distance funciton.
        ''' </summary>
        Private ReadOnly distance As Func(Of TItem, TItem, TDistance)

        ''' <summary>
        ''' The destination point.
        ''' </summary>
        Private ReadOnly destination As TItem

        ''' <summary>
        ''' Cached values.
        ''' </summary>
        Private ReadOnly cache As ConcurrentDictionary(Of TItem, TDistance)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TravelingCosts(Of TItem,TDistance)"/> class.
        ''' </summary>
        ''' <param name="distance">The distance function.</param>
        ''' <param name="destination">The destination point.</param>
        Public Sub New(distance As Func(Of TItem, TItem, TDistance), destination As TItem)
            Me.distance = distance
            Me.destination = destination

            cache = New ConcurrentDictionary(Of TItem, TDistance)()
        End Sub

        ''' <summary>
        ''' Calculates distance from the departure to the destination.
        ''' </summary>
        ''' <param name="departure">The point of departure.</param>
        ''' <returns>The distance from the departure to the destination.</returns>
        Public Function From(departure As TItem) As TDistance
            Dim result As TDistance
            If Not cache.TryGetValue(departure, result) Then
                result = distance(departure, destination)
                cache.TryAdd(departure, result)
            End If

            Return result
        End Function

        ''' <summary>
        ''' Compares 2 points by the distance from the destination.
        ''' </summary>
        ''' <param name="x">Left point.</param>
        ''' <param name="y">Right point.</param>
        ''' <returns>
        ''' -1 if x is closer to the destination than y;
        ''' 0 if x and y are equally far from the destination;
        ''' 1 if x is farther from the destination than y.
        ''' </returns>
        Public Function Compare(x As TItem, y As TItem) As Integer Implements IComparer(Of TItem).Compare
            Dim fromX = From(x)
            Dim fromY = From(y)
            Return DistanceComparer.Compare(fromX, fromY)
        End Function
    End Class
End Namespace
