Imports Microsoft.VisualBasic.Serialization.JSON

Module Module2

    Sub Main()

        Call create_axis({-10.3301, 13.7566}, 20).GetJson(True).__DEBUG_ECHO

        Pause()

    End Sub

    Function create_axis(dataseries#(), ticks%, Optional decimalDigits% = 2) As Double()

        Dim maxdata = dataseries.Max
        Dim mindata = dataseries.Min
        Dim zero_flag = 0

        If (mindata <= 0 AndAlso maxdata >= 0) Then
            zero_flag = 1
        End If

        Dim Range = maxdata - mindata
        Dim nice_ticks = {0.1, 0.2, 0.5, 1, 0.15, 0.25, 0.75}


        Dim steps = Range / (ticks - 1)
        Dim rounded
        Dim digits


        If (steps >= 1) Then
            rounded = Math.Round(steps)
            digits = rounded.ToString().Length
        Else
            Dim places = steps.ToString().Split("."c)(1)
            Dim first_place = 0
            For i = 0 To places.Length - 1
                If (places(i) <> "0" AndAlso first_place = 0) Then
                    first_place = i
                End If
            Next
            digits = -CInt(first_place)
        End If


        Dim candidate_steps As New List(Of Double)
        For i = 0 To nice_ticks.Length - 1
            candidate_steps.Add(nice_ticks(i) * Math.Pow(10, digits))
            candidate_steps.Add(nice_ticks(i) * Math.Pow(10, digits - 1))
            candidate_steps.Add(nice_ticks(i) * Math.Pow(10, digits + 1))
        Next

        Dim min_steps As Double
        Dim step_array As New List(Of Double)
        Dim candidate_arrays As New List(Of Double)
        Dim maxLenghth As Double() = {}

        For i = 0 To candidate_steps.Count - 1
            steps = candidate_steps(i)

            ' starting value depends on whether Or Not 0 Is in the array
            If (zero_flag = 1) Then
                min_steps = Math.Ceiling(Math.Abs(mindata) / steps)
                step_array = {-min_steps * steps}.AsList
            Else
                step_array = {Math.Floor(mindata / steps) * steps}.AsList
            End If

            Dim stepnum = 1
            Do While (step_array(step_array.Count - 1) < maxdata)
                step_array.Add((step_array(0) + steps * stepnum))
                stepnum += 1
            Loop

            ' this arbitrarily enforces step_arrays of length between 4 And 10
            If (step_array.Count < 11 AndAlso step_array.Count > 4) Then
                If maxLenghth.Length < step_array.Count Then
                    maxLenghth = step_array.ToArray
                End If
            End If
        Next

        For i As Integer = 0 To maxLenghth.Length - 1
            maxLenghth(i) = Math.Round(maxLenghth(i), decimalDigits)
        Next

        Return maxLenghth
    End Function

End Module
