#Region "Microsoft.VisualBasic::9df9f6c84fab11bf4cb9193eb5fa65be, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Tcp\Persistent\MessagePushServices\Abstract.vb"

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

    '     Delegate Sub
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Protocols

Namespace Net.Persistent

    ''' <summary>
    ''' 离线数据请求
    ''' </summary>
    ''' <param name="FromUSER_ID"></param>
    ''' <param name="USER_ID"></param>
    ''' <param name="Message"></param>
    Public Delegate Sub OffLineMessageSendHandler(FromUSER_ID As Long, USER_ID As Long, Message As RequestStream)
    Public Delegate Function PushMessage(USER_ID As Long, Message As RequestStream) As RequestStream
End Namespace
