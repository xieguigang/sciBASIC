#Region "Microsoft.VisualBasic::0288dbd4670bf6f5c270d4fd398d497c, ..\sciBASIC#\Data\DataFrame.Extensions\Math.vb"

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
Imports Microsoft.VisualBasic.Data.csv.IO
Imports sys = System.Math

''' <summary>
''' Vector math extensions for <see cref="DataSet"/> or its collection.
''' </summary>
Public Module DataSetMath

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Log(d As DataSet, Optional base# = sys.E) As DataSet
        Return New DataSet With {
            .ID = d.ID,
            .Properties = d.Properties _
            .ToDictionary(Function(c) c.Key,
                          Function(c) sys.Log(c.Value))
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Log(data As IEnumerable(Of DataSet), Optional base# = sys.E) As IEnumerable(Of DataSet)
        Return data.Select(Function(d) d.Log(base))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Log2(data As IEnumerable(Of DataSet)) As IEnumerable(Of DataSet)
        Return data.Log(base:=2)
    End Function
End Module

