#Region "Microsoft.VisualBasic::825d1c3f4fcc3d62dfb1eba787c438a4, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework\Kernel\GeneticAlgorithm\Common.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Collections.Generic
Imports System.Threading

Namespace Kernel.GeneticAlgorithm

    Module Common
      
        Private _inputsAndExpectedOutputs As KeyValuePair(Of Integer, Integer)() = New KeyValuePair(Of Integer, Integer)(2) {}
        Private _baseNeurons As INeuron()

        Private _random As New Random()
        Public Function GetRandom(maxValueExclusive As Integer) As Integer
            Return _random.[Next](maxValueExclusive)
        End Function

        Friend Sub Main(args As String())
            Dim canUse_KeyAvailable As Boolean = True
            Try
                GC.KeepAlive(Console.KeyAvailable)
            Catch
                canUse_KeyAvailable = False
            End Try

            Console.WriteLine("This program will use a genetic algorithm to try to discover a formula to get")
            Console.WriteLine("the results you want. It will not try to find the fastest or smaller formula")
            Console.WriteLine("only one that works.")
            Console.WriteLine("Note that only integer values are considered, so 1/2 * 2 equals 0, as")
            Console.WriteLine("the 1/2 gives 0, not 0.5, and later multiplying it by 2 will keep the 0 value.")
            Console.WriteLine("So take this into account when analysing the results.")

            Console.WriteLine()
            Console.Write("How many input/outputs are you going to provide? ")
            Dim count As Integer = Integer.Parse(Console.ReadLine())


            ' If this was a professional application, we should check if we have at least a count of two.
            ' In fact, we should not parse the strings directly, as the user may have typed an invalid
            ' value.
            ' We should also need to check if there's no duplicate input value, as having the
            ' same input twice with 2 different values means we will never find an answer.
            ' But I did a lazy job. Any invalid value will either crash the application or will
            ' cause it to never end.
            _inputsAndExpectedOutputs = New KeyValuePair(Of Integer, Integer)(count - 1) {}

            For i As Integer = 0 To count - 1
                Console.Write("Give an input value: ")
                Dim input As Integer = Integer.Parse(Console.ReadLine())

                Console.Write("and F(" & input & ") = ")
                Dim output As Integer = Integer.Parse(Console.ReadLine())

                Console.WriteLine()
                _inputsAndExpectedOutputs(i) = New KeyValuePair(Of Integer, Integer)(input, output)
            Next

            Dim initialValues As HashSet(Of Integer) = New HashSet(Of Integer)()
            initialValues.Add(0)
            initialValues.Add(1)
            initialValues.Add(2)
            initialValues.Add(3)
            initialValues.Add(10)

            For Each pair As KeyValuePair(Of Integer, Integer) In _inputsAndExpectedOutputs
                _AddSomeValues(initialValues, pair.Key)
                _AddSomeValues(initialValues, pair.Value)
            Next

            Dim baseNeurons = New List(Of INeuron)()
            baseNeurons.Add(InputNeuron.Instance)
            For Each value As Integer In initialValues
                baseNeurons.Add(New IntNeuron(value))
            Next

            _baseNeurons = baseNeurons.ToArray()

            Console.WriteLine()
            Console.Write(" Starting: ")
            _RunGeneticAlgorithm(canUse_KeyAvailable)
        End Sub

        Private Sub _RunGeneticAlgorithm(canUse_KeyAvailable As Boolean)
            ' I fixed this number of neurons simply because it worked for me. You can play
            ' with this number if you want.
            Dim neurons As INeuron() = New INeuron(5699) {}
            For i As Integer = 0 To neurons.Length - 1
                neurons(i) = _CreateFirstNeuron()
            Next

            Dim results = New KeyValuePair(Of DifferenceInfo, INeuron)(neurons.Length - 1) {}

            Dim iteration As Long = 0
            While True
                iteration += 1

                If iteration Mod 100 = 0 Then
                    Console.Write("."c)
                End If

                _GetAllDifferences_Ordered(neurons, results)

                ' If the user press a key while we run our analysis, we show the
                ' best neuron for the current run.
                Dim bestNeuron = results(0).Value
                If canUse_KeyAvailable AndAlso Console.KeyAvailable Then
                    Console.WriteLine(bestNeuron)
                    Console.ReadKey()
                End If

                Dim found As Boolean = _VerifyIfFinished(iteration, bestNeuron)
                If found Then
                    Return
                End If

                _KillRepetitiveAndOverComplexNeurons(results, iteration)

                If iteration Mod 5700 <> 0 Then
                    _ReproduceNeurons(neurons, results)
                Else
                    _ResetNeurons(neurons)
                End If
            End While
        End Sub

        Private Sub _ResetNeurons(neurons As INeuron())
            Console.WriteLine()
            Console.Write("Resetting: ")

            For i As Integer = 0 To neurons.Length - 1
                neurons(i) = _CreateFirstNeuron()
            Next
        End Sub

        Private Sub _GetAllDifferences_Ordered(neurons As INeuron(), results As KeyValuePair(Of DifferenceInfo, INeuron)())
            For i As Integer = 0 To neurons.Length - 1
                Dim neuron = neurons(i)
                Dim result = GetDifferenceInfo(neurons(i))

                results(i) = New KeyValuePair(Of DifferenceInfo, INeuron)(result, neuron)
            Next

            Array.Sort(results, Function(a, b) a.Key.CompareTo(b.Key))
        End Sub

        Private Function _VerifyIfFinished(iteration As Long, bestNeuron As INeuron) As Boolean
            If GetDifferenceInfo(bestNeuron) = New DifferenceInfo() Then
                Console.WriteLine()
                Console.WriteLine("Found")
                Console.WriteLine("F(x) = " & Convert.ToString(bestNeuron))
                Console.WriteLine("Iterations " & iteration)

                Console.WriteLine()
                Console.WriteLine("Now Testing")

                For Each pair As KeyValuePair(Of Integer, Integer) In _inputsAndExpectedOutputs
                    Dim input As Integer = pair.Key
                    Dim expectedOutput As Integer = pair.Value
                    Dim realOutput As System.Nullable(Of Integer) = bestNeuron.Execute(input)

                    Console.WriteLine("F({0}) = {1}. Is it correct? {2}", input, realOutput, expectedOutput = realOutput)
                Next

                Console.WriteLine("Now write other inputs to see the outputs.")

                While True
                    Dim line As String = Console.ReadLine()
                    If String.IsNullOrWhiteSpace(line) Then
                        Return True
                    End If

                    Dim input As Integer = Integer.Parse(line)
                    Dim output As System.Nullable(Of Integer) = bestNeuron.Execute(input)
                    Console.WriteLine("F({0}) = {1}", input, output)
                End While
            End If

            Return False
        End Function

        Private Sub _KillRepetitiveAndOverComplexNeurons(results As KeyValuePair(Of DifferenceInfo, INeuron)(), iteration As Long)
            If iteration Mod 10 = 0 Then
                Dim previousResult As DifferenceInfo = results(0).Key
                For i As Integer = 1 To results.Length - 1
                    Dim pair As KeyValuePair(Of DifferenceInfo, INeuron) = results(i)
                    If pair.Key = previousResult OrElse pair.Value.Complexity > 15 Then
                        results(i) = New KeyValuePair(Of DifferenceInfo, INeuron)(New DifferenceInfo(), _CreateFirstNeuron())
                    Else
                        previousResult = pair.Key
                    End If
                Next
            End If
        End Sub

        Private Sub _ReproduceNeurons(neurons As INeuron(), results As KeyValuePair(Of DifferenceInfo, INeuron)())
            Dim half As Integer = results.Length \ 2
            Dim neuronIndex As Integer = 0

            For i As Integer = 0 To half - 1
                ' we get the neuron from the ordered results, because we only want
                ' the best neurons to reproduce.
                Dim neuron As INeuron = results(i).Value
                neurons(System.Math.Max(System.Threading.Interlocked.Increment(neuronIndex), neuronIndex - 1)) = neuron.Reproduce()
                neurons(System.Math.Max(System.Threading.Interlocked.Increment(neuronIndex), neuronIndex - 1)) = neuron.Reproduce()
            Next
        End Sub

        Private Sub _AddSomeValues(values As HashSet(Of Integer), value As Integer)
            values.Add(value)
            values.Add(value + (value \ 10))
            values.Add(value - (value \ 10))
            values.Add(value + (value \ 7))
            values.Add(value - (value \ 7))
        End Sub


        Private Function GetDifferenceInfo(neuron As INeuron) As DifferenceInfo
            Dim differenceCount As Integer = 0
            Dim totalDifference As Long = 0

            Dim result1 As System.Nullable(Of Integer) = neuron.Execute(1)
            Dim result2 As System.Nullable(Of Integer) = neuron.Execute(2)
            Dim result3 As System.Nullable(Of Integer) = neuron.Execute(3)

            Try
                For Each pair As KeyValuePair(Of Integer, Integer) In _inputsAndExpectedOutputs
                    Dim input As Integer = pair.Key
                    Dim expectedOutput As Integer = pair.Value

                    Dim output As Integer? = neuron.Execute(input)
                    If Not output.HasValue Then
                        ' the error was so big that we consider as 2 missed values... after all,
                        ' we don't have a _totalMissValue to add here.
                        differenceCount += 2
                        Continue For
                    End If

                    Dim diff As Integer = System.Math.Abs(expectedOutput - output.Value)
                    If diff > 0 Then
                        differenceCount += 1
                        totalDifference += diff
                    End If
                Next

                If differenceCount > 0 Then
                    totalDifference += neuron.Complexity

                    ' we must give the right value for at least 2 inputs
                    ' before we can say that we are getting near some result.
                    ' After all, by simply returning one of the input variables
                    ' we always get at least one right value, and this doesn't
                    ' makes us any near the result.
                    Dim inputCount As Integer = _inputsAndExpectedOutputs.Length
                    If differenceCount >= inputCount - 1 AndAlso differenceCount < inputCount Then
                        differenceCount = inputCount
                    End If
                End If

                Return New DifferenceInfo(differenceCount, totalDifference)
            Catch
                Return New DifferenceInfo(Integer.MaxValue, Long.MaxValue)
            End Try
        End Function

        Private Function _CreateFirstNeuron() As INeuron
            If GetRandom(10) = 0 Then
                Dim result As INeuron = _baseNeurons(GetRandom(_baseNeurons.Length))
                Return result
            End If

            Dim neuron1 As INeuron = _CreateNeuron()
            Dim neuron2 As INeuron = _CreateNeuron()

            Select Case GetRandom(6)
                Case 0
                    Return BinaryNeuron.Add(neuron1, neuron2)
                Case 1
                    Return BinaryNeuron.Subtract(neuron1, neuron2)
                Case 2
                    Return BinaryNeuron.Multiply(neuron1, neuron2)
                Case 3
                    Return BinaryNeuron.Divide(neuron1, neuron2)
                Case 4
                    Return BinaryNeuron.[Mod](neuron1, neuron2)
                Case 5
                    Return BinaryNeuron.Power(neuron1, neuron2)
                Case Else
                    Throw New InvalidOperationException()
            End Select
        End Function
        Private Function _CreateNeuron() As INeuron
            Dim neuronIndex As Integer = GetRandom(_baseNeurons.Length + 1)

            If neuronIndex < _baseNeurons.Length Then
                Return _baseNeurons(neuronIndex)
            End If

            Return _CreateFirstNeuron()
        End Function

        Friend Function _ReproduceNeuron_Random(existingPair As INeuron, neuron1 As INeuron, neuron2 As INeuron) As INeuron
            Select Case GetRandom(21)
                Case 0
                    Return BinaryNeuron.Add(existingPair, New IntNeuron(1))
                Case 1
                    Return BinaryNeuron.Subtract(existingPair, New IntNeuron(1))
                Case 2
                    Return BinaryNeuron.Multiply(existingPair, New IntNeuron(2))
                Case 3
                    Return BinaryNeuron.Divide(existingPair, New IntNeuron(2))
                Case 4
                    Return BinaryNeuron.Add(neuron1, neuron2)
                Case 5
                    Return BinaryNeuron.Subtract(neuron1, neuron2)
                Case 6
                    Return BinaryNeuron.Multiply(neuron1, neuron2)
                Case 7
                    Return BinaryNeuron.Divide(neuron1, neuron2)
                Case 8
                    Return BinaryNeuron.Add(InputNeuron.Instance, existingPair)
                Case 9
                    Return neuron1
                Case 10
                    Return neuron2
                Case 11
                    Return BinaryNeuron.Add(existingPair, InputNeuron.Instance)
                Case 12
                    Return _CreateFirstNeuron()
                Case 13
                    Return _CreateNeuron()
                Case 14
                    Return BinaryNeuron.Subtract(existingPair, InputNeuron.Instance)
                Case 15
                    Return BinaryNeuron.Multiply(existingPair, InputNeuron.Instance)
                Case 16
                    Return BinaryNeuron.Divide(existingPair, InputNeuron.Instance)
                Case 17
                    Return BinaryNeuron.[Mod](existingPair, InputNeuron.Instance)
                Case 18
                    Return BinaryNeuron.[Mod](neuron1, neuron2)
                Case 19
                    Return BinaryNeuron.Power(neuron1, New IntNeuron(2))
                Case 20
                    Return BinaryNeuron.Power(neuron1, neuron2)
                Case Else
                    Throw New InvalidOperationException()
            End Select
        End Function
    End Module
End Namespace
