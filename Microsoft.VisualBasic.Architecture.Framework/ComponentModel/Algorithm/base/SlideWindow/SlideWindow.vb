#Region "Microsoft.VisualBasic::403ac0f0abeb9980d910350ca8ee9d5c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Algorithm\base\SlideWindow\SlideWindow.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' Create a collection of slide Windows data for the target collection object.
    ''' </summary>
    Public Module SlideWindow

        ''' <summary>
        ''' Create a collection of slide Windows data for the target collection object.(创建一个滑窗集合)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="slideWindowSize">The windows size of the created slide window.(窗口的大小)</param>
        ''' <param name="offset">在序列之上移动的步长</param>
        ''' <returns></returns>
        ''' <param name="extTails">引用类型不建议打开这个参数</param>
        ''' <remarks></remarks>
        <Extension> Public Function CreateSlideWindows(Of T)(
                                    data As IEnumerable(Of T),
                                    slideWindowSize As Integer,
                           Optional offset As Integer = 1,
                           Optional extTails As Boolean = False) As SlideWindowHandle(Of T)()
            Return data.SlideWindows(slideWindowSize, offset, extTails).ToArray
        End Function

        ''' <summary>
        ''' Create a collection of slide Windows data for the target collection object.(创建一个滑窗集合)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="slideWindowSize">The windows size of the created slide window.(窗口的大小)</param>
        ''' <param name="offset">在序列之上移动的步长</param>
        ''' <returns></returns>
        ''' <param name="extTails">引用类型不建议打开这个参数</param>
        ''' <remarks></remarks>
        <Extension> Public Iterator Function SlideWindows(Of T)(
                                    data As IEnumerable(Of T),
                                    slideWindowSize As Integer,
                           Optional offset As Integer = 1,
                           Optional extTails As Boolean = False) As IEnumerable(Of SlideWindowHandle(Of T))

            Dim tmp As New List(Of T)(data)
            Dim n As Integer = tmp.Count

            If n = 0 Then
                ' 没有任何数据，则返回一个空集合
                Return
            ElseIf slideWindowSize >= n Then
                Yield New SlideWindowHandle(Of T)() With {
                    .Left = 0,
                    .Elements = tmp.ToArray
                }
                ' 这里要return，否则会出现重复的数据
                Return
            End If

            If offset < 1 Then
                Call VBDebugger.Warning($"The offset parameter '{offset}' is not correct, set its value to 1 as default!")
                offset = 1
            End If

            Dim p As Integer = 0

            n = n - slideWindowSize - 1

            For i As Integer = 0 To n Step offset
                Dim buf As T() = tmp.Take(slideWindowSize).ToArray

                Yield New SlideWindowHandle(Of T)() With {
                    .Elements = buf,
                    .Left = i,
                    .p = p
                }
                tmp.RemoveRange(0, offset)

                p += 1
            Next

            If Not tmp.IsNullOrEmpty Then

                Dim left As Integer = n + 1

                If extTails Then
                    For Each x In __extendTails(tmp, slideWindowSize, left, p)
                        Yield x
                    Next
                Else
                    Yield New SlideWindowHandle(Of T)() With {
                        .Left = left,
                        .Elements = tmp.ToArray,
                        .p = p
                    }
                End If
            End If
        End Function

        Private Iterator Function __extendTails(Of T)(
                                  lstTemp As List(Of T),
                                  slideWindowSize As Integer,
                                  left As Integer,
                                  p As Integer) As IEnumerable(Of SlideWindowHandle(Of T))

            Dim array As T() = lstTemp.ToArray

            For i As Integer = 0 To slideWindowSize - 1
                Yield New SlideWindowHandle(Of T) With {
                    .Left = left,
                    .p = p,
                    .Elements = array
                }

                p += 1
                left += 1
            Next
        End Function
    End Module
End Namespace
