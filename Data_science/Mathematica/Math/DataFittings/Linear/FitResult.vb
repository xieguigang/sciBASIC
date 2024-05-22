#Region "Microsoft.VisualBasic::6d42548a2570bfa2c18e954ec8a0f03f, Data_science\Mathematica\Math\DataFittings\Linear\FitResult.vb"

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

    '   Total Lines: 136
    '    Code Lines: 58 (42.65%)
    ' Comment Lines: 66 (48.53%)
    '    - Xml Docs: 95.45%
    ' 
    '   Blank Lines: 12 (8.82%)
    '     File Size: 4.75 KB


    ' Class FitResult
    ' 
    '     Properties: AdjustR_square, ErrorTest, FactorSize, Intercept, IsPolyFit
    '                 Polynomial, R_square, RMSE, Slope, SSE
    '                 SSR
    ' 
    '     Function: IFitted_GetY, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' 线性回归结果
''' </summary>
''' <remarks>
''' 在讨论模型时，所谓“线性”并不意味就是直线。回归模型相对于参数是线性的，但是相对于解释变量可以是非线性关系。
''' 比如以下这些常见形式都是线性回归模型：
''' 
''' |模型名称   |表达式                            |
''' |-----------|---------------------------------|
''' |一般线性模型|    y  = c + b*    x  +         u|
''' |线性对数模型|    y  = c + b*log(x) +         u|
''' |对数线性模型|log(y) = c + b*    x  +         u|
''' |双对数模型  |log(y) = c + b*log(x) +         u|
''' |二次回归模型|    y  = c + b*    x  + r*x^2 + u|
''' </remarks>
Public Class FitResult : Implements IFitted

    ''' <summary>
    ''' 拟合后的方程系数，根据阶次获取拟合方程的系数，
    ''' 如getFactor(2)，就是获取``y = a0 + a1*x + a2*x^2 + ... + apoly_n*x^poly_n``中a2的值
    ''' </summary>
    Public Property Polynomial As Formula Implements IFitted.Polynomial
    ''' <summary>
    ''' 回归平方和
    ''' </summary>
    Public Property SSR As Double
    ''' <summary>
    ''' (剩余平方和)
    ''' </summary>
    Public Property SSE As Double
    ''' <summary>
    ''' RMSE均方根误差
    ''' </summary>
    Public Property RMSE As Double
    ''' <summary>
    ''' 保存拟合后的y值，在拟合时可设置为不保存节省内存
    ''' </summary>
    Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

    ''' <summary>
    ''' 根据x获取拟合方程的y值
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property GetY(x As Double) As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return DirectCast(Polynomial, Polynomial).F(x)
        End Get
    End Property

    Public ReadOnly Property IsPolyFit As Boolean
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Not DirectCast(Polynomial, Polynomial).IsLinear
        End Get
    End Property

    ''' <summary>
    ''' 获取斜率
    ''' </summary>
    ''' <returns>斜率值</returns>
    Public ReadOnly Property Slope() As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return DirectCast(Polynomial, Polynomial).Factors(1)
        End Get
    End Property

    ''' <summary>
    ''' 获取截距
    ''' </summary>
    ''' <returns>截距值</returns>
    Public ReadOnly Property Intercept() As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return DirectCast(Polynomial, Polynomial).Factors(0)
        End Get
    End Property

    ''' <summary>
    ''' 确定系数，系数是0~1之间的数，是数理上判定拟合优度的一个量
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property R_square() As Double Implements IFitted.R2
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return 1 - (SSE / (SSR + SSE))
        End Get
    End Property

    ''' <summary>
    ''' 调整R方（Adjusted R-Square）
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' Adjusted R-Squared 抵消样本数量对 R-Squared 的影响，做到了真正的 0~1，越大越好。
    ''' </remarks>
    Public ReadOnly Property AdjustR_square As Double
        Get
            ' n 是样本数量，p 是特征数量。
            Dim n As Integer = ErrorTest.Length
            Dim p As Integer = FactorSize - 1

            Return 1 - (1 - R_square) * (n - 1) / (n - p - 1)
        End Get
    End Property

    ''' <summary>
    ''' 获取拟合方程系数的个数
    ''' </summary>
    ''' <returns>拟合方程系数的个数</returns>
    Public ReadOnly Property FactorSize As Integer
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return DirectCast(Polynomial, Polynomial).Factors.Length
        End Get
    End Property

    ''' <summary>
    ''' <see cref="Polynomial.Factors"/>:
    ''' 
    ''' ``y = a0 + a1*x + a2*x^2 + ... + apoly_n*x^poly_n``
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Return $"{Polynomial} @ R2={R_square.ToString("F4")}"
    End Function

    Private Function IFitted_GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
        Return Polynomial.Evaluate(x)
    End Function
End Class
