#Region "Microsoft.VisualBasic::fe0e195744016013d6bc30ab975104cc, Microsoft.VisualBasic.Core\src\Scripting\Runtime\Where.vb"

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

    '   Total Lines: 121
    '    Code Lines: 71 (58.68%)
    ' Comment Lines: 34 (28.10%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (13.22%)
    '     File Size: 3.92 KB


    '     Enum Logics
    ' 
    '         [And], [AndAlso], [Not], [Or], [OrElse]
    '         [XOr]
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module [Where]
    ' 
    '         Function: __and, __andAlso, __or, __orElse, BuildAll
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Scripting.Runtime

    Public Enum Logics

        ''' <summary>
        ''' 两边的逻辑表达式都会被计算
        ''' </summary>
        [And]
        ''' <summary>
        ''' 两边的逻辑表达式都会被计算
        ''' </summary>
        [Or]
        ''' <summary>
        ''' 相比于<see cref="Logics.And"/>，这个逻辑运算是会短路的
        ''' </summary>
        [AndAlso]
        ''' <summary>
        ''' 相比于<see cref="Logics.Or"/>，这个逻辑运算是会短路的
        ''' </summary>
        [OrElse]
        [Not]
        [XOr]

    End Enum

    ''' <summary>
    ''' Merge the logical lambda into one lambda function by a specifc logical operation in <see cref="Logics"/>.
    ''' </summary>
    Public Module [Where]

        ''' <summary>
        ''' Not support <see cref="Logics.Not"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="logic"></param>
        ''' <param name="tests"></param>
        ''' <returns></returns>
        Public Function BuildAll(Of T)(logic As Logics, ParamArray tests As Func(Of T, Boolean)()) As Func(Of T, Boolean)
            Select Case logic
                Case Logics.And
                    Return Function(x) x.__and(tests)
                Case Logics.AndAlso
                    Return Function(x) x.__andAlso(tests)
                Case Logics.Or
                    Return Function(x) x.__or(tests)
                Case Logics.OrElse
                    Return Function(x) x.__orElse(tests)
                Case Logics.XOr
                    Return Function(x) Not x.__orElse(tests)
                Case Else
                    Throw New NotSupportedException(logic.ToString)
            End Select
        End Function

        <Extension>
        Private Function __orElse(Of T)(x As T, tests As Func(Of T, Boolean)()) As Boolean
            For Each test As Func(Of T, Boolean) In tests
                If test(x) = True Then
                    Return True
                End If
            Next

            Return False
        End Function

        <Extension>
        Private Function __andAlso(Of T)(x As T, tests As Func(Of T, Boolean)()) As Boolean
            For Each test As Func(Of T, Boolean) In tests
                If test(x) = False Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' <see cref="Logics.Or"/>, 所有的表达式都会被计算
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="tests"></param>
        ''' <returns></returns>
        <Extension>
        Private Function __or(Of T)(x As T, tests As Func(Of T, Boolean)()) As Boolean
            Dim [true] As Boolean = False

            For Each test As Func(Of T, Boolean) In tests
                If test(x) = True Then
                    [true] = True
                End If
            Next

            Return [true]
        End Function

        ''' <summary>
        ''' <see cref="Logics.And"/>, 所有的表达式都会被计算
        ''' </summary>
        ''' <param name="tests"></param>
        ''' <returns></returns>
        <Extension>
        Private Function __and(Of T)(x As T, tests As Func(Of T, Boolean)()) As Boolean
            Dim [false] As Boolean = False

            For Each test As Func(Of T, Boolean) In tests
                If test(x) = False Then
                    [false] = True
                End If
            Next

            If [false] Then
                Return False
            Else
                Return True
            End If
        End Function
    End Module
End Namespace
