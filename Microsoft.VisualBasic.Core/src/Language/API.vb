#Region "Microsoft.VisualBasic::ef7f209661ae7d45eb0706d7a27e1e37, Microsoft.VisualBasic.Core\src\Language\API.vb"

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

    '   Total Lines: 253
    '    Code Lines: 143 (56.52%)
    ' Comment Lines: 85 (33.60%)
    '    - Xml Docs: 85.88%
    ' 
    '   Blank Lines: 25 (9.88%)
    '     File Size: 9.46 KB


    '     Module LanguageAPI
    ' 
    '         Function: (+2 Overloads) [As], [ByRef], (+2 Overloads) [Default], (+2 Overloads) [When], AsDefault
    '                   AsNumeric, AsString, AsVector, Empty, IsNothing
    '                   Let, list, Self, (+2 Overloads) TryCastArray, TypeDef
    '                   TypeInfo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.Perl
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Language

    ''' <summary>
    ''' The VisualBasic language syntax helper API.
    ''' </summary>
    Public Module LanguageAPI

        ''' <summary>
        ''' The default value assertor. If target object assert result is nothing or empty, then this function will returns True.
        ''' </summary>
        Friend ReadOnly defaultAssert As New [Default](Of Predicate(Of Object)) With {
            .value = AddressOf ExceptionHandle.Default,
            .assert = Function(assert)
                          Return assert Is Nothing
                      End Function
        }

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TypeDef(Of T)() As TypeSchema
            Return New TypeSchema(GetType(T))
        End Function

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function TypeInfo(Of T)(x As T) As TypeSchema
            Return TypeDef(Of T)()
        End Function

        'Private Sub test()
        '    If TypeDef(Of Integer)() Or {GetType(Integer), GetType(Double)} Then
        '    End If
        'End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function Empty(Of T)() As [Default](Of T())
            Return {}
        End Function

        ''' <summary>
        ''' simulate the ``%||%`` operator in R language.
        ''' 
        ''' 模拟R语言之中的``%||%``操作符
        ''' 
        ''' ```R
        ''' `%||%` &lt;- function(x, y) if (is.null(x)) y else x
        ''' 
        ''' NULL %||% 123
        ''' # 123
        ''' 
        ''' 233 %||% 123
        ''' # 233
        ''' ```
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="isNothing"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function [Default](Of T)(x As T, Optional isNothing As Predicate(Of Object) = Nothing) As [Default](Of T)
            Return New [Default](Of T) With {
                .value = x,
                .assert = isNothing Or defaultAssert
            }
        End Function

        ''' <summary>
        ''' Using this value as the default value for this <typeparamref name="T"/> type.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="[If]"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function AsDefault(Of T)(x As T, Optional [If] As Predicate(Of Object) = Nothing) As [Default](Of T)
            Return [Default](x, [If])
        End Function

        <DebuggerStepThrough>
        Public Function [Default](Of  T)(value As T) As [Default](Of T)
            Return New [Default](Of T) With {
                .value = value,
                .assert = defaultAssert
            }
        End Function

        ''' <summary>
        ''' Value Or Default When .... is true
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="[default]"></param>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [When](Of T)([default] As T, expression As Boolean) As [Default](Of T)
            Return [default].AsDefault().When(expression)
        End Function

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [When](Of T)([default] As T, expression As Predicate(Of T)) As [Default](Of T)
            Return [default].AsDefault().When(assert:=expression)
        End Function

        ''' <summary>
        ''' Helper for update the value property of <see cref="Value(Of T)"/>
        ''' 
        ''' ```vbnet
        ''' Call Let$(<see cref="Value(Of T)"/> = x)
        ''' ```
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Let$(Of T)(value As T)
            Try
                Return CStrSafe(value)
            Catch ex As Exception
                ' 在这里知识进行帮助值的设置，所以这个错误无所谓，直接忽略掉
                Return Nothing
            End Try
        End Function

#Region "Helper for ``With``"

        ''' <summary>
        ''' My self: <see cref="LanguageAPI.ByRef(Of T)(T)"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function Self(Of T)() As Func(Of T, T)
            Return AddressOf [ByRef]
        End Function

        ''' <summary>
        ''' Extension method for VisualBasic ``With`` anonymous variable syntax source reference helper
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension> Public Function [ByRef](Of T)(x As T) As T
            Return x
        End Function

        ''' <summary>
        ''' Extension method for VisualBasic ``With`` anonymous variable syntax for determine that source reference is nothing or not?
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension>
        Public Function IsNothing(Of T As Class)(x As T) As Boolean
            Return x Is Nothing
        End Function
#End Region

        ''' <summary>
        ''' 确保总是返回一个目标类型的数组
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="var"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TryCastArray(Of T)(var As [Variant](Of T, T())) As T()
            If var Is Nothing Then
                Return {}
            ElseIf var Like GetType(T) Then
                Return {var.TryCast(Of T)}
            Else
                Return var.TryCast(Of T())
            End If
        End Function

        ''' <summary>
        ''' 确保总是返回一个目标类型的数组
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="var"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TryCastArray(Of T)(var As [Variant](Of T(), T)) As T()
            If var Is Nothing Then
                Return {}
            ElseIf var Like GetType(T) Then
                Return {var.TryCast(Of T)}
            Else
                Return var.TryCast(Of T())
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function AsVector(strings As IEnumerable(Of String)) As StringVector
            Return New StringVector(strings)
        End Function

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function list(ParamArray args As ArgumentReference()) As Dictionary(Of String, Object)
            Return args.ToDictionary(Function(a) a.name, Function(a) a.value)
        End Function

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsNumeric(list As Dictionary(Of String, Object)) As Dictionary(Of String, Double)
            Return list.ToDictionary(Function(t) t.Key, Function(t) CDbl(t.Value))
        End Function

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsString(list As Dictionary(Of String, Object)) As Dictionary(Of String, String)
            Return list.ToDictionary(Function(t) t.Key, Function(t) Scripting.ToString(t.Value))
        End Function

        <DebuggerStepThrough>
        <Extension>
        Public Function [As](Of A, B, T)(list As IEnumerable(Of [Variant](Of A, B))) As IEnumerable(Of T)
            Return list.Select(Function(x) CType(x.Value, T))
        End Function

        <DebuggerStepThrough>
        <Extension>
        Public Function [As](Of A, B, C, T)(list As IEnumerable(Of [Variant](Of A, B, C))) As IEnumerable(Of T)
            Return list.Select(Function(x) CType(x.Value, T))
        End Function
    End Module
End Namespace
