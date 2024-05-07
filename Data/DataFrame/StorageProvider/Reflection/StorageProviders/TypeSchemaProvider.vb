#Region "Microsoft.VisualBasic::09563b37b7594b1fa14472a16bd3b4d0, G:/GCModeller/src/runtime/sciBASIC#/Data/DataFrame//StorageProvider/Reflection/StorageProviders/TypeSchemaProvider.vb"

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

    '   Total Lines: 296
    '    Code Lines: 184
    ' Comment Lines: 62
    '   Blank Lines: 50
    '     File Size: 13.53 KB


    '     Module TypeSchemaProvider
    ' 
    '         Function: __generateMask, GetInterfaces, GetMetaAttribute, GetProperties, GetThisElement
    '                   IsDataIgnored, IsEnum, IsKeyValuePair
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace StorageProvider.Reflection

    Public Module TypeSchemaProvider

        ReadOnly ignored As Type = GetType(Reflection.Ignored)
        ReadOnly dataIgnores As Type = GetType(DataIgnoredAttribute)

        ''' <summary>
        ''' 当忽略的标志不为空的时候，说明这个属性是被忽略掉的
        ''' </summary>
        ''' <param name="[property]"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsDataIgnored([property] As PropertyInfo) As Boolean
            Return Not [property].GetCustomAttributes(attributeType:=ignored, inherit:=True).IsNullOrEmpty OrElse
                   Not [property].GetCustomAttributes(attributeType:=dataIgnores, inherit:=True).IsNullOrEmpty
        End Function

        ''' <summary>
        ''' 返回的字典对象之中的Value部分是自定义属性
        ''' </summary>
        ''' <returns></returns>
        Public Function GetProperties(type As Type, Explicit As Boolean) As Dictionary(Of PropertyInfo, ComponentModels.StorageProvider)
            Dim Properties As PropertyInfo() = type.GetProperties(BindingFlags.Public Or BindingFlags.Instance)

            Properties = LinqAPI.Exec(Of PropertyInfo) <=
 _
                From prop As PropertyInfo
                In Properties
                Let isIgnored As Boolean = prop.IsDataIgnored
                Where Not isIgnored AndAlso
                    prop.GetIndexParameters.IsNullOrEmpty  ' 从这里筛选掉需要被忽略掉的属性以及有参数的属性
                Select prop

            Dim hash As Dictionary(Of PropertyInfo, ComponentModels.StorageProvider) =
                (From [Property] As PropertyInfo
                 In Properties
                 Let IAC = GetInterfaces([Property], Explicit)
                 Where Not IAC Is Nothing
                 Select [Property], IAC) _
 _
                    .ToDictionary(Function(obj) obj.Property,
                                  Function(obj)
                                      Return obj.IAC
                                  End Function)
            Return hash
        End Function

        ''' <summary>
        ''' 当目标属性上面没有任何自定义属性数据的时候，会检查是否为简单数据类型，假若是则会自动添加一个NullMask，
        ''' 假若不是，则会返回空集合，则说明这个属性不会被用于序列化和反序列化。
        ''' 假若返回来的是空值，则说明是复杂类型
        ''' </summary>
        ''' <param name="[Property]">对于LINQ的Column属性也会接受的</param>
        ''' <returns></returns>
        Public Function GetInterfaces([Property] As PropertyInfo, Explicit As Boolean, Optional forcePrimitive As Boolean = True) As ComponentModels.StorageProvider
            Dim attrs As Object() = [Property].GetCustomAttributes(
                attributeType:=GetType(CollectionAttribute),
                inherit:=True
            )

            If Not attrs.IsNullOrEmpty Then
                Dim attr = DirectCast(attrs(Scan0), Reflection.CollectionAttribute)
                Return ComponentModels.CollectionColumn.CreateObject(
                    attr, [Property], GetThisElement([Property].PropertyType))
            End If

            attrs = [Property].GetCustomAttributes(attributeType:=GetType(Reflection.MetaAttribute), inherit:=True)

            If Not attrs.IsNullOrEmpty Then
                Dim attr = DirectCast(attrs(Scan0), Reflection.MetaAttribute)
                Return New ComponentModels.MetaAttribute(attr, [Property])
            End If

            attrs = [Property].GetCustomAttributes(attributeType:=GetType(Reflection.ColumnAttribute), inherit:=True)

            If Not attrs.IsNullOrEmpty Then
                ' 查找列数据定义
                ' 枚举和键值对可能会通过这个属性来取别名，所以这里还需要进行额外的处理
                Dim attr As ColumnAttribute = DirectCast(attrs(Scan0), Reflection.ColumnAttribute)

                ' 由于已经定义了Column类型自定义属性定义，所以Column里面可能是非基本类型，
                ' 这里不再强制需要基本类型了
                Return __generateMask(
                    [Property],
                    [alias]:=attr.Name,
                    forcePrimitive:=False,
                    ColumnMaps:=attr
                )
            End If

            '#If NET48 Then
            '            ' 请注意，由于这个属性之间有继承关系，这一最基本的类型会放在最后以防止出现重复
            '            attrs = [Property].GetCustomAttributes(attributeType:=GetType(System.Data.Linq.Mapping.ColumnAttribute), inherit:=True)

            '            If Not attrs.IsNullOrEmpty Then
            '                ' 也可能是别名属性
            '                Dim attr = DirectCast(attrs(Scan0), System.Data.Linq.Mapping.ColumnAttribute)
            '                Return __generateMask([Property], [alias]:=attr.Name, forcePrimitive:=forcePrimitive)
            '            End If
            '#Else
            ' 请注意，由于这个属性之间有继承关系，这一最基本的类型会放在最后以防止出现重复
            attrs = [Property].GetCustomAttributes(attributeType:=GetType(SchemaMaps.ColumnAttribute), inherit:=True)

            If Not attrs.IsNullOrEmpty Then
                ' 也可能是别名属性
                Dim attr = DirectCast(attrs(Scan0), SchemaMaps.ColumnAttribute)
                Return __generateMask([Property], [alias]:=attr.Name, forcePrimitive:=forcePrimitive)
            End If
            ' #End If

            attrs = [Property].GetCustomAttributes(attributeType:=GetType(SchemaMaps.DataFrameColumnAttribute), inherit:=True)

            If Not attrs.IsNullOrEmpty Then
                Dim attr = DirectCast(attrs(Scan0), SchemaMaps.DataFrameColumnAttribute)
                Return __generateMask([Property], [alias]:=attr.Name, forcePrimitive:=forcePrimitive)
            End If

            attrs = [Property].GetCustomAttributes(attributeType:=GetType(SchemaMaps.Field), inherit:=True)

            If Not attrs.IsNullOrEmpty Then
                Dim attr = DirectCast(attrs(Scan0), SchemaMaps.Field)
                Return __generateMask([Property], [alias]:=attr.Name, forcePrimitive:=forcePrimitive)
            End If

#If NETCOREAPP Then
            attrs = [Property].GetCustomAttributes(attributeType:=GetType(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute), inherit:=True)

            If Not attrs.IsNullOrEmpty Then
                Dim attr = DirectCast(attrs(Scan0), System.ComponentModel.DataAnnotations.Schema.ColumnAttribute)
                Return __generateMask([Property], [alias]:=attr.Name, forcePrimitive:=forcePrimitive)
            End If
#End If

            If Not Explicit Then
                Return __generateMask([Property], "", forcePrimitive)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 这个函数是针对没有任何自定义属性标记的属性对象而言的
        ''' </summary>
        ''' <param name="[Property]"></param>
        ''' <param name="[alias]"></param>
        ''' <returns></returns>
        Private Function __generateMask([Property] As PropertyInfo,
                                        [alias] As String,
                                        forcePrimitive As Boolean,
                                        Optional ColumnMaps As ColumnAttribute = Nothing) As ComponentModels.StorageProvider

            Dim _getName As String = If(String.IsNullOrEmpty([alias]), [Property].Name, [alias])

            ' 直接返回结果的情况：
            ' 1. 列类型为基本类型
            ' 2. 类属性定义之中定义了自定义解析器
            If Scripting.IsPrimitive([Property].PropertyType) OrElse ((Not ColumnMaps Is Nothing) AndAlso (Not ColumnMaps.CustomParser Is Nothing)) Then
                ' 属性值的类型是简单类型，则其标记的类型只能是普通列
                Dim column As ColumnAttribute = If(ColumnMaps, New ColumnAttribute(_getName))
                Return ComponentModels.Column.CreateObject(column, [Property])
            End If

            Dim valueType As New Value(Of Type)
            Dim elType As New Value(Of Type)

            If Not (valueType = GetMetaAttribute([Property].PropertyType)) Is Nothing Then
                ' 是字典类型
                Return New ComponentModels.MetaAttribute(New MetaAttribute(valueType.Value), [Property])
            ElseIf Not (elType = (GetThisElement([Property].PropertyType, forcePrimitive)) Is Nothing OrElse
                        elType.Value.Equals(GetType(Void))) Then
                ' 是集合类型
                Return ComponentModels.CollectionColumn.CreateObject(New CollectionAttribute(_getName), [Property], elType.Value)
            ElseIf IsKeyValuePair([Property]) Then
                ' 是键值对
                Return ComponentModels.KeyValuePair.CreateObject(_getName, [Property])
            ElseIf IsEnum([Property]) Then
                ' 是枚举类型
                Return ComponentModels.Enum.CreateObject(_getName, [Property])
            End If

            Return Nothing
        End Function

        Private Function IsEnum([property] As PropertyInfo) As Boolean
            If [property].PropertyType.Equals(GetType(Object)) Then
                Call [property].ToString.Warning
                Return False ' System.Object的basetype是空值，所以要先判断一下是不是Object
            End If

            If Not [property].PropertyType _
                .BaseType _
                .Equals(GetType(System.Enum)) Then
                Return False
            End If

            Return True
        End Function

        Const KeyValuePair$ = "System.Collections.Generic.KeyValuePair`2"
        Const KeyValuePairObject$ = "Microsoft.VisualBasic.ComponentModel.Collection.Generic.KeyValuePairObject`2"

        ''' <summary>
        ''' 这个属性的类型可以同时允许系统的内建的键值对类型，也可以是<see cref="KeyValuePairObject"/>
        ''' </summary>
        ''' <param name="[property]"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function IsKeyValuePair([property] As PropertyInfo) As Boolean
            Dim type As Type = [property].PropertyType
            Dim fullName As String = $"{type.Namespace }.{type.Name}"

            If Not (String.Equals(KeyValuePair, fullName) OrElse
                    String.Equals(KeyValuePairObject, fullName)) Then

                Return False
            End If

            Dim genericTypes As Type() = type.GetGenericArguments
            Dim TKey As Type = genericTypes.First
            Dim TValue As Type = genericTypes.Last

            If Not Scripting.IsPrimitive(TKey) OrElse Not Scripting.IsPrimitive(TValue) Then
                Return False
            End If

            Return True
        End Function

        Const List As String = "List`1"

        ''' <summary>
        ''' 获取集合类型的元素类型，假若获取不到，则会返回类型<see cref="System.Void"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <param name="forcePrimitive">当本参数为False的时候，假若不是集合类型，函数会返回Nothing</param>
        <Extension>
        Public Function GetThisElement(type As Type, Optional forcePrimitive As Boolean = True) As Type
            Dim intfs As Type() = type.GetInterfaces

            If Array.IndexOf(intfs, GetType(IEnumerable)) = -1 Then
                If forcePrimitive Then
                    Return GetType(Void)
                Else
                    Return Nothing
                End If
            End If

            Dim elementType As Type = type.GetElementType

            If elementType Is Nothing Then
                ' 可能是List对象类型
                Dim fullName$ = $"{type.Namespace}.{type.Name}"

                If Not String.Equals(List, fullName) Then
                    If forcePrimitive Then
                        Return GetType(Void)
                    End If
                End If

                elementType = type.GetGenericArguments.FirstOrDefault

                If elementType Is Nothing AndAlso Not forcePrimitive Then
                    Return type
                End If
            End If

            If Scripting.IsPrimitive(elementType) Then
                Return elementType
            Else
                ' 目标类型不是基本类型，但是被指定强制输出基本类型，所以在这里只能够输出空值
                If forcePrimitive Then
                    Return GetType(Void)
                Else
                    ' 不强制函数输出基本类型，则这里直接返回目标类型
                    Return elementType
                End If
            End If
        End Function

        <Extension>
        Public Function GetMetaAttribute(type As Type) As Type
            If Not type.IsGenericType Then
                If (type.Equals(GetType(Object))) OrElse type.BaseType.Equals(GetType(Object)) Then
                    Return Nothing
                Else
                    Return GetMetaAttribute(type.BaseType)
                End If
            End If

            Dim genericType As Type = type.GetGenericTypeDefinition

            If Not genericType.Equals(GetType(Dictionary(Of ,))) Then
                Return Nothing
            End If

            Dim generics As Type() = type.GetGenericArguments

            ' 第一个参数类型只能是字符串，后面的参数类型只能是简单类型
            If Not generics.First.Equals(GetType(String)) Then
                Return Nothing
            End If

            If Not Scripting.IsPrimitive(generics.Last) Then
                Return Nothing
            End If

            Return generics.Last
        End Function
    End Module
End Namespace
