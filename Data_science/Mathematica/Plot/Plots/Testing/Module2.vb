Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module2

    Sub Main()

        Call CreateAxisTicks({-10.3301, 13.7566}, 20).GetJson(True).__DEBUG_ECHO

        Pause()

    End Sub

    ''' <summary>
    ''' ### An Algorithm for Creating and Selecting Graph Axes
    ''' > http://austinclemens.com/blog/2016/01/09/an-algorithm-for-creating-a-graphs-axes/
    ''' </summary>
    ''' <param name="dataSeries#"></param>
    ''' <param name="ticks%"></param>
    ''' <param name="decimalDigits%"></param>
    ''' <returns></returns>
    <Extension>
    Function CreateAxisTicks(dataSeries#(), Optional ticks% = 10, Optional decimalDigits% = 2) As Double()

        ' First, get the minimum and maximum of the series, toggle the zero_flag variable 
        ' if 0 Is between Then the min And max, And Get the range Of the data.
        Dim max# = dataSeries.Max
        Dim min# = dataSeries.Min
        Dim zeroFlag As Boolean = False

        If (min <= 0 AndAlso max >= 0) Then
            zeroFlag = True
        End If

        Dim range = max - min
        Dim niceTicks#() = {0.1, 0.2, 0.5, 1, 0.15, 0.25, 0.75}
        Dim steps = range / (ticks - 1)
        Dim rounded As Double
        Dim digits As Integer


        If (steps >= 1) Then
            rounded = Math.Round(steps)
            digits = rounded.ToString().Length
        Else
            Dim places = steps.ToString().Split("."c)(1)
            Dim firstPlace% = 0

            For i = 0 To places.Length - 1
                If (places(i) <> "0" AndAlso firstPlace = 0) Then
                    firstPlace = i
                End If
            Next

            digits = -firstPlace
        End If

        Dim candidateSteps As New List(Of Double)

        For i As Integer = 0 To niceTicks.Length - 1
            candidateSteps.Add(niceTicks(i) * Math.Pow(10, digits))
            candidateSteps.Add(niceTicks(i) * Math.Pow(10, digits - 1))
            candidateSteps.Add(niceTicks(i) * Math.Pow(10, digits + 1))
        Next

        Dim minSteps As Double
        Dim stepArray As New List(Of Double)
        Dim candidateArray#() = {}

        For i As Integer = 0 To candidateSteps.Count - 1
            steps = candidateSteps(i)

            ' starting value depends on whether Or Not 0 Is in the array
            If (zeroFlag) Then
                minSteps = Math.Ceiling(Math.Abs(min) / steps)
                stepArray = {-minSteps * steps}.AsList
            Else
                stepArray = {Math.Floor(min / steps) * steps}.AsList
            End If

            Dim stepnum% = 1

            Do While (stepArray(stepArray.Count - 1) < max)
                stepArray.Add((stepArray(0) + steps * stepnum))
                stepnum += 1
            Loop

            ' this arbitrarily enforces step_arrays of length between 4 And 10
            If (stepArray.Count < 11 AndAlso stepArray.Count > 4) Then
                If candidateArray.Length < stepArray.Count Then
                    candidateArray = stepArray.ToArray
                End If
            End If
        Next

        For i As Integer = 0 To candidateArray.Length - 1
            candidateArray(i) = Math.Round(candidateArray(i), decimalDigits)
        Next

        Return candidateArray
    End Function
End Module
