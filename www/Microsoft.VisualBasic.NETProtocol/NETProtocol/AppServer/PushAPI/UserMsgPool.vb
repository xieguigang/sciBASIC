#Region "Microsoft.VisualBasic::0ef744c166decdba9b71780bde6c4411, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\AppServer\PushAPI\UserMsgPool.vb"

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

    '     Class UserMsgPool
    ' 
    '         Function: Pop
    ' 
    '         Sub: Allocation, Push
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Parallel

Namespace NETProtocol.PushAPI

    ''' <summary>
    ''' 用来缓存消息信息的用户的消息池
    ''' </summary>
    Public Class UserMsgPool

        ''' <summary>
        ''' 按照先后顺序排列的用户消息队列
        ''' </summary>
        ReadOnly __msgs As New Dictionary(Of Long, Queue(Of RequestStream))

        ''' <summary>
        ''' 为新的用户分配存储空间
        ''' </summary>
        ''' <param name="uid"></param>
        Public Sub Allocation(uid As Long)
            Call __msgs.Add(uid, New Queue(Of RequestStream))
        End Sub

        ''' <summary>
        ''' 想用户消息池之中写入数据缓存
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="msg"></param>
        Public Sub Push(uid As Long, msg As RequestStream)
            Call __msgs(uid).Enqueue(msg)
        End Sub

        ''' <summary>
        ''' 读取一条数据
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <returns></returns>
        Public Function Pop(uid As Long) As RequestStream
            Dim pool = __msgs(uid)
            If pool.Count = 0 Then
                Return Nothing
            Else
                Return pool.Dequeue()
            End If
        End Function
    End Class
End Namespace
