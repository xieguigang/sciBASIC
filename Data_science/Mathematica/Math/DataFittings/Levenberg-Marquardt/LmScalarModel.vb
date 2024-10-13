#Region "Microsoft.VisualBasic::93515bc9aa863db821479856db3caa23, Data_science\Mathematica\Math\DataFittings\Levenberg-Marquardt\LmScalarModel.vb"

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

    '   Total Lines: 44
    '    Code Lines: 8 (18.18%)
    ' Comment Lines: 29 (65.91%)
    '    - Xml Docs: 96.55%
    ' 
    '   Blank Lines: 7 (15.91%)
    '     File Size: 2.02 KB


    '     Interface LmScalarModel
    ' 
    '         Properties: MeasuredData
    ' 
    '         Function: eval, hessian, jacobian
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace LevenbergMarquardt

    ' Created by duy on 27/1/15.


    ''' <summary>
    ''' LmScalarModel is an interface for models (functions) whose ranges are
    ''' single real-valued numbers
    ''' </summary>
    Public Interface LmScalarModel
        ''' <summary>
        ''' Gets the vector of real numbers containing measured output data,
        ''' </summary>
        ReadOnly Property MeasuredData As Double()

        ''' <summary>
        ''' Evaluates the model's estimated output for the k-th input data that
        ''' corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model </param>
        ''' <returns> Estimated output value produced by the model </returns>
        Function eval(dataIdx As Integer, optParams As Double()) As Double

        ''' <summary>
        ''' Computes the model's Jacobian vector for the k-th input data that
        ''' corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model </param>
        ''' <returns> Jacobian vector of the model for the specified input data </returns>
        Function jacobian(dataIdx As Integer, optParams As Double()) As Double()

        ''' <summary>
        ''' Computes the model's Hessian matrix for the k-th input data that
        ''' corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model </param>
        ''' <returns> Hessian matrix of the model for the specified input data </returns>
        Function hessian(dataIdx As Integer, optParams As Double()) As Double()()
    End Interface

End Namespace

