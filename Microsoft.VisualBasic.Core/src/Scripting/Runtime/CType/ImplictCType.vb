#Region "Microsoft.VisualBasic::9681b0176aeaf675bcc991d3601dda5d, Microsoft.VisualBasic.Core\src\Scripting\Runtime\CType\ImplictCType.vb"

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

    '   Total Lines: 89
    '    Code Lines: 55 (61.80%)
    ' Comment Lines: 20 (22.47%)
    '    - Xml Docs: 85.00%
    ' 
    '   Blank Lines: 14 (15.73%)
    '     File Size: 3.66 KB


    '     Module ImplictCType
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Function: (+2 Overloads) GetNarrowingOperator, GetOperatorMethod, (+2 Overloads) GetWideningOperator
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Scripting.Runtime

    Public Module ImplictCType

        ''' <summary>
        ''' 2070 = SpecialName
        ''' </summary>
        Const operatorType As BindingFlags = BindingFlags.Public Or BindingFlags.Static Or 2070

        ''' <summary>
        ''' x -> out
        ''' </summary>
        Const op_Explicit$ = NameOf(op_Explicit)
        ''' <summary>
        ''' in -> x
        ''' </summary>
        Const op_Implicit$ = NameOf(op_Implicit)

        Public Delegate Function IImplictCTypeOperator(Of TIn, TOut)(obj As TIn) As TOut

        Public Function GetNarrowingOperator(Of TIn, TOut)() As IImplictCTypeOperator(Of TIn, TOut)
            Dim op As MethodInfo = CType(GetType(TIn), TypeInfo).GetOperatorMethod(GetType(TOut), op_Explicit)

            If op Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As IImplictCTypeOperator(Of TIn, TOut) = op.CreateDelegate(GetType(IImplictCTypeOperator(Of TIn, TOut)))
                Return op_Explicit
            End If
        End Function

        Public Function GetWideningOperator(Of TIn, TOut)() As IImplictCTypeOperator(Of TIn, TOut)
            Dim op As MethodInfo = CType(GetType(TOut), TypeInfo).GetOperatorMethod(GetType(TIn), op_Implicit)

            If op Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As IImplictCTypeOperator(Of TIn, TOut) = op.CreateDelegate(GetType(IImplictCTypeOperator(Of TIn, TOut)))
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
        ''' <param name="obj"></param>
        ''' <returns>函数找不到会返回Nothing</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function GetOperatorMethod(obj As TypeInfo, out As Type, name$) As MethodInfo
            Return obj.GetMethods(operatorType) _
                      .Where(Function(m) m.Name = name AndAlso m.ReturnType Is out) _
                      .FirstOrDefault
        End Function

        <Extension>
        Public Function GetNarrowingOperator(Of T)(type As Type) As IImplictCTypeOperator(Of Object, T)
            ' 函数找不到会返回Nothing
            Dim op As MethodInfo = CType(type, TypeInfo).GetOperatorMethod(GetType(T), op_Explicit)

            If op Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As IImplictCTypeOperator(Of Object, T) = Function(obj) DirectCast(op.Invoke(Nothing, {obj}), T)
                Return op_Explicit
            End If
        End Function

        <Extension>
        Public Function GetWideningOperator(Of T)(type As Type) As IImplictCTypeOperator(Of T, Object)
            ' 函数找不到会返回Nothing
            Dim op As MethodInfo = CType(type, TypeInfo).GetOperatorMethod(type, op_Implicit)

            If op Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As IImplictCTypeOperator(Of T, Object) = Function(obj) op.Invoke(Nothing, New Object() {obj})
                Return op_Explicit
            End If
        End Function
    End Module
End Namespace
