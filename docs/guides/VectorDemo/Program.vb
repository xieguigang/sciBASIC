Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.ComponentModel.Ranges

Module Program

    Sub Main()

        Dim strings = New Func(Of WeightString)(AddressOf WeightString.Rand).RepeatCalls(200).VectorShadows

        Dim asciiRands$() = strings.str
        Dim strWeights#() = strings.weight

        Dim subsetLessThan50 As WeightString() = strings(strings <= 50)


        Pause()
    End Sub
End Module

Public Structure WeightString

    Public Property str$
    Public Property weight#

    Public Overrides Function ToString() As String
        Return $"({weight}%)  {str}"
    End Function

    Public Shared Function Rand() As WeightString
        Return New WeightString With {
            .str = RandomASCIIString(20),
            .weight = Math.GetRandomValue(New DoubleRange(1, 100))
        }
    End Function

    Public Shared Operator <=(w As WeightString, n#) As Boolean
        Return w.weight <= n
    End Operator

    Public Shared Operator >=(w As WeightString, n#) As Boolean
        Return w.weight >= n
    End Operator
End Structure