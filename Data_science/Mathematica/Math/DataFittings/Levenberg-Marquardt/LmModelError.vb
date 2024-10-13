#Region "Microsoft.VisualBasic::63c8e4089d99fae84907ff194c928eda, Data_science\Mathematica\Math\DataFittings\Levenberg-Marquardt\LmModelError.vb"

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

    '   Total Lines: 48
    '    Code Lines: 10 (20.83%)
    ' Comment Lines: 33 (68.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (10.42%)
    '     File Size: 2.35 KB


    '     Class LmModelError
    ' 
    '         Function: hessian
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace LevenbergMarquardt
    ''' <summary>
    ''' Created by duy on 20/1/15.
    ''' </summary>
    Public MustInherit Class LmModelError

        ''' <summary>
        ''' Evaluates the error function with input optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <returns> Double value of the error function </returns>
        MustOverride Function eval(optParams As Double()) As Double

        ''' <summary>
        ''' Computes the Jacobian vector of the error function with input
        ''' optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <returns> Jacobian vector of the error function </returns>
        MustOverride Function jacobian(optParams As Double()) As Double()

        ''' <summary>
        ''' Computes the Hessian matrix of the error function with input
        ''' optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <param name="approxHessianFlg"> A boolean flag to indicate whether the Hessian
        '''                         matrix can be approximated instead of having to be
        '''                         computed exactly </param>
        ''' <returns> Hessian matrix of the error function </returns>
        MustOverride Function hessian(optParams As Double(), approxHessianFlg As Boolean) As Double()()

        ''' <summary>
        ''' Computes the Hessian matrix of the error function with input
        ''' optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <returns> The exact Hessian matrix of the error function </returns>
        Protected Function hessian(optParams As Double()) As Double()()
            Return hessian(optParams, False)
        End Function
    End Class

End Namespace

