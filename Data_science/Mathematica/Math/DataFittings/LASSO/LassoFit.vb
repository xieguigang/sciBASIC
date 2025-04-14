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

        ''' <summary>
        ''' Number of lambda values
        ''' </summary>
        ''' <returns></returns>
        Public Property numberOfLambdas As Integer

        ''' <summary>
        ''' Intercepts
        ''' </summary>
        ''' <returns></returns>
        Public Property intercepts As Double()

        ''' <summary>
        ''' Compressed weights for each solution
        ''' </summary>
        ''' <returns></returns>
        Public Property compressedWeights As Double()()

        ''' <summary>
        ''' Pointers to compressed weights
        ''' </summary>
        ''' <returns></returns>
        Public Property indices As Integer()

        ''' <summary>
        ''' Number of weights for each solution
        ''' </summary>
        ''' <returns></returns>
        Public Property numberOfWeights As Integer()

        ''' <summary>
        ''' Number of non-zero weights for each solution
        ''' </summary>
        ''' <returns></returns>
        Public Property nonZeroWeights As Integer()

        ''' <summary>
        ''' The value of lambdas for each solution
        ''' </summary>
        ''' <returns></returns>
        Public Property lambdas As Double()

        ''' <summary>
        ''' R^2 value for each solution
        ''' </summary>
        ''' <returns></returns>
        Public Property rsquared As Double()

        ''' <summary>
        ''' Total number of passes over data
        ''' </summary>
        ''' <returns></returns>
        Public Property numberOfPasses As Integer

        ''' <summary>
        ''' number of the data features that input from the training data
        ''' </summary>
        ''' <returns></returns>
        Public Property numFeatures As Integer

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
            Dim sb As New StringBuilder()
            Dim numberOfSolutions = numberOfLambdas
            sb.Append("Compression R2 values:" & vbLf)
            For i = 0 To numberOfSolutions - 1
                sb.Append(i + 1.ToString() & vbTab & nonZeroWeights(i).ToString() & vbTab & MathUtil.getFormattedDouble(rsquared(i), 4) & vbTab & MathUtil.getFormattedDouble(lambdas(i), 5) & vbLf)
            Next
            Return sb.ToString().Trim()
        End Function

    End Class

End Namespace
