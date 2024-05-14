#Region "Microsoft.VisualBasic::e4ec9b3d357358057a887b1a9b253bb8, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Property\DynamicProperty.vb"

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

    '   Total Lines: 232
    '    Code Lines: 137
    ' Comment Lines: 69
    '   Blank Lines: 26
    '     File Size: 9.41 KB


    '     Class DynamicPropertyBase
    ' 
    '         Properties: MyHashCode, Properties
    ' 
    '         Function: EnumerateKeys, GetEnumerator, GetItemValue, GetNames, HasProperty
    '                   IDynamicsObject_GetItemValue, ToString
    ' 
    '         Sub: (+2 Overloads) Add, (+2 Overloads) SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' This abstract object has a <see cref="propertyTable"/> dictionary keeps as 
    ''' a dynamics property source.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class DynamicPropertyBase(Of T)
        Implements IDynamicMeta(Of T)
        ' 因为ienumerable接口在进行json序列化的时候会被调用
        ' 为了避免出现这个bug，所以在这里使用enumeration接口来保持兼容性
        Implements Enumeration(Of NamedValue(Of T))
        Implements IDynamicsObject

        ''' <summary>
        ''' The dynamics property object with specific type of value.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Can not serialize the dictionary object in to xml document. **and this property ensure that the value always not null!**</remarks>
        <DynamicMetadata>
        <XmlIgnore>
        Public Overridable Property Properties As Dictionary(Of String, T) Implements IDynamicMeta(Of T).Properties
            Get
                If propertyTable Is Nothing Then
                    propertyTable = New Dictionary(Of String, T)
                End If
                Return propertyTable
            End Get
            Set(value As Dictionary(Of String, T))
                propertyTable = value
            End Set
        End Property

        ''' <summary>
        ''' 动态属性表
        ''' </summary>
        Protected propertyTable As Dictionary(Of String, T)

        ''' <summary>
        ''' Gets/sets item value by using property name.
        ''' (这个函数为安全的函数，当目标属性不存在的时候，会返回空值)
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public Overridable Overloads Property ItemValue(name As String) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If Properties.ContainsKey(name) Then
                    Return Properties(name)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As T)
                Properties(name) = value
            End Set
        End Property

        ''' <summary>
        ''' Get a value package at once using a key collection, 
        ''' if the key is not exists in the property, then its 
        ''' correspoding value is nothing.
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        Default Public Overloads Property ItemValue(keys As IEnumerable(Of String)) As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return keys.Select(Function(s) Me(s)).ToArray
            End Get
            Set(value As T())
                For Each key As SeqValue(Of String) In keys.SeqIterator
                    Me(key.value) = value(key)
                Next
            End Set
        End Property

        ''' <summary>
        ''' Get a value package at once using a key collection, 
        ''' if the key is not exists in the property, then its 
        ''' correspoding value is nothing.
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        Default Public Overloads Property ItemValue(keys As IEnumerable(Of INamedValue)) As T()
            Get
                Return keys.Select(Function(s) Me(s.Key)).ToArray
            End Get
            Set
                Me(keys.Select(Function(s) s.Key).ToArray) = Value
            End Set
        End Property

        ''' <summary>
        ''' Get a value package at once using a key collection, 
        ''' if the key is not exists in the property, then its 
        ''' correspoding value is nothing.
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        Default Public Overloads Property ItemValue(keys As IEnumerable(Of IReadOnlyId)) As T()
            Get
                Return keys.Select(Function(s) Me(s.Identity)).ToArray
            End Get
            Set(value As T())
                Me(keys.Select(Function(s) s.Identity).ToArray) = value
            End Set
        End Property

        ''' <summary>
        ''' Add a property into the property table
        ''' </summary>
        ''' <param name="propertyName$"></param>
        ''' <param name="value"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(propertyName$, value As T)
            If propertyTable Is Nothing Then
                propertyTable = New Dictionary(Of String, T)
            End If

            Call propertyTable.Add(propertyName, value)
        End Sub

        Public Sub SetValue(propertyName$, value As T)
            If propertyTable Is Nothing Then
                propertyTable = New Dictionary(Of String, T)
            End If

            propertyTable(propertyName) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetItemValue(propertyName As String) As T
            Return ItemValue(propertyName)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub Add(propertyName As String, value As Object) Implements IDynamicsObject.Add
            Call Add(propertyName, Conversion.CTypeDynamic(Of T)(value))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub SetValue(propertyName As String, value As Object) Implements IDynamicsObject.SetValue
            Call SetValue(propertyName, Conversion.CTypeDynamic(Of T)(value))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function IDynamicsObject_GetItemValue(propertyName As String) As Object Implements IDynamicsObject.GetItemValue
            Return GetItemValue(propertyName)
        End Function

        ''' <summary>
        ''' Determines whether the Dictionary contains the specified key.
        ''' </summary>
        ''' <param name="name">The key to locate in the Dictionary.</param>
        ''' <returns>
        ''' true if the Dictionary contains an element with the specified key; 
        ''' otherwise, false.
        ''' </returns>
        Public Function HasProperty(name As String) As Boolean Implements IDynamicsObject.HasName
            If propertyTable Is Nothing Then
                Return False
            Else
                Return propertyTable.ContainsKey(name)
            End If
        End Function

        ''' <summary>
        ''' Get all keys in <see cref="Properties"/>
        ''' </summary>
        ''' <returns></returns>
        Private Function GetNames() As IEnumerable(Of String) Implements IDynamicsObject.GetNames
            Return Properties.Keys
        End Function

        ''' <summary>
        ''' 枚举这个动态字典类型之中的所有的键名，这个函数是默认不包含有类型自有的属性名称的
        ''' </summary>
        ''' <param name="joinProperties">是否包括属性名称，默认不包含</param>
        ''' <returns></returns>
        Public Function EnumerateKeys(Optional joinProperties As Boolean = False) As String()
            Dim out As New List(Of String)

            If joinProperties Then
                out += MyClass.GetType _
                    .GetProperties(PublicProperty) _
                    .Where(Function(p) p.GetIndexParameters.IsNullOrEmpty) _
                    .Select(Function(p) p.Name) _
                    .ToArray
            End If

            If Not propertyTable Is Nothing Then
                out += propertyTable.Keys
            End If

            Return out.Distinct.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"{Properties.Count} Property(s)."
        End Function

        ''' <summary>
        ''' Using for debugger view, this property is usually usefull for the dictionary view 
        ''' to see if any duplicated was existed? 
        ''' </summary>
        ''' <returns></returns>
        Protected Overridable ReadOnly Property MyHashCode As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetHashCode()
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(dynamic As DynamicPropertyBase(Of T)) As Func(Of String, T)
            Return Function(pName$) dynamic(pName)
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of NamedValue(Of T)) Implements Enumeration(Of NamedValue(Of T)).GenericEnumerator
            For Each [property] In propertyTable
                Yield New NamedValue(Of T)([property].Key, [property].Value)
            Next
        End Function
    End Class
End Namespace
