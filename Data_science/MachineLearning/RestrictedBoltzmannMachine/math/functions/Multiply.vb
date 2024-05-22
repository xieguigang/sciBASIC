#Region "Microsoft.VisualBasic::37cea09a7223fd0ef0db80f911b31d68, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\Multiply.vb"

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

    '   Total Lines: 20
    '    Code Lines: 12 (60.00%)
    ' Comment Lines: 3 (15.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (25.00%)
    '     File Size: 447 B


    '     Class Multiply
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Multiply
        Inherits DoubleFunction

        Private ReadOnly value As Double

        Public Sub New(value As Double)
            Me.value = value
        End Sub

        Public Overrides Function apply(v As Double) As Double
            Return v * value
        End Function
    End Class

End Namespace
