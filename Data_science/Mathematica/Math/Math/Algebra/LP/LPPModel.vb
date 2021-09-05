Imports Microsoft.VisualBasic.ComponentModel

Namespace LinearAlgebra.LinearProgramming

    Public Class LPPModel : Inherits XmlDataModel

        Public Property objectiveFunctionType As String
        Public Property variableNames As String()
        Public Property objectiveFunctionCoefficients As Double()
        Public Property constraintCoefficients As Double()()
        Public Property constraintTypes As String()
        Public Property constraintRightHandSides As Double()
        Public Property objectiveFunctionValue As Double

        ''' <summary>
        ''' the model name
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String

    End Class
End Namespace