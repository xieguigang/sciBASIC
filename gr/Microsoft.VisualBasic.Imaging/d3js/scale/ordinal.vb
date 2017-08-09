#Region "Microsoft.VisualBasic::5139c06b49f70c812c086708b9f4a63d, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\d3js\scale\ordinal.vb"

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

Namespace d3js.scale

    Public Class ordinal

        Public Function domain(values#()) As ordinal

        End Function

        Public Function domain(values$()) As ordinal

        End Function

        Public Function domain(values%()) As ordinal
            Return domain(values.Select(Function(n) CDbl(n)).ToArray)
        End Function

        Public Function rangeBands() As ordinal

        End Function

        Public Function range(Optional values#() = Nothing) As ordinal

        End Function

        Public Shared Narrowing Operator CType(ordinal As ordinal) As Double()

        End Operator
    End Class
End Namespace
