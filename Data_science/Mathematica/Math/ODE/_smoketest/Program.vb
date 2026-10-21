Imports std = System.Math

''' <summary>
''' The self test entry point of the ODE solver package.
''' 
''' ```shell
''' dotnet _smoketest/bin/Debug/net10.0/smoketest.dll
''' ```
''' </summary>
Module Program

    Friend ReadOnly failures As New List(Of String)
    Friend assertions As Integer

    Function Main(args As String()) As Integer
        Console.WriteLine("=== Microsoft.VisualBasic.Math.ODEsSolver self test ===")

        Call Cases.RunScalarCases()
        Call Cases.RunDynamicsCases()
        Call Cases.RunAdaptiveCases()
        Call Cases.RunRobustnessCases()
        Call Benchmark.Run()

        Console.WriteLine()
        Console.WriteLine($"assertions: {assertions}, failure(s): {failures.Count}")

        If failures.Count = 0 Then
            Console.WriteLine("PASS: 0 failure(s)")
            Return 0
        Else
            For Each message As String In failures
                Console.WriteLine("  failed: " & message)
            Next

            Console.WriteLine($"FAIL: {failures.Count} failure(s)")
            Return 1
        End If
    End Function

    ''' <summary>
    ''' Assert the given condition.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="condition"></param>
    ''' <param name="detail"></param>
    Public Sub Check(name As String, condition As Boolean, Optional detail As String = "")
        assertions += 1

        If Not condition Then
            failures.Add(name & " " & detail)
            Console.WriteLine($"  [fail] {name} {detail}")
        End If
    End Sub

    ''' <summary>
    ''' Assert that the numeric result is equal to the expected value in the given tolerance.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="actual"></param>
    ''' <param name="expected"></param>
    ''' <param name="tolerance"></param>
    Public Sub CheckClose(name As String, actual As Double, expected As Double, tolerance As Double)
        Dim diff As Double = std.Abs(actual - expected)

        Call Check(name, diff <= tolerance,
                   $"expected={expected:G6} actual={actual:G6} diff={diff:G4} tolerance={tolerance:G4}")
    End Sub

    ''' <summary>
    ''' Assert that the given action throws the expected exception type.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="name"></param>
    ''' <param name="action"></param>
    Public Sub CheckThrows(Of T As Exception)(name As String, action As Action)
        assertions += 1

        Try
            Call action()

            failures.Add(name & ": expected " & GetType(T).Name & ", but no exception was thrown")
            Console.WriteLine($"  [fail] {name}: expected {GetType(T).Name}")
        Catch ex As T
            ' expected
        Catch ex As Exception
            failures.Add(name & ": expected " & GetType(T).Name & ", but got " & ex.GetType().Name & ": " & ex.Message)
            Console.WriteLine($"  [fail] {name}: expected {GetType(T).Name}, got {ex.GetType().Name}")
        End Try
    End Sub

    ''' <summary>
    ''' Get the last value of the given array.
    ''' </summary>
    <Runtime.CompilerServices.Extension>
    Public Function LastValue(v As Double()) As Double
        If v Is Nothing OrElse v.Length = 0 Then
            Return Double.NaN
        Else
            Return v(v.Length - 1)
        End If
    End Function

    ''' <summary>
    ''' Get the max absolute difference between the two arrays.
    ''' </summary>
    <Runtime.CompilerServices.Extension>
    Public Function MaxDiff(a As Double(), b As Double()) As Double
        Dim max As Double = 0.0

        If a Is Nothing OrElse b Is Nothing Then
            Return Double.NaN
        End If
        If a.Length <> b.Length Then
            Return Double.PositiveInfinity
        End If

        For i As Integer = 0 To a.Length - 1
            Dim d As Double = std.Abs(a(i) - b(i))

            If d > max Then
                max = d
            End If
        Next

        Return max
    End Function
End Module
