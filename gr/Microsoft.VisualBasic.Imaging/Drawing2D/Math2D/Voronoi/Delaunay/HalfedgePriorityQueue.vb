#Region "Microsoft.VisualBasic::738fa3f1aafb7ca8ec07fc2c117db85f, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\HalfedgePriorityQueue.vb"

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

    '   Total Lines: 126
    '    Code Lines: 93 (73.81%)
    ' Comment Lines: 10 (7.94%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 23 (18.25%)
    '     File Size: 4.39 KB


    '     Class HalfedgePriorityQueue
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Bucket, Empty, ExtractMin, IsEmpty, Min
    ' 
    '         Sub: AdjustMinBucket, Dispose, Init, Insert, Remove
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Math2D.DelaunayVoronoi

    ' Also know as heap
    Public Class HalfedgePriorityQueue

        Private hash As Halfedge()
        Private count As Integer
        Private minBucked As Integer
        Private hashSize As Integer

        Private ymin As Single
        Private deltaY As Single

        Public Sub New(ymin As Single, deltaY As Single, sqrtSitesNb As Integer)
            Me.ymin = ymin
            Me.deltaY = deltaY
            hashSize = 4 * sqrtSitesNb
            Init()
        End Sub

        Public Sub Dispose()
            ' Get rid of dummies
            For i = 0 To hashSize - 1
                hash(i).Dispose()
            Next
            hash = Nothing
        End Sub

        Public Sub Init()
            count = 0
            minBucked = 0
            hash = New Halfedge(hashSize - 1) {}
            ' Dummy Halfedge at the top of each hash
            For i = 0 To hashSize - 1
                hash(i) = Halfedge.CreateDummy()
                hash(i).nextInPriorityQueue = Nothing
            Next
        End Sub

        Public Sub Insert(halfedge As Halfedge)
            Dim previous As Halfedge
            Dim [next] As Value(Of Halfedge) = Value(Of Halfedge).Default

            Dim insertionBucket = Bucket(halfedge)
            If insertionBucket < minBucked Then
                minBucked = insertionBucket
            End If
            previous = hash(insertionBucket)
            While ([next] = previous.nextInPriorityQueue) IsNot Nothing AndAlso (halfedge.ystar > [next].Value.ystar OrElse halfedge.ystar = [next].Value.ystar AndAlso halfedge.vertex.x > [next].Value.vertex.x)
                previous = [next]
            End While
            halfedge.nextInPriorityQueue = previous.nextInPriorityQueue
            previous.nextInPriorityQueue = halfedge
            count += 1
        End Sub

        Public Sub Remove(halfedge As Halfedge)
            Dim previous As Halfedge
            Dim removalBucket = Bucket(halfedge)

            If halfedge.vertex IsNot Nothing Then
                previous = hash(removalBucket)
                While previous.nextInPriorityQueue IsNot halfedge
                    previous = previous.nextInPriorityQueue
                End While
                previous.nextInPriorityQueue = halfedge.nextInPriorityQueue
                count -= 1
                halfedge.vertex = Nothing
                halfedge.nextInPriorityQueue = Nothing
                halfedge.Dispose()
            End If
        End Sub

        Private Function Bucket(halfedge As Halfedge) As Integer
            Dim theBucket As Integer = (halfedge.ystar - ymin) / deltaY * hashSize
            If theBucket < 0 Then theBucket = 0
            If theBucket >= hashSize Then theBucket = hashSize - 1
            Return theBucket
        End Function

        Private Function IsEmpty(bucket As Integer) As Boolean
            Return hash(bucket).nextInPriorityQueue Is Nothing
        End Function

        ' 
        ' * move minBucket until it contains an actual Halfedge (not just the dummy at the top);

        Private Sub AdjustMinBucket()
            While minBucked < hashSize - 1 AndAlso IsEmpty(minBucked)
                minBucked += 1
            End While
        End Sub

        Public Function Empty() As Boolean
            Return count = 0
        End Function

        ' 
        ' * @return coordinates of the Halfedge's vertex in V*, the transformed Voronoi diagram

        Public Function Min() As Vector2D
            AdjustMinBucket()
            Dim answer = hash(minBucked).nextInPriorityQueue
            Return New Vector2D(answer.vertex.x, answer.ystar)
        End Function

        ' 
        ' * Remove and return the min Halfedge

        Public Function ExtractMin() As Halfedge
            Dim answer As Halfedge

            ' Get the first real Halfedge in minBucket
            answer = hash(minBucked).nextInPriorityQueue

            hash(minBucked).nextInPriorityQueue = answer.nextInPriorityQueue
            count -= 1
            answer.nextInPriorityQueue = Nothing

            Return answer
        End Function
    End Class
End Namespace
