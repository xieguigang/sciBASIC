Imports System.Runtime.CompilerServices
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace KMeans

    Public Module Kmedoids

        <Extension>
        Public Function DoKMedoids(source As IEnumerable(Of ClusterEntity), k As Integer, maxSteps As Integer) As ClusterEntity()
            Dim points = source.ToArray

            If k > points.Length OrElse k < 1 Then
                Throw New Exception("K must be between 0 and set size")
            End If

            Dim medoids As ClusterEntity() = New ClusterEntity(k - 1) {}
            Dim resultClusterPoints As String() = New String(k - 1) {}
            Dim resultPoints As ClusterEntity() = New ClusterEntity(points.Length - 1) {}
            Dim stepmedoids As ClusterEntity() = New ClusterEntity(k - 1) {}
            Dim medoidsIndexes As New List(Of Integer)()
            Dim randIndex As Integer = 0

            ' initilizing random different medoids
            For i As Integer = 0 To k - 1
                randIndex = randf.NextInteger(points.Length)

                If Not medoidsIndexes.Contains(randIndex) Then
                    stepmedoids(i) = points(randIndex)
                    medoidsIndexes.Add(randIndex)
                Else
                    i -= 1
                End If
            Next

            Dim [step] As Integer = 0
            Dim resultSumFunc As Double = Double.MaxValue
            Dim stepSumFunc As Double = 0

            While [step] < maxSteps
                ' initial clustering to medoids
                Dim clusterSumFunc As Double() = New Double(k - 1) {}
                Dim clusterPoints As String() = New String(k - 1) {}

                Dim dist As Double = 0

                For i As Integer = 0 To points.Length - 1
                    Dim minDist As Double = Double.MaxValue
                    dist = 0
                    For c As Integer = 0 To k - 1
                        dist = points(i).Properties.EuclideanDistance(stepmedoids(c).Properties)

                        If dist < minDist Then
                            points(i).cluster = c
                            minDist = dist
                        End If
                    Next

                    ' getting sumFunc result for all clusters
                    clusterSumFunc(points(i).cluster) += minDist
                    stepSumFunc += minDist

                    If clusterPoints(points(i).cluster) Is Nothing Then
                        clusterPoints(points(i).cluster) = ""
                    End If

                    clusterPoints(points(i).cluster) += " " & points(i).ToString()
                Next

                ' if result of sumFinc is better than previous, save the configuration
                If stepSumFunc < resultSumFunc Then
                    resultSumFunc = stepSumFunc

                    For i As Integer = 0 To k - 1
                        medoids(i) = stepmedoids(i)
                        resultClusterPoints(i) = clusterPoints(i)
                    Next

                    Call Array.ConstrainedCopy(points, Scan0, resultPoints, Scan0, points.Length)
                End If

                stepSumFunc = 0
                [step] += 1

                ' random swapping medoids with nonmedoids
                Dim clusterSwapRandomCost As Integer() = New Integer(k - 1) {}
                Dim indexOfSwapCandidate As Integer() = New Integer(k - 1) {}

                Dim randomValue As Integer = 0

                For i As Integer = 0 To points.Length - 1
                    randomValue = randf.NextInteger(Integer.MaxValue)

                    If clusterSwapRandomCost(points(i).cluster) < randomValue AndAlso stepmedoids(points(i).cluster) IsNot points(i) Then
                        indexOfSwapCandidate(points(i).cluster) = i
                        clusterSwapRandomCost(points(i).cluster) = randomValue
                    End If
                Next

                For i As Integer = 0 To k - 1
                    stepmedoids(i) = points(indexOfSwapCandidate(i))
                Next
            End While

            Return resultPoints
        End Function
    End Module
End Namespace