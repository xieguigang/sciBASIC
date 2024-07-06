#Region "Microsoft.VisualBasic::9c55a3a0414c95d42f87fb64f820138f, Data\DataFrame\Extensions\Math.vb"

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

    '   Total Lines: 34
    '    Code Lines: 27 (79.41%)
    ' Comment Lines: 3 (8.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (11.76%)
    '     File Size: 1.21 KB


    ' Module DataSetMath
    ' 
    '     Function: (+2 Overloads) Log, Log2
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports std = System.Math

''' <summary>
''' Vector math extensions for <see cref="DataSet"/> or its collection.
''' </summary>
Public Module DataSetMath

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Log(d As DataSet, Optional base# = std.E) As DataSet
        Return New DataSet With {
            .ID = d.ID,
            .Properties = d.Properties _
            .ToDictionary(Function(c) c.Key,
                          Function(c)
                              Return std.Log(c.Value, newBase:=base)
                          End Function)
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Log(data As IEnumerable(Of DataSet), Optional base# = std.E) As IEnumerable(Of DataSet)
        Return data.Select(Function(d) d.Log(base))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Log2(data As IEnumerable(Of DataSet)) As IEnumerable(Of DataSet)
        Return data.Log(base:=2)
    End Function
End Module
