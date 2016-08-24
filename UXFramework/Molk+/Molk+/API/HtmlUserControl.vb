#Region "Microsoft.VisualBasic::1e08cda73efef64e2f3fa61d8196e5c8, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\HtmlUserControl.vb"

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

Imports System.IO
Imports System.Net.Sockets
Imports Microsoft.VisualBasic.MolkPlusTheme.Bend.Util

Namespace API

    Public Class HtmlControl : Implements System.Collections.Generic.IEnumerable(Of Func(Of String, String))

        Public Property HTML As String
        ''' <summary>
        ''' Get/Post请求所触发的事件，已经被转换为小写形式的了
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Events As SortedDictionary(Of String, Func(Of String, String)) =
            New SortedDictionary(Of String, Func(Of String, String))

        Public Sub [AddHandler](uri As String, [EventHandler] As Func(Of String, String))
            Call Events.Add(uri.ToLower, EventHandler)
        End Sub

        Public Event EventTrigger(uri As String, ByRef resultHtml As String)

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

    Public Class HtmlUserControl

        Public Property Control As HtmlControl
            Get
                Return _control
            End Get
            Set(value As HtmlControl)
                _control = value
                _eventHandler = New __eventHandler(Me)
                Call Run(AddressOf _eventHandler.Run)
                Call wbCtlRender.Navigate($"http://127.0.0.1:{_eventHandler.LocalPort}/")
            End Set
        End Property

        Dim _eventHandler As __eventHandler
        Dim _control As HtmlControl

        ''' <summary>
        ''' 服务器得到一个Get请求之后触发一个事件
        ''' </summary>
        Private Class __eventHandler : Inherits Bend.Util.HttpServer

            Public Property Control As HtmlUserControl

            ''' <summary>
            ''' 对控件进行渲染的HTML文档
            ''' </summary>
            ''' <param name="Control"></param>
            Sub New(Control As HtmlUserControl)
                Call MyBase.New(Net.TCPExtensions.GetFirstAvailablePort)
                Me.Control = Control
            End Sub

            Public Overrides Sub handleGETRequest(p As HttpProcessor)
                If p.IsWWWRoot Then
                    Call p.writeSuccess()
                    Call p.outputStream.WriteLine(Control.Control.HTML)
                    Return
                Else
                    Call Control.wbCtlRender.Stop()
                End If

                Call Control.Control.HandleInvoke(p.http_url)
            End Sub

            Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As StreamReader)
                Call Control.wbCtlRender.Stop()

                If p.IsWWWRoot Then
                    Return
                End If

                Call Control.Control.HandleInvoke(p.http_url)
            End Sub

            Protected Overrides Function __httpProcessor(client As TcpClient) As HttpProcessor
                Return New HttpProcessor(client, Me)
            End Function
        End Class
    End Class
End Namespace
