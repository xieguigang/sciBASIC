Imports System

Public Class MainX

    Public Shared Function Main() As Integer
        Dim mix As Mixture = New Mixture(New DataSet("", 3))

        mix.printStats()

        Dim oldLog As Double = mix.logLike()
        Dim newLog = oldLog - 100.0
        Do
            oldLog = newLog
            mix.Expectation()
            mix.Maximization()
            newLog = mix.logLike()
            Console.WriteLine(newLog)
        Loop While newLog <> 0 AndAlso Math.Abs(newLog - oldLog) > 0.00000000000001

        mix.printStats()

        Return 0
    End Function

End Class
