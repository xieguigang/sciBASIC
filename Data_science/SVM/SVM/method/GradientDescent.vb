#Region "Microsoft.VisualBasic::9a240a864f264452270b59bdba5ba11b, ..\sciBASIC#\Data_science\SVM\SVM\method\GradientDescent.vb"

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

Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.SVM.Model

Namespace Method

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class GradientDescent
        Inherits Optimizer

        Private Const DEFAULT_ITERATIONS As Integer = 10000
        Private Const STOP_DIFFERENCE As Double = 0.00001

        Public Sub New(line As Line, points As IList(Of LabeledPoint))
            Me.New(line, points, DEFAULT_ITERATIONS)
        End Sub

        Public Sub New(line As Line, points As IList(Of LabeledPoint), iterations As Integer)
            MyBase.New(line, points, iterations)
        End Sub

        Protected Friend Overrides Function innerOptimize() As Line
            Dim arguments As SvmArgument() = New SvmArgument(_iterations) {}
            Dim stepSize As Double = 1.0R / __lipschitzConstant(_points)

            arguments(0) = New SvmArgument(_line.NormalVector.Clone(), _line.Offset)

            For i As Integer = 1 To arguments.Length - 1
                If _cancelled Then
                    Return Nothing
                End If

                Dim derivation As SvmArgument = calcDerivation(arguments(i - 1))
                arguments(i) = arguments(i - 1).Next(stepSize, derivation)

                If [stop](arguments(i - 1), arguments(i)) Then
                    Return arguments(i).ToLine()
                End If
            Next

            Return arguments(arguments.Length - 1).ToLine()
        End Function

        Private Function calcDerivation(arg As SvmArgument) As SvmArgument
            Dim argOffset As Double = arg.Offset
            Dim argVector As NormalVector = arg.NormalVector
            Dim sum As New NormalVector({0, 0})
            Dim offsetSum As Double = 0

            For Each point As LabeledPoint In _points
                Dim factor As Double = 1 - point.Y * ((argVector.W * point.X).Sum + argOffset)
                factor = Math.Max(0, factor) * point.Y

                sum.W = sum.W + point.X * factor
                offsetSum += factor
            Next

            Dim resVec As New NormalVector({argVector.W1 - 2 * C * sum.W1, argVector.W2 - 2 * C * sum.W2})
            Dim resOffset As Double = -2 * C * offsetSum

            Return New SvmArgument(resVec, resOffset)
        End Function

        Private Shared Function __lipschitzConstant(points As LabeledPoint()) As Double
            Dim sum As Double = 0
            Dim sum2 As Double = 0

            For Each p As LabeledPoint In points
                Dim norm As Double = p.X.SumMagnitude
                sum += Math.Pow(norm, 2)
                sum2 += norm
            Next

            sum = 1 + 2 * C * sum
            sum2 = 1 + 2 * C * sum2

            Return Math.Max(Math.Max(Math.Max(sum, sum2), 2 * C), 1)
        End Function

        Private Function [stop](before As SvmArgument, after As SvmArgument) As Boolean
            Return Math.Abs(Math.Abs(before.NormalVector.W1) - Math.Abs(after.NormalVector.W1)) < STOP_DIFFERENCE AndAlso
                Math.Abs(Math.Abs(before.NormalVector.W2) - Math.Abs(after.NormalVector.W2)) < STOP_DIFFERENCE AndAlso
                Math.Abs(Math.Abs(before.Offset) - Math.Abs(after.Offset)) < STOP_DIFFERENCE
        End Function
    End Class

End Namespace
