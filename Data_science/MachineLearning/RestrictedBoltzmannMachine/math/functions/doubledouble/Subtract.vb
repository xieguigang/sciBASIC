#Region "Microsoft.VisualBasic::eb2b8478ca1058205047174d5cf9e0a1, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\doubledouble\Subtract.vb"

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
    '    Code Lines: 8 (66.67%)
    ' Comment Lines: 3 (25.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (8.33%)
    '     File Size: 339 B


    '     Class Subtract
    ' 
    '         Function: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions.doubledouble
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Subtract
        Inherits DoubleDoubleFunction
        Public Overrides Function apply(v As Double, v2 As Double) As Double
            Return v - v2
        End Function
    End Class

End Namespace
