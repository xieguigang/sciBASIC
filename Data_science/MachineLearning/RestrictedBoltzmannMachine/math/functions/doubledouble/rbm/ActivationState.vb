#Region "Microsoft.VisualBasic::c0d7d7c87e2c701af4cee88aeddb6f3a, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\doubledouble\rbm\ActivationState.vb"

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

    '   Total Lines: 12
    '    Code Lines: 8
    ' Comment Lines: 3
    '   Blank Lines: 1
    '     File Size: 363 B


    '     Class ActivationState
    ' 
    '         Function: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions.doubledouble.rbm
    ''' <summary>
    ''' Created by kenny on 5/25/14.
    ''' </summary>
    Public Class ActivationState
        Inherits DoubleDoubleFunction
        Public Overrides Function apply(x As Double, y As Double) As Double
            Return If(x >= y, 1.0, 0.0)
        End Function
    End Class

End Namespace
