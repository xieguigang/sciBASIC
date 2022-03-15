#Region "Microsoft.VisualBasic::73e304381202fc509b63eb5cde26204b, sciBASIC#\Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\BindProperty.vb"

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

    '   Total Lines: 266
    '    Code Lines: 132
    ' Comment Lines: 97
    '   Blank Lines: 37
    '     File Size: 10.99 KB


    '     Structure BindProperty
    ' 
    '         Properties: Identity, IsNull, IsPrimitive, Type
    ' 
    '         Constructor: (+7 Overloads) Sub New
    ' 
    '         Function: FromSchemaTable, GetValue, ToString
    ' 
    '         Sub: SetValue, WriteScriptValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If netcore5 = 1 Then
Imports System.Data
#End If

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' Schema for <see cref="Attribute"/> and its bind <see cref="PropertyInfo"/>/<see cref="FieldInfo"/> object target.
    ''' (使用这个对象将公共的域或者属性的读写统一起来)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure BindProperty(Of T As Attribute)
        Implements IReadOnlyId
        Implements INamedValue
        Implements IProperty

        ''' <summary>
        ''' The property/field object that bind with its custom attribute <see cref="field"/> of type <typeparamref name="T"/>
        ''' </summary>
        Dim member As MemberInfo
        ''' <summary>
        ''' The flag for this field binding.
        ''' </summary>
        Dim field As T
        Dim name As String

        ReadOnly caster As LoadObject

        Friend ReadOnly handleSetValue As Action(Of Object, Object)
        Friend ReadOnly handleGetValue As Func(Of Object, Object)

#Region "Property List"

        ''' <summary>
        ''' Gets the type of this <see cref="member"/>.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As Type

        ''' <summary>
        ''' The map name or the <see cref="PropertyInfo.Name"/>.
        ''' (这个属性会首先查找标记的自定义属性的名称结果，如果不存在才会使用属性或者字段的反射成员名称)
        ''' </summary>
        ''' <returns></returns>
        Public Property Identity As String Implements IReadOnlyId.Identity, INamedValue.Key
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If name.StringEmpty Then
                    name = member.Name
                End If

                Return name
            End Get
            Friend Set(value As String)
                name = value
            End Set
        End Property

        ''' <summary>
        ''' Is this map data is null on its attribute or property data?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsNull As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return member Is Nothing OrElse field Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the <see cref="System.Type"/> is one of the primitive types.
        ''' </summary>
        ''' <returns>
        ''' true if the <see cref="System.Type"/> is one of the primitive types; otherwise, false.
        ''' </returns>
        Public ReadOnly Property IsPrimitive As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Scripting.IsPrimitive(Type)
            End Get
        End Property
