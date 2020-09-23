#Region "Microsoft.VisualBasic::e070894d565c13d8b551f0290564fcd5, gr\network-visualization\Datavisualization.Network\Layouts\ForceDirected\Planner.vb"

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

    '     Module Planner
    ' 
    '         Function: CalculateTotalAttraction, CalculateTotalRepulsion, CreateRandomLocations, GetAttractionForce, GetRepulsionForce
    '                   Plan
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

Namespace Layouts.ForceDirected

    ''' <summary>
    ''' Class Planner.
    ''' </summary>
    Public Module Planner
        ''' <summary>
        ''' The node displacement termination threshold
        ''' </summary>
        Private Const TerminationThreshold As Double = 0.0001R

        ''' <summary>
        ''' The repulsion strength between two unconnected vertices
        ''' </summary>
        Private Const VertexRepulsionForceStrength As Double = 5.0R

        ''' <summary>
        ''' The attraction strength between two connected vertices
        ''' </summary>
        Private Const VertexAttractionForceStrength As Double = 0.9R

        ''' <summary>
        ''' Plans the specified graph.
        ''' </summary>
        ''' <param name="graph">The graph.</param>
        ''' <param name="MaximumIterations">
        ''' The maximum number of iterations
        ''' </param>
        Public Function Plan(graph As NetworkGraph, Optional MaximumIterations As Integer = 1000, Optional progress As Action(Of String) = Nothing) As IReadOnlyDictionary(Of Node, Vector2D)
            ' create initial random locations for each vertex
            Dim currentLocations = CreateRandomLocations(graph)

            ' loop until the number of iterations exceeds the hard limit
            Const terminationThreshold = Planner.TerminationThreshold * Planner.TerminationThreshold
            Dim i As i32 = 0

            If progress Is Nothing Then
                progress = Sub()
                               ' do nothing
                           End Sub
            End If

            While i < MaximumIterations
                ' the total displacement, used as a stop condition
                Dim totalDisplacement = 0.0R
                ' prepare the new positions
                Dim newLocations As New Dictionary(Of Node, Vector2D)(currentLocations)

                ' iterate over all vertices ...
                For Each vertex As Node In graph.vertex
                    ' obtain the vertex location
                    Dim locationOf As Vector2D = currentLocations(vertex)

                    ' and process against all other vertices
                    Dim netForce As FDGVector2 = FDGVector2.Zero
                    netForce += CalculateTotalRepulsion(graph, vertex, locationOf, currentLocations)
                    netForce += CalculateTotalAttraction(graph, vertex, locationOf, currentLocations) ' TODO: make use of force symmetry along edges here

                    ' finally, update the vertex' position
                    locationOf += netForce
                    newLocations(vertex) = locationOf

                    ' update the total displacement
                    totalDisplacement += netForce.SquaredNorm()
                Next

                ' calculate the center
                Dim center As FDGVector2 = newLocations.Aggregate(FDGVector2.Zero, Function(vector, location) vector + New FDGVector2(location.Value)) * (1.0R / currentLocations.Count)

                ' adjust each node to prevent creep
                For Each location In newLocations
                    currentLocations(location.Key) = location.Value - center
                Next

                ' early exit if nodes don't move much anymore
                If totalDisplacement < terminationThreshold Then
                    Exit While
                Else
                    Call progress($"[{++i}/{MaximumIterations}]")
                End If
            End While

            Return currentLocations
        End Function

        ''' <summary>
        ''' Calculates the total repulsion force.
        ''' </summary>
        ''' <param name="graph">The graph.</param>
        ''' <param name="vertex">The vertex.</param>
        ''' <param name="vertexLocation"></param>
        ''' <param name="currentLocations">The current locations.</param>
        ''' <returns>Vector.</returns>
        Private Function CalculateTotalRepulsion(
 graph As NetworkGraph, vertex As Node, vertexLocation As Vector2D,
 currentLocations As IReadOnlyDictionary(Of Node, Vector2D)) As FDGVector2
            Dim forces = From other As Node
                         In graph.vertex.AsParallel()
                         Where Not other Is vertex
                         Let otherLocation = currentLocations(other)
                         Select GetRepulsionForce(vertexLocation, otherLocation)

            Dim force = forces.Aggregate(DirectCast(FDGVector2.Zero, FDGVector2), Function(current, sumOfForces) sumOfForces + current)
            Return force
        End Function

        ''' <summary>
        ''' Calculates the total attraction force.
        ''' </summary>
        ''' <param name="graph">The graph.</param>
        ''' <param name="vertex">The vertex.</param>
        ''' <param name="vertexLocation"></param>
        ''' <param name="currentLocations">The current locations.</param>
        ''' <returns>Vector.</returns>
        Private Function CalculateTotalAttraction(
 graph As NetworkGraph, vertex As Node, vertexLocation As Vector2D,
 currentLocations As IReadOnlyDictionary(Of Node, Vector2D)) As FDGVector2
            Dim forces = From edges As Edge
                         In graph.GetEdges(vertex).AsParallel()
                         Let other = edges.Other(vertex)
                         Let otherLocation = currentLocations(other)
                         Select GetAttractionForce(graph, vertex, vertexLocation, other, otherLocation)

            Dim force = forces.Aggregate(DirectCast(FDGVector2.Zero, FDGVector2), Function(current, sumOfForces) sumOfForces + current)
            Return force
        End Function

        ''' <summary>
        ''' Creates the initial random locations.
        ''' </summary>
        ''' <param name="graph">The graph.</param>
        ''' <returns>ILookup&lt;Vertex, Location&gt;.</returns>
        Private Function CreateRandomLocations(
       graph As NetworkGraph) As Dictionary(Of Node, Vector2D)
            Dim random = randf.seeds
            Dim initialLocations = graph.vertex.ToDictionary(Function(v) v, Function(v) New Vector2D(10000 * random.NextDouble(), 10000 * random.NextDouble()))
            Return initialLocations
        End Function

        ''' <summary>
        ''' Gets the repulsion force.
        ''' </summary>
        ''' <param name="of">The node under observation.</param>
        ''' <param name="from">The other node.</param>
        ''' <param name="repulsionForce">The repulsion force.</param>
        ''' <returns>The force vector.</returns>
        Private Function GetRepulsionForce([of] As Vector2D, from As Vector2D, Optional repulsionForce As Double = VertexRepulsionForceStrength) As FDGVector2
            ' get the proximity and the direction
            Dim currentDistance As Double
            Dim direction = New FDGVector2([of] - from).Normalized(currentDistance)

            If currentDistance = 0.0 OrElse (direction.x = 0.0 AndAlso direction.y = 0.0) Then
                Return FDGVector2.Zero
            Else
                ' strength decrease is proportional to the squared distance;
                ' this imitates Coulomb's Law of the repulsion of charged particles (F = k*Q1*Q2/r^2)
                ' where we assume the charge to be equal for all particles.
                Dim strength = repulsionForce / (currentDistance * currentDistance)

                ' return the force vector
                Return direction * strength
            End If
        End Function

        ''' <summary>
        ''' Gets the attraction force.
        ''' </summary>
        ''' <param name="graph">The graph.</param>
        ''' <param name="of">The node under observation.</param>
        ''' <param name="locationOf">The location of.</param>
        ''' <param name="from">The other node.</param>
        ''' <param name="locationFrom">The location from.</param>
        ''' <param name="attractionStrength">The attraction strength.</param>
        ''' <returns>The force vector.</returns>
        Private Function GetAttractionForce(
 graph As NetworkGraph,
 [of] As Node, locationOf As Vector2D,
 from As Node, locationFrom As Vector2D, Optional attractionStrength As Double = VertexAttractionForceStrength) As FDGVector2
            ' if vertices are unconnected, there is no force that
            ' pulls them together.
            Dim edges As Edge() = graph.GetEdges([of], from).ToArray

            If edges.Length = 0 Then
                Return FDGVector2.Zero
            End If

            ' the actual distance between the two vertices is given by the edge weight
            Dim expectedDistance = Aggregate edge As Edge In edges Into Sum(edge.weight)

            ' fetch the force direction and the actual distance
            Dim currentDistance As Double
            Dim direction = New FDGVector2(locationOf - locationFrom).Normalized(currentDistance)

            If currentDistance = 0.0 OrElse (direction.x = 0.0 AndAlso direction.y = 0.0) Then
                Return FDGVector2.Zero
            Else
                ' determine the spring strength by Hooke's law:
                ' If the expected distance is larger than the current distance, the spring is
                ' too short and should thus expand; hence the attraction force is zero.
                ' If the expected distance is smaller than the current distance, the spring needs
                ' to contract, hence the strength is positive.
                Dim strength = attractionStrength * stdNum.Max(currentDistance - expectedDistance, 0)

                ' In order to contract, we reverse the force direction
                Return direction * -strength
            End If
        End Function
    End Module
End Namespace

