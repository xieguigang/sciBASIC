#Region "Microsoft.VisualBasic::7d60664d81650c4bffb3e3dd4ce33c50, Microsoft.VisualBasic.Core\src\My\JavaScript\Linq.vb"

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

    '   Total Lines: 196
    '    Code Lines: 109 (55.61%)
    ' Comment Lines: 55 (28.06%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 32 (16.33%)
    '     File Size: 6.94 KB


    '     Interface requestAnimationFrame
    ' 
    '         Sub: requestAnimationFrame
    ' 
    '     Module Linq
    ' 
    '         Function: findIndex, (+2 Overloads) map, pop, (+5 Overloads) reduce, shift
    '                   (+2 Overloads) sort
    ' 
    '         Sub: (+2 Overloads) splice
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language

Namespace My.JavaScript

    Public Interface requestAnimationFrame

        Sub requestAnimationFrame(callback As IGraphics)
    End Interface

    <HideModuleName> Public Module Linq

        <Extension>
        Public Sub splice(Of T)(array As T(), index As Integer, howmany As Integer, ParamArray items As T())
            Throw New NotImplementedException
        End Sub

        <Extension>
        Public Sub splice(Of T)(list As List(Of T), index As Integer, howmany As Integer, ParamArray items As T())
            Throw New NotImplementedException
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function findIndex(Of T)(array As T(), predicate As Predicate(Of T)) As Integer
            Return System.Array.FindIndex(array, predicate)
        End Function

        <Extension>
        Public Function map(Of T, V)(data As IEnumerable(Of T), project As Func(Of T, Integer, V)) As V()
            Return data.Select(project).ToArray
        End Function

        <Extension>
        Public Function map(Of T, V)(data As IEnumerable(Of T), project As Func(Of T, V)) As V()
            Return data.Select(project).ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T">序列的类型</typeparam>
        ''' <typeparam name="V">序列进行降维之后的结果类型</typeparam>
        ''' <param name="seq"></param>
        ''' <param name="produce"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        <Extension>
        Public Function reduce(Of T, V)(seq As IEnumerable(Of T), produce As Func(Of V, T, Integer, V), Optional init As V = Nothing) As V
            Dim i As i32 = Scan0

            For Each x As T In seq
                init = produce(init, x, ++i)
            Next

            Return init
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T">序列的类型</typeparam>
        ''' <typeparam name="V">序列进行降维之后的结果类型</typeparam>
        ''' <param name="seq"></param>
        ''' <param name="produce"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        <Extension>
        Public Function reduce(Of T, V)(seq As IEnumerable(Of T), produce As Func(Of V, T, Integer, T(), V), Optional init As V = Nothing) As V
            Dim i As i32 = Scan0
            Dim rawArray As T() = seq.ToArray

            For Each x As T In seq
                init = produce(init, x, ++i, rawArray)
            Next

            Return init
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="V">序列进行降维之后的结果类型</typeparam>
        ''' <param name="seq"></param>
        ''' <param name="produce"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        <Extension>
        Public Function reduce(Of V)(seq As Array, produce As Func(Of V, Object, Integer, V), Optional init As V = Nothing) As V
            Dim i As i32 = Scan0

            For Each x As Object In seq
                init = produce(init, x, ++i)
            Next

            Return init
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="V">序列进行降维之后的结果类型</typeparam>
        ''' <param name="seq"></param>
        ''' <param name="produce"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        <Extension>
        Public Function reduce(Of V)(seq As Array, produce As Func(Of V, Object, V), Optional init As V = Nothing) As V
            For Each x As Object In seq
                init = produce(init, x)
            Next

            Return init
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T">序列的类型</typeparam>
        ''' <typeparam name="V">序列进行降维之后的结果类型</typeparam>
        ''' <param name="seq"></param>
        ''' <param name="produce"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        <Extension>
        Public Function reduce(Of T, V)(seq As IEnumerable(Of T), produce As Func(Of V, T, V), Optional init As V = Nothing) As V
            For Each x As T In seq
                init = produce(init, x)
            Next

            Return init
        End Function

        <Extension>
        Public Function sort(Of T)(seq As IEnumerable(Of T), comparer As Comparison(Of T)) As IEnumerable(Of T)
            With New List(Of T)(seq)
                Call .Sort(comparer)
                Return .AsEnumerable
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function sort(Of T)(seq As IEnumerable(Of T), comparer As Func(Of T, T, Double)) As IEnumerable(Of T)
            Return seq.sort(Function(x, y)
                                Dim d As Double = comparer(x, y)

                                If d > 0 Then
                                    Return 1
                                ElseIf d < 0 Then
                                    Return -1
                                Else
                                    Return 0
                                End If
                            End Function)
        End Function

        ''' <summary>
        ''' 用于删除并返回数组的最后一个元素。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <returns></returns>
        <Extension>
        Public Function pop(Of T)(ByRef a As T()) As T
            Dim last = a.LastOrDefault

            If a.Length > 0 Then
                ReDim Preserve a(a.Length - 1)
            End If

            Return last
        End Function

        ''' <summary>
        ''' 用于把数组的第一个元素从其中删除，并返回第一个元素的值。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <returns></returns>
        <Extension>
        Public Function shift(Of T)(ByRef a As T()) As T
            Dim first = a.FirstOrDefault

            If a.Length > 0 Then
                Dim resize As T() = New T(a.Length - 1) {}

                Array.ConstrainedCopy(a, 1, resize, Scan0, resize.Length)
                a = resize
            End If

            Return first
        End Function
    End Module
End Namespace
