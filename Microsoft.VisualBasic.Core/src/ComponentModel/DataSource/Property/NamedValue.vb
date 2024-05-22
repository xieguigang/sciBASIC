#Region "Microsoft.VisualBasic::b5e45dc8e792775a8c3ccd9cddf5409b, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Property\NamedValue.vb"

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

    '   Total Lines: 218
    '    Code Lines: 140 (64.22%)
    ' Comment Lines: 50 (22.94%)
    '    - Xml Docs: 88.00%
    ' 
    '   Blank Lines: 28 (12.84%)
    '     File Size: 8.42 KB


    '     Structure NamedValue
    ' 
    '         Properties: Description, IsEmpty, Name, Value, ValueType
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: FixValue, getValueStr, ToString
    '         Operators: (+2 Overloads) +, <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NET48 And Not NETCOREAPP Then
Imports System.Web.Script.Serialization
#End If

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' The value object have a name string.(一个具有自己的名称的变量值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NamedValue(Of T) : Implements INamedValue
        Implements IKeyValuePairObject(Of String, T)
        Implements IsEmpty

        ''' <summary>
        ''' Identifier tag data. you can using this property value as a dictionary key.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property Name As String Implements INamedValue.Key, IKeyValuePairObject(Of String, T).Key

        ''' <summary>
        ''' Object value
        ''' </summary>
        ''' <returns></returns>
        <XmlElement> Public Property Value As T Implements IKeyValuePairObject(Of String, T).Value

        ''' <summary>
        ''' Additional description text about this variable <see cref="Value"/>
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property Description As String

        ''' <summary>
        ''' Does this object have value?
        ''' </summary>
        ''' <returns></returns>
        <XmlIgnore, DataIgnored>
        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return String.IsNullOrEmpty(Name) AndAlso Value Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Returns object gettype
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ValueType As Type
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                ' 假若类型参数T是基类型的话，则直接使用GetType(T)只能够得到基类型的信息，无法得到当前的实现类型的信息
                ' 故而要在这里使用对象自身的GetType方法来获取真正的类型信息
                Return Value.GetType
            End Get
        End Property

        Default Public ReadOnly Property TryMethodInvoke(arg As Object) As Object
            Get
                If Value Is Nothing Then
                    Return Nothing
                Else
                    Dim type As Type = ValueType

                    If type.IsInheritsFrom(GetType([Delegate])) Then
                        Dim del As [Delegate] = CObj(Value)
                        Return del.DynamicInvoke({arg})
                    Else
                        Dim indexProp As PropertyInfo = type _
                            .GetProperties(PublicProperty) _
                            .FirstOrDefault(Function(p)
                                                Return p.CanRead AndAlso Not p.GetIndexParameters.IsNullOrEmpty
                                            End Function)

                        If Not indexProp Is Nothing Then
                            Return indexProp.GetMethod.Invoke(Value, {arg})
                        Else
                            Return Nothing
                        End If
                    End If
                End If
            End Get
        End Property

        ''' <summary>
        ''' Creates a object bind with a specific <see cref="Name"/>.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        Sub New(name$, Optional value As T = Nothing, Optional describ As String = Nothing)
            Me.Name = name
            Me.Value = value
            Me.Description = describ
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(item As KeyValuePair(Of String, T))
            Call Me.New(item.Key, item.Value)
        End Sub

        ''' <summary>
        ''' View object.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Try
                ' 2018-4-9 在这里将显示修改为类型的ToString方法，这样子就可以由
                ' 用户来通过重写ToString方法来自定义显示，而非强制使用GetJson方法
                ' 将全部的对象都显示出来，对于属性很多的对象GetJson方法显示的效果不是太好
                If Description.StringEmpty Then
                    Return $"{Name} -> {getValueStr()}"
                Else
                    Return $"{Name} -> {getValueStr()} ({Description.TrimNewLine})"
                End If
            Catch ex As Exception
                Return Name
            End Try
        End Function

        Private Function getValueStr() As String
            If Value Is Nothing Then
                Return "null"
            End If

            If DataFramework.IsPrimitive(ValueType) OrElse Not ValueType.IsArray Then
                Return Value.ToString
            Else
                Return DirectCast(CObj(Value), Array) _
                    .AsObjectEnumerator _
                    .Select(Function(o) any.ToString(o)) _
                    .JoinBy("; ")
            End If
        End Function

        Public Function FixValue(h As Func(Of T, T)) As NamedValue(Of T)
            Return New NamedValue(Of T)(Name, h(Value))
        End Function

        Public Shared Operator +(obj As NamedValue(Of T)) As T
            Return obj.Value
        End Operator

        ''' <summary>
        ''' 这个函数会将<paramref name="value"/>插入为<paramref name="table"/>的第一个元素
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Shared Operator +(value As NamedValue(Of T), table As Dictionary(Of String, T)) As Dictionary(Of String, T)
            Dim newTable As New Dictionary(Of String, T) From {
                {value.Name, value.Value}
            }

            For Each map As KeyValuePair(Of String, T) In table
                Call newTable.Add(map.Key, map.Value)
            Next

            Return newTable
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(tuple As NamedValue(Of T), compares As T) As Boolean
            If tuple.Value Is Nothing Then
                If compares Is Nothing Then
                    Return True
                Else
                    Return False
                End If
            Else
                If compares Is Nothing Then
                    Return False
                Else
                    Return tuple.Value.Equals(compares)
                End If
            End If
        End Operator

#If NET_48 Or netcore5 = 1 Then

        ''' <summary>
        ''' Convert from tuple
        ''' </summary>
        ''' <param name="tuple"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(tuple As (name$, value As T)) As NamedValue(Of T)
            Return New NamedValue(Of T)(tuple.name, tuple.value)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(tuple As (name$, value As T, describ$)) As NamedValue(Of T)
            Return New NamedValue(Of T)(tuple.name, tuple.value, tuple.describ)
        End Operator

#End If

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(tuple As NamedValue(Of T), compares As T) As Boolean
            Return Not tuple = compares
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(value As NamedValue(Of T)) As T
            Return value.Value
        End Operator
    End Structure
End Namespace
