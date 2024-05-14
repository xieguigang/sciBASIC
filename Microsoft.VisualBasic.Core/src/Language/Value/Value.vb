#Region "Microsoft.VisualBasic::f827c09b0f3b003a0adb8f1f317f5e7c, Microsoft.VisualBasic.Core\src\Language\Value\Value.vb"

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

    '   Total Lines: 310
    '    Code Lines: 162
    ' Comment Lines: 110
    '   Blank Lines: 38
    '     File Size: 12.38 KB


    '     Class Value
    ' 
    '         Properties: HasValue, Value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: [Default], (+2 Overloads) Equals, GetJson, GetUnderlyingType, (+2 Overloads) GetValueOrDefault
    '                   IsNothing, ToString
    '         Operators: -, (+3 Overloads) +, <=, (+2 Overloads) <>, (+2 Overloads) =
    '                    >=, (+4 Overloads) Like
    '         Interface IValueOf
    ' 
    '             Properties: Value
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language

    ''' <summary>
    ''' You can applying this data type into a dictionary object to makes the mathematics calculation more easily.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Value(Of T) : Implements IValueOf

        ''' <summary>
        ''' This object have a <see cref="IValueOf.Value"/> property for stores its data
        ''' </summary>
        Public Interface IValueOf

            ''' <summary>
            ''' value property for this object stores its data
            ''' </summary>
            ''' <returns></returns>
            Property Value As T
        End Interface

        ''' <summary>
        ''' Gets a value indicating whether the current System.Nullable`1 object has a valid
        ''' value of its underlying type.
        ''' </summary>
        ''' <returns>true if the current System.Nullable`1 object has a value; false if the current
        ''' System.Nullable`1 object has no value.</returns>
        Public Overridable ReadOnly Property HasValue As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Not Value Is Nothing
            End Get
        End Property

        Shared ReadOnly defaultItem As PropertyInfo

        ''' <summary>
        ''' Get data from <see cref="Value"/> through its index method
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Default Public Overridable Property Index(key As Object) As Object
            Get
                If defaultItem Is Nothing Then
                    Throw New MissingMemberException($"target clr type '{GetType(T)}' has no default property, could not get data through its index method!")
                End If

                Return defaultItem.GetValue(Value, index:={key})
            End Get
            Set(value As Object)
                If defaultItem Is Nothing Then
                    Throw New MissingMemberException($"target clr type '{GetType(T)}' has no default property, could not set data through its index method!")
                End If

                Call defaultItem.SetValue(Me.Value, value, index:={key})
            End Set
        End Property

        ''' <summary>
        ''' Retrieves the value of the current System.Nullable`1 object, or the object's
        ''' default value.
        ''' </summary>
        ''' <returns>The value of the System.Nullable`1.Value property if the System.Nullable`1.HasValue
        ''' property is true; otherwise, the default value of the current System.Nullable`1
        ''' object. The type of the default value is the type argument of the current System.Nullable`1
        ''' object, and the value of the default value consists solely of binary zeroes.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetValueOrDefault() As T
            Return GetValueOrDefault(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves the value of the current System.Nullable`1 object, or the specified
        ''' default value.
        ''' </summary>
        ''' <param name="defaultValue">A value to return if the System.Nullable`1.HasValue property is false.</param>
        ''' <returns>The value of the System.Nullable`1.Value property if the System.Nullable`1.HasValue
        ''' property is true; otherwise, the defaultValue parameter.</returns>
        Public Function GetValueOrDefault(defaultValue As T) As T
            If Value Is Nothing Then
                Return defaultValue
            Else
                Return Value
            End If
        End Function

        ''' <summary>
        ''' Indicates whether the current System.Nullable`1 object is equal to a specified
        ''' object.
        ''' </summary>
        ''' <param name="other">An object.</param>
        ''' <returns>true if the other parameter is equal to the current System.Nullable`1 object;
        ''' otherwise, false. This table describes how equality is defined for the compared
        ''' values: Return ValueDescriptiontrueThe System.Nullable`1.HasValue property is
        ''' false, and the other parameter is null. That is, two null values are equal by
        ''' definition.-or-The System.Nullable`1.HasValue property is true, and the value
        ''' returned by the System.Nullable`1.Value property is equal to the other parameter.falseThe
        ''' System.Nullable`1.HasValue property for the current System.Nullable`1 structure
        ''' is true, and the other parameter is null.-or-The System.Nullable`1.HasValue property
        ''' for the current System.Nullable`1 structure is false, and the other parameter
        ''' is not null.-or-The System.Nullable`1.HasValue property for the current System.Nullable`1
        ''' structure is true, and the value returned by the System.Nullable`1.Value property
        ''' is not equal to the other parameter.</returns>
        Public Overrides Function Equals(other As Object) As Boolean
            If other Is Nothing Then
                Return False
            ElseIf Not other.GetType Is GetType(T) Then
                Return False
            Else
                Return Value.Equals(other)
            End If
        End Function

        ''' <summary>
        ''' Value equals of <see cref="Value"/>
        ''' </summary>
        ''' <param name="other"></param>
        ''' <returns></returns>
        Public Overloads Function Equals(other As T) As Boolean
            Return Value.Equals(other)
        End Function

        ''' <summary>
        ''' The object value with a specific type define.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property Value As T Implements IValueOf.Value

        ''' <summary>
        ''' Creates an reference value object with the specific object value
        ''' </summary>
        ''' <param name="value"></param>
        Sub New(value As T)
            Me.Value = value
        End Sub

        ''' <summary>
        ''' Value is Nothing
        ''' </summary>
        Sub New()
            Call MyBase.New
            Value = Nothing
        End Sub

        Shared Sub New()
            Dim type As Type = GetType(T)
            Dim properties = type.GetProperties

            defaultItem = properties _
                .Where(Function(pi) pi.GetIndexParameters.Length = 1) _
                .Where(Function(pi) pi.Attributes.HasFlag(PropertyAttributes.HasDefault)) _
                .FirstOrDefault
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function GetUnderlyingType() As Type
            If _Value Is Nothing Then
                Return GetType(T)
            Else
                Return _Value.GetType()
            End If
        End Function

        ''' <summary>
        ''' Is the <see cref="Value"/> is nothing.
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsNothing() As Boolean
            Return Value Is Nothing
        End Function

        ''' <summary>
        ''' Display <see cref="value"/> ``ToString()`` function value.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return InputHandler.ToString(Value)
        End Function

        ''' <summary>
        ''' Get json string of the <see cref="Value"/>.
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetJson() As String
            Return Value.GetJson
        End Function

        Public Shared Function [Default]() As Value(Of T)
            Return New Value(Of T)(Nothing)
        End Function

        Public Overloads Shared Operator +(list As Generic.List(Of Value(Of T)), x As Value(Of T)) As Generic.List(Of Value(Of T))
            Call list.Add(x)
            Return list
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator +(x As Value(Of T), list As IEnumerable(Of T)) As List(Of T)
            Return (+x).Join(list)
        End Operator

        Public Overloads Shared Operator -(list As Generic.List(Of Value(Of T)), x As Value(Of T)) As Generic.List(Of Value(Of T))
            Call list.Remove(x)
            Return list
        End Operator

        Public Shared Operator <=(value As Value(Of T), o As T) As T
            value.Value = o
            Return o
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(x As Value(Of T)) As T
            Return x.Value
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(x As T) As Value(Of T)
            Return New Value(Of T)(x)
        End Operator

        ''' <summary>
        ''' Gets the <see cref="Value"/> property value.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(x As Value(Of T)) As T
            Return x.Value
        End Operator

        ''' <summary>
        ''' Inline value assignment: ``Dim s As String = Value(Of String) = var``
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="o"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(value As Value(Of T), o As T) As T
            value.Value = o
            Return o
        End Operator

        ''' <summary>
        ''' value equals?
        ''' </summary>
        ''' <param name="o"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Shared Operator =(o As T, value As Value(Of T)) As Boolean
            Return o.Equals(value.Value)
        End Operator

        Public Shared Operator <>(o As T, value As Value(Of T)) As Boolean
            Return Not o = value
        End Operator

        ''' <summary>
        ''' Type match operator, this may consider inherits of base type and interface implementation.
        ''' </summary>
        ''' <param name="o"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Unlike this operation its behavior, the Variant type its type match operator 
        ''' is a type exactly match operation.
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Like(o As Value(Of T), type As Type) As Boolean
            If o.GetUnderlyingType Is type Then
                Return True
            End If

            If type.IsInterface Then
                Return o.GetUnderlyingType.ImplementInterface(type)
            ElseIf type.IsClass Then
                Return o.GetUnderlyingType.IsInheritsFrom(type)
            Else
                Return False
            End If
        End Operator

        Public Shared Operator <>(value As Value(Of T), o As T) As T
            Throw New NotSupportedException
        End Operator

        Public Shared Operator >=(value As Value(Of T), o As T) As T
            Throw New NotSupportedException
        End Operator

        Public Shared Operator Like(ref As Value(Of T), val As T) As Boolean
            If ref Is Nothing OrElse Not ref.HasValue Then
                Return val Is Nothing
            ElseIf val Is Nothing Then
                Return False
            Else
                Return ref.Value.Equals(val)
            End If
        End Operator
    End Class
End Namespace
