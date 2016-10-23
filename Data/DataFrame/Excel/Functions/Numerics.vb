#Region "Microsoft.VisualBasic::ed8d1c7cda64319631d9aa5e4dbb2648, ..\visualbasic_App\Data\DataFrame\Excel\Functions\Numerics.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices

Namespace Excel

    Public Module Numerics

        <Extension>
        Public Function SUM(data As DocumentStream.File, ParamArray cells As String()) As Double
            Return cells.Select(Function(c) data.CellValue(c).ParseDouble).Sum
        End Function

        <Extension>
        Public Function SUM(data As DocumentStream.File, range As String) As Double
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
