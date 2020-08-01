Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

Namespace FuzzyCMeans

    Public Class MembershipMatrix

        ReadOnly matrix As New Dictionary(Of FuzzyCMeansEntity, List(Of Double))

        Public Iterator Function GetMembershipMatrix() As IEnumerable(Of List(Of Double))
            For Each item In matrix
                Yield item.Value
            Next
        End Function

        Public Iterator Function GetMemberships() As IEnumerable(Of (entity As FuzzyCMeansEntity, membership As List(Of Double)))
            For Each item In matrix
                Yield (item.Key, item.Value)
            Next
        End Function

        Public Shared Function CreateMembershipMatrix(distancesToClusterCenters As MembershipMatrix, fuzzificationParameter As Double) As MembershipMatrix
            Dim map As New MembershipMatrix

            For Each pair In distancesToClusterCenters.matrix
                Dim unNormaizedMembershipValues As New List(Of Double)()
                Dim sum As Double = 0
                Dim rawValues As List(Of Double) = pair.Value

                For i As Integer = 0 To rawValues.Count - 1
                    Dim distance As Double = rawValues(i)

                    If distance = 0 Then
                        distance = 0.0000001
                    End If

                    Dim membershipValue As Double = stdNum.Pow(1 / distance, (1 / (fuzzificationParameter - 1)))
                    sum += membershipValue
                    unNormaizedMembershipValues.Add(membershipValue)
                Next

                Dim membershipValues As New List(Of Double)()

                For Each membershipValue As Double In unNormaizedMembershipValues
                    membershipValues.Add((membershipValue / sum))
                Next

                map.matrix.Add(pair.Key, membershipValues)
            Next

            Return map
        End Function

        Public Shared Function DistanceToClusterCenters(ls As IEnumerable(Of FuzzyCMeansEntity), clusterCenters As List(Of FuzzyCMeansEntity)) As MembershipMatrix
            Dim map As New MembershipMatrix

            For Each x As FuzzyCMeansEntity In ls
                Dim distancesToCenters As New List(Of Double)()

                For Each c As FuzzyCMeansEntity In clusterCenters
                    Dim distance As Double

                    For i As Integer = 0 To x.Length - 1
                        distance += stdNum.Pow(x(i) - c(i), 2)
                    Next

                    distance = stdNum.Sqrt(distance)
                    distancesToCenters.Add(distance)
                Next

                Call map.matrix.Add(x, distancesToCenters)
            Next

            Return map
        End Function
    End Class
End Namespace