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
        Return categories _
            .Select(Function(category)
                        Return category.Select(Function(item) (item, category.name))
                    End Function) _
            .IteratesALL _
            .ToDictionary(Function(val) val.Item1,
                          Function(category)
                              Return category.Item2
                          End Function)
    End Function
End Module
