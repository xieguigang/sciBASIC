#Region "Microsoft.VisualBasic::566a5359f103960306dcd8f231de2d24, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Methods.vb"

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

    '   Total Lines: 76
    '    Code Lines: 42
    ' Comment Lines: 26
    '   Blank Lines: 8
    '     File Size: 2.83 KB


    ' Module MethodsExtension
    ' 
    '     Function: (+2 Overloads) AsLazy, (+2 Overloads) Invoke, IsMethodOverridesOf, (+2 Overloads) TryInvoke
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices

<HideModuleName>
Public Module MethodsExtension

    ''' <summary>
    ''' 尝试将目标对象放入到函数指针之中来运行，运行失败的时候回返回<paramref name="default"/>默认值
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="TOut"></typeparam>
    ''' <param name="input"></param>
    ''' <param name="proc"></param>
    ''' <param name="[default]"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function TryInvoke(Of T, TOut)(proc As Func(Of T, TOut), input As T, Optional [default] As TOut = Nothing) As TOut
        Return New Func(Of TOut)(Function() proc(input)).TryInvoke([default])
    End Function

    <Extension> Public Function TryInvoke(Of T)(proc As Func(Of T), Optional [default] As T = Nothing) As T
        Try
            Return proc()
        Catch ex As Exception
            Call App.LogException(ex)
            Return [default]
        End Try
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsLazy(Of T)(factory As Func(Of T)) As Lazy(Of T)
        Return New Lazy(Of T)(factory)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsLazy(Of T)(lambda As LambdaExpression) As Lazy(Of T)
        Return DirectCast(lambda.Compile, Func(Of T)).AsLazy
    End Function

    ''' <summary>
    ''' Invoke a static method without parameters
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="method"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Invoke(Of T)(method As MethodInfo) As T
        Return method.Invoke(Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Invoke a static method without parameters
    ''' </summary>
    ''' <param name="method"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Invoke(method As MethodInfo) As Object
        Return method.Invoke(Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Does current <paramref name="method"/> is overrides from the base <paramref name="type"/>
    ''' </summary>
    ''' <param name="method"></param>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <Extension>
    Public Function IsMethodOverridesOf(method As MethodInfo, type As Type) As Boolean
        Return Not method.DeclaringType Is type AndAlso method.DeclaringType.IsInheritsFrom(type, strict:=False)
    End Function
End Module
