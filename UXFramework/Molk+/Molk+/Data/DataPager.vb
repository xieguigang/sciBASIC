#Region "Microsoft.VisualBasic::22f34a46bc78dcf2d7bc809afa90ea5a, ..\visualbasic_App\UXFramework\Molk+\Molk+\Data\DataPager.vb"

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

Imports Microsoft.VisualBasic.MolkPlusTheme
Imports Microsoft.VisualBasic.Linq.Extensions

Public Interface IDataPager

    ReadOnly Property CurrentPage As Integer
    ReadOnly Property BufferLength As Long
    ReadOnly Property Count As Integer
    Function GetDataSummary() As String

    Sub InvokeFirstPage()
    Sub InvokeLastPage()
    Sub InvokeNextPage()
    Sub InvokePreviousPage()
    Sub InvokePage(idx As Integer)
    Sub InvokePage(idx As String)

    Function GetCurrentPages(n As Integer) As String()

End Interface

Public Class DataPager(Of T) : Implements IEnumerable(Of T())
    Implements IDataPager

    Dim Pages As T()()
    Dim _DataDisplay As Action(Of T())

    Sub New(data As Generic.IEnumerable(Of T), Partitions As Integer, DataDisplay As Action(Of T()))
        Pages = data.Split(Partitions)
        Count = Pages.Length
        _BufferLength = data.LongCount
        _DataDisplay = DataDisplay
    End Sub

    Public ReadOnly Property CurrentPage As Integer Implements IDataPager.CurrentPage
    Public ReadOnly Property BufferLength As Long Implements IDataPager.BufferLength
    Public ReadOnly Property Count As Integer Implements IDataPager.Count

    Public Function GetFirst() As T()
        _CurrentPage = 0
        Return Pages(_CurrentPage)
    End Function

    Public Function GetLast() As T()
        _CurrentPage = Pages.Length - 1
        Return Pages(_CurrentPage)
    End Function

    Public Function GetNext() As T()

        If _CurrentPage = Me.Count - 1 Then
            Return Nothing
        Else
            _CurrentPage += 1
            Return Pages(_CurrentPage)
        End If
    End Function

    Public Function GetPrevious() As T()
        If _CurrentPage = 0 Then
            Return Nothing
        Else
            _CurrentPage -= 1
            Return Pages(_CurrentPage)
        End If
    End Function

    Public Overrides Function ToString() As String
        Return $"共{Me.BufferLength}条数据，第{_CurrentPage} / { Me.Count}页"
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of T()) Implements IEnumerable(Of T()).GetEnumerator
        For i As Integer = 0 To Me.Count - 1
            Yield Pages(i)
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function

    Public Function GetDataSummary() As String Implements IDataPager.GetDataSummary
        Return Me.ToString
    End Function

    Private Sub __displayDataPage(Page As T())
        Call Me._DataDisplay(Page)
    End Sub

    Public Sub InvokeFirstPage() Implements IDataPager.InvokeFirstPage
        Call __displayDataPage(GetFirst)
    End Sub

    Public Sub InvokeLastPage() Implements IDataPager.InvokeLastPage
        Call __displayDataPage(GetLast)
    End Sub

    Public Sub InvokeNextPage() Implements IDataPager.InvokeNextPage
        Call __displayDataPage(GetNext)
    End Sub

    Public Sub InvokePreviousPage() Implements IDataPager.InvokePreviousPage
        Call __displayDataPage(GetPrevious)
    End Sub

    Public Sub InvokePage(idx As Integer) Implements IDataPager.InvokePage
        _CurrentPage = idx
        Call __displayDataPage(Me.Pages(idx))
    End Sub

    Public Sub InvokePage(idx As String) Implements IDataPager.InvokePage
        Call InvokePage(CInt(Val(idx)))
    End Sub

    ''' <summary>
    ''' 函数会优先显示当前页码的后面的页数，当到达底部的时候会显示前面的页数
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCurrentPages(n As Integer) As String() Implements IDataPager.GetCurrentPages
        If CurrentPage <= n Then
            Return (From i As Integer In n.Sequence Select CStr(i)).ToArray
        ElseIf CurrentPage >= Me.Count - n
            Return (From i As Integer In n.Sequence Select CStr(Me.Count - i)).ToArray.Reverse.ToArray
        Else
            '显示当前页的后面的页数
            Return (From i As Integer In n.Sequence Select CStr(CurrentPage + i)).ToArray
        End If
    End Function
End Class
