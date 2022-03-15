#Region "Microsoft.VisualBasic::b43dd450ac1875ebd780ed6c399af063, sciBASIC#\vs_solutions\dev\ApplicationServices\Win32\WindowsServices.vb"

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

    '   Total Lines: 131
    '    Code Lines: 47
    ' Comment Lines: 68
    '   Blank Lines: 16
    '     File Size: 6.43 KB


    '     Module WindowsServices
    ' 
    '         Properties: Initialized, ServicesLogs
    ' 
    '         Function: (+3 Overloads) Initialize, LogsInstaller
    ' 
    '         Sub: RegisterURLProtocol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.Win32
Imports Logs = Microsoft.VisualBasic.ApplicationServices.Debugging.Logging.EventLog

Namespace Win32

    ''' <summary>
    ''' Windows event logger services &amp; installer services.
    ''' (这个模块主要的功能是日志服务，包括在安装阶段对日志记录的创建以及自定义url协议的创建等，
    ''' 请注意，这个模块之中的大部分的功能都需要你的应用程序是在管理员权限之下运行的)
    ''' </summary>
    Public Module WindowsServices

        ''' <summary>
        ''' Windows system logging services interface, you can viewing the application log events from Event Viewer:
        ''' Explorer >> Manage >> Event Viewer >> Applications and Services Logs >> &lt;Your_Product>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ServicesLogs As Logs

        ''' <summary>
        ''' Does component <see cref="ServicesLogs"/> have been initialized?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Initialized As Boolean
            Get
                Return Not ServicesLogs Is Nothing AndAlso ServicesLogs.Initialized
            End Get
        End Property

        ''' <summary>
        ''' You should execute the log category entry creates job under the administrators privileges!
        ''' </summary>
        ''' <returns></returns>
        Public Function Initialize() As Boolean
            Return Initialize(GetType(Logs).FullName, App.AssemblyName)
        End Function

        ''' <summary>
        ''' You should execute the log category entry creates job under the administrators privileges!
        ''' </summary>
        ''' <param name="ServicesName"></param>
        ''' <returns></returns>
        Public Function Initialize(ServicesName As String) As Boolean
            Return Initialize(ServicesName, App.AssemblyName)
        End Function

        ''' <summary>
        ''' You should execute the log category entry creates job under the administrators privileges!
        ''' </summary>
        ''' <param name="ServicesName"></param>
        ''' <param name="Product">This value is usually the property value of <see cref="App.AssemblyName"/></param>
        ''' <returns></returns>
        Public Function Initialize(ServicesName As String, Product As String) As Boolean
            WindowsServices._ServicesLogs = New Logs(ServicesName, Product)
            If Not WindowsServices.ServicesLogs.Initialized Then
                Call $"Exception during register node: {WindowsServices.ServicesLogs.ToString}.".__DEBUG_ECHO
                Call $"You should execute the log category entry creates job under the administrators privileges!".__DEBUG_ECHO
            End If

            Return WindowsServices.ServicesLogs.Initialized
        End Function

        ''' <summary>
        ''' You should execute the log category entry creates job under the administrators privileges!
        ''' </summary>
        ''' <param name="ServicesName"></param>
        ''' <param name="Products">This value is usually the property value of <see cref="App.AssemblyName"/></param>
        ''' <returns></returns>
        Public Function LogsInstaller(ServicesName As String, ParamArray Products As String()) As Boolean
            Dim b As Boolean = True

            For Each Product As String In Products
                If Not WindowsServices.Initialize(ServicesName, Product) Then
                    b = False
                End If
            Next

            If Not b Then
                Call "Parts of the log entry was installed failed....".__DEBUG_ECHO
            End If

            Return b
        End Function

        ''' <summary>
        ''' (**** Please notice, that the application has To have admin privileges To be able To write the needed stuff into registry. ****)
        ''' 
        ''' Everyone knows HTTP-URLs. Windows Shell also enables to define own ``URL protocols``. 
        ''' Some programs (like Visual Studio Help ``ms-help://`` ... or Steam ``steam://`` ...) take advantage of this feature. 
        ''' By creating some registry entries one is able to set up a self-made URL protocol. 
        ''' This allows to access your applications by URL (originating from every software).
        ''' 
        ''' Please notice, that **the application has To have admin privileges To be able To write the needed stuff into registry**. 
        ''' You can test your application very easy by opening Windows Explorer And typing ``yoururlprotocol://testdata`` 
        ''' into the path/address field.
        ''' 
        ''' Registers an user defined URL protocol for the usage with the Windows Shell, the Internet Explorer and Office.
        ''' 
        ''' Example for an URL of an user defined URL protocol:
        ''' 
        ''' ```
        ''' rainbird://RemoteControl/OpenFridge/GetBeer
        ''' ```
        ''' </summary>
        ''' <param name="protocolName">
        ''' Name of the protocol (e.g. "rainbird" for "rainbird://...")
        ''' </param>
        ''' <param name="applicationPath">
        ''' Complete file system path to the EXE file, which processes the URL being called (the complete URL is handed over as a Command Line Parameter).
        ''' </param>
        ''' <param name="description">
        ''' Description (e.g. "URL:Rainbird Custom URL")
        ''' </param>
        Public Sub RegisterURLProtocol(protocolName As String, applicationPath As String, description As String)
            ' Create new key for desired URL protocol
            Dim protocol As RegistryKey = Registry.ClassesRoot.CreateSubKey(protocolName)

            ' Assign protocol
            Call protocol.SetValue(Nothing, description)
            Call protocol.SetValue("URL Protocol", String.Empty)

            ' Register Shell values
            Call Registry.ClassesRoot.CreateSubKey(protocolName & "\Shell")
            Call Registry.ClassesRoot.CreateSubKey(protocolName & "\Shell\open")
            protocol = Registry.ClassesRoot.CreateSubKey(protocolName & "\Shell\open\command")

            ' Specify application handling the URL protocol
            Call protocol.SetValue(Nothing, """" & applicationPath, +""" %1")
        End Sub
    End Module
End Namespace
