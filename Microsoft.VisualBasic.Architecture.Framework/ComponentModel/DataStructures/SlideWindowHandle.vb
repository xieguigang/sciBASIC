Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' A slide window data model.(滑窗操作的数据模型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Structure SlideWindowHandle(Of T)
        Implements Generic.IEnumerable(Of T), ComponentModel.IAddressHandle

        ''' <summary>
        ''' The position of the current Windows in the Windows list.(在创建的滑窗的队列之中当前的窗口对象的位置)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property p As Long Implements IAddressHandle.AddrHwnd
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
            Return $"{p}  ----> {String.Join(", ", Elements.ToArray(Function(obj) obj.ToString))}"
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
                                    data As Generic.IEnumerable(Of T),
                                    slideWindowSize As Integer,
                                    Optional offset As Integer = 1,
                                    Optional extTails As Boolean = False) As SlideWindowHandle(Of T)()

            Dim n As Integer = data.Count

            If slideWindowSize >= n Then
                Return {New SlideWindowHandle(Of T)() With {
                    .Left = 0,
                    .Elements = data.ToArray}
                }
            End If

            If offset < 1 Then
                Call $"The offset parameter '{offset}' is not correct, set its value to 1 as default!".__DEBUG_ECHO
                offset = 1
            End If

            Dim TempList As List(Of T) = data.ToList
            Dim List As List(Of SlideWindowHandle(Of T)) =
                New List(Of SlideWindowHandle(Of T))
            Dim p As Integer = 0

            n = n - slideWindowSize - 1

            For i As Integer = 0 To n Step offset
                Dim ChunkBuffer As T() = TempList.Take(slideWindowSize).ToArray
                Call List.Add(New SlideWindowHandle(Of T)() With {
                              .Elements = ChunkBuffer,
                              .Left = i,
                              .p = p})
                Call TempList.RemoveRange(0, offset)

                p += 1
            Next

            If Not TempList.IsNullOrEmpty Then

                Dim left As Integer = n + 1

                If extTails Then
                    Call List.AddRange(__extendTails(TempList,
                                                     slideWindowSize,
                                                     left,
                                                     p))
                Else
                    Dim last As New SlideWindowHandle(Of T)() With {
                        .Left = left,
                        .Elements = TempList.ToArray,
                        .p = p
                    }
                    Call List.Add(last)
                End If
            End If

            Return List.ToArray
        End Function

        Private Function __extendTails(Of T)(lstTemp As List(Of T),
                                             slideWindowSize As Integer,
                                             left As Integer,
                                             p As Integer) As SlideWindowHandle(Of T)()
            ' Dim last As T = lstTemp.Last
            Dim list As New List(Of SlideWindowHandle(Of T))
            Dim array As T() = lstTemp.ToArray

            For i As Integer = 0 To slideWindowSize - 1
                Dim Tail As New SlideWindowHandle(Of T) With {
                    .Left = left,
                    .p = p,
                    .Elements = array
                }

                Call list.Add(Tail)
                ' Call lstTemp.RemoveAt(Scan0)
                ' Call lstTemp.Add(last)

                p += 1
                left += 1
            Next

            Return list.ToArray
        End Function
    End Module
End Namespace