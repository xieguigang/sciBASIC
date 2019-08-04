#Region "Microsoft.VisualBasic::6d9161140ee90f4abb7f8668c65da11d, Microsoft.VisualBasic.Core\Net\PortMapping.vb"

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

    ' Module PortMapping
    ' 
    '     Properties: Mappings
    ' 
    '     Function: AuthorizeApplication, GetFirewallManager, GloballyOpenPort, SetPortsMapping
    ' 
    '     Sub: CheckFirewallRunningState, InternalCreateMappingCollection
    ' 
    ' /********************************************************************************/

#End Region

Imports NATUPNPLib
Imports NETCONLib
Imports NetFwTypeLib

''' <summary>
''' 服务器程序使用这个模块之中的方法实现在内网和外网之间的端口映射
''' </summary>
''' <remarks></remarks>
Public Module PortMapping

    ''' <summary>
    ''' 防护墙firewall manager需要被引用在程序中。防火墙firewall manager的CLSID是，
    ''' {304CE942-6E39-40D8-943A-B913C40C9CD4}
    ''' </summary>
    ''' <remarks></remarks>
    Private Const CLSID_FIREWALL_MANAGER As String = "{304CE942-6E39-40D8-943A-B913C40C9CD4}"

    Private Function GetFirewallManager() As NetFwTypeLib.INetFwMgr
        Dim FwCLSID = New Guid(CLSID_FIREWALL_MANAGER)
        Dim objType As Type = Type.GetTypeFromCLSID(FwCLSID)
        Dim FwObj = Activator.CreateInstance(objType)
        Return TryCast(FwObj, NetFwTypeLib.INetFwMgr)
    End Function

    ''' <summary>
    ''' 检查防火墙是否在运行
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub CheckFirewallRunningState()
        Dim FwManager As INetFwMgr = GetFirewallManager()
        Dim FirewallEnabled As Boolean =
            FwManager.LocalPolicy.CurrentProfile.FirewallEnabled

        If Not FirewallEnabled Then
            FwManager.LocalPolicy.CurrentProfile.FirewallEnabled = True
        End If
    End Sub

    ''' <summary>
    ''' ProgID for the AuthorizedApplication object
    ''' </summary>
    ''' <remarks></remarks>
    Private Const PROGID_AUTHORIZED_APPLICATION As String = "HNetCfg.FwAuthorizedApplication"

    ''' <summary>
    ''' 授权到应用程序
    ''' <example>Call AuthorizeApplication("Notepad", "C:\Windows\Notepad.exe", NET_FW_SCOPE_.NET_FW_SCOPE_ALL, NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY)</example>
    ''' </summary>
    ''' <param name="title"></param>
    ''' <param name="applicationPath"></param>
    ''' <param name="scope"></param>
    ''' <param name="ipVersion"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AuthorizeApplication(Title As String, ApplicationPath As String, Scope As NET_FW_SCOPE_, ipVersion As NET_FW_IP_VERSION_) As Boolean

        ' Create the type from prog id
        Dim FwAuthInfo As Type = Type.GetTypeFromProgID(PROGID_AUTHORIZED_APPLICATION)
        Dim FwAuth As Object = Activator.CreateInstance(FwAuthInfo)
        Dim AuthRule As INetFwAuthorizedApplication = TryCast(FwAuth, INetFwAuthorizedApplication)

        ApplicationPath = FileIO.FileSystem.GetFileInfo(ApplicationPath).FullName

        AuthRule.Name = Title
        AuthRule.ProcessImageFileName = ApplicationPath
        AuthRule.Scope = Scope
        AuthRule.IpVersion = ipVersion
        AuthRule.Enabled = True

        Dim FwManager As INetFwMgr = GetFirewallManager()

        Try
            Call FwManager.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(AuthRule)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Const PROGID_OPEN_PORT As String = "HNetCfg.FWOpenPort"

    ''' <summary>
    ''' 如果需要打开特定的端口，那么调用下面的方法
    ''' </summary>
    ''' <param name="title"></param>
    ''' <param name="portNo"></param>
    ''' <param name="scope"></param>
    ''' <param name="protocol"></param>
    ''' <param name="ipVersion"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GloballyOpenPort(Title As String, portNo As Integer, Scope As NET_FW_SCOPE_, Protocol As NET_FW_IP_PROTOCOL_, ipVersion As NET_FW_IP_VERSION_) As Boolean
        Dim FwTypeInfo As Type = Type.GetTypeFromProgID(PROGID_OPEN_PORT)
        Dim OpenedPortRule As INetFwOpenPort = TryCast(Activator.CreateInstance(FwTypeInfo), INetFwOpenPort)
        OpenedPortRule.Name = Title
        OpenedPortRule.Port = portNo
        OpenedPortRule.Scope = Scope
        OpenedPortRule.Protocol = Protocol
        OpenedPortRule.IpVersion = ipVersion

        Dim FwManager As INetFwMgr = GetFirewallManager()

        Try
            Call FwManager.LocalPolicy.CurrentProfile.GloballyOpenPorts.Add(OpenedPortRule)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

#Region "Port Mapping"

    Dim NAT As New UPnPNAT()
    Dim _InternalMappingsCache As IStaticPortMappingCollection

    Public ReadOnly Property Mappings() As IStaticPortMappingCollection
        Get
            If _InternalMappingsCache Is Nothing Then
                Call InternalCreateMappingCollection()
            End If

            Return _InternalMappingsCache
        End Get
    End Property

    Private Sub InternalCreateMappingCollection()
        Try
            If NAT.NATEventManager IsNot Nothing Then
                _InternalMappingsCache = NAT.StaticPortMappingCollection
            End If
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' 创建内网和外网的端口映射的规则
    ''' </summary>
    ''' <param name="wanPort">外网的端口号</param>
    ''' <param name="lanPort">内网的端口号</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SetPortsMapping(WanPort As Integer, LanPort As Integer, Optional Describes As String = "Server ports mapping rule") As Boolean
        Dim i As Integer = 3

        While Mappings Is Nothing AndAlso i > 0
            Call Threading.Thread.Sleep(2000)
            i -= 1
        End While

        If Mappings IsNot Nothing Then
            Try
                Call Mappings.Remove(WanPort, "TCP")
            Catch ex As Exception
                Call Console.WriteLine(ex.ToString)
            End Try
            Try
                Call Mappings.Add(WanPort, "TCP", LanPort, AsynchronousClient.LocalIP, True, Describes)
                Return True
            Catch ex As Exception
                Call Console.WriteLine(ex.ToString)
                Return False
            End Try
        Else
            Call Console.WriteLine("[DEBUG] Mappings is nothing!")
            Return False
        End If
    End Function

#End Region
End Module
