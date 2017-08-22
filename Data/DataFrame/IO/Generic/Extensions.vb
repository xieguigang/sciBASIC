#Region "Microsoft.VisualBasic::bbe72b7c06a9489793d27d7898450803, ..\sciBASIC#\Data\DataFrame\IO\Generic\Extensions.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq

Namespace IO

    ''' <summary>
    ''' Data extension for <see cref="DataSet"/> and <see cref="EntityObject"/>
    ''' </summary>
    Public Module Extensions

        <Extension>
        Public Function EuclideanDistance(a As DataSet, b As DataSet, names$()) As Double
            Dim d# = Aggregate key As String
                     In names
                     Let x = a(key)
                     Let y = b(key)
                     Into Sum((x - y) ^ 2) '

            Return Math.Sqrt(d)
        End Function

        <Extension>
        Public Function Transpose(source As IEnumerable(Of DataSet)) As DataSet()
            Dim list As DataSet() = source.ToArray
            Dim allKeys = list.PropertyNames

            Return allKeys _
                .Select(Function(key)
                            Return New DataSet With {
                                .ID = key,
                                .Properties = list _
                                    .ToDictionary(Function(x) x.ID,
                                                  Function(x) x(key))
                            }
                        End Function) _
                .ToArray
        End Function

        <Extension>
        Public Function PropertyNames(table As IDictionary(Of String, DataSet)) As String()
            Return table.Values.PropertyNames
        End Function

        ''' <summary>
        ''' Gets the union collection of the keys from <see cref="DataSet.Properties"/> 
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PropertyNames(list As IEnumerable(Of DataSet)) As String()
            Return list _
                .Select(Function(o) o.EnumerateKeys(False)) _
                .IteratesALL _
                .Distinct _
                .ToArray
        End Function

        <Extension>
        Public Function Vector(datasets As IEnumerable(Of DataSet), property$) As Double()
            Return datasets _
                .Select(Function(x) x([property])) _
                .ToArray
        End Function

        <Extension>
        Public Function NamedMatrix(data As IEnumerable(Of DataSet)) As NamedValue(Of Dictionary(Of String, Double))()
            Return data _
                .Select(Function(x)
                            Return New NamedValue(Of Dictionary(Of String, Double)) With {
                                .Name = x.ID,
                                .Value = x.Properties
                            }
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Convert the property value collection data like <see cref="PropertyValue"/> as the csv table value.
        ''' (使用这个函数请确保相同编号的对象集合之中是没有相同的属性名称的，
        ''' 但是假若会存在重复的名称的话，这些重复的名称的值会被<see cref="JoinBy"/>操作，分隔符为``分号``)
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DataFrame(Of T As IPropertyValue)(data As IEnumerable(Of T)) As EntityObject()
            Dim objects = data.GroupBy(Function(k) k.Key).ToArray
            Dim out As EntityObject() = objects _
                .Select(AddressOf CreateObject) _
                .ToArray
            Return out
        End Function

        ''' <summary>
        ''' Creates a <see cref="EntityObject"/> from a group of the property value collection.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateObject(Of [Property] As IPropertyValue)(g As IGrouping(Of String, [Property])) As EntityObject
            Dim data As [Property]() = g.ToArray
            Dim props As Dictionary(Of String, String) = data _
                .GroupBy(Function(p) p.Property) _
                .ToDictionary(Function(k) k.Key,
                              Function(v) v.Select(
                              Function(s) s.Value).JoinBy("; "))

            Return New EntityObject With {
                .ID = g.Key,
                .Properties = props
            }
        End Function

        ''' <summary>
        ''' 批量的从目标对象集合之中选出目标属性值
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="key$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Values(data As IEnumerable(Of EntityObject), key$) As String()
            Return data _
                .Select(Function(r) r(key$)) _
                .ToArray
        End Function
    End Module
End Namespace
