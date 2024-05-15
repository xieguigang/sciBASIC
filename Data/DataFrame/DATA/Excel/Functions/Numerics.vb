#Region "Microsoft.VisualBasic::450b3e6ad19d5d069b2311b2521082f0, Data\DataFrame\DATA\Excel\Functions\Numerics.vb"

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

    '   Total Lines: 17
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 495 B


    '     Module Numerics
    ' 
    '         Function: (+2 Overloads) SUM
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Excel

    Public Module Numerics

        <Extension>
        Public Function SUM(data As IO.File, ParamArray cells As String()) As Double
            Return cells.Select(Function(c) data.CellValue(c).ParseDouble).Sum
        End Function

        <Extension>
        Public Function SUM(data As IO.File, range As String) As Double
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
