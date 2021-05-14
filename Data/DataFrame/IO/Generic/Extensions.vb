#Region "Microsoft.VisualBasic::2e9d917bd76ead24c91e506eb526bdb8, Data\DataFrame\IO\Generic\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: asCharacter, AsCharacter, AsDataSet, CreateObject, DataFrame
    '                   EuclideanDistance, GroupBy, NamedMatrix, NamedValues, Project
    '                   (+2 Overloads) PropertyNames, (+2 Overloads) Transpose, Values, (+2 Overloads) Vector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace IO

    ''' <summary>
    ''' Data extension for <see cref="DataSet"/> and <see cref="EntityObject"/>
    ''' </summary>
    ''' 
    <HideModuleName>
    Public Module Extensions

        <Extension>
        Public Function EuclideanDistance(a As DataSet, b As DataSet, Optional names$() = Nothing) As Double
            If names.IsNullOrEmpty Then
                names = (a.Properties.Keys.AsList + b.Properties.Keys).Distinct.ToArray
            End If

            Dim d# = Aggregate key As String
                     In names
                     Let x = a(key)
                     Let y = b(key)
                     Into Sum((x - y) ^ 2) '

            Return stdNum.Sqrt(d)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function NamedValues(matrix As IEnumerable(Of DataSet), propertyName$) As Dictionary(Of String, Double)
            Return matrix.ToDictionary(Function(d) d.ID, Function(d) d(propertyName))
        End Function

        ''' <summary>
        ''' 矩阵转置：将矩阵的行列进行颠倒
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
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
                                                  Function(x)
                                                      Return x(key)
                                                  End Function)
                            }
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' 矩阵转置：将矩阵的行列进行颠倒
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Transpose(source As IEnumerable(Of EntityObject)) As EntityObject()
            Dim list As EntityObject() = source.ToArray
            Dim allKeys = list.PropertyNames

            Return allKeys _
                .Select(Function(key)
                            Return New EntityObject With {
                                .ID = key,
                                .Properties = list _
                                    .ToDictionary(Function(x) x.ID,
                                                  Function(x)
                                                      Return x(key)
                                                  End Function)
                            }
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' 可以使用这个拓展函数进行重排序
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="keys$"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Project(data As IEnumerable(Of DataSet), keys$()) As IEnumerable(Of DataSet)
            Return data _
                .Select(Function(x)
                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = keys.ToDictionary(
                                    Function(k) k,
                                    Function(k) x.ItemValue(k))
                            }
                        End Function)
        End Function

        ''' <summary>
        ''' Grouping of the property value by property names
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="groupKeys"></param>
        ''' <param name="aggregate$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GroupBy(data As IEnumerable(Of DataSet), groupKeys As Dictionary(Of String, String()), Optional aggregate$ = "average") As IEnumerable(Of DataSet)
            With aggregate.GetAggregateFunction
                Return data _
                    .Select(Function(x)
                                Dim values = groupKeys.ToDictionary(
                                    Function(k) k.Key,
                                    Function(k) .ByRef(x(k.Value)))

                                Return New DataSet With {
                                    .ID = x.ID,
                                    .Properties = values
                                }
                            End Function)
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PropertyNames(table As IDictionary(Of String, DataSet)) As String()
            Return table.Values.PropertyNames
        End Function

        ''' <summary>
        ''' Gets the union collection of the keys from <see cref="DataSet.Properties"/>.
        ''' (包含所有的已经去除重复了的属性名称)
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PropertyNames(Of T)(list As IEnumerable(Of DynamicPropertyBase(Of T))) As String()
            Return list _
                .Where(Function(a) Not a Is Nothing) _
                .Select(Function(o) o.EnumerateKeys(False)) _
                .IteratesALL _
                .Distinct _
                .ToArray
        End Function

        ''' <summary>
        ''' 取出某一个给定的属性的所有值。取出来的数据元素之间的顺序是和<paramref name="datasets"/>之中的元素的顺序是一致的。
        ''' </summary>
        ''' <param name="datasets"></param>
        ''' <param name="property">字典的键名称</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function Vector(datasets As IEnumerable(Of DataSet), property$) As Double()
            Return datasets _
                .Select(Function(x) x([property])) _
                .ToArray
        End Function

        ''' <summary>
        ''' Column value projection
        ''' </summary>
        ''' <param name="datasets"></param>
        ''' <param name="property$"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Vector(datasets As IEnumerable(Of EntityObject), property$) As String()
            Return datasets _
                .Select(Function(x) x([property])) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsCharacter(source As IEnumerable(Of DataSet)) As IEnumerable(Of EntityObject)
            Return source.Select(AddressOf asCharacter)
        End Function

        ''' <summary>
        ''' Convert a numeric dataset object as character dataset
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function asCharacter(data As DataSet) As EntityObject
            Return New EntityObject With {
                .ID = data.ID,
                .Properties = data.Properties.AsCharacter
            }
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
                              Function(v)
                                  Return v.Select(Function(s) s.Value).JoinBy("; ")
                              End Function)

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
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(data As IEnumerable(Of EntityObject), key$) As String()
            Return data _
                .Select(Function(r) r(key$)) _
                .ToArray
        End Function

        ''' <summary>
        ''' 将字符串数据集转换为数值类型的数据集
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="blank">如果目标属性的值是空字符串的话，将该属性值设置为这个参数值默认的值</param>
        ''' <param name="ignores">在转换的时候需要忽略掉的属性值</param>
        ''' <returns></returns>
        <Extension>
        Public Function AsDataSet(data As IEnumerable(Of EntityObject),
                                  Optional ignores As Index(Of String) = Nothing,
                                  Optional blank# = 0) As IEnumerable(Of DataSet)

            Dim array As EntityObject() = data.ToArray
            Dim allKeys = array _
                .Select(Function(x) x.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .Where(Function(key) Not key Like ignores) _
                .ToArray
            Dim toDouble = Function(obj As EntityObject, key$)
                               Return If(obj.Properties.ContainsKey(key), Val(obj(key)), blank)
                           End Function

            Return data _
                .Select(Function(obj)
                            Return New DataSet With {
                                .ID = obj.ID,
                                .Properties = allKeys _
                                    .ToDictionary(Function(x) x,
                                                  Function(x)
                                                      Return toDouble(obj, key:=x)
                                                  End Function)
                            }
                        End Function)
        End Function
    End Module
End Namespace
