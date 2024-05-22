﻿#Region "Microsoft.VisualBasic::29d81b272cab652301eb9579beeb7524, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\RangeModel\IRange.vb"

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

    '   Total Lines: 16
    '    Code Lines: 6 (37.50%)
    ' Comment Lines: 6 (37.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (25.00%)
    '     File Size: 349 B


    '     Interface IRange
    ' 
    '         Properties: Max, Min
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Ranges.Model

    Public Interface IRange(Of T As IComparable)

        ''' <summary>
        ''' Minimum value
        ''' </summary>
        ReadOnly Property Min As T

        ''' <summary>
        ''' Maximum value
        ''' </summary>
        ReadOnly Property Max As T

    End Interface
End Namespace
