
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Math.Information

    ''' <summary>
    ''' Unweighted entropy similarity
    ''' </summary>
    Public Module EntropyScore

        <Extension>
        Private Function Entropy(v As Dictionary(Of String, Double)) As Double
            Dim sumAll As Double = v.Values.Sum
            Dim p As Double() = (From x As Double In v.Values Select x / sumAll).ToArray
            Dim s As Double = p.ShannonEntropy

            Return s
        End Function

        ''' <summary>
        ''' Unweighted entropy similarity
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DiffEntropy(v1 As Dictionary(Of String, Double), v2 As Dictionary(Of String, Double)) As Double
            Dim SAB = Mix(v1, v2).Entropy
            Dim SA = v1.Entropy
            Dim SB = v2.Entropy
            ' Unweighted entropy similarity
            Dim s As Double = 1 - (2 * SAB - SA - SB) / stdNum.Log(4)

            Return If(s < 0, 0, s)
        End Function

        Private Function Mix(v1 As Dictionary(Of String, Double), v2 As Dictionary(Of String, Double)) As Dictionary(Of String, Double)
            Return v1 _
                .JoinIterates(v2) _
                .GroupBy(Function(d) d.Key) _
                .ToDictionary(Function(d) d.Key,
                              Function(d)
                                  Return Aggregate x In d Into Sum(x.Value)
                              End Function)
        End Function

    End Module
End Namespace