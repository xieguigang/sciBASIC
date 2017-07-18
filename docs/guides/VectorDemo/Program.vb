Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic

Module Program

    Sub Main()

        Dim strings = New Func(Of WeightString)(AddressOf WeightString.Rand).RepeatCalls(200, sleep:=2).VectorShadows

        Dim asciiRands$() = strings.str
        Dim strWeights#() = strings.weight

        Dim subsetLessThan50 As WeightString() = strings(strings <= 50)
        Dim subsetGreaterThan90 As WeightString() = strings(strings >= 90)

        strings.weight = 200.Sequence.As(Of Double)

        subsetLessThan50 = strings(strings <= 50)
        subsetGreaterThan90 = strings(strings >= 90)

        Dim target As Char = RandomASCIIString(20)(10)

        Dim charsCount%() = strings.Count(target)
        Dim sums%() = strings.Sum

        Pause()
    End Sub
End Module

Public Class WeightString

    Public Property str$
    Public Property weight#

    Public Overrides Function ToString() As String
        Return $"({weight}%)  {str}"
    End Function

    Public Function Count(c As Char) As Integer
        Return str.Count(c)
    End Function

    Public Function Sum() As Integer
        Return str.Select(AddressOf Asc).Sum
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
End Class