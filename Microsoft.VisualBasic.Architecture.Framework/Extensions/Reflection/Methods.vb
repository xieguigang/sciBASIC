#Region "Microsoft.VisualBasic::35ce4bc0b116e3a595feeb6a2cde5d63, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Reflection\Methods.vb"

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

Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices

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
End Module
