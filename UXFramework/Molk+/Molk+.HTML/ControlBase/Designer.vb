#Region "Microsoft.VisualBasic::b5322d8342181e02abbaf1bf42d522c9, ..\visualbasic_App\UXFramework\Molk+\Molk+.HTML\ControlBase\Designer.vb"

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

''' <summary>
''' Descripts the control appearances using html document and trigger event handler entry point.
''' </summary>
Public Class Designer : Implements System.Collections.Generic.IEnumerable(Of Func(Of String, String))

    ''' <summary>
    ''' 使用html文档为控件的外观来提供描述
    ''' </summary>
    ''' <returns></returns>
    Public Property HTML As String
    ''' <summary>
    ''' Get/Post请求所触发的事件，已经被转换为小写形式的了
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Events As SortedDictionary(Of String, Func(Of String, String)) =
        New SortedDictionary(Of String, Func(Of String, String))

    Protected Friend __refreshHandle As Action

    Public Sub [AddHandler](uri As String, [EventHandler] As Func(Of String, String))
        Call Events.Add(uri.ToLower, EventHandler)
    End Sub

    Public Event EventTrigger(uri As String, ByRef resultHtml As String)

    Protected Overridable Function __buildHTML() As String
        Return HTML
    End Function

    ''' <summary>
    ''' 返回执行之后的得到的html页面
    ''' </summary>
    ''' <param name="url"></param>
    ''' <returns></returns>
    Public Function HandleInvoke(url As String) As String
        Dim luri As String = url.ToLower
        Dim result As String = ""

        If Events.ContainsKey(luri) Then
            Dim handler = Events(luri)

            Try
                result = handler(luri)
            Catch ex As Exception
                Call App.LogException(New Exception(url, ex))
                Return HTML
            End Try
        End If

        Try
            RaiseEvent EventTrigger(url, result)
        Catch ex As Exception
            Call App.LogException(New Exception(url, ex))
            result = HTML
        End Try

        Return result
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of Func(Of String, String)) Implements IEnumerable(Of Func(Of String, String)).GetEnumerator
        For Each [Handle] As KeyValuePair(Of String, Func(Of String, String)) In Events
            Yield Handle.Value
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
