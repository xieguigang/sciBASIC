#Region "Microsoft.VisualBasic::b23e537755fd42b894c1f2692db6397b, ..\sciBASIC#\Data_science\Bootstrapping\Sampling.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Linq

Public Module Sampling

    <Extension>
    Public Iterator Function SamplingByPoints(Of T)(data As IEnumerable(Of NamedValue(Of DoubleTagged(Of T)())),
                                                    points As IEnumerable(Of Double),
                                                    Optional err# = 0.00001) As IEnumerable(Of NamedValue(Of DoubleTagged(Of T)()))

    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SamplingBySplitNParts(Of T)(data As IEnumerable(Of NamedValue(Of DoubleTagged(Of T)())),
                                                n%,
                                                Optional err# = 0.00001) As IEnumerable(Of NamedValue(Of DoubleTagged(Of T)()))
        With data.ToArray
            Return .SamplingByPoints(
                points:= .Select(Function(s) s.Value.Select(Function(p) p.Tag)).IteratesALL.Range.Enumerate(n),
                err:=err)
        End With
    End Function
End Module

