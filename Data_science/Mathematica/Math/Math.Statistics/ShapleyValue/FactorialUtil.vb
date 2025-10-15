#Region "Microsoft.VisualBasic::6c1edd4ad1cabefac0768926157d860a, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\FactorialUtil.vb"

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
    '    Code Lines: 10 (83.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (16.67%)
    '     File Size: 371 B


    '     Class FactorialUtil
    ' 
    '         Function: factorial
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ShapleyValue

    Public Class FactorialUtil

        Public Shared Function factorial(n As Long) As Long
            If n > 20 Then
                Throw New ArithmeticException("Capacity exceded for factorial " & n.ToString())
            End If
            Return If(n > 1, n * factorial(n - 1), 1)
        End Function
    End Class
End Namespace
