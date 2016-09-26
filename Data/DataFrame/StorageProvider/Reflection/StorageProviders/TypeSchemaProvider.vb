#Region "Microsoft.VisualBasic::554a5008324e6e1b3f82538cef2f27bb, ..\visualbasic_App\Data\DataFrame\StorageProvider\Reflection\StorageProviders\TypeSchemaProvider.vb"

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

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.csv.DataImports
Imports Microsoft.VisualBasic.Language

Namespace StorageProvider.Reflection

    Public Module TypeSchemaProvider

        ''' <summary>
        ''' 返回的字典对象之中的Value部分是自定义属性
        ''' </summary>
        ''' <returns></returns>
        Public Function GetProperties(type As Type, Explicit As Boolean) As Dictionary(Of PropertyInfo, ComponentModels.StorageProvider)
            Dim ignored As Type = GetType(Reflection.Ignored)
            Dim Properties As PropertyInfo() = type.GetProperties(BindingFlags.Public Or BindingFlags.Instance)

            Properties = LinqAPI.Exec(Of PropertyInfo) <=
 _
                From prop As PropertyInfo
                In Properties
                Let isIgnored As Boolean =
                    Not prop.GetCustomAttributes(attributeType:=ignored, inherit:=True).IsNullOrEmpty ' 当忽略的标志不为空的时候，说明这个属性是被忽略掉的
                Where Not isIgnored AndAlso
                    prop.GetIndexParameters.IsNullOrEmpty  ' 从这里筛选掉需要被忽略掉的属性以及有参数的属性
                Select prop

            Dim hash As Dictionary(Of PropertyInfo, ComponentModels.StorageProvider) =
                (From [Property] As PropertyInfo
                 In Properties
                 Let IAC = GetInterfaces([Property], Explicit)
                 Where Not IAC Is Nothing
                 Select [Property], IAC).ToDictionary(Function(obj) obj.Property, Function(obj) obj.IAC)
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
                attributeType:=GetType(Reflection.CollectionAttribute),
                inherit:=True)

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
                Dim attr = DirectCast(attrs(Scan0), Reflection.ColumnAttribute) '枚举和键值对可能会通过这个属性来取别名，所以这里还需要进行额外的处理
                Return __generateMask([Property], [alias]:=attr.Name, forcePrimitive:=forcePrimitive)
            End If  '请注意，由于这个属性之间有继承关系，座椅最基本的类型会放在最后以防止出现重复

            attrs = [Property].GetCustomAttributes(attributeType:=GetType(System.Data.Linq.Mapping.ColumnAttribute), inherit:=True)
            If Not attrs.IsNullOrEmpty Then
                Dim attr = DirectCast(attrs(Scan0), System.Data.Linq.Mapping.ColumnAttribute)  '也可能是别名属性
                Return __generateMask([Property], [alias]:=attr.Name, forcePrimitive:=forcePrimitive)
            End If

            If Not Explicit Then
                Return __generateMask([Property], "", forcePrimitive)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="[Property]"></param>
        ''' <param name="[alias]"></param>
        ''' <returns></returns>
        Private Function __generateMask([Property] As PropertyInfo, [alias] As String, forcePrimitive As Boolean) As ComponentModels.StorageProvider
            Dim _getName As String = If(String.IsNullOrEmpty([alias]), [Property].Name, [alias])

            If Scripting.IsPrimitive([Property].PropertyType) Then
                Dim column As New ColumnAttribute(_getName)  '属性值的类型是简单类型，则其标记的类型只能是普通列
                Return New ComponentModels.Column(column, [Property])
            End If

            Dim valueType As New Value(Of Type)
            Dim elType As New Value(Of Type)

            If Not (valueType = GetMetaAttribute([Property].PropertyType)) Is Nothing Then

                Return New ComponentModels.MetaAttribute(
                    New Reflection.MetaAttribute(valueType.value), [Property])
            ElseIf Not (elType = (GetThisElement([Property].PropertyType, forcePrimitive)) Is Nothing OrElse
                        elType.value.Equals(GetType(Void))) Then

                Return ComponentModels.CollectionColumn.CreateObject(
                    New CollectionAttribute(_getName), [Property], elType.value)
            ElseIf IsKeyValuePair([Property]) Then
                Return ComponentModels.KeyValuePair.CreateObject(_getName, [Property])
            ElseIf IsEnum([Property]) Then
                Return ComponentModels.Enum.CreateObject(_getName, [Property])
            End If

            Return Nothing
        End Function

        Private Function IsEnum([Property] As System.Reflection.PropertyInfo) As Boolean
            If Not [Property].PropertyType.BaseType.Equals(GetType(System.Enum)) Then
                Return False
            End If

            Return True
        End Function

        Const KeyValuePair As String = "System.Collections.Generic.KeyValuePair`2"
        Const KeyValuePairObject As String = "Microsoft.VisualBasic.ComponentModel.Collection.Generic.KeyValuePairObject`2"

        ''' <summary>
        ''' 这个属性的类型可以同时允许系统的内建的键值对类型，也可以是<see cref="KeyValuePairObject"/>
        ''' </summary>
        ''' <param name="Property"></param>
        ''' <returns></returns>
        Public Function IsKeyValuePair([Property] As PropertyInfo) As Boolean
            Dim Type As System.Type = [Property].PropertyType
            Dim TypeFullName As String = $"{Type.Namespace }.{Type.Name }"

            If Not (String.Equals(KeyValuePair, TypeFullName) OrElse String.Equals(KeyValuePairObject, TypeFullName)) Then
                Return False
            End If

            Dim GenericEntries As Type() = Type.GetGenericArguments
            Dim TKey As Type = GenericEntries.First
            Dim TValue As Type = GenericEntries.Last

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
                    Return GetType(System.Void)
                Else
                    Return Nothing
                End If
            End If

            Dim elType As Type = type.GetElementType

            If elType Is Nothing Then
                Dim TypeFullName As String = $"{type.Namespace}.{type.Name}"    ' 可能是List对象类型

                If Not String.Equals(List, TypeFullName) Then
                    If forcePrimitive Then
                        Return GetType(System.Void)
                    End If
                End If

                elType = type.GetGenericArguments.FirstOrDefault

                If elType Is Nothing AndAlso Not forcePrimitive Then
                    Return type
                End If
            End If

            If Scripting.IsPrimitive(elType) Then
                Return elType
            Else  '  目标类型不是基本类型，但是被指定强制输出基本类型，所以在这里只能够输出空值
                If forcePrimitive Then
                    Return GetType(Void)
                Else ' 不强制函数输出基本类型，则这里直接返回目标类型
                    Return elType
                End If
            End If
        End Function

        Public Function GetMetaAttribute(type As Type) As Type
            If Not type.IsGenericType Then
                If (type.Equals(GetType(Object))) OrElse type.BaseType.Equals(GetType(Object)) Then
                    Return Nothing
                Else
                    Return GetMetaAttribute(type.BaseType)
                End If
            End If

            Dim gType As Type = type.GetGenericTypeDefinition

            If Not gType.Equals(GetType(Dictionary(Of ,))) Then
                Return Nothing
            End If

            Dim TypeDef As Type() = type.GetGenericArguments

            '第一个参数类型只能是字符串，后面的参数类型只能是简单类型
            If Not TypeDef.First.Equals(GetType(String)) Then
                Return Nothing
            End If

            If Not Scripting.IsPrimitive(TypeDef.Last) Then
                Return Nothing
            End If

            Return TypeDef.Last
        End Function
    End Module
End Namespace
