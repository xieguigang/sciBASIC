#Region "Microsoft.VisualBasic::e4c72b277708c782047ac955e18ac269, Data_science\MachineLearning\MachineLearning\SVM\Parameter\ParameterSelection.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 215
    '    Code Lines: 86 (40.00%)
    ' Comment Lines: 113 (52.56%)
    '    - Xml Docs: 85.84%
    ' 
    '   Blank Lines: 16 (7.44%)
    '     File Size: 12.17 KB


    '     Module ParameterSelection
    ' 
    '         Function: GetList, (+5 Overloads) Grid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Runtime.InteropServices
Imports System.Threading
Imports stdNum = System.Math

Namespace SVM

    ''' <summary>
    ''' This class contains routines which perform parameter selection for a model which uses C-SVC and
    ''' an RBF kernel.
    ''' </summary>
    Public Module ParameterSelection

        ''' <summary>
        ''' Default number of times to divide the data.
        ''' </summary>
        Public Const NFOLD As Integer = 5
        ''' <summary>
        ''' Default minimum power of 2 for the C value (-5)
        ''' </summary>
        Public Const MIN_C As Integer = -5
        ''' <summary>
        ''' Default maximum power of 2 for the C value (15)
        ''' </summary>
        Public Const MAX_C As Integer = 15
        ''' <summary>
        ''' Default power iteration step for the C value (2)
        ''' </summary>
        Public Const C_STEP As Integer = 2
        ''' <summary>
        ''' Default minimum power of 2 for the Gamma value (-15)
        ''' </summary>
        Public Const MIN_G As Integer = -15
        ''' <summary>
        ''' Default maximum power of 2 for the Gamma Value (3)
        ''' </summary>
        Public Const MAX_G As Integer = 3
        ''' <summary>
        ''' Default power iteration step for the Gamma value (2)
        ''' </summary>
        Public Const G_STEP As Integer = 2

        ''' <summary>
        ''' Used to control the degree of parallelism used in grid exploration.  Default value is the number of processors.
        ''' </summary>
        Public Threads As Integer = Environment.ProcessorCount

        ''' <summary>
        ''' Returns a logarithmic list of values from minimum power of 2 to the maximum power of 2 using the provided iteration size.
        ''' </summary>
        ''' <param name="minPower">The minimum power of 2</param>
        ''' <param name="maxPower">The maximum power of 2</param>
        ''' <param name="iteration">The iteration size to use in powers</param>
        ''' <returns></returns>
        Public Function GetList(minPower As Double, maxPower As Double, iteration As Double) As List(Of Double)
            Dim list As List(Of Double) = New List(Of Double)()
            Dim d = minPower

            While d <= maxPower
                list.Add(stdNum.Pow(2, d))
                d += iteration
            End While

            Return list
        End Function

        ''' <summary>
        ''' Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the
        ''' combination which performed best.  The default ranges of C and Gamma values are used.  Use this method if there is no validation data available, and it will
        ''' divide it 5 times to allow 5-fold validation (training on 4/5 and validating on 1/5, 5 times).
        ''' </summary>
        ''' <param name="problem">The training data</param>
        ''' <param name="createParams">The parameters to use when optimizing</param>
        ''' <param name="report">Function used to report results</param>
        ''' <param name="C">The optimal C value will be put into this variable</param>
        ''' <param name="Gamma">The optimal Gamma value will be put into this variable</param>
        ''' <returns>A list of grid squares and their results</returns>
        Public Function Grid(problem As Problem, createParams As Func(Of Parameter), report As Action(Of GridSquare), <Out> ByRef C As Double, <Out> ByRef Gamma As Double) As List(Of GridSquare)
            Return Grid(problem, createParams, GetList(MIN_C, MAX_C, C_STEP), GetList(MIN_G, MAX_G, G_STEP), report, NFOLD, C, Gamma)
        End Function
        ''' <summary>
        ''' Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the
        ''' combination which performed best.  Use this method if there is no validation data available, and it will
        ''' divide it 5 times to allow 5-fold validation (training on 4/5 and validating on 1/5, 5 times).
        ''' </summary>
        ''' <param name="problem">The training data</param>
        ''' <param name="createParams">The parameters to use when optimizing</param>
        ''' <param name="CValues">The set of C values to use</param>
        ''' <param name="GammaValues">The set of Gamma values to use</param>
        ''' <param name="report">Function used to report results</param>
        ''' <param name="C">The optimal C value will be put into this variable</param>
        ''' <param name="Gamma">The optimal Gamma value will be put into this variable</param>
        ''' <returns>A list of grid squares and their results</returns>
        Public Function Grid(problem As Problem, createParams As Func(Of Parameter), CValues As List(Of Double), GammaValues As List(Of Double), report As Action(Of GridSquare), <Out> ByRef C As Double, <Out> ByRef Gamma As Double) As List(Of GridSquare)
            Return Grid(problem, createParams, CValues, GammaValues, report, NFOLD, C, Gamma)
        End Function

        ''' <summary>
        ''' Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the
        ''' combination which performed best.  Use this method if validation data isn't available, as it will
        ''' divide the training data and train on a portion of it and test on the rest.
        ''' </summary>
        ''' <param name="problem">The training data</param>
        ''' <param name="createParams">The parameters to use when optimizing</param>
        ''' <param name="CValues">The set of C values to use</param>
        ''' <param name="GammaValues">The set of Gamma values to use</param>
        ''' <param name="report">Function used to report results</param>
        ''' <param name="nrfold">The number of times the data should be divided for validation</param>
        ''' <param name="C">The optimal C value will be placed in this variable</param>
        ''' <param name="Gamma">The optimal Gamma value will be placed in this variable</param>
        ''' <returns>A list of grid squares and their results</returns>
        Public Function Grid(problem As Problem, createParams As Func(Of Parameter), CValues As List(Of Double), GammaValues As List(Of Double), report As Action(Of GridSquare), nrfold As Integer, <Out> ByRef C As Double, <Out> ByRef Gamma As Double) As List(Of GridSquare)
            C = 0
            Gamma = 0
            Dim squares As List(Of GridSquare) = New List(Of GridSquare)()

            For Each testC In CValues

                For Each testGamma In GammaValues
                    squares.Add(New GridSquare With {
                        .C = testC,
                        .Gamma = testGamma
                    })
                Next
            Next

            Dim parameters As ThreadLocal(Of Parameter) = New ThreadLocal(Of Parameter)(Function() createParams())
            System.Threading.Tasks.Parallel.ForEach(squares, New ParallelOptions With {
                .MaxDegreeOfParallelism = Threads
            }, Sub(square)
                   parameters.Value.c = square.C
                   parameters.Value.gamma = square.Gamma
                   square.Score = PerformCrossValidation(problem, parameters.Value, nrfold)
                   If report IsNot Nothing Then report(square)
               End Sub)
            Dim best As GridSquare = squares.OrderByDescending(Function(o) o.Score).First()
            C = best.C
            Gamma = best.Gamma
            Return squares.OrderBy(Function(o) o.C).ThenBy(Function(o) o.Gamma).ToList()
        End Function
        ''' <summary>
        ''' Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the
        ''' combination which performed best.  Uses the default values of C and Gamma.
        ''' </summary>
        ''' <param name="problem">The training data</param>
        ''' <param name="validation">The validation data</param>
        ''' <param name="createParams">The parameters to use when optimizing</param>
        ''' <param name="report">Function used to report results</param>
        ''' <param name="C">The optimal C value will be placed in this variable</param>
        ''' <param name="Gamma">The optimal Gamma value will be placed in this variable</param>
        ''' <returns>A list of grid squares and their results</returns>
        Public Function Grid(problem As Problem, validation As Problem, createParams As Func(Of Parameter), report As Action(Of GridSquare), <Out> ByRef C As Double, <Out> ByRef Gamma As Double) As List(Of GridSquare)
            Return Grid(problem, validation, createParams, GetList(MIN_C, MAX_C, C_STEP), GetList(MIN_G, MAX_G, G_STEP), report, C, Gamma)
        End Function
        ''' <summary>
        ''' Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the
        ''' combination which performed best.
        ''' </summary>
        ''' <param name="problem">The training data</param>
        ''' <param name="validation">The validation data</param>
        ''' <param name="createParams">The parameters to use when optimizing</param>
        ''' <param name="CValues">The C values to use</param>
        ''' <param name="GammaValues">The Gamma values to use</param>
        ''' <param name="report">Function used to report results</param>
        ''' <param name="C">The optimal C value will be placed in this variable</param>
        ''' <param name="Gamma">The optimal Gamma value will be placed in this variable</param>
        ''' <returns>A list of grid squares and their results</returns>
        Public Function Grid(problem As Problem, validation As Problem, createParams As Func(Of Parameter), CValues As List(Of Double), GammaValues As List(Of Double), report As Action(Of GridSquare), <Out> ByRef C As Double, <Out> ByRef Gamma As Double) As List(Of GridSquare)
            C = 0
            Gamma = 0
            Dim squares As List(Of GridSquare) = New List(Of GridSquare)()

            For Each testC In CValues

                For Each testGamma In GammaValues
                    squares.Add(New GridSquare With {
                        .C = testC,
                        .Gamma = testGamma
                    })
                Next
            Next

            Dim parameters As ThreadLocal(Of Parameter) = New ThreadLocal(Of Parameter)(Function() createParams())
            System.Threading.Tasks.Parallel.ForEach(squares, New ParallelOptions With {
                .MaxDegreeOfParallelism = Threads
            }, Sub(square)
                   parameters.Value.c = square.C
                   parameters.Value.gamma = square.Gamma
                   Dim model = Train(problem, parameters.Value)
                   square.Score = Predict(validation, Nothing, model, False)
                   If report IsNot Nothing Then report(square)
               End Sub)
            Dim best As GridSquare = squares.OrderByDescending(Function(o) o.Score).First()
            C = best.C
            Gamma = best.Gamma
            Return squares.OrderBy(Function(o) o.C).ThenBy(Function(o) o.Gamma).ToList()
        End Function
    End Module
End Namespace
