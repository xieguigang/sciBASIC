Imports std = System.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace AffinityPropagation
    Public Class AffinityPropagation

        Private _max_iteration, _convergence As Integer
        Private _damping As Single
        Private _graph As Graph

        Public ReadOnly Property Centers As HashSet(Of Integer)

        Public Sub New(number_of_points As Integer,
                       Optional damping As Single = 0.9F,
                       Optional max_iteration As Integer = 1000,
                       Optional convergence As Integer = 200)

            If number_of_points < 1 Then
                Throw New ArgumentOutOfRangeException("Number of points can't be 0 or a negative value")
            End If

            _graph = New Graph(number_of_points)
            _damping = damping
            _max_iteration = max_iteration
            _convergence = convergence
            Centers = New HashSet(Of Integer)(number_of_points)
        End Sub
        Private Function Preference() As Single

            Dim m = _graph.SimMatrixElementsCount - _graph.VerticesCount - 1
            'get the middle element of the array with quickselect without sorting the array 
            Dim s = k2thSmallest(_graph.Edges, 0, m, m / 2 + 1)
            Return Convert.ToSingle(If(m Mod 2 = 0, (s(0) + s(1)) / 2, s(0)))

        End Function

        Private Sub BuildGraph(points As Edge())
            _graph.Edges = points
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
                p.Similarity += Convert.ToSingle((0.0000000000000001 * p.Similarity + 1.0E-300) * (randf.NextNumber / (Integer.MaxValue + 1.0)))

                'add out/in edges to vertices
                _graph.outEdges(p.Source)(indexes_source(p.Source)) = p
                _graph.inEdges(p.Destination)(indexes_destination(p.Destination)) = p
                indexes_source(p.Source) += 1
                indexes_destination(p.Destination) += 1
                i += 1
            End While
            Console.WriteLine("Graph Constructed")
        End Sub

        Private Sub Update(ByRef variable As Single, newValue As Single)
            variable = Convert.ToSingle(_damping * variable + (1.0 - _damping) * newValue)
        End Sub

        Private Sub UpdateResponsabilities()
            Dim edges As Edge()
            Dim max1, max2, argmax1 As Single
            Dim Similarity = 0.0F
            Dim i = 0
            Dim k = 0

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
                Dim temp = 0.0F
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

        Private Sub UpdateAvailabilities()
            Dim edges As Edge()
            Dim sum As Double = 0.0, temp = 0.0F, temp1 = 0.0F, last = 0.0F
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

        Private Function UpdateExamplars(examplar As Integer()) As Boolean
            Dim changed = False
            Dim edges As Edge()
            Dim Similarity = 0.0F, maxValue = 0.0F
            Dim argmax As Integer
            Dim i = 0

            While i < _graph.VerticesCount
                edges = _graph.outEdges(i)
                maxValue = -Single.PositiveInfinity
                argmax = i
                Dim k = 0

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
                    Centers.Clear()
                End If
                Centers.Add(argmax)
                i += 1
            End While
            Return changed
        End Function

        Public Function Fit(input As Edge()) As Integer()
            If input.Length <> _graph.SimMatrixElementsCount Then
                Throw New Exception($"The provided array size mismatch with the size given in the constructor  ({input.Length}!={_graph.SimMatrixElementsCount})")
            End If

            BuildGraph(input)
            Dim examplar = New Integer(_graph.VerticesCount - 1) {}
            Dim i = 0
            Dim nochange = 0

            While i < _graph.VerticesCount
                examplar(i) = -1
                i += 1
            End While

            i = 0

            While i < _max_iteration AndAlso nochange < _convergence
                UpdateResponsabilities()
                UpdateAvailabilities()
                If UpdateExamplars(examplar) Then nochange = 0
                i += 1
                nochange += 1
            End While
            Return examplar
        End Function

    End Class
End Namespace
