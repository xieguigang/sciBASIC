#Region "Microsoft.VisualBasic::e1e905039449dddec899ace141086f3c, ..\sciBASIC#\Data\DataFrame\Excel\Coordinates.vb"

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
Imports System.Text.RegularExpressions

Namespace Excel

    Public Module Coordinates

        <Extension>
        Public Function CellValue(data As IO.File, c As String) As String
            Dim y As Integer = CInt(Regex.Match(c, "\d+").Value)
            Dim x As String = Mid(c, 1, c.Length - CStr(y).Length)
            Return data.Cell(XValue(x), y)
        End Function

        Public Function XValue(x As String) As Integer
            Throw New NotImplementedException
        End Function

        <Extension>
        Public Function RangeSelects(data As IO.File, range As String) As String()
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
