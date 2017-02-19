#Region "Microsoft.VisualBasic::d6a0df570a1d1548930080a0027e13f2, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\SlideWindowHandle.vb"

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
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' A slide window data model.(滑窗操作的数据模型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Structure SlideWindowHandle(Of T)
        Implements IEnumerable(Of T), IAddressHandle

        ''' <summary>
        ''' The position of the current Windows in the Windows list.(在创建的滑窗的队列之中当前的窗口对象的位置)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property p As Integer Implements IAddressHandle.Address
        ''' <summary>
        ''' The elements in this slide window.(这个划窗之中的元素的列表)
        ''' </summary>
        ''' <returns></returns>
        Public Property Elements As T()

        ''' <summary>
        ''' The left start position of the current slide Windows segment on the original sequence.
        ''' (当前窗口在原始的序列之中的左端起始位点)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Left As Integer

        Public ReadOnly Property Right As Integer
            Get
                Return Left + Length
            End Get
        End Property

        ''' <summary>
        ''' The length of the slide window.(窗口长度)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Length As Integer
            Get
                If Elements.IsNullOrEmpty Then
                    Return 0
                Else
                    Return Elements.Length
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{p} --> {Elements.GetJson}"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each element As T In Elements
                Yield element
            Next
        End Function

        Private Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            Elements = Nothing
        End Sub
    End Structure

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

            Dim tmp As List(Of T) = data.ToList
            Dim n As Integer = tmp.Count

            If slideWindowSize >= n Then
                Yield New SlideWindowHandle(Of T)() With {
                    .Left = 0,
                    .Elements = tmp.ToArray
                }
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