#End Region

        Private Sub New(type As Type)
            Me.Type = type

            If Scripting.CasterString.ContainsKey(type) Then
                Me.caster = Scripting.CasterString(type)
            End If
        End Sub

        Sub New(attr As T, prop As PropertyInfo, Optional getName As IToString(Of T) = Nothing)
            Call Me.New(prop.PropertyType)

            field = attr
            member = prop

            If Not getName Is Nothing Then
                name = getName(attr)
            End If

            ' Compile the property get/set as the delegate
            With prop
                handleSetValue = AddressOf prop.SetValue  ' .DeclaringType.PropertySet(.Name)
                handleGetValue = AddressOf prop.GetValue  ' .DeclaringType.PropertyGet(.Name)
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New([property] As PropertyInfo)
            Call Me.New(Nothing, [property])
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(field As FieldInfo)
            Call Me.New(Nothing, field)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(method As MethodInfo)
            Call Me.New(Nothing, method)
        End Sub

        Sub New(attr As T, field As FieldInfo, Optional getName As IToString(Of T) = Nothing)
            Call Me.New(field.FieldType)

            Me.field = attr
            Me.member = field

            If Not getName Is Nothing Then
                name = getName(attr)
            End If

            With field
                handleSetValue = AddressOf field.SetValue  ' .DeclaringType.FieldSet(.Name)
                handleGetValue = AddressOf field.GetValue  ' .DeclaringType.FieldGet(.Name)
            End With
        End Sub

        Sub New(attr As T, method As MethodInfo, Optional getName As IToString(Of T) = Nothing)
            Call Me.New(method.ReturnType)

            Me.field = attr
            Me.member = method

            If Not getName Is Nothing Then
                name = getName(attr)
            End If

            If method.IsNonParametric(optionalAsNone:=True) Then
                Throw New InvalidConstraintException("Only allows parameterless method or all of the parameter should be optional!")
            End If

            With field
                handleSetValue = Sub(a, b)
                                     Throw New ReadOnlyException
                                 End Sub
                handleGetValue = Function(obj) method.Invoke(obj, {})
            End With
        End Sub

        ' Exceptions:
        '   T:System.ArgumentException:
        '     The index array does not contain the type of arguments needed.-or- The property's
        '     set accessor is not found. -or-value cannot be converted to the type of System.Reflection.PropertyInfo.PropertyType.
        '
        '   T:System.Reflection.TargetException:
        '     In the .NET for Windows Store apps or the Portable Class Library, catch System.Exception
        '     instead.The object does not match the target type, or a property is an instance
        '     property but obj is null.
        '
        '   T:System.Reflection.TargetParameterCountException:
        '     The number of parameters in index does not match the number of parameters the
        '     indexed property takes.
        '
        '   T:System.MethodAccessException:
        '     In the .NET for Windows Store apps or the Portable Class Library, catch the base
        '     class exception, System.MemberAccessException, instead.There was an illegal attempt
        '     to access a private or protected method inside a class.
        '
        '   T:System.Reflection.TargetInvocationException:
        '     An error occurred while setting the property value. For example, an index value
        '     specified for an indexed property is out of range. The System.Exception.InnerException
        '     property indicates the reason for the error.

        ''' <summary>
        ''' Sets the property value of a specified object with optional index values for
        ''' index properties.
        ''' (这个设置值的函数只适用于``Class``类型，对于``Structure``类型而言，则无法正常的工作)
        ''' </summary>
        ''' <param name="obj">The object whose property value will be set.</param>
        ''' <param name="value">The new property value.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetValue(obj As Object, value As Object) Implements IProperty.SetValue
            ' 2017-6-26 目前value参数为空值的话，会报错，故而在这里添加了一个If分支判断
            If value IsNot Nothing Then
                Call handleSetValue(obj, value)
            End If
        End Sub

        Public Sub WriteScriptValue(obj As Object, value As String)
            If Not value Is Nothing Then
                Call SetValue(obj, caster(value))
            End If
        End Sub

        ' Exceptions:
        '   T:System.ArgumentException:
        '     The index array does not contain the type of arguments needed.-or- The property's
        '     get accessor is not found.
        '
        '   T:System.Reflection.TargetException:
        '     In the .NET for Windows Store apps or the Portable Class Library, catch System.Exception
        '     instead.The object does not match the target type, or a property is an instance
        '     property but obj is null.
        '
        '   T:System.Reflection.TargetParameterCountException:
        '     The number of parameters in index does not match the number of parameters the
        '     indexed property takes.
        '
        '   T:System.MethodAccessException:
        '     In the .NET for Windows Store apps or the Portable Class Library, catch the base
        '     class exception, System.MemberAccessException, instead.There was an illegal attempt
        '     to access a private or protected method inside a class.
        '
        '   T:System.Reflection.TargetInvocationException:
        '     An error occurred while retrieving the property value. For example, an index
        '     value specified for an indexed property is out of range. The System.Exception.InnerException
        '     property indicates the reason for the error.

        ''' <summary>
        ''' Returns the property value of a specified object with optional index values for
        ''' indexed properties.
        ''' </summary>
        ''' <param name="x">The object whose property value will be returned.</param>
        ''' <returns>The property value of the specified object.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetValue(x As Object) As Object Implements IProperty.GetValue
            Return handleGetValue(x)
        End Function

        ''' <summary>
        ''' Display this schema maps in Visualbasic style.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"Dim {member.Name} As {Type.FullName}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromSchemaTable(x As KeyValuePair(Of T, PropertyInfo)) As BindProperty(Of T)
            Return New BindProperty(Of T) With {
                .field = x.Key,
                .member = x.Value
            }
        End Function
    End Structure
End Namespace
