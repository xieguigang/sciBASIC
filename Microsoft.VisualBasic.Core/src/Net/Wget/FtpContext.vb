#Region "Microsoft.VisualBasic::34da92ca5189067289438b96e4639053, Microsoft.VisualBasic.Core\src\Net\Wget\FtpContext.vb"

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

    '   Total Lines: 35
    '    Code Lines: 23 (65.71%)
    ' Comment Lines: 5 (14.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (20.00%)
    '     File Size: 1.23 KB


    '     Class FtpContext
    ' 
    '         Properties: password, server, username
    ' 
    '         Function: CreateFtpClient, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.FTP

Namespace Net.WebClient

    Public Class FtpContext

        Public Property username As String
        Public Property password As String
        Public Property server As String

        ''' <summary>
        ''' 创建一个新的 FTP 客户端实例。
        ''' 使用 <see cref="server"/> 作为主机、21 作为端口；
        ''' 若提供了用户名/密码则用于认证，否则使用匿名登录。
        ''' </summary>
        Public Function CreateFtpClient() As FtpClient
            Dim creds As FtpCredentials
            If Not (username.StringEmpty OrElse password.StringEmpty) Then
                creds = New FtpCredentials(username, password)
            Else
                creds = FtpCredentials.Anonymous
            End If

            Return New FtpClient(server, 21, Nothing, creds)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{username Or "anonymous".AsDefault}@ftp://{server}"
        End Function

    End Class
End Namespace
