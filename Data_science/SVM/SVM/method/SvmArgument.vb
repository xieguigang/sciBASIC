Imports Microsoft.VisualBasic.DataMining.SVM.Model

Namespace Method

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class SvmArgument : Implements ICloneable

        Public Property Offset As Double
        Public Property NormalVector As NormalVector

        Public Sub New(normalVector As NormalVector, offset As Double)
            Me.NormalVector = normalVector
            Me.Offset = offset
        End Sub

        Public Function Multipy(factor As Double) As SvmArgument
            Dim res As SvmArgument = Me.Clone()
            res.NormalVector.W = res.NormalVector.W * factor
            res.Offset = res.Offset * factor
            Return res
        End Function

        Public Function Minus(arg As SvmArgument) As SvmArgument
            Dim res As SvmArgument = Me.Clone()
            res.NormalVector.W = res.NormalVector.W - arg.NormalVector.W
            res.Offset = res.Offset - arg.Offset
            Return res
        End Function

        Public Function [Next](stepSize As Double, derivation As SvmArgument) As SvmArgument
            Return Minus(derivation.Multipy(stepSize))
        End Function

        Public Function ToLine() As Line
            Dim ___offset As Double = Offset / NormalVector.W2 * -1
            Return New Line(NormalVector.Clone(), ___offset)
        End Function

        Public Overrides Function ToString() As String
            Return ToLine.ToString
        End Function

        Public Function Norm() As Double
            Return Math.Sqrt(Math.Pow(NormalVector.W1, 2) + Math.Pow(NormalVector.W2, 2) + Math.Pow(Offset, 2))
        End Function

        Public Function Clone() As SvmArgument
            Return New SvmArgument(NormalVector.Clone(), Offset)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
    End Class

End Namespace