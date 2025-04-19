#Region "Microsoft.VisualBasic::d2c9a5bbe2a9db6fa426f54ad398f917, Data_science\Mathematica\Math\DataFittings\LASSO\LassoFit.vb"

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

    '   Total Lines: 139
    '    Code Lines: 66 (47.48%)
    ' Comment Lines: 47 (33.81%)
    '    - Xml Docs: 97.87%
    ' 
    '   Blank Lines: 26 (18.71%)
    '     File Size: 4.89 KB


    '     Class LassoFit
    ' 
    '         Properties: compressedWeights, featureNames, indices, intercepts, lambdas
    '                     nonZeroWeights, numberOfLambdas, numberOfPasses, numberOfWeights, numFeatures
    '                     rsquared
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: getWeights, toDataFrame, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

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

        Public Property featureNames As String()

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

        Public Function toDataFrame() As Dictionary(Of String, Array)
            Dim len As Integer = intercepts.Length
            Dim weights As New Dictionary(Of String, Double())

            For i As Integer = 0 To featureNames.Length - 1
                Dim offset = indices(i)
                Dim vector As New List(Of Double)

                For idx As Integer = 0 To len - 1
                    Call vector.Add(compressedWeights(idx)(i))
                Next

                weights(featureNames(offset)) = vector.ToArray
            Next

            Dim output As New Dictionary(Of String, Array) From {
                {"intercepts", intercepts},
                {"Df", nonZeroWeights},
                {"rsquared", rsquared},
                {"lambdas", lambdas},
                {"numberOfWeights", numberOfWeights}
            }

            For Each w In weights
                Call output.Add("w." & w.Key, w.Value)
            Next

            Return output
        End Function

    End Class

End Namespace

