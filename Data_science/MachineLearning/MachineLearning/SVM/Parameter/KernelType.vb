#Region "Microsoft.VisualBasic::9744808cc8a340d85c9c452ef09d504d, Data_science\MachineLearning\MachineLearning\SVM\Parameter\KernelType.vb"

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

    '   Total Lines: 28
    '    Code Lines: 9 (32.14%)
    ' Comment Lines: 18 (64.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (3.57%)
    '     File Size: 706 B


    '     Enum KernelType
    ' 
    '         LINEAR, POLY, PRECOMPUTED, RBF, SIGMOID
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVM

    ''' <summary>
    ''' Contains the various kernel types this library can use.
    ''' </summary>
    Public Enum KernelType
        ''' <summary>
        ''' Linear: u'*v
        ''' </summary>
        LINEAR
        ''' <summary>
        ''' Polynomial: (gamma*u'*v + coef0)^degree
        ''' </summary>
        POLY
        ''' <summary>
        ''' Radial basis function: exp(-gamma*|u-v|^2)
        ''' </summary>
        RBF
        ''' <summary>
        ''' Sigmoid: tanh(gamma*u'*v + coef0)
        ''' </summary>
        SIGMOID
        ''' <summary>
        ''' Precomputed kernel
        ''' </summary>
        PRECOMPUTED
    End Enum
End Namespace
