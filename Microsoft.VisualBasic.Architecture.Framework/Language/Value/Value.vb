#Region "Microsoft.VisualBasic::dfc787c2c39b1a6dfcd1caca22003d3a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Value\Value.vb"

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


Namespace Language

    ''' <summary>
    ''' You can applying this data type into a dictionary object to makes the mathematics calculation more easily.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Value(Of T) : Implements IValueOf

        ''' <summary>
        ''' This object have a <see cref="IValueOf.value"/> property for stores its data
        ''' </summary>
        Public Interface IValueOf

            ''' <summary>
            ''' value property for this object stores its data
            ''' </summary>
            ''' <returns></returns>
            Property value As T
        End Interface

        '
        ' Summary:
        '     Gets a value indicating whether the current System.Nullable`1 object has a valid
        '     value of its underlying type.
        '
        ' Returns:
        '     true if the current System.Nullable`1 object has a value; false if the current
        '     System.Nullable`1 object has no value.
        Public ReadOnly Property HasValue As Boolean
            Get
                Return Not value Is Nothing
            End Get
        End Property

        '
        ' Summary:
        '     Retrieves the value of the current System.Nullable`1 object, or the object's
        '     default value.
        '
        ' Returns:
        '     The value of the System.Nullable`1.Value property if the System.Nullable`1.HasValue
        '     property is true; otherwise, the default value of the current System.Nullable`1
        '     object. The type of the default value is the type argument of the current System.Nullable`1
        '     object, and the value of the default value consists solely of binary zeroes.
        Public Function GetValueOrDefault() As T
            Return GetValueOrDefault(Nothing)
        End Function
        '
        ' Summary:
        '     Retrieves the value of the current System.Nullable`1 object, or the specified
        '     default value.
        '
        ' Parameters:
        '   defaultValue:
        '     A value to return if the System.Nullable`1.HasValue property is false.
        '
        ' Returns:
        '     The value of the System.Nullable`1.Value property if the System.Nullable`1.HasValue
        '     property is true; otherwise, the defaultValue parameter.
        Public Function GetValueOrDefault(defaultValue As T) As T
            If value Is Nothing Then
                Return defaultValue
            Else
                Return value
            End If
        End Function
        '
        ' Summary:
        '     Indicates whether the current System.Nullable`1 object is equal to a specified
        '     object.
        '
        ' Parameters:
        '   other:
        '     An object.
        '
        ' Returns:
        '     true if the other parameter is equal to the current System.Nullable`1 object;
        '     otherwise, false. This table describes how equality is defined for the compared
        '     values: Return ValueDescriptiontrueThe System.Nullable`1.HasValue property is
        '     false, and the other parameter is null. That is, two null values are equal by
        '     definition.-or-The System.Nullable`1.HasValue property is true, and the value
        '     returned by the System.Nullable`1.Value property is equal to the other parameter.falseThe
        '     System.Nullable`1.HasValue property for the current System.Nullable`1 structure
        '     is true, and the other parameter is null.-or-The System.Nullable`1.HasValue property
        '     for the current System.Nullable`1 structure is false, and the other parameter
        '     is not null.-or-The System.Nullable`1.HasValue property for the current System.Nullable`1
        '     structure is true, and the value returned by the System.Nullable`1.Value property
        '     is not equal to the other parameter.
        Public Overrides Function Equals(other As Object) As Boolean
            If other Is Nothing Then
                Return False
            ElseIf Not other.GetType Is GetType(T) Then
                Return False
            Else
                Return value.Equals(other)
            End If
        End Function

        ''' <summary>
        ''' The object value with a specific type define.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property value As T Implements IValueOf.value

        ''' <summary>
        ''' Creates an reference value object with the specific object value
        ''' </summary>
        ''' <param name="value"></param>
        Sub New(value As T)
            Me.value = value
        End Sub

        ''' <summary>
        ''' Value is Nothing
        ''' </summary>
        Sub New()
            value = Nothing
        End Sub

        ''' <summary>
        ''' Is the value is nothing.
        ''' </summary>
        ''' <returns></returns>
        Public Function IsNothing() As Boolean
            Return value Is Nothing
        End Function

        ''' <summary>
        ''' Display <see cref="value"/> ``ToString()`` function value.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Scripting.InputHandler.ToString(value)
        End Function

        Public Overloads Shared Operator +(list As Generic.List(Of Value(Of T)), x As Value(Of T)) As Generic.List(Of Value(Of T))
            Call list.Add(x)
            Return list
        End Operator

        Public Overloads Shared Operator +(x As Value(Of T), list As IEnumerable(Of T)) As List(Of T)
            Return (+x).Join(list)
        End Operator

        Public Overloads Shared Operator -(list As Generic.List(Of Value(Of T)), x As Value(Of T)) As Generic.List(Of Value(Of T))
            Call list.Remove(x)
            Return list
        End Operator

        Public Shared Operator <=(value As Value(Of T), o As T) As T
            value.value = o
            Return o
        End Operator

        Public Shared Narrowing Operator CType(x As Value(Of T)) As T
            Return x.value
        End Operator

        Public Shared Widening Operator CType(x As T) As Value(Of T)
            Return New Value(Of T)(x)
        End Operator

        ''' <summary>
        ''' Gets the <see cref="Value"/> property value.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator +(x As Value(Of T)) As T
            Return x.value
        End Operator

        ''' <summary>
        ''' Inline value assignment: ``Dim s As String = Value(Of String) = var``
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="o"></param>
        ''' <returns></returns>
        Public Shared Operator =(value As Value(Of T), o As T) As T
            value.value = o
            Return o
        End Operator

        Public Shared Operator <>(value As Value(Of T), o As T) As T
            Throw New NotSupportedException
        End Operator

        Public Shared Operator >=(value As Value(Of T), o As T) As T
            Throw New NotSupportedException
        End Operator

        'Public Shared Operator &(o As Value(Of T)) As T
        '    Return o.value
        'End Operator
    End Class
End Namespace
