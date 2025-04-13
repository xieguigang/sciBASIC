Imports System.Text

Namespace LASSO
    ''' <summary>
    ''' This class is a container for arrays and values that
    ''' are computed during computation of a lasso fit. It also
    ''' contains the final weights of features.
    ''' 
    ''' @author Yasser Ganjisaffar (http://www.ics.uci.edu/~yganjisa/)
    ''' </summary>

    Public Class LassoFit
        ' Number of lambda values
        Public numberOfLambdas As Integer

        ' Intercepts
        Public intercepts As Double()

        ' Compressed weights for each solution
        Public compressedWeights As Double()()

        ' Pointers to compressed weights
        Public indices As Integer()

        ' Number of weights for each solution
        Public numberOfWeights As Integer()

        ' Number of non-zero weights for each solution
        Public nonZeroWeights As Integer()

        ' The value of lambdas for each solution
        Public lambdas As Double()

        ' R^2 value for each solution
        Public rsquared As Double()

        ' Total number of passes over data
        Public numberOfPasses As Integer

        Private numFeatures As Integer

        Public Sub New(numberOfLambdas As Integer, maxAllowedFeaturesAlongPath As Integer, numFeatures As Integer)
            intercepts = New Double(numberOfLambdas - 1) {}
            compressedWeights = MathUtil.allocateDoubleMatrix(numberOfLambdas, maxAllowedFeaturesAlongPath)
            indices = New Integer(maxAllowedFeaturesAlongPath - 1) {}
            numberOfWeights = New Integer(numberOfLambdas - 1) {}
            lambdas = New Double(numberOfLambdas - 1) {}
            rsquared = New Double(numberOfLambdas - 1) {}
            nonZeroWeights = New Integer(numberOfLambdas - 1) {}
            Me.numFeatures = numFeatures
        End Sub

        Public Overridable Function getWeights(lambdaIdx As Integer) As Double()
            Dim weights = New Double(numFeatures - 1) {}
            For i = 0 To numberOfWeights(lambdaIdx) - 1
                weights(indices(i)) = compressedWeights(lambdaIdx)(i)
            Next
            Return weights
        End Function

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim numberOfSolutions = numberOfLambdas
            sb.Append("Compression R2 values:" & vbLf)
            For i = 0 To numberOfSolutions - 1
                sb.Append(i + 1.ToString() & vbTab & nonZeroWeights(i).ToString() & vbTab & MathUtil.getFormattedDouble(rsquared(i), 4) & vbTab & MathUtil.getFormattedDouble(lambdas(i), 5) & vbLf)
            Next
            Return sb.ToString().Trim()
        End Function

    End Class

End Namespace
