#Region "Microsoft.VisualBasic::c9da8f29acd9f7ff8fc92e8d45e0942b, Data_science\Mathematica\Math\DataFittings\Linear\MLR\Error.vb"

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

    '   Total Lines: 32
    '    Code Lines: 25 (78.12%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (21.88%)
    '     File Size: 1.05 KB


    '     Structure [Error]
    ' 
    '         Properties: X, Y, Yfit
    ' 
    '         Function: RunTest, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace Multivariate

    ''' <summary>
    ''' 多元线性回归在单个样本点上的拟合误差记录。(The fit error of a multiple linear regression at a single sample point.)
    ''' </summary>
    Public Structure [Error] : Implements IFitError

        ''' <summary>
        ''' 样本点的自变量向量 x。(The feature/independent variable vector of the sample point.)
        ''' </summary>
        Public Property X As Vector
        ''' <summary>
        ''' 样本点的实际观测值 y。(The actual observed dependent value y.)
        ''' </summary>
        Public Property Y As Double Implements IFitError.Y
        ''' <summary>
        ''' 多元线性回归模型对样本点的预测值 y-hat。(The predicted value y-hat from the multiple linear regression model.)
        ''' </summary>
        Public Property Yfit As Double Implements IFitError.Yfit

        ''' <summary>
        ''' 返回该样本点拟合误差的文本表示，格式为 ``|y - yfit| = |y - yfit|``。
        ''' (Returns the text representation of the absolute fit error.)
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"{std.Abs(Y - Yfit)} = |{Y} - {Yfit}|"
        End Function

        ''' <summary>
        ''' 对给定样本矩阵 X 与观测值向量 Y 逐一运行拟合测试，生成每个样本点的拟合误差记录。
        ''' (Runs the fit test against the given sample matrix X and observed values Y, yielding the fit error for each sample point.)
        ''' </summary>
        ''' <param name="MLR">已训练好的多元线性回归模型。(The trained multiple linear regression model.)</param>
        ''' <param name="X">样本自变量矩阵，每一行代表一个样本点的特征向量。(The sample feature matrix, each row is a sample's feature vector.)</param>
        ''' <param name="Y">与 X 各行对应的实际观测值向量。(The observed values corresponding to each row of X.)</param>
        ''' <returns>每个样本点拟合误差 <see cref="[Error]"/> 的枚举序列。(An enumerable sequence of fit errors per sample point.)</returns>
        Public Shared Iterator Function RunTest(MLR As MLRFit, X As GeneralMatrix, Y As Vector) As IEnumerable(Of [Error])
            For Each xi In X.RowVectors.SeqIterator
                Dim yi = Y.Item(index:=xi)
                Dim yfit = MLR.Fx(xi)

                Yield New [Error] With {
                    .X = xi,
                    .Y = yi,
                    .Yfit = yfit
                }
            Next
        End Function
    End Structure

End Namespace
