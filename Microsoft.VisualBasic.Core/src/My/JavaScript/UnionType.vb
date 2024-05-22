#Region "Microsoft.VisualBasic::02a6b956c15a6bc900237956efeeadbb, Microsoft.VisualBasic.Core\src\My\JavaScript\UnionType.vb"

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

    '   Total Lines: 80
    '    Code Lines: 64 (80.00%)
    ' Comment Lines: 5 (6.25%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 11 (13.75%)
    '     File Size: 2.70 KB


    '     Class UnionType
    ' 
    '         Properties: IsLambda
    ' 
    '         Function: [GetType], ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace My.JavaScript

    ''' <summary>
    ''' Union type of the value and its value generator function.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class UnionType(Of T)

        Public value As T
        Public lambda As Func(Of T)
        Public lambda1 As Func(Of Object, T)
        Public lambda2 As Func(Of Object, Object, T)
        Public lambda3 As Func(Of Object, Object, Object, T)

        Default Public ReadOnly Property Eval(a As Object) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return lambda1(a)
            End Get
        End Property

        Default Public ReadOnly Property Eval(a As Object, b As Object) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return lambda2(a, b)
            End Get
        End Property

        Default Public ReadOnly Property Eval(a As Object, b As Object, c As Object) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return lambda3(a, b, c)
            End Get
        End Property

        Public ReadOnly Property IsLambda As Boolean
            Get
                With {
                    lambda, lambda1, lambda2, lambda3
                }
                    Return .Any(Function(f) Not f Is Nothing)
                End With
            End Get
        End Property

        Public Overloads Function [GetType]() As Type
            If IsLambda Then
                ' 是一个函数
                Return GetType(Func(Of T))
            Else
                Return GetType(T)
            End If
        End Function

        Public Overrides Function ToString() As String
            If Me.GetType Is GetType(T) Then
                Return $"Dim <Anonymous> As {GetType(T).FullName} = {value.ToString}"
            Else
                Return $"<Anonymous> Function(...) As {GetType(T).FullName}"
            End If
        End Function

        Public Shared Narrowing Operator CType(uv As UnionType(Of T)) As T
            If uv.GetType Is GetType(T) Then
                Return uv.value
            ElseIf Not uv.lambda Is Nothing Then
                Return uv.lambda()
            Else
                Throw New InvalidCastException()
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(val As T) As UnionType(Of T)
            Return New UnionType(Of T) With {.value = val}
        End Operator
    End Class
End Namespace
