#Region "Microsoft.VisualBasic::0e70c342e660754818246ec64c8bf625, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\Program.vb"

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
Imports System.IO
Imports System.Linq
Imports NeuralNetwork.Network

Friend Class Program
	#Region "-- Constants --"
	Private Const MaxEpochs As Integer = 5000
	Private Const MinimumError As Double = 0.01
	Private Const TrainingType As TrainingType = Network.TrainingType.MinimumError
	#End Region

	#Region "-- Variables --"
	Private Shared _numInputParameters As Integer
	Private Shared _numHiddenLayerNeurons As Integer
	Private Shared _numOutputParameters As Integer
	Private Shared _network As Network.Network
	Private Shared _dataSets As List(Of DataSet)
	#End Region

	#Region "-- Main --"
	Friend Shared Sub Main()
		Greet()
		SetupNetwork()
		TrainNetwork()
		VerifyTraining()
	End Sub
	#End Region

	#Region "-- Network Training --"
	Private Shared Sub TrainNetwork()
		PrintNewLine()
		PrintUnderline(50)
		Console.WriteLine("Training...")

		Train()

		PrintNewLine()
		Console.WriteLine("Training Complete!")
		PrintNewLine()
	End Sub

	Private Shared Sub VerifyTraining()
		Console.WriteLine("Let's test it!")
		PrintNewLine()

		While True
			PrintUnderline(50)
			Dim values = GetInputData(String.Format("Type {0} inputs: ", _numInputParameters))
			Dim results = _network.Compute(values)
			PrintNewLine()

            For Each result In results
                Console.WriteLine("Output: {0}", result)
            Next

            PrintNewLine()

			Dim convertedResults = New Double(results.Length - 1) {}
            For i As Integer = 0 To results.Length - 1
                convertedResults(i) = If(results(i) > 0.5, 1, 0)
            Next

            Dim message = String.Format("Was the result supposed to be {0}? (yes/no/exit)", [String].Join(" ", convertedResults))
			If Not GetBool(message) Then
				Dim offendingDataSet = _dataSets.FirstOrDefault(Function(x) x.Values.SequenceEqual(values) AndAlso x.Targets.SequenceEqual(convertedResults))
				_dataSets.Remove(offendingDataSet)

				Dim expectedResults = GetExpectedResult("What were the expected results?")
				If Not _dataSets.Exists(Function(x) x.Values.SequenceEqual(values) AndAlso x.Targets.SequenceEqual(expectedResults)) Then
					_dataSets.Add(New DataSet(values, expectedResults))
				End If

				PrintNewLine()
				Console.WriteLine("Retraining Network...")
				PrintNewLine()

				Train()
			Else
				PrintNewLine()
				Console.WriteLine("Neat!")
				Console.WriteLine("Encouraging Network...")
				PrintNewLine()

				Train()
			End If
		End While
	End Sub

	Private Shared Sub Train()
		_network.Train(_dataSets, If(TrainingType = TrainingType.Epoch, MaxEpochs, MinimumError))
	End Sub
	#End Region

	#Region "-- Network Setup --"
	Private Shared Sub Greet()
		Console.WriteLine("We're going to create an artificial Neural Network!")
		Console.WriteLine("The network will use back propagation to train itself.")
		PrintUnderline(50)
		PrintNewLine()
	End Sub

	Private Shared Sub SetupNetwork()
		If GetBool("Do you want to read from the space delimited data.txt file? (yes/no/exit)") Then
			SetupFromFile()
		Else
			SetNumInputParameters()
			SetNumNeuronsInHiddenLayer()
			SetNumOutputParameters()
			GetTrainingData()
		End If

		Console.WriteLine("Creating Network...")
		_network = New Network.Network(_numInputParameters, _numHiddenLayerNeurons, _numOutputParameters)
		PrintNewLine()
	End Sub

	Private Shared Sub SetNumInputParameters()
		PrintNewLine()
		Console.WriteLine("How many input parameters will there be? (2 or more)")
		_numInputParameters = GetInput("Input Parameters: ", 2)
		PrintNewLine(2)
	End Sub

	Private Shared Sub SetNumNeuronsInHiddenLayer()
		Console.WriteLine("How many neurons in the hidden layer? (2 or more)")
		_numHiddenLayerNeurons = GetInput("Neurons: ", 2)
		PrintNewLine(2)
	End Sub

	Private Shared Sub SetNumOutputParameters()
		Console.WriteLine("How many output parameters will there be? (1 or more)")
		_numOutputParameters = GetInput("Output Parameters: ", 1)
		PrintNewLine(2)
	End Sub

	Private Shared Sub GetTrainingData()
		PrintUnderline(50)
		Console.WriteLine("Now, we need some input data.")
		PrintNewLine()

		_dataSets = New List(Of DataSet)()
        For i As Integer = 0 To 3
            Dim values = GetInputData([String].Format("Data Set {0}", i + 1))
            Dim expectedResult = GetExpectedResult([String].Format("Expected Result for Data Set {0}:", i + 1))
            _dataSets.Add(New DataSet(values, expectedResult))
        Next
    End Sub

	Private Shared Function GetInputData(message As String) As Double()
		Console.WriteLine(message)
		Dim line = GetLine()

		While line Is Nothing OrElse line.Split(" "C).Count() <> _numInputParameters
			Console.WriteLine("{0} inputs are required.", _numInputParameters)
			PrintNewLine()
			Console.WriteLine(message)
			line = GetLine()
		End While

		Dim values = New Double(_numInputParameters - 1) {}
		Dim lineNums = line.Split(" "C)
        For i As Integer = 0 To lineNums.Length - 1
            Dim num As Double
            If [Double].TryParse(lineNums(i), num) Then
                values(i) = num
            Else
                Console.WriteLine("You entered an invalid number.  Try again")
                PrintNewLine(2)
                Return GetInputData(message)
            End If
        Next

        Return values
	End Function

	Private Shared Function GetExpectedResult(message As String) As Double()
		Console.WriteLine(message)
		Dim line = GetLine()

		While line Is Nothing OrElse line.Split(" "C).Count() <> _numOutputParameters
			Console.WriteLine("{0} outputs are required.", _numOutputParameters)
			PrintNewLine()
			Console.WriteLine(message)
			line = GetLine()
		End While

		Dim values = New Double(_numOutputParameters - 1) {}
		Dim lineNums = line.Split(" "C)
        For i As Integer = 0 To lineNums.Length - 1
            Dim num As Integer
            If Integer.TryParse(lineNums(i), num) AndAlso (num = 0 OrElse num = 1) Then
                values(i) = num
            Else
                Console.WriteLine("You must enter 1s and 0s!")
                PrintNewLine(2)
                Return GetExpectedResult(message)
            End If
        Next

        Return values
	End Function
	#End Region

	#Region "-- I/O Help --"
	Private Shared Sub SetupFromFile()
		_dataSets = New List(Of DataSet)()
		Dim fileContent = File.ReadAllText("data.txt")
        Dim lines = fileContent.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)

        If lines.Length < 2 Then
			WriteError("There aren't enough lines in the file.  The first line should have 3 integers representing the number of inputs, the number of hidden neurons and the number of outputs." & vbCr & vbLf & "There should also be at least one line of data.")
		Else
			Dim setupParameters = lines(0).Split(" "C)
			If setupParameters.Length <> 3 Then
				WriteError("There aren't enough setup parameters.")
			End If

			If Not Integer.TryParse(setupParameters(0), _numInputParameters) OrElse Not Integer.TryParse(setupParameters(1), _numHiddenLayerNeurons) OrElse Not Integer.TryParse(setupParameters(2), _numOutputParameters) Then
				WriteError("The setup parameters are malformed.  There must be 3 integers.")
			End If

			If _numInputParameters < 2 Then
				WriteError("The number of input parameters must be greater than or equal to 2.")
			End If

			If _numHiddenLayerNeurons < 2 Then
				WriteError("The number of hidden neurons must be greater than or equal to 2.")
			End If

			If _numOutputParameters < 1 Then
				WriteError("The number of hidden neurons must be greater than or equal to 1.")
			End If
		End If

        For lineIndex As Integer = 1 To lines.Length - 1
            Dim items = lines(lineIndex).Split(" "c)
            If items.Length <> _numInputParameters + _numOutputParameters Then
                WriteError([String].Format("The data file is malformed.  There were {0} elements on line {1} instead of {2}", items.Length, lineIndex + 1, _numInputParameters + _numOutputParameters))
            End If

            Dim values = New Double(_numInputParameters - 1) {}
            For i As Integer = 0 To _numInputParameters - 1
                Dim num As Double
                If Not Double.TryParse(items(i), num) Then
                    WriteError([String].Format("The data file is malformed.  On line {0}, input parameter {1} is not a valid number.", lineIndex + 1, items(i)))
                Else
                    values(i) = num
                End If
            Next

            Dim expectedResults = New Double(_numOutputParameters - 1) {}
            For i As Integer = 0 To _numOutputParameters - 1
                Dim num As Integer
                If Not Integer.TryParse(items(_numInputParameters + i), num) Then
                    Console.WriteLine("The data file is malformed.  On line {0}, output paramater {1} is not a valid number.", lineIndex, items(i))
                Else
                    expectedResults(i) = num
                End If
            Next
            _dataSets.Add(New DataSet(values, expectedResults))
        Next
    End Sub
	#End Region

	#Region "-- Console Helpers --"

	Private Shared Function GetLine() As String
		Dim line = Console.ReadLine()
		Return If(line Is Nothing, String.Empty, line.Trim())
	End Function

	Private Shared Function GetInput(message As String, min As Integer) As Integer
		Console.Write(message)
		Dim num = GetNumber()

		While num < min
			Console.Write(message)
			num = GetNumber()
		End While

		Return num
	End Function

	Private Shared Function GetNumber() As Integer
		Dim num As Integer
		Dim line = GetLine()
		Return If(line IsNot Nothing AndAlso Integer.TryParse(line, num), num, 0)
	End Function

	Private Shared Function GetBool(message As String) As Boolean
		Console.WriteLine(message)
		Console.Write("Answer: ")
		Dim line = GetLine()

		Dim answer As Boolean
		While line Is Nothing OrElse Not TryGetBoolResponse(line.ToLower(), answer)
			If line = "exit" Then
				Environment.[Exit](0)
			End If

			Console.WriteLine(message)
			Console.Write("Answer: ")
			line = GetLine()
		End While

		PrintNewLine()
		Return answer
	End Function

	Private Shared Function TryGetBoolResponse(line As String, ByRef answer As Boolean) As Boolean
		answer = False
		If String.IsNullOrEmpty(line) Then
			Return False
		End If

		If Boolean.TryParse(line, answer) Then
			Return True
		End If

		Select Case line(0)
			Case "y"C
				answer = True
				Return True
			Case "n"C
				Return True
		End Select

		Return False
	End Function

	Private Shared Sub PrintNewLine(Optional numNewLines As Integer = 1)
        For i As Integer = 0 To numNewLines - 1
            Console.WriteLine()
        Next
    End Sub

	Private Shared Sub PrintUnderline(numUnderlines As Integer)
        For i As Integer = 0 To numUnderlines - 1
            Console.Write("-"c)
        Next
        PrintNewLine(2)
	End Sub

	Private Shared Sub WriteError([error] As String)
		Console.WriteLine([error])
		Console.ReadLine()
		Environment.[Exit](0)
	End Sub
	#End Region
End Class
