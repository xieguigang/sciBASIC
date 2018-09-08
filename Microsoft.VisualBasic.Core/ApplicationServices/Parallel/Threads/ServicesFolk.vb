#Region "Microsoft.VisualBasic::e1ba5a0dba6fb894a52bf987d07b8a33, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\ServicesFolk.vb"

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

    '     Module ServicesFolk
    ' 
    '         Function: Folk, ReturnPortal
    '         Class __getChildPortal
    ' 
    '             Function: HandleRequest, WaitForPortal
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Win32
Imports SetValueAction = Microsoft.VisualBasic.Linq.SetValue(Of Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket)

Namespace Parallel

    ''' <summary>
    ''' 主服务和子服务之间的相互作用的特点是子服务不会知道主服务节点的数据接口，所有的交互都是通过子服务上面的一个模块来监听主服务来实现的
    ''' 当主服务有数据需要向子服务更新的时候，会主动发送数据请求至子服务节点
    ''' </summary>
    ''' <remarks>当前的用户规模还比较小这里仅仅是实现了本地的调用，后面考虑到业务吞吐量的问题，会将服务的调用分开到两台物理主机之上</remarks>
    Public Module ServicesFolk

        ''' <summary>
        ''' 函数返回子进程的交互数据通信的端口号
        ''' </summary>
        ''' <param name="assm"></param>
        ''' <param name="CLI">命令行参数字符串，可以在这里加入一些其他的自定义数据</param>
        ''' <returns>函数返回子服务的交互端口</returns>
        Public Function Folk(assm As String, ByRef CLI As String, Optional ByRef folked As Process = Nothing) As Integer
            Dim Portal As Integer

            '开通一个临时的端口用来和子服务交互
            Using TempListen As New Net.TcpSynchronizationServicesSocket(
                Net.TCPExtensions.GetFirstAvailablePort,
                Sub(ex) App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName))

                Dim __getChildPortal As New __getChildPortal

                Call SetValueAction.InvokeSet(Of Net.Abstract.DataRequestHandler)(
                     TempListen,
                     NameOf(TempListen.Responsehandler),
                     AddressOf __getChildPortal.HandleRequest)

                Call RunTask(AddressOf TempListen.Run)
                Call TempListen.WaitForStart()

                Dim path As String =
                    If(assm.FileExists, FileIO.FileSystem.GetFileInfo(assm).FullName, $"{App.HOME}/{assm}")
#If DEBUG Then
                Call $"Invoke start  {path.ToFileURL}  @{MethodBase.GetCurrentMethod.GetFullName}".__DEBUG_ECHO
#End If
                Dim FolkSvr As Process =
                    Process.Start(path, $"{CLI} {ParentPortal} {TempListen.LocalPort}")

                __getChildPortal.PID = FolkSvr.Id
                Portal = __getChildPortal.WaitForPortal
                folked = FolkSvr
                CLI = __getChildPortal.addArgs
            End Using

            Dim msg As String = $"Get folked child services {assm} local_services:={Portal}"

            Call msg.__DEBUG_ECHO

            If WindowsServices.Initialized Then
                Call ServicesLogs.WriteEntry(msg, EventLogEntryType.SuccessAudit)
            End If

            Return Portal
        End Function

        Private Class __getChildPortal

            Public PID As Integer
            Public Portal As Integer = -100
            ''' <summary>
            ''' 所返回来的额外的参数信息
            ''' </summary>
            Public addArgs As String

            Public Function HandleRequest(uid As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
                If uid <> PID Then
                    Return NetResponse.RFC_TOKEN_INVALID
                End If

                Dim result As String = request.GetUTF8String

                Portal = Scripting.CTypeDynamic(Of Integer)(result)
                addArgs = CommandLine.GetTokens(result).ElementAtOrDefault(1)

                Return NetResponse.RFC_OK
            End Function

            Public Function WaitForPortal() As Integer
                Do While Portal < 0
                    Call Threading.Thread.Sleep(1)
                Loop

                Return Portal
            End Function
        End Class

        Const ParentPortal As String = "--portal"

        ''' <summary>
        ''' 子服务向服务主节点返回端口号数据，这个方法需要要在子服务上面的服务程序启动之后再调用
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <param name="Port"></param>
        ''' <param name="addArgs">额外返回的参数信息</param>
        ''' <returns></returns>
        Public Function ReturnPortal(CLI As CommandLine.CommandLine, Port As Integer, Optional addArgs As String = "") As Boolean
            Dim parentPortal As Integer = CLI.GetInt32(ServicesFolk.ParentPortal)
            Dim Client As New Net.AsynInvoke("127.0.0.1", parentPortal)
#If DEBUG Then
            Call $"{MethodBase.GetCurrentMethod.GetFullName} ==> ""{CLI}"" returns {Port}".__DEBUG_ECHO
#End If
            Dim request As New RequestStream(Process.GetCurrentProcess.Id, 0, $"{CStr(Port)} ""{addArgs}""") With {
                .uid = Process.GetCurrentProcess.Id
            }
            Dim response As RequestStream = Client.SendMessage(request)
            Return response.Protocol = HTTP_RFC.RFC_OK
        End Function
    End Module
End Namespace
