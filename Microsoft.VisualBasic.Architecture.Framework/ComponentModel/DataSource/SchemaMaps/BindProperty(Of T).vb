#Region "Microsoft.VisualBasic::8a4a9a7d54908ccf4d7e2b8cd44a9b3a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\SchemaMaps\BindProperty(Of T).vb"

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
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Emit.Delegates

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
        ''' The property/field object that bind with its custom attribute <see cref="Field"/> of type <typeparamref name="T"/>
        ''' </summary>
        Dim member As MemberInfo
        ''' <summary>
        ''' The flag for this field binding.
        ''' </summary>
        Dim Field As T

        ReadOnly __setValue As Action(Of Object, Object)
        ReadOnly __getValue As Func(Of Object, Object)

#Region "Property List"

        ''' <summary>
        ''' Gets the type of this property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As Type

        ''' <summary>
        ''' The map name or the <see cref="PropertyInfo.Name"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property Identity As String Implements IReadOnlyId.Identity, INamedValue.Key
            Get
                Return member.Name
            End Get
            Private Set(value As String)
                ' DO NOTHING
            End Set
        End Property

        ''' <summary>
        ''' Is this map data is null on its attribute or property data?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsNull As Boolean
            Get
                Return member Is Nothing OrElse Field Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the <see cref="System.Type"/> is one of the primitive types.
        ''' </summary>
        ''' <returns>true if the <see cref="System.Type"/> is one of the primitive types; otherwise, false.</returns>
        Public ReadOnly Property IsPrimitive As Boolean
            Get
                Return Scripting.IsPrimitive(Type)
            End Get
        End Property
#End Region

        Sub New(attr As T, prop As PropertyInfo)
            Field = attr
            member = prop
            Type = prop.PropertyType

            With prop ' Compile the property get/set as the delegate
                __setValue = .DeclaringType.PropertySet(.Name)
                __getValue = .DeclaringType.PropertyGet(.Name)
            End With
        End Sub

        Sub New([property] As PropertyInfo)
            Call Me.New(Nothing, [property])
        End Sub

        Sub New(field As FieldInfo)
            Call Me.New(Nothing, field)
        End Sub

        Sub New(attr As T, field As FieldInfo)
            Me.Field = attr
            Me.member = field
            Type = field.FieldType

            With field
                __setValue = .DeclaringType.FieldSet(.Name)
                __getValue = .DeclaringType.FieldGet(.Name)
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
        Public Sub SetValue(obj As Object, value As Object) Implements IProperty.SetValue
            ' 2017-6-26 目前value参数为空值的话，会报错，故而在这里添加了一个If分支判断
            If value IsNot Nothing Then
                Call __setValue(obj, value)
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
        Public Function GetValue(x As Object) As Object Implements IProperty.GetValue
            Return __getValue(x)
        End Function

        ''' <summary>
        ''' Display this schema maps in Visualbasic style.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"Dim {member.Name} As {Type.FullName}"
        End Function

        Public Shared Function FromSchemaTable(x As KeyValuePair(Of T, PropertyInfo)) As BindProperty(Of T)
            Return New BindProperty(Of T) With {
                .Field = x.Key,
                .member = x.Value
            }
        End Function
    End Structure
End Namespace
