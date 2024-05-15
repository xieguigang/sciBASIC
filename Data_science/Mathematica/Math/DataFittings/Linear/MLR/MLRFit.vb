#Region "Microsoft.VisualBasic::0890285a632e5d592d319fbc95415233, Data_science\Mathematica\Math\DataFittings\Linear\MLR\MLRFit.vb"

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

    '   Total Lines: 75
    '    Code Lines: 39
    ' Comment Lines: 27
    '   Blank Lines: 9
    '     File Size: 2.58 KB


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
        ''' 
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
        ''' sum of squared
        ''' </summary>
        Public Property SSE As Double
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
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Fx(x As Vector) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (x * beta).Sum
            End Get
        End Property

        Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New MultivariatePolynomial With {.Factors = beta}
            End Get
        End Property

        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            Return Fx(New Vector(x))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LinearFitting(x As NumericMatrix, f As Vector) As MLRFit
            Return x.LinearFitting(f)
        End Function
    End Class
End Namespace
