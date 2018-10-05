#Region "Microsoft.VisualBasic::66d9b7055368a1e84811f2d776bf7b79, Microsoft.VisualBasic.Core\Language\Value\DefaultValue\Default.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Interface IDefaultValue
    ' 
    '         Properties: DefaultValue
    ' 
    '     Interface IsEmpty
    ' 
    '         Properties: IsEmpty
    ' 
    '     Structure DefaultValue
    ' 
    '         Properties: DefaultValue, IsEmpty
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) [When], getDefault, ToString
    '         Operators: (+2 Overloads) +, (+6 Overloads) Or
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Perl

Namespace Language.Default

    Public Delegate Function Assert(Of T)(obj As T) As Boolean

    ''' <summary>
    ''' + Test of A eqauls to B?
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Public Delegate Function BinaryAssert(Of T)(x As T, y As T) As Boolean

    Public Interface IDefaultValue(Of T)
        ReadOnly Property DefaultValue As T
    End Interface

    ''' <summary>
    ''' Apply on the structure type that assert the object is null or not.
    ''' </summary>
    Public Interface IsEmpty
        ReadOnly Property IsEmpty As Boolean
    End Interface

    ''' <summary>
    ''' The default value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure DefaultValue(Of T) : Implements IDefaultValue(Of T)
        Implements IsEmpty

        Public ReadOnly Property DefaultValue As T Implements IDefaultValue(Of T).DefaultValue
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If LazyValue Is Nothing Then
                    Return Value
                Else
                    ' using lazy loading, if the default value takes time to creates.
                    Return LazyValue.Value()
                End If
            End Get
        End Property

        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return LazyValue Is Nothing AndAlso assert(Value)
            End Get
        End Property

        ''' <summary>
        ''' The default value for <see cref="DefaultValue"/>
        ''' </summary>
        Dim Value As T

        ''' <summary>
        ''' 假若生成目标值的时间比较久，可以将其申明为Lambda表达式，这样子可以进行惰性加载
        ''' </summary>
        Dim LazyValue As Lazy(Of T)

        ''' <summary>
        ''' asset that if target value is null?
        ''' </summary>
        Dim assert As Assert(Of Object)

        Sub New(value As T, Optional assert As Assert(Of Object) = Nothing)
            Me.Value = value
            Me.assert = assert Or defaultAssert
        End Sub

        Sub New(lazy As Func(Of T), Optional assert As Assert(Of Object) = Nothing)
            Me.LazyValue = lazy.AsLazy
            Me.assert = assert Or defaultAssert
        End Sub

        Public Function [When](expression As Boolean) As DefaultValue(Of T)
            assert = Function(null) expression
            Return Me
        End Function

        Public Function [When](assert As Assert(Of T)) As DefaultValue(Of T)
            Me.assert = Function(o) assert(DirectCast(o, T))
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"default({Value})"
        End Function

        ''' <summary>
        ''' Add handler
        ''' </summary>
        ''' <param name="[default]"></param>
        ''' <param name="assert"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +([default] As DefaultValue(Of T), assert As Assert(Of Object)) As DefaultValue(Of T)
            Return New DefaultValue(Of T) With {
                .assert = assert,
                .Value = [default].Value
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +([default] As DefaultValue(Of T), assert As Expression(Of Func(Of Boolean))) As DefaultValue(Of T)
            Return New DefaultValue(Of T) With {
                .assert = Function(null) (assert.Compile())(),
                .Value = [default].Value
            }
        End Operator

        ''' <summary>
        ''' if <see cref="assert"/> is true, then will using default <see cref="value"/>, 
        ''' otherwise, return the source <paramref name="obj"/>.
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="[default]"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or(obj As T, [default] As DefaultValue(Of T)) As T
            Return getDefault(obj, [default].DefaultValue, If([default].assert, ExceptionHandle.defaultHandler))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function getDefault(value As T, [default] As T, assert As Assert(Of Object))
            Return If(assert(value), [default], value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or([default] As DefaultValue(Of T), obj As T) As T
            Return getDefault([default].DefaultValue, obj, If([default].assert, ExceptionHandle.defaultHandler))
        End Operator

        ''' <summary>
        ''' 这个操作符允许链式计算默认值：
        ''' 
        ''' A OR B OR C OR x OR y OR z
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or(x As DefaultValue(Of T), y As DefaultValue(Of T)) As T
            Return x.DefaultValue Or y
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(obj As T) As DefaultValue(Of T)
            Return New DefaultValue(Of T) With {
                .Value = obj,
                .assert = AddressOf ExceptionHandle.Default
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType([default] As DefaultValue(Of T)) As T
            Return [default].DefaultValue
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(lazy As Func(Of T)) As DefaultValue(Of T)
            Return New DefaultValue(Of T) With {
                .LazyValue = lazy.AsLazy,
                .assert = AddressOf ExceptionHandle.Default
            }
        End Operator
    End Structure
End Namespace
