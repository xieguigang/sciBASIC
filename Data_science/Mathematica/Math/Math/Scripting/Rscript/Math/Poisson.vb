#Region "Microsoft.VisualBasic::4033fc4a09396e9cb10a77d759b7f273, sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\Rscript\Math\Poisson.vb"

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

    '   Total Lines: 111
    '    Code Lines: 51
    ' Comment Lines: 48
    '   Blank Lines: 12
    '     File Size: 5.12 KB


    '     Module Poisson
    ' 
    '         Function: Dpois, qpois, rPois
    ' 
    '     Module Normal
    ' 
    '         Function: dnorm, pnorm, qnorm, (+2 Overloads) rnorm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Scripting.Rscript.MathExtension

    ''' <summary>
    ''' Density, distribution function, quantile function and random generation for the Poisson distribution with parameter lambda.
    ''' </summary>
    ''' <remarks></remarks>
    <Package("RBase.Math.Poisson", Description:="Density, distribution function, quantile function and random generation for the Poisson distribution with parameter lambda.")>
    Public Module Poisson

        ''' <summary>
        ''' Density, distribution function, quantile function and random generation for the Poisson distribution with parameter lambda.
        ''' </summary>
        ''' <param name="n">number of random values to return.</param>
        ''' <param name="lambda">vector of (non-negative) means.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("rPois")>
        Public Function rPois(n As Integer, lambda As Vector) As Vector
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' Density, distribution function, quantile function and random generation for the Poisson distribution with parameter lambda.
        ''' </summary>
        ''' <param name="x">vector of (non-negative integer) quantiles.</param>
        ''' <param name="lambda">vector of (non-negative) means.</param>
        ''' <param name="log">logical; if TRUE, probabilities p are given as log(p).</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Dpois")>
        Public Function Dpois(x As Vector, lambda As Vector, Optional log As Boolean = False) As Vector
            Throw New NotImplementedException
        End Function

        <ExportAPI("qpois")>
        Public Function qpois(p As Vector, lambda As Vector, Optional lowertail As Boolean = True, Optional logp As Boolean = False) As Vector
            Throw New NotImplementedException
        End Function
    End Module

    ''' <summary>
    ''' The Normal Distribution
    ''' Density, distribution function, quantile function and random generation for the normal distribution with mean equal to mean and standard deviation equal to sd.
    ''' </summary>
    ''' <remarks></remarks>
    <[Namespace]("RBase.Math.Normal", Description:="Density, distribution function, quantile function and random generation for the normal distribution with mean equal to mean and standard deviation equal to sd.")>
    Public Module Normal

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="p">vector of probabilities.</param>
        ''' <param name="mean">vector of means.</param>
        ''' <param name="sd">vector of standard deviations.</param>
        ''' <param name="lowertail">logical; if TRUE (default), probabilities are P[X ≤ x] otherwise, P[X > x].</param>
        ''' <param name="logp">logical; if TRUE, probabilities p are given as log(p).</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("qnorm")>
        Public Function qnorm(p As Vector, Optional mean As Vector = NULL, Optional sd As Integer = 1, Optional lowertail As Boolean = True, Optional logp As Boolean = False) As Vector
            If Missing(mean) Then
                mean = Vector.Zero
            End If

            Throw New NotImplementedException
        End Function

        <ExportAPI("pnorm")>
        Public Function pnorm(x As Vector) As Vector
            Throw New NotImplementedException
        End Function

        <ExportAPI("dnorm")>
        Public Function dnorm(x As Vector) As Vector
            Throw New NotImplementedException
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("rnorm")>
        Public Function rnorm(x As Vector, m As Double, sd As Double) As Vector
            Return New Vector(x.Select(Function(n) Distributions.pnorm.ProbabilityDensity(n, m, sd)))
        End Function

        ''' <summary>
        ''' Returns a vector with specific parameters normal distribution of length n elements.
        ''' Distribution data was generated by the n length random seeds as the normal distribution 
        ''' inputs.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="m"></param>
        ''' <param name="sd"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function rnorm(n As Integer, m As Double, sd As Double) As Vector
            ' 不清楚在R之中是否是这样子来实现的
            Return n.Sequence _
                .Select(Function(x) RandomExtensions.NextGaussian(m, sd)) _
                .AsVector
        End Function
    End Module
End Namespace
