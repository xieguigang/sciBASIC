#Region "Microsoft.VisualBasic::7f5dcc987316634741c548c127568bde, Microsoft.VisualBasic.Core\src\My\JavaScript\Linq.vb"

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

    '   Total Lines: 222
    '    Code Lines: 113 (50.90%)
    ' Comment Lines: 77 (34.68%)
    '    - Xml Docs: 94.81%
    ' 
    '   Blank Lines: 32 (14.41%)
    '     File Size: 9.39 KB


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
        ''' 对序列执行"降维"（折叠 / Reduce）操作：从初始累积值 <paramref name="init"/> 出发，
        ''' 依次将序列中的每个元素通过累积函数 <paramref name="produce"/> 合并到累积值上，
        ''' 最终把整个序列折叠成一个单一结果值。
        ''' </summary>
        ''' <typeparam name="T">序列中源元素（项）的类型</typeparam>
        ''' <typeparam name="V">降维之后的结果（累积值）类型</typeparam>
        ''' <param name="seq">待折叠的源序列。若为 <see langword="Nothing"/>，
        ''' 则不调用累积函数，直接返回 <paramref name="init"/>。</param>
        ''' <param name="produce">累积函数：第一个参数为当前累积值（类型 V），
        ''' 第二个参数为当前遍历到的元素（类型 T），返回值作为新的累积值（类型 V）。</param>
        ''' <param name="init">累积的初始值。默认为 <see langword="Nothing"/>；
        ''' 当 V 为值类型时即该类型的默认值（如 Integer 的 0、Boolean 的 False）。</param>
        ''' <returns>折叠完成后的最终累积值。若 <paramref name="seq"/> 为 <see langword="Nothing"/>
        ''' 或为空序列，则原样返回 <paramref name="init"/>。</returns>
        ''' <remarks>
        ''' <para>本方法相当于函数式编程中的 fold，是 <see cref="Enumerable.Aggregate"/> 的简化版本，
        ''' 并对 <see langword="Nothing"/> 序列做了容错（返回初始值而不是抛异常）。</para>
        ''' <para>注意：<paramref name="init"/> 始终作为累积起点参与运算，
        ''' 首个元素并不会自动取代初始值（与某些语言中省略初始值时的行为不同）。</para>
        ''' <para>累积函数的参数顺序为（累积值, 当前元素）。若累积运算不满足交换律
        ''' （例如减法、除法、字符串拼接），参数顺序与遍历顺序会影响最终结果。</para>
        ''' <para>若 <paramref name="seq"/> 不为空而 <paramref name="produce"/> 为 <see langword="Nothing"/>，
        ''' 运行时将引发 <see cref="NullReferenceException"/>；
        ''' 累积函数自身抛出的异常也会原样向上传播。</para>
        ''' </remarks>
        ''' <example>
        ''' 求和：Dim sum = New Integer() {1, 2, 3}.reduce(Function(acc, x) acc + x) ' 结果：6
        ''' 拼接：Dim s = {"a", "b", "c"}.reduce(Function(acc, x) acc &amp; x) ' 结果："abc"
        ''' 容错：Dim n = CType(Nothing, IEnumerable(Of Integer)).reduce(Function(acc, x) acc + x) ' 结果：0
        ''' </example>
        <Extension>
        Public Function reduce(Of T, V)(seq As IEnumerable(Of T), produce As Func(Of V, T, V), Optional init As V = Nothing) As V
            If seq Is Nothing Then
                Return init
            Else
                For Each x As T In seq
                    init = produce(init, x)
                Next
            End If

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
