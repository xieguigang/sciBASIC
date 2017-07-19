Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic

Module Program

    Sub Main()
        ' warm up the CPU
        Dim j%

        For i As Integer = 0 To 200000
            j = i - 100
        Next

        Dim run = Sub()
                      Call Time(AddressOf VectorTest)
                      Call Time(AddressOf LinqTest)

                      ' change order
                      Call Time(AddressOf LinqTest)
                      Call Time(AddressOf VectorTest)
                  End Sub

        For i As Integer = 0 To 10
            Call run()
            Call New String("-", 100).__DEBUG_ECHO
        Next

        Pause()
    End Sub

    Sub VectorTest()

        Dim strings = New Func(Of WeightString)(AddressOf WeightString.Rand) _
            .RepeatCalls(2000, sleep:=2) _
            .VectorShadows

        Dim asciiRands As IEnumerable(Of String) = strings.str & "ABCDE"
        Dim strWeights As IEnumerable(Of Double) = strings.weight

        Dim subsetLessThan50 As IEnumerable(Of WeightString) = strings(strings <= 50)
        Dim subsetGreaterThan90 As IEnumerable(Of WeightString) = strings(strings >= 90)

        strings.weight = 2000.Sequence.As(Of Double)

        subsetLessThan50 = strings(strings <= 50)
        subsetGreaterThan90 = strings(strings >= 90)

        Dim target As Char = RandomASCIIString(20)(10)

        Dim charsCount As IEnumerable(Of Integer) = strings.Count(target)
        Dim sums As IEnumerable(Of Integer) = strings(strings >= 1000).Sum

        '  Pause()
    End Sub

    Sub LinqTest()
        Dim strings = New Func(Of WeightString)(AddressOf WeightString.Rand).RepeatCalls(2000, sleep:=2)

        Dim asciiRands As IEnumerable(Of String) = strings.Select(Function(s) s.str & "ABCDE").ToArray
        Dim strWeights As IEnumerable(Of Double) = strings.Select(Function(s) s.weight).ToArray

        Dim subsetLessThan50 As IEnumerable(Of WeightString) = strings.Where(Function(s) s <= 50).ToArray
        Dim subsetGreaterThan90 As IEnumerable(Of WeightString) = strings.Where(Function(s) s >= 90).ToArray

        For Each w In 2000.Sequence.As(Of Double).SeqIterator
            strings(w).weight = w.value
        Next

        subsetLessThan50 = strings.Where(Function(s) s <= 50).ToArray
        subsetGreaterThan90 = strings.Where(Function(s) s >= 90).ToArray

        Dim target As Char = RandomASCIIString(20)(10)

        Dim charsCount As IEnumerable(Of Integer) = strings.Select(Function(s) s.Count(target)).ToArray
        Dim sums As IEnumerable(Of Integer) = strings.Where(Function(s) s >= 1000).Select(Function(s) s.Sum).ToArray

        ' Pause()
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