Imports Microsoft.VisualBasic.DataMining.KMeans
Imports std = System.Math

Namespace GMM

    ''' <summary>
    ''' GMM problem solver
    ''' </summary>
    Public Class Solver

        Public Shared Function Predicts(ds As IEnumerable(Of ClusterEntity), Optional components As Integer = 3) As Mixture
            Return Training(New Mixture(New DataSet(ds, components)))
        End Function

        Private Shared Function Training(mix As Mixture) As Mixture
            mix.printStats()

            Dim oldLog As Double = mix.logLike()
            Dim newLog = oldLog - 100.0
            Do
                oldLog = newLog
                mix.Expectation()
                mix.Maximization()
                newLog = mix.logLike()
                Console.WriteLine(newLog)
            Loop While newLog <> 0 AndAlso std.Abs(newLog - oldLog) > 0.00000000000001

            mix.printStats()

            Return mix
        End Function

        Public Shared Function Predicts(x As IEnumerable(Of Double), Optional components As Integer = 3) As Mixture
            Return Training(New Mixture(New DataSet(x, components)))
        End Function
    End Class
End Namespace