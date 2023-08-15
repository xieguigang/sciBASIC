Imports System.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports std = System.Math

Namespace GMM

    ''' <summary>
    ''' GMM problem solver
    ''' </summary>
    Public Class Solver

        Public Shared Function Predicts(ds As IEnumerable(Of ClusterEntity),
                                        Optional components As Integer = 3,
                                        Optional threshold As Double = 0.00001) As Mixture

            Return Training(New Mixture(New DatumList(ds, components)), threshold)
        End Function

        Private Shared Function Training(mix As Mixture, threshold As Double) As Mixture
            Dim dev As TextWriter = App.StdOut

            mix.printStats(dev)

            Dim oldLog As Double = mix.logLike()
            Dim newLog = oldLog - 100.0
            Dim i As i32 = 1

            Do
                oldLog = newLog
                mix.Expectation()
                mix.Maximization()
                newLog = mix.logLike()

                dev.WriteLine($" [{vbTab}{++i}]{vbTab}new-loglike: {newLog}")
            Loop While newLog <> 0 AndAlso std.Abs(newLog - oldLog) > threshold

            mix.printStats(dev)

            Return mix
        End Function

        Public Shared Function Predicts(x As IEnumerable(Of Double),
                                        Optional components As Integer = 3,
                                        Optional threshold As Double = 0.00001) As Mixture

            Return Training(New Mixture(New DatumList(x, components)), threshold)
        End Function
    End Class
End Namespace