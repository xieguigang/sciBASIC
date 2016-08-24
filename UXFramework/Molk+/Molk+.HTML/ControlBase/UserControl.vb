#Region "Microsoft.VisualBasic::bb1576ad4586cab19dc98fe098d013e9, ..\visualbasic_App\UXFramework\Molk+\Molk+.HTML\ControlBase\UserControl.vb"

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
Imports Microsoft.VisualBasic.MolkPlusTheme.HTML.HttpInternal
Imports Microsoft.VisualBasic.Parallel

Public Class UserControl

    Public Property Control As Designer
        Get
            Return _control
        End Get
        Set(value As Designer)
            If value Is Nothing Then
                Call wbCtlRender.Navigate("about:blank")
            Else
                Call __hook(value)
            End If
        End Set
    End Property

    Private Sub __hook(handler As Designer)
        _control = handler
        _eventHandler = New __eventHandler(Me)
        Call RunTask(AddressOf _eventHandler.Run)
        Call wbCtlRender.Navigate($"http://127.0.0.1:{_eventHandler.LocalPort}/")
    End Sub

    Dim _eventHandler As __eventHandler
    Dim _control As Designer

    Public Sub ScrollEnable(value As Boolean)
        wbCtlRender.ScrollBarsEnabled = value
    End Sub

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub __refresh(current As String)
        Call wbCtlRender.Navigate($"http://127.0.0.1:{_eventHandler.LocalPort}/{current}")
    End Sub

    Private Sub UserControl_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        Call _eventHandler.Dispose()
    End Sub

    ''' <summary>
    ''' 服务器得到一个Get请求之后触发一个事件
    ''' </summary>
    Private Class __eventHandler : Inherits HttpServer

        Public Property Control As UserControl

        Dim __currentUri As String

        ''' <summary>
        ''' 对控件进行渲染的HTML文档
        ''' </summary>
        ''' <param name="Control"></param>
        Sub New(Control As UserControl)
            Call MyBase.New(Net.TCPExtensions.GetFirstAvailablePort)
            Me.Control = Control
            Me.Control.Control.__refreshHandle = AddressOf __refresh
        End Sub

        Private Sub __refresh()
            Call Control.__refresh(__currentUri)
        End Sub

        Public Overrides Sub handleGETRequest(p As HttpInternal.HttpProcessor)
            Dim html As String

            If Not p.IsWWWRoot Then
                html = Control.Control.HandleInvoke(p.http_url)
            Else
                html = Control.Control.HTML
            End If

            Call p.writeSuccess()
            Call p.outputStream.WriteLine(html)
        End Sub

        Public Overrides Sub handlePOSTRequest(p As HttpInternal.HttpProcessor, inputData As StreamReader)
            If p.IsWWWRoot Then
                Return
            End If

            Dim html As String = Control.Control.HandleInvoke(p.http_url)
            Call p.writeSuccess()
            Call p.outputStream.WriteLine(html)
        End Sub

        Protected Overrides Function __httpProcessor(client As TcpClient) As HttpInternal.HttpProcessor
            Return New HttpInternal.HttpProcessor(client, Me)
        End Function
    End Class
End Class
