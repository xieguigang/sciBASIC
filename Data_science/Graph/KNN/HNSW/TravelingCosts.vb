#Region "Microsoft.VisualBasic::6e13cabcd45635520a690f2848c71507, Data_science\Graph\KNN\HNSW\TravelingCosts.vb"

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

    '   Total Lines: 80
    '    Code Lines: 28 (35.00%)
    ' Comment Lines: 41 (51.25%)
    '    - Xml Docs: 90.24%
    ' 
    '   Blank Lines: 11 (13.75%)
    '     File Size: 3.14 KB


    '     Class TravelingCosts
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Compare, From
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
