#Region "Microsoft.VisualBasic::0395eaa63384cb39da588b3b04db5552, Data_science\MachineLearning\MachineLearning\SVM\Training.vb"

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

    '   Total Lines: 100
    '    Code Lines: 53 (53.00%)
    ' Comment Lines: 32 (32.00%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 15 (15.00%)
    '     File Size: 4.06 KB


    '     Module Training
    ' 
    '         Function: doCrossValidation, PerformCrossValidation, Train
    ' 
    '         Sub: SetRandomSeed
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

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace SVM

    ''' <summary>
    ''' Class containing the routines to train SVM models.
    ''' </summary>
    Public Module Training

        Private Function doCrossValidation(problem As Problem, parameters As Parameter, nr_fold As Integer) As Double
            Dim i As Integer
            Dim target = New SVMPrediction(problem.count - 1) {}

            Call svm_cross_validation(problem, parameters, nr_fold, target)

            Dim total_correct = 0
            Dim total_error As Double = 0
            Dim sumv As Double = 0, sumy As Double = 0, sumvv As Double = 0, sumyy As Double = 0, sumvy As Double = 0

            If parameters.svmType = SvmType.EPSILON_SVR OrElse parameters.svmType = SvmType.NU_SVR Then
                For i = 0 To problem.count - 1
                    Dim y = problem.Y(i)
                    Dim v = target(i)
                    total_error += (v.unifyValue - y) * (v.unifyValue - y)
                    sumv += v.unifyValue
                    sumy += y
                    sumvv += v.unifyValue ^ 2
                    sumyy += y * y
                    sumvy += v.unifyValue * y
                Next

                Return (problem.count * sumvy - sumv * sumy) / (stdNum.Sqrt(problem.count * sumvv - sumv * sumv) * stdNum.Sqrt(problem.count * sumyy - sumy * sumy))
            Else

                For i = 0 To problem.count - 1
                    If target(i).class = problem.Y(i) Then
                        total_correct += 1
                    End If
                Next
            End If

            Return total_correct / problem.count
        End Function

        Public Sub SetRandomSeed(seed As Integer)
            Procedures.setRandomSeed(seed)
        End Sub

        ''' <summary>
        ''' Performs cross validation.
        ''' </summary>
        ''' <param name="problem">The training data</param>
        ''' <param name="parameters">The parameters to test</param>
        ''' <param name="nrfold">The number of cross validations to use</param>
        ''' <returns>The cross validation score</returns>
        Public Function PerformCrossValidation(problem As Problem, parameters As Parameter, nrfold As Integer) As Double
            Dim [error] = svm_check_parameter(problem, parameters)

            If [error].StringEmpty Then
                Return doCrossValidation(problem, parameters, nrfold)
            Else
                Throw New Exception([error])
            End If
        End Function

        ''' <summary>
        ''' Trains a model using the provided training data and parameters.
        ''' </summary>
        ''' <param name="problem">The training data</param>
        ''' <param name="parameters">The parameters to use</param>
        ''' <returns>A trained SVM Model</returns>
        Public Function Train(problem As Problem, parameters As Parameter) As Model
            Dim [error] As String = svm_check_parameter(problem, parameters)

            If [error].StringEmpty Then
                Return svm_train(problem, parameters)
            Else
                Throw New Exception([error])
            End If
        End Function
    End Module
End Namespace
