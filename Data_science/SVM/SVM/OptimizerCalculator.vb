#Region "Microsoft.VisualBasic::a78e5fbb626ab3464238eb35e6020e2e, ..\sciBASIC#\Data_science\SVM\SVM\OptimizerCalculator.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.SVM.Method
Imports Microsoft.VisualBasic.DataMining.SVM.Model

''' <summary>
''' @author Ralf Wondratschek
''' </summary>
Public Module OptimizerCalculator

    Public C As Double = 100.0

    ''' <summary>
    ''' SVM caller
    ''' </summary>
    ''' <param name="optimizer"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Calculate(optimizer As Optimizer) As Line
        Dim result As Line = optimizer.Optimize()

        ''            
        '' + Sometimes a method diverges, so we need to catch this case
        ''             
        Dim diverged As Boolean =
            Double.IsNaN(result.Offset()) OrElse
            Double.IsNaN(result.NormalVector().W1()) OrElse
            Double.IsNaN(result.NormalVector().W2())

        If (diverged) Then
            result = New Line(New NormalVector({-1, 1}), 0)
        End If

        Return result
    End Function

    <Extension>
    Public Sub Calculate(ByRef CartesianCoordinateSystem As CartesianCoordinateSystem, Optional method As Optimizers = Optimizers.SubGradientDescent)
        With CartesianCoordinateSystem
            Dim line As Line = .Line

            If line Is Nothing Then
                line = New Line(New NormalVector({-1, 1}), 0)
                .Line = (line)
            End If

            line.NormalVector.W(0) = line.NormalVector.W1 / line.NormalVector.W2
            line.NormalVector.W(1) = 1

            Dim optimizer As Optimizer

            Select Case method
                Case Optimizers.SubGradientDescent
                    optimizer = New SubGradientDescent(.Line, .Points)
                Case Optimizers.GradientDescent
                    optimizer = New GradientDescent(.Line, .Points)
                Case Optimizers.NewtonMethod
                    optimizer = New NewtonMethod(.Line, .Points)
                Case Else
                    Throw New NotImplementedException
            End Select

            .Line = optimizer.Calculate
        End With
    End Sub
End Module

