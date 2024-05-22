#Region "Microsoft.VisualBasic::2b94e72bbce38acfe2b38197e5bf7e9e, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\base\SlideWindow\Extensions.vb"

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

    '   Total Lines: 141
    '    Code Lines: 93 (65.96%)
    ' Comment Lines: 27 (19.15%)
    '    - Xml Docs: 85.19%
    ' 
    '   Blank Lines: 21 (14.89%)
    '     File Size: 6.42 KB


    '     Module SlideWindowExtensions
    ' 
    '         Function: __extendTails, (+5 Overloads) [Select], CreateSlideWindows, SlideWindows
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' Create a collection of slide Windows data for the target collection object.
    ''' </summary>
    Public Module SlideWindowExtensions

        ''' <summary>
        ''' Create a collection of slide Windows data for the target collection object.(创建一个滑窗集合)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="winSize">The windows size of the created slide window.(窗口的大小)</param>
        ''' <param name="offset">在序列之上移动的步长</param>
        ''' <returns></returns>
        ''' <param name="extTails">引用类型不建议打开这个参数</param>
        ''' <remarks></remarks>
        <Extension> Public Function CreateSlideWindows(Of T)(data As IEnumerable(Of T), winSize%,
                           Optional offset% = 1,
                           Optional extTails As Boolean = False) As SlideWindow(Of T)()
            Return data.SlideWindows(winSize, offset, extTails).ToArray
        End Function

        ''' <summary>
        ''' Create a collection of slide Windows data for the target collection object.(创建一个滑窗集合)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="winSize">The windows size of the created slide window.(窗口的大小)</param>
        ''' <param name="offset">在序列之上移动的步长</param>
        ''' <returns></returns>
        ''' <param name="extTails">引用类型不建议打开这个参数</param>
        ''' <remarks></remarks>
        <Extension> Public Iterator Function SlideWindows(Of T)(data As IEnumerable(Of T), winSize%, Optional offset% = 1, Optional extTails As Boolean = False) As IEnumerable(Of SlideWindow(Of T))
            Dim tmp As New List(Of T)(data)
            Dim n% = tmp.Count

            If n = 0 Then
                ' 没有任何数据，则返回一个空集合
                Return
            ElseIf winSize >= n Then
                Yield New SlideWindow(Of T)() With {
                    .Left = 0,
                    .Items = tmp.ToArray
                }
                ' 这里要return，否则会出现重复的数据
                Return
            End If

            If offset < 1 Then
                Call VBDebugger.Warning($"The offset parameter '{offset}' is not correct, set its value to 1 as default!")
                offset = 1
            End If

            Dim p As i32 = 0

            n = n - winSize - 1

            For i As Integer = 0 To n Step offset
                Dim buf As T() = tmp.Take(winSize).ToArray

                Yield New SlideWindow(Of T)() With {
                    .Items = buf,
                    .Left = i,
                    .Index = ++p
                }

                tmp.RemoveRange(0, offset)
            Next

            If Not tmp.IsNullOrEmpty Then
                Dim left = n + 1

                If extTails Then
                    ' 在这里需要使用CInt拷贝一下指针p的值，否则会以引用的方式
                    ' 传递进入下一层函数之中造成当前的函数调用栈被修改
                    For Each x In __extendTails(tmp, winSize, left, CInt(p))
                        Yield x
                    Next
                Else
                    Yield New SlideWindow(Of T)() With {
                        .Left = left,
                        .Items = tmp.ToArray,
                        .Index = p
                    }
                End If
            End If
        End Function

        Private Iterator Function __extendTails(Of T)(tempList As List(Of T), winSize%, left%, p As i32) As IEnumerable(Of SlideWindow(Of T))
            Dim array As T() = tempList.ToArray

            For i As Integer = 0 To winSize - 1
                Yield New SlideWindow(Of T) With {
                    .Left = left,
                    .Index = ++p,
                    .Items = array
                }

                left += 1
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [Select](Of T, TOut)(windows As IEnumerable(Of SlideWindow(Of T)), projection As Func(Of T, T, TOut)) As IEnumerable(Of TOut)
            Return windows.Select(selector:=Function(win) projection(win(0), win(1)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [Select](Of T, TOut)(windows As IEnumerable(Of SlideWindow(Of T)), projection As Func(Of T, T, T, TOut)) As IEnumerable(Of TOut)
            Return windows.Select(selector:=Function(win) projection(win(0), win(1), win(2)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [Select](Of T, TOut)(windows As IEnumerable(Of SlideWindow(Of T)), projection As Func(Of T, T, T, T, TOut)) As IEnumerable(Of TOut)
            Return windows.Select(selector:=Function(win) projection(win(0), win(1), win(2), win(3)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [Select](Of T, TOut)(windows As IEnumerable(Of SlideWindow(Of T)), projection As Func(Of T, T, T, T, T, TOut)) As IEnumerable(Of TOut)
            Return windows.Select(selector:=Function(win) projection(win(0), win(1), win(2), win(3), win(4)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [Select](Of T, TOut)(windows As IEnumerable(Of SlideWindow(Of T)), projection As Func(Of T, T, T, T, T, T, TOut)) As IEnumerable(Of TOut)
            Return windows.Select(selector:=Function(win)
                                                Dim i As i32 = Scan0
                                                Return projection(win(++i), win(++i), win(++i), win(++i), win(++i), win(++i))
                                            End Function)
        End Function
    End Module
End Namespace
