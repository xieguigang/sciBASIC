#Region "Microsoft.VisualBasic::96e0d4846115dc8f43e0df5a09783e90, Scripting\Runtime\CType\Narrowing.vb"

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

    '     Module NarrowingReflection
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Function: (+2 Overloads) GetNarrowingOperator, GetOperatorMethod
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Scripting.Runtime

    Public Module NarrowingReflection

        ''' <summary>
        ''' 2070 = SpecialName
        ''' </summary>
        Const NarrowingOperator As BindingFlags = BindingFlags.Public Or BindingFlags.Static Or 2070
        Const op_Explicit$ = NameOf(op_Explicit)

        Public Delegate Function INarrowingOperator(Of TIn, TOut)(obj As TIn) As TOut

        Public Function GetNarrowingOperator(Of TIn, TOut)() As INarrowingOperator(Of TIn, TOut)
            Dim op As MethodInfo = CType(GetType(TIn), TypeInfo).GetOperatorMethod(Of TOut)

            If op Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As INarrowingOperator(Of TIn, TOut) = op.CreateDelegate(GetType(INarrowingOperator(Of TIn, TOut)))
                Return op_Explicit
            End If
        End Function

        ''' <summary>
        ''' 直接使用GetMethod方法仍然会出错？？如果目标类型是继承类型，基类型也有一个收缩的操作符的话，会爆出目标不明确的错误
        ''' 
        ''' ```vbnet
        ''' type.GetMethod(op_Explicit, NarrowingOperator)
        ''' ```
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="type"></param>
        ''' <returns>函数找不到会返回Nothing</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function GetOperatorMethod(Of T)(type As TypeInfo) As MethodInfo
            Return type.DeclaredMethods _
                       .Where(Function(m) m.Name = op_Explicit AndAlso m.ReturnType Is GetType(T)) _
                       .FirstOrDefault
        End Function

        <Extension>
        Public Function GetNarrowingOperator(Of T)(type As Type) As INarrowingOperator(Of Object, T)
            ' 函数找不到会返回Nothing
            Dim op As MethodInfo = CType(type, TypeInfo).GetOperatorMethod(Of T)

            If op Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As INarrowingOperator(Of Object, T) = Function(obj) DirectCast(op.Invoke(Nothing, {obj}), T)
                Return op_Explicit
            End If
        End Function
    End Module
End Namespace
