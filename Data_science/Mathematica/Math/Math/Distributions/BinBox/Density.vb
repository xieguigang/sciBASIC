#Region "Microsoft.VisualBasic::5d2906a6f31f0bb524b51d8a777d6a88, Data_science\Mathematica\Math\Math\Distributions\BinBox\Density.vb"

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

    '   Total Lines: 13
    '    Code Lines: 9 (69.23%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (30.77%)
    '     File Size: 313 B


    '     Class Density
    ' 
    '         Properties: axis, density
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Distributions.BinBox

    Public Class Density

        Public Property axis As Double
        Public Property density As Double

        Public Shared Narrowing Operator CType(density As Density) As Double
            Return density.density
        End Operator

    End Class
End Namespace
