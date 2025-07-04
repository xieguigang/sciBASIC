#Region "Microsoft.VisualBasic::29597023eca2a3d5d5e2b0ed06f10a59, Data_science\Mathematica\Math\DataFittings\Linear\Extensions.vb"

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

    '   Total Lines: 90
    '    Code Lines: 44 (48.89%)
    ' Comment Lines: 34 (37.78%)
    '    - Xml Docs: 79.41%
    ' 
    '   Blank Lines: 12 (13.33%)
    '     File Size: 2.99 KB


    ' Module Extensions
    ' 
    '     Function: CorrelationCoefficient, DefaultWeights, (+2 Overloads) LinearRegression, X, Y
    '               Yfit
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

<HideModuleName>
Public Module Extensions

    ''' <summary>
    ''' get input X
    ''' </summary>
    ''' <param name="fit"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function X(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) DirectCast(point, TestPoint).X).AsVector
    End Function

    ''' <summary>
    ''' get input Y
    ''' </summary>
    ''' <param name="fit"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Y(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) point.Y).AsVector
    End Function

    ''' <summary>
    ''' get predicted Y
    ''' </summary>
    ''' <param name="fit"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Yfit(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) point.Yfit).AsVector
    End Function

    ''' <summary>
    ''' 对标准曲线进行线性回归建模
    ''' 
    ''' + ``<paramref name="weighted"/> = TRUE``: <see cref="WeightedFit"/>
    ''' + ``<paramref name="weighted"/> = FALSE``: <see cref="FitResult"/>
    ''' 
    ''' </summary>
    ''' <param name="line"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function LinearRegression(line As PointF(), weighted As Weights) As IFitted
        ' X是实验值，可能会因为标准曲线溶液配制的问题出现，
        ' 所以这个可能会需要使用异常点检测
        Dim X As Vector = line.X.AsVector
        ' Y是从文件之中读取出来的浓度梯度信息，'
        ' 认为这个除非文件录入有错， 否则将不会出现异常点
        Dim Y As Vector = line.Y.AsVector

        Return LinearRegression(X, Y, weighted)
    End Function

    Public Function DefaultWeights(X As Vector) As Vector
        Return 1 / (X ^ 2)
    End Function

    Public Function LinearRegression(X As Vector, Y As Vector, weighted As Weights) As IFitted
        Dim fit As IFitted

        If Not weighted Is Nothing Then
            fit = WeightedLinearRegression.Regress(X, Y, weighted(X), 1)
        Else
            fit = LeastSquares.LinearFit(X, Y)
        End If

        Return fit
    End Function

    ''' <summary>
    ''' Sqrt of R<sup>2</sup>
    ''' </summary>
    ''' <param name="lm"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CorrelationCoefficient(lm As IFitted) As Double
        Return std.Sqrt(lm.R2)
    End Function

End Module
