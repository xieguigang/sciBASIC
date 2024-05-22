#Region "Microsoft.VisualBasic::f78849505eec409176a8dd2dcd87825e, Data_science\DataMining\BinaryTree\AffinityPropagation\AffinityPropagation.vb"

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

    '   Total Lines: 277
    '    Code Lines: 182 (65.70%)
    ' Comment Lines: 47 (16.97%)
    '    - Xml Docs: 80.85%
    ' 
    '   Blank Lines: 48 (17.33%)
    '     File Size: 10.28 KB


    '     Class AffinityPropagation
    ' 
    '         Properties: centers
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Fit, Preference, UpdateExamplars
    ' 
    '         Sub: BuildGraph, Update, UpdateAvailabilities, UpdateResponsabilities
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace AffinityPropagation

    ''' <summary>
    ''' ### Affinity propagation
    ''' 
    ''' In statistics and data mining, affinity propagation (AP) is a clustering
    ''' algorithm based on the concept of "message passing" between data points.
    ''' Unlike clustering algorithms such as k-means or k-medoids, affinity 
    ''' propagation does not require the number of clusters to be determined or 
    ''' estimated before running the algorithm. Similar to k-medoids, affinity 
    ''' propagation finds "exemplars," members of the input set that are representative 
    ''' of clusters.
    ''' </summary>
    Public Class AffinityPropagation

        Dim _max_iteration, _convergence As Integer
        Dim _damping As Single
        Dim _graph As Graph

        Public ReadOnly Property centers As New HashSet(Of Integer)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(ds As IEnumerable(Of Double()),
                Optional damping As Single = 0.9F,
                Optional max_iteration As Integer = 1000,
                Optional convergence As Integer = 200)

            Call Me.New(
                ds:=ds.SafeQuery.Select(Function(d, i) New ClusterEntity(i + 1, d)),
                damping:=damping,
                max_iteration:=max_iteration,
                convergence:=convergence)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ds"></param>
        ''' <param name="damping">[0.5,1]</param>
        ''' <param name="max_iteration"></param>
        ''' <param name="convergence"></param>
        Public Sub New(ds As IEnumerable(Of ClusterEntity),
                       Optional damping As Single = 0.9F,
                       Optional max_iteration As Integer = 1000,
                       Optional convergence As Integer = 200)

            Dim matrix As ClusterEntity() = ds.ToArray
            Dim input As Edge() = matrix.SparseSimilarityMatrix

            _graph = New Graph(matrix.Length)
            _graph.Edges = input
            _damping = damping
            _max_iteration = max_iteration
            _convergence = convergence
        End Sub

        Private Function Preference() As Double
            Dim m = _graph.SimMatrixElementsCount - _graph.VerticesCount - 1
            'get the middle element of the array with quickselect without sorting the array 
            Dim s = k2thSmallest(_graph.Edges, 0, m, m / 2 + 1)

            Return If(m Mod 2 = 0, (s(0) + s(1)) / 2, s(0))
        End Function

        Private Sub BuildGraph()
            Dim _preference As Single = Preference()
            Dim i = 0

            While i < _graph.VerticesCount
                _graph.Edges(_graph.Edges.Length - (_graph.VerticesCount - i)) = New Edge(i, i, _preference)
                i += 1
            End While

            Dim indexes_source = New Integer(_graph.VerticesCount - 1) {}
            Dim indexes_destination = New Integer(_graph.VerticesCount - 1) {}

            i = 0

            While i < _graph.Edges.Length
                Dim p = _graph.Edges(i)
                'Add noise to avoid degeneracies
                p.Similarity += (0.0000000000000001 * p.Similarity + 1.0E-300) * (randf.NextNumber / (Integer.MaxValue + 1.0))

                'add out/in edges to vertices
                _graph.outEdges(p.Source)(indexes_source(p.Source)) = p
                _graph.inEdges(p.Destination)(indexes_destination(p.Destination)) = p
                indexes_source(p.Source) += 1
                indexes_destination(p.Destination) += 1
                i += 1
            End While
        End Sub

        Private Sub Update(ByRef variable As Double, newValue As Double)
            variable = _damping * variable + (1.0 - _damping) * newValue
        End Sub

        ''' <summary>
        ''' The "responsibility" matrix R has values r(i, k) that quantify how well
        ''' -suited xk is to serve as the exemplar for xi, relative to other 
        ''' candidate exemplars for xi.
        ''' </summary>
        Private Sub UpdateResponsabilities()
            Dim edges As Edge()
            Dim max1, max2, argmax1 As Double
            Dim Similarity = 0.0F
            Dim i = 0
            Dim k = 0
            Dim temp As Double

            While i < _graph.VerticesCount
                edges = _graph.outEdges(i)
                max1 = -Single.PositiveInfinity
                max2 = -Single.PositiveInfinity
                argmax1 = -1
                For k = 0 To edges.Length - 1
                    Similarity = edges(k).Similarity + edges(k).Availability
                    If Similarity > max1 Then
                        max1.Swap(Similarity)
                        argmax1 = k
                    End If
                    If Similarity > max2 Then
                        max2 = Similarity
                    End If

                Next

                'Update the Responsability
                temp = 0.0F
                k = 0

                While k < edges.Length
                    If k <> argmax1 Then
                        temp = edges(k).Responsability
                        Update(temp, edges(k).Similarity - max1)
                        edges(k).Responsability = temp
                    Else
                        temp = edges(k).Responsability
                        Update(temp, edges(k).Similarity - max2)
                        edges(k).Responsability = temp
                    End If

                    k += 1
                End While

                i += 1
            End While
        End Sub

        ''' <summary>
        ''' The "availability" matrix A contains values a(i, k) that represent how 
        ''' "appropriate" it would be for xi to pick xk as its exemplar, taking into
        ''' account other points' preference for xk as an exemplar.
        ''' </summary>
        Private Sub UpdateAvailabilities()
            Dim edges As Edge()
            Dim sum As Double = 0.0
            Dim temp As Double = 0.0F
            Dim temp1 As Double = 0.0F, last = 0.0F
            Dim k = 0

            While k < _graph.VerticesCount
                edges = _graph.inEdges(k)
                'calculate sum of positive responsabilities
                sum = 0.0F
                Dim i = 0

                While i < edges.Length - 1
                    sum += std.Max(0.0F, edges(i).Responsability)
                    i += 1
                End While

                'calculate the availabilities
                last = edges(edges.Length - 1).Responsability
                i = 0

                While i < edges.Length - 1
                    temp1 = edges(i).Availability
                    Update(temp1, std.Min(0.0F, last + sum - std.Max(0.0F, edges(i).Responsability)))

                    edges(i).Availability = temp1
                    i += 1
                End While
                'calculate self-Availability
                temp = edges(edges.Length - 1).Availability
                Update(temp, sum)
                edges(edges.Length - 1).Availability = temp
                k += 1
            End While
        End Sub

        ''' <summary>
        ''' The diagonal of s (i.e.{\displaystyle s(i,i)}) Is particularly important, 
        ''' as it represents the instance preference, meaning how likely a particular 
        ''' instance Is to become an exemplar. When it Is set to the same value for 
        ''' all inputs, it controls how many classes the algorithm produces. A value 
        ''' close to the minimum possible similarity produces fewer classes, while a
        ''' value close to Or larger than the maximum possible similarity produces 
        ''' many classes. It Is typically initialized to the median similarity of all
        ''' pairs of inputs.
        ''' </summary>
        ''' <param name="examplar"></param>
        ''' <returns></returns>
        Private Function UpdateExamplars(examplar As Integer()) As Boolean
            Dim changed = False
            Dim edges As Edge()
            Dim Similarity = 0.0F, maxValue = 0.0F
            Dim argmax As Integer
            Dim i = 0
            Dim k = 0

            While i < _graph.VerticesCount
                edges = _graph.outEdges(i)
                maxValue = -Single.PositiveInfinity
                argmax = i
                k = 0

                While k < edges.Length
                    Similarity = edges(k).Availability + edges(k).Responsability

                    If Similarity > maxValue Then
                        maxValue = Similarity
                        argmax = edges(k).Destination
                    End If

                    k += 1
                End While

                If examplar(i) <> argmax Then
                    examplar(i) = argmax
                    changed = True
                    centers.Clear()
                End If

                centers.Add(argmax)
                i += 1
            End While

            Return changed
        End Function

        Public Function Fit() As Integer()
            Dim examplar = New Integer(_graph.VerticesCount - 1) {}
            Dim i = 0
            Dim nochange = 0

            Call BuildGraph()

            While i < _graph.VerticesCount
                examplar(i) = -1
                i += 1
            End While

            i = 0

            While i < _max_iteration AndAlso nochange < _convergence
                UpdateResponsabilities()
                UpdateAvailabilities()

                If UpdateExamplars(examplar) Then
                    nochange = 0
                End If

                i += 1
                nochange += 1
            End While

            Return examplar
        End Function

    End Class
End Namespace
