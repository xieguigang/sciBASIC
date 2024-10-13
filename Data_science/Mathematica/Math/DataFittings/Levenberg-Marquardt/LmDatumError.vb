#Region "Microsoft.VisualBasic::41f671fb4d07491c39dc65ebc98fdb1b, Data_science\Mathematica\Math\DataFittings\Levenberg-Marquardt\LmDatumError.vb"

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

    '   Total Lines: 59
    '    Code Lines: 11 (18.64%)
    ' Comment Lines: 41 (69.49%)
    '    - Xml Docs: 97.56%
    ' 
    '   Blank Lines: 7 (11.86%)
    '     File Size: 2.79 KB


    '     Class LmDatumError
    ' 
    '         Function: hessian
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace LevenbergMarquardt

    ' Created by duy on 14/4/15.

    ''' <summary>
    ''' LmDatumError is an interface for evaluating error, Jacobian matrix and
    ''' Hessian matrix of a single piece of observed data
    ''' </summary>
    Public MustInherit Class LmDatumError
        ''' <summary>
        ''' Gets the total number of observed data
        ''' </summary>
        ''' <returns> An integer which is the number of observed data </returns>
        MustOverride ReadOnly Property NumData As Integer

        ''' <summary>
        ''' Evaluates value of the error function for the k-th observed data that
        ''' corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model
        ''' @return </param>
        MustOverride Function eval(dataIdx As Integer, optParams As Double()) As Double

        ''' <summary>
        ''' Evaluates the Jacobian vector of the error function for the k-th observed
        ''' data that corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model
        ''' @return </param>
        MustOverride Function jacobian(dataIdx As Integer, optParams As Double()) As Double()

        ''' <summary>
        ''' Evaluates the Hessian matrix of the error function for the k-th observed
        ''' data that corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model </param>
        ''' <param name="approxHessianFlg"> A boolean flag to indicate whether the Hessian
        '''                         matrix can be approximated instead of having to be
        '''                         computed exactly
        ''' @return </param>
        MustOverride Function hessian(dataIdx As Integer, optParams As Double(), approxHessianFlg As Boolean) As Double()()

        ''' <summary>
        ''' Evaluates the Hessian matrix of the error function for the k-th observed
        ''' data that corresponds to the parameter vector. The Hessian matrix is
        ''' computed exactly
        ''' </summary>
        ''' <param name="dataIdx"> </param>
        ''' <param name="optParams">
        ''' @return </param>
        Protected Function hessian(dataIdx As Integer, optParams As Double()) As Double()()
            Return hessian(dataIdx, optParams, False)
        End Function
    End Class

End Namespace

