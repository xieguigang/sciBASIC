Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.SVM.Model

Namespace Method

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class SubGradientDescent : Inherits Optimizer

        Private Const DEFAULT_ITERATIONS As Integer = 500000
        Private Const DEFAULT_STEP_PARAMETER As Double = 100.0R
        Private Const STOP_DIFFERENCE As Double = 0.001

        Private mStepParameter As Double

        Public Sub New(line As Line, points As IList(Of LabeledPoint))
            Me.New(line, points, DEFAULT_ITERATIONS)
        End Sub

        Public Sub New(line As Line, points As IList(Of LabeledPoint), iterations As Integer)
            Me.New(line, points, iterations, DEFAULT_STEP_PARAMETER)
        End Sub

        Public Sub New(line As Line, points As IList(Of LabeledPoint), iterations As Integer, stepParameter As Double)
            MyBase.New(line, points, iterations)
            mStepParameter = stepParameter
        End Sub

        Protected Friend Overrides Function innerOptimize() As Line
            Dim arguments As SvmArgument() = New SvmArgument(_iterations) {}
            arguments(0) = New SvmArgument(_line.NormalVector.Clone(), _line.Offset)

            For i As Integer = 1 To arguments.Length - 1
                If _cancelled Then
                    Return Nothing
                End If

                Dim ___subGradient As SvmArgument = getSubGradient(arguments(i - 1), 1.0R / 2.0R)
                Dim ___stepSize As Double = getStepSize(i, mStepParameter)

                arguments(i) = arguments(i - 1) _
                    .Minus(___subGradient _
                           .Multipy(1.0R / ___subGradient.Norm()) _
                           .Multipy(___stepSize))

                If [stop](arguments(i - 1), arguments(i)) Then
                    Return arguments(i).ToLine()
                End If
            Next

            Return arguments(arguments.Length - 1).ToLine()
        End Function

        Private Function getSubGradient(arg As SvmArgument, t As Double) As SvmArgument
            Dim argOffset As Double = arg.Offset
            Dim argVector As NormalVector = arg.NormalVector

            If t < 0 OrElse t > 1 Then
                Throw New System.ArgumentException
            End If

            Dim sum As New NormalVector({0, 0})
            Dim offsetSum As Double = 0

            For Each point As LabeledPoint In _points

                Dim factor As Double = 1 - point.Y * ((argVector.W * point.X).Sum + argOffset)

                If factor > 0 Then
                    sum.W = sum.W + -1 * point.Y * point.X
                    offsetSum += -1 * point.Y
                ElseIf factor = 0 Then
                    sum.W = sum.W + (1 - t) * -1 * point.Y * point.X
                    offsetSum += (1 - t) * -1 * point.Y
                End If
            Next

            Dim resVec As New NormalVector({argVector.W1 + C * sum.W1, argVector.W2 + C * sum.W2})
            Dim resOffset As Double = C * offsetSum

            Return New SvmArgument(resVec, resOffset)
        End Function

        Private Function getStepSize(iteration As Integer, stepParameter As Double) As Double
            Return stepParameter / iteration
        End Function

        Private Function [stop](before As SvmArgument, after As SvmArgument) As Boolean
            Return Math.Abs(Math.Abs(before.NormalVector.W1) - Math.Abs(after.NormalVector.W1)) < STOP_DIFFERENCE AndAlso
                Math.Abs(Math.Abs(before.NormalVector.W2) - Math.Abs(after.NormalVector.W2)) < STOP_DIFFERENCE AndAlso
                Math.Abs(Math.Abs(before.Offset) - Math.Abs(after.Offset)) < STOP_DIFFERENCE
        End Function
    End Class
End Namespace