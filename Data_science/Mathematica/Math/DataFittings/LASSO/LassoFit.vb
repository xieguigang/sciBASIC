#Region "Microsoft.VisualBasic::00e2910b47047f63df86c44e973fe124, Data_science\Mathematica\Math\DataFittings\LASSO\LassoFit.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data

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
            compressedWeights = RectangularArray.Matrix(Of Double)(numberOfLambdas, maxAllowedFeaturesAlongPath)
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

        <Obsolete("use toNumericTable instead", False)>
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

        ''' <summary>
        ''' 将 LASSO 的正则化路径导出为统一的二维表对象 <see cref="NumericTable"/>：
        ''' 
        ''' 1. 每一行对应 lambda 路径上面的一个步骤
        ''' 2. 表的第一列是模型的截距，其余的特征列是各个变量的回归系数
        ''' 3. 标签矩阵之中附带 ``rsquared``、``lambdas``、``Df`` 与
        '''    ``numberOfWeights`` 这几个路径指标
        ''' </summary>
        ''' <param name="featureNames">
        ''' 特征列的名称。为空的时候使用 <see cref="featureNames"/> 属性；
        ''' 数量不匹配的时候自动生成 ``[1]..[n]`` 序号
        ''' </param>
        ''' <returns></returns>
        Public Function toNumericTable(Optional featureNames As String() = Nothing) As NumericTable
            Dim names As String() = If(featureNames, Me.featureNames)

            If names Is Nothing OrElse names.Length <> numFeatures Then
                names = Enumerable.Range(1, numFeatures).Select(Function(i) $"[{i}]").ToArray
            End If

            Dim lambdaSize As Integer = intercepts.Length
            Dim header As String() = New String(names.Length) {}
            Dim labelNames As String() = {"intercepts", "rsquared", "lambdas", "Df", "numberOfWeights"}
            Dim matrix As Double()() = New Double(lambdaSize - 1)() {}
            Dim labels As Double()() = New Double(lambdaSize - 1)() {}

            header(0) = "intercept"

            For j As Integer = 0 To names.Length - 1
                header(j + 1) = names(j)
            Next

            For i As Integer = 0 To lambdaSize - 1
                Dim w As Double() = getWeights(i)
                Dim row As Double() = New Double(names.Length) {}

                row(0) = intercepts(i)

                For j As Integer = 0 To names.Length - 1
                    row(j + 1) = If(j < w.Length, w(j), 0.0)
                Next

                matrix(i) = row
                labels(i) = New Double() {
                    intercepts(i),
                    rsquared(i),
                    lambdas(i),
                    CDbl(nonZeroWeights(i)),
                    CDbl(numberOfWeights(i))
                }
            Next

            Return New NumericTable(matrix,
                                    Enumerable.Range(1, lambdaSize).Select(Function(i) CStr(i)).ToArray,
                                    header) With {
                .labels = labels,
                .labelNames = labelNames,
                .name = "LASSO regularization path",
                .description = "each row is one step along the lambda regularization path"
            }
        End Function

    End Class

End Namespace
