#Region "Microsoft.VisualBasic::e012597933fd0b5bad590ab8f132bb71, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Language\Value.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Language

    ''' <summary>
    ''' var in VisualBasic
    ''' </summary>
    Public Class Value : Inherits Value(Of Object)

        Public Overrides Function ToString() As String
            If value Is Nothing Then
                Return Nothing
            End If
            Return value.ToString
        End Function
    End Class

    '''' <summary>
    '''' Language reference pointer
    '''' </summary>
    '''' <typeparam name="T"></typeparam>
    'Public Class Ref(Of T) : Inherits Value(Of T)

    '    Public Overrides Property value As T
    '        Get
    '            Return __pointer.Value
    '        End Get
    '        Set(value As T)
    '            __pointer.Value = value
    '        End Set
    '    End Property

    '    ReadOnly __pointer As PropertyValue(Of T)

    '    Sub New(ByRef value As T)
    '        __pointer = New PropertyValue(Of T)(Function() value, Sub(x) value = x)
    '    End Sub
    'End Class

    ''' <summary>
    ''' You can applying this data type into a dictionary object to makes the mathematics calculation more easily.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Value(Of T)

        ''' <summary>
        ''' The object value with a specific type define.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property value As T

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
        ''' Value assignment: ``Dim s As String = Value(Of String) = var``
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
    End Class
End Namespace
