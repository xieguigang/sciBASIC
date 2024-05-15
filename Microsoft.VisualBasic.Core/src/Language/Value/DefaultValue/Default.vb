#Region "Microsoft.VisualBasic::d3753a739d8821df26dad91fc2759cd0, Microsoft.VisualBasic.Core\src\Language\Value\DefaultValue\Default.vb"

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

    '   Total Lines: 233
    '    Code Lines: 128
    ' Comment Lines: 75
    '   Blank Lines: 30
    '     File Size: 8.97 KB


    '     Delegate Function
    ' 
    ' 
    '     Interface IDefault
    ' 
    '         Properties: DefaultValue
    ' 
    '     Interface IsEmpty
    ' 
    '         Properties: IsEmpty
    ' 
    '     Structure [Default]
    ' 
    '         Properties: DefaultValue, IsEmpty
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) [When], getDefault, GetNumericAssert, ToString
    '         Operators: (+2 Overloads) +, (+6 Overloads) Or
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Perl

Namespace Language.Default

    ''' <summary>
    ''' + Test of A eqauls to B?
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Public Delegate Function BinaryAssert(Of T)(x As T, y As T) As Boolean

    Public Interface IDefault(Of T)
        ReadOnly Property DefaultValue As T
    End Interface

    ''' <summary>
    ''' Apply on the structure type that assert the object is null or not.
    ''' </summary>
    Public Interface IsEmpty

        ''' <summary>
        ''' Does current object has any value inside?
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsEmpty As Boolean

    End Interface

    ''' <summary>
    ''' The default value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure [Default](Of T)
        Implements IDefault(Of T)
        Implements IsEmpty

        Public ReadOnly Property DefaultValue As T Implements IDefault(Of T).DefaultValue
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If Not constructor Is Nothing Then
                    Return constructor()
                ElseIf Not lazy Is Nothing Then
                    ' using lazy loading, if the default value takes time to creates.
                    Return lazy.Value()
                Else
                    Return value
                End If
            End Get
        End Property

        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return lazy Is Nothing AndAlso constructor Is Nothing AndAlso True = assert(value)
            End Get
        End Property

#Region "Default Value/Generator"
        ''' <summary>
        ''' The default value for <see cref="DefaultValue"/>
        ''' </summary>
        Dim value As T

        ''' <summary>
        ''' 假若生成目标值的时间比较久，可以将其申明为Lambda表达式，这样子可以进行惰性加载
        ''' </summary>
        Dim lazy As Lazy(Of T)

        ''' <summary>
        ''' 与<see cref="lazy"/>不同的是，这个会一直产生新的数据
        ''' </summary>
        Dim constructor As Func(Of T)
#End Region

        ''' <summary>
        ''' asset that if target value is null? If this function returns true when 
        ''' test on the object, means object value is missing or null, then default 
        ''' value <see cref="DefaultValue"/> will be returns.
        ''' </summary>
        Dim assert As Predicate(Of Object)

        ''' <summary>
        ''' 这个判断函数优化了对数字类型的判断
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 在VB之中，数值类型在未赋值的状态下默认值为零，意味着此时该数值的值为空
        ''' 但是不清楚这样子判断是否会出现bug？
        ''' </remarks>
        Public Shared Function GetNumericAssert(n As Object) As Boolean
            If n Is Nothing Then
                ' 可空类型的数值类型
                Return True
            End If

            Select Case n.GetType
                Case GetType(Integer), GetType(Long), GetType(ULong), GetType(UInteger), GetType(Short), GetType(UShort)
                    Return CInt(n) = 0 OrElse CDbl(n).IsNaNImaginary
                Case GetType(Double), GetType(Single), GetType(Decimal)
                    Return CDbl(n) = 0.0 OrElse CDbl(n).IsNaNImaginary
                Case Else
#If DEBUG Then
                    Call n.GetType.FullName.Warning
#End If
                    Return ExceptionHandle.Default(obj:=n)
            End Select
        End Function

        Sub New(value As T, Optional assert As Predicate(Of Object) = Nothing)
            Me.value = value
            Me.assert = assert Or defaultAssert
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="populator"></param>
        ''' <param name="assert"></param>
        ''' <param name="isLazy">
        ''' + 如果这个参数为true，则表示表达式为lazy加载，只会执行一次
        ''' + 反之当这个参数为false的时候，则表达式会不断的产生新的值
        ''' </param>
        Sub New(populator As Func(Of T), Optional assert As Predicate(Of Object) = Nothing, Optional isLazy As Boolean = True)
            If isLazy Then
                Me.lazy = populator.AsLazy
            Else
                Me.constructor = populator
            End If

            Me.assert = assert Or defaultAssert
        End Sub

        Public Function [When](expression As Boolean) As [Default](Of T)
            assert = Function(null) expression
            Return Me
        End Function

        Public Function [When](assert As Predicate(Of T)) As [Default](Of T)
            Me.assert = Function(o) assert(DirectCast(o, T))
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"default({value})"
        End Function

        ''' <summary>
        ''' Add handler
        ''' </summary>
        ''' <param name="[default]"></param>
        ''' <param name="assert"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +([default] As [Default](Of T), assert As Predicate(Of Object)) As [Default](Of T)
            Return New [Default](Of T) With {
                .assert = assert,
                .value = [default].value
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +([default] As [Default](Of T), assert As Expression(Of Func(Of Boolean))) As [Default](Of T)
            Return New [Default](Of T) With {
                .assert = Function(null) (assert.Compile())(),
                .value = [default].value
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
        Public Shared Operator Or(obj As T, [default] As [Default](Of T)) As T
            Return getDefault(obj, Function() [default].DefaultValue, If([default].assert, ExceptionHandle.defaultHandler))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function getDefault(value As T, [default] As Func(Of T), assert As Predicate(Of Object))
            Return If(assert(value), [default](), value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or([default] As [Default](Of T), obj As T) As T
            Return getDefault([default].DefaultValue, Function() obj, If([default].assert, ExceptionHandle.defaultHandler))
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
        Public Shared Operator Or(x As [Default](Of T), y As [Default](Of T)) As T
            Return x.DefaultValue Or y
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Widening Operator CType(obj As T) As [Default](Of T)
            Return New [Default](Of T) With {
                .value = obj,
                .assert = AddressOf ExceptionHandle.Default
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType([default] As [Default](Of T)) As T
            Return [default].DefaultValue
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(lazy As Func(Of T)) As [Default](Of T)
            Return New [Default](Of T) With {
                .lazy = lazy.AsLazy,
                .assert = AddressOf ExceptionHandle.Default
            }
        End Operator
    End Structure
End Namespace
