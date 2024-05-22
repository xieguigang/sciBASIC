#Region "Microsoft.VisualBasic::72c864e4a19c2b393aac50a1610ab8d3, Microsoft.VisualBasic.Core\src\Extensions\Collection\CategoryOperations.vb"

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

    '   Total Lines: 65
    '    Code Lines: 54 (83.08%)
    ' Comment Lines: 6 (9.23%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (7.69%)
    '     File Size: 2.81 KB


    ' Module CategoryOperations
    ' 
    '     Function: AsGroups, AsNamedVector, CategoryValues, IGrouping
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Specialized
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports r = System.Text.RegularExpressions.Regex

Public Module CategoryOperations

    <Extension>
    Public Function AsGroups(Of T)(table As Dictionary(Of String, T())) As IEnumerable(Of NamedCollection(Of T))
        Return table.Select(Function(item)
                                Return New NamedCollection(Of T) With {
                                    .Name = item.Key,
                                    .Value = item.Value
                                }
                            End Function)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function IGrouping(Of T)(source As IEnumerable(Of NamedCollection(Of T))) As IEnumerable(Of IGrouping(Of String, T))
        Return source.Select(Function(x) DirectCast(x, IGrouping(Of String, T)))
    End Function

    <Extension>
    Public Function AsNamedVector(Of T)(groups As IEnumerable(Of IGrouping(Of String, T))) As IEnumerable(Of NamedCollection(Of T))
        Return groups.Select(Function(group)
                                 Return New NamedCollection(Of T) With {
                                    .Name = group.Key,
                                    .Value = group.ToArray
                                 }
                             End Function)
    End Function

    ''' <summary>
    ''' transform ``[category => items]`` to ``[item -> category][]``
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="categories"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CategoryValues(Of T)(categories As IEnumerable(Of NamedCollection(Of T))) As Dictionary(Of T, String)
#If NET_48 Or netcore5 = 1 Then
        Return categories _
            .Select(Function(category)
                        Return category.Select(Function(item) (item, category.name))
                    End Function) _
            .IteratesALL _
            .ToDictionary(Function(val) val.Item1,
                          Function(category)
                              Return category.Item2
                          End Function)
#Else
        Throw New NotImplementedException
#End If
    End Function
End Module
