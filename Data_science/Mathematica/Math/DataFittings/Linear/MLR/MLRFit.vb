#Region "Microsoft.VisualBasic::18802beaad5693656daf767f529b47b2, Data_science\Mathematica\Math\DataFittings\Linear\MLR\MLRFit.vb"

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

    '   Total Lines: 96
    '    Code Lines: 39 (40.62%)
    ' Comment Lines: 48 (50.00%)
    '    - Xml Docs: 95.83%
    ' 
    '   Blank Lines: 9 (9.38%)
    '     File Size: 5.01 KB


    '     Class MLRFit
    ' 
    '         Properties: beta, ErrorTest, Fx, N, p
    '                     Polynomial, R2, SSE, SST
    ' 
    '         Function: GetY, LinearFitting
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Multivariate

    ''' <summary>
    ''' Multiple linear regression.(多元线性回归)
    ''' 
    ''' Problem of predicting appropriate values of given feature set as inputvector
    ''' using supervised linear regression with multiple dimensional sample input 
    ''' </summary>
    Public Class MLRFit : Implements IFitted

        ''' <summary>
        ''' 样本数量(观测点的个数)。(The number of samples / observed data points.)
        ''' </summary>
        Public Property N As Integer
        ''' <summary>
        ''' number of dependent variables
        ''' </summary>
        Public Property p As Integer
        ''' <summary>
        ''' regression coefficients
        ''' </summary>
        Public Property beta As Double()
        ''' <summary>
        ''' 残差平方和 (Sum of Squared Errors, SSE)，即拟合值与实际观测值偏差的平方和。(The sum of squared errors between fitted and observed values.)
        ''' </summary>
        Public Property SSE As Double
        ''' <summary>
        ''' 总平方和 (Total Sum of Squares, SST)，即观测值偏离其均值的平方和，用于计算拟合优度 R2。(The total sum of squares of observed values about their mean, used for computing R2.)
        ''' </summary>
        Public Property SST As Double

        Public ReadOnly Property R2 As Double Implements IFitted.R2
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return 1.0 - SSE / SST
            End Get
        End Property

        ''' <summary>
        ''' Evaluate the regression value from a given X vector
        ''' 
        ''' ```
        ''' f(x) = ax1 + bx2 + cx3 + dx4 + ...
        ''' ```
        ''' </summary>
        ''' <param name="x">自变量向量，各元素依次对应各特征维度的系数。(The independent variable vector; each element corresponds to a feature dimension's coefficient.)</param>
        ''' <returns>回归模型对向量 x 的预测值 y-hat。(The predicted value y-hat of the regression model for vector x.)</returns>
        Public Overridable ReadOnly Property Fx(x As Vector) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (x * beta).Sum
            End Get
        End Property

        ''' <summary>
        ''' 由回归系数 beta 构造的多元多项式模型。(The multivariate polynomial model constructed from the regression coefficients beta.)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New MultivariatePolynomial With {.Factors = beta}
            End Get
        End Property

        ''' <summary>
        ''' 拟合后的逐点误差测试结果，保存每个样本点的实际值与预测值。(The per-point fit error test results, storing the actual and predicted values for each sample point.)
        ''' </summary>
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

        ''' <summary>
        ''' 使用给定的自变量值计算多元线性回归的预测值 y-hat。(Computes the predicted value y-hat of the multiple linear regression for the given independent variables.)
        ''' </summary>
        ''' <param name="x">自变量值序列，依次对应各特征维度。(The independent variable values, in order of feature dimensions.)</param>
        ''' <returns>回归模型对 x 的预测值。(The predicted value of the regression model for x.)</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            Return Fx(New Vector(x))
        End Function

        ''' <summary>
        ''' 便捷入口：对给定的自变量矩阵与观测值向量执行多元线性回归拟合。(A convenience entry point that performs multiple linear regression fitting on the given feature matrix and observed values.)
        ''' </summary>
        ''' <param name="x">自变量矩阵，每一行是一个样本点。(The feature matrix, each row is a sample point.)</param>
        ''' <param name="f">与 x 各行对应的观测值向量。(The observed value vector corresponding to each row of x.)</param>
        ''' <returns>拟合得到的 <see cref="MLRFit"/> 模型。(The fitted multiple linear regression model.)</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LinearFitting(x As NumericMatrix, f As Vector) As MLRFit
            Return x.LinearFitting(f)
        End Function
    End Class
End Namespace
