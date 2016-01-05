Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.MyServices.Internal
Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Security
Imports System.Security.Permissions
Imports System.Threading

Namespace Microsoft.VisualBasic.Devices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class Network
        ' Events
        Public Custom Event NetworkAvailabilityChanged As NetworkAvailableEventHandler
            AddHandler(handler As NetworkAvailableEventHandler)
                Try
                    Me.m_Connected = Me.IsAvailable
                Catch exception As SecurityException
                    Return
                Catch exception2 As PlatformNotSupportedException
                    Return
                End Try
                Dim syncObject As Object = Me.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    If (Me.m_NetworkAvailabilityEventHandlers Is Nothing) Then
                        Me.m_NetworkAvailabilityEventHandlers = New ArrayList
                    End If
                    Me.m_NetworkAvailabilityEventHandlers.Add(handler)
                    If (Me.m_NetworkAvailabilityEventHandlers.Count = 1) Then
                        Me.m_NetworkAvailabilityChangedCallback = New SendOrPostCallback(AddressOf Me.NetworkAvailabilityChangedHandler)
                        If (Not AsyncOperationManager.SynchronizationContext Is Nothing) Then
                            Me.m_SynchronizationContext = AsyncOperationManager.SynchronizationContext
                            Try
                                AddHandler NetworkChange.NetworkAddressChanged, New NetworkAddressChangedEventHandler(AddressOf Me.OS_NetworkAvailabilityChangedListener)
                            Catch exception3 As PlatformNotSupportedException
                            Catch exception4 As NetworkInformationException
                            End Try
                        End If
                    End If
                End SyncLock
            End AddHandler
            RemoveHandler(handler As NetworkAvailableEventHandler)
                If ((Not Me.m_NetworkAvailabilityEventHandlers Is Nothing) AndAlso (Me.m_NetworkAvailabilityEventHandlers.Count > 0)) Then
                    Me.m_NetworkAvailabilityEventHandlers.Remove(handler)
                    If (Me.m_NetworkAvailabilityEventHandlers.Count = 0) Then
                        RemoveHandler NetworkChange.NetworkAddressChanged, New NetworkAddressChangedEventHandler(AddressOf Me.OS_NetworkAvailabilityChangedListener)
                        Me.DisconnectListener()
                    End If
                End If
            End RemoveHandler
            RaiseEvent(sender As Object, e As NetworkAvailableEventArgs)
                If (Not Me.m_NetworkAvailabilityEventHandlers Is Nothing) Then
                    Dim enumerator As IEnumerator
                    Try
                        enumerator = Me.m_NetworkAvailabilityEventHandlers.GetEnumerator
                        Do While enumerator.MoveNext
                            Dim current As NetworkAvailableEventHandler = DirectCast(enumerator.Current, NetworkAvailableEventHandler)
                            If (Not current Is Nothing) Then
                                current.Invoke(sender, e)
                            End If
                        Loop
                    Finally
                        If TypeOf enumerator Is IDisposable Then
                            TryCast(enumerator, IDisposable).Dispose()
                        End If
                    End Try
                End If
            End RaiseEvent
        End Event

        ' Methods
        Friend Sub DisconnectListener()
            RemoveHandler NetworkChange.NetworkAddressChanged, New NetworkAddressChangedEventHandler(AddressOf Me.OS_NetworkAvailabilityChangedListener)
        End Sub

        Public Sub DownloadFile(address As String, destinationFileName As String)
            Me.DownloadFile(address, destinationFileName, "", "", False, &H186A0, False)
        End Sub

        Public Sub DownloadFile(address As Uri, destinationFileName As String)
            Me.DownloadFile(address, destinationFileName, "", "", False, &H186A0, False)
        End Sub

        Public Sub DownloadFile(address As String, destinationFileName As String, userName As String, password As String)
            Me.DownloadFile(address, destinationFileName, userName, password, False, &H186A0, False)
        End Sub

        Public Sub DownloadFile(address As Uri, destinationFileName As String, userName As String, password As String)
            Me.DownloadFile(address, destinationFileName, userName, password, False, &H186A0, False)
        End Sub

        Public Sub DownloadFile(address As Uri, destinationFileName As String, networkCredentials As ICredentials, showUI As Boolean, connectionTimeout As Integer, overwrite As Boolean)
            Me.DownloadFile(address, destinationFileName, networkCredentials, showUI, connectionTimeout, overwrite, UICancelOption.ThrowException)
        End Sub

        Public Sub DownloadFile(address As String, destinationFileName As String, userName As String, password As String, showUI As Boolean, connectionTimeout As Integer, overwrite As Boolean)
            Me.DownloadFile(address, destinationFileName, userName, password, showUI, connectionTimeout, overwrite, UICancelOption.ThrowException)
        End Sub

        <SecuritySafeCritical>
        Public Sub DownloadFile(address As Uri, destinationFileName As String, networkCredentials As ICredentials, showUI As Boolean, connectionTimeout As Integer, overwrite As Boolean, onUserCancel As UICancelOption)
            If (connectionTimeout <= 0) Then
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("connectionTimeOut", "Network_BadConnectionTimeout", New String(0 - 1) {})
            End If
            If (address Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("address")
            End If
            Using extended As WebClientExtended = New WebClientExtended
                extended.Timeout = connectionTimeout
                extended.UseNonPassiveFtp = showUI
                Dim path As String = FileSystem.NormalizeFilePath(destinationFileName, "destinationFileName")
                If Directory.Exists(path) Then
                    Throw ExceptionUtils.GetInvalidOperationException("Network_DownloadNeedsFilename", New String(0 - 1) {})
                End If
                If (File.Exists(path) And Not overwrite) Then
                    Dim args As String() = New String() {destinationFileName}
                    Throw New IOException(Utils.GetResourceString("IO_FileExists_Path", args))
                End If
                If (Not networkCredentials Is Nothing) Then
                    extended.Credentials = networkCredentials
                End If
                Dim dialog As ProgressDialog = Nothing
                If (showUI AndAlso Environment.UserInteractive) Then
                    Call New UIPermission(UIPermissionWindow.SafeSubWindows).Demand
                    dialog = New ProgressDialog
                    Dim textArray2 As String() = New String() {address.AbsolutePath}
                    dialog.Text = Utils.GetResourceString("ProgressDialogDownloadingTitle", textArray2)
                    Dim textArray3 As String() = New String() {address.AbsolutePath, path}
                    dialog.LabelText = Utils.GetResourceString("ProgressDialogDownloadingLabel", textArray3)
                End If
                Dim directoryName As String = path.GetDirectoryName(path)
                If (directoryName = "") Then
                    Throw ExceptionUtils.GetInvalidOperationException("Network_DownloadNeedsFilename", New String(0 - 1) {})
                End If
                If Not Directory.Exists(directoryName) Then
                    Directory.CreateDirectory(directoryName)
                End If
                Call New WebClientCopy(extended, dialog).DownloadFile(address, path)
                If ((showUI AndAlso Environment.UserInteractive) AndAlso ((onUserCancel = UICancelOption.ThrowException) And dialog.UserCanceledTheDialog)) Then
                    Throw New OperationCanceledException
                End If
            End Using
        End Sub

        Public Sub DownloadFile(address As Uri, destinationFileName As String, userName As String, password As String, showUI As Boolean, connectionTimeout As Integer, overwrite As Boolean)
            Me.DownloadFile(address, destinationFileName, userName, password, showUI, connectionTimeout, overwrite, UICancelOption.ThrowException)
        End Sub

        Public Sub DownloadFile(address As String, destinationFileName As String, userName As String, password As String, showUI As Boolean, connectionTimeout As Integer, overwrite As Boolean, onUserCancel As UICancelOption)
            If (String.IsNullOrEmpty(address) OrElse (address.Trim = "")) Then
                Throw ExceptionUtils.GetArgumentNullException("address")
            End If
            Dim uri As Uri = Me.GetUri(address.Trim)
            Dim networkCredentials As ICredentials = Me.GetNetworkCredentials(userName, password)
            Me.DownloadFile(uri, destinationFileName, networkCredentials, showUI, connectionTimeout, overwrite, onUserCancel)
        End Sub

        Public Sub DownloadFile(address As Uri, destinationFileName As String, userName As String, password As String, showUI As Boolean, connectionTimeout As Integer, overwrite As Boolean, onUserCancel As UICancelOption)
            Dim networkCredentials As ICredentials = Me.GetNetworkCredentials(userName, password)
            Me.DownloadFile(address, destinationFileName, networkCredentials, showUI, connectionTimeout, overwrite, onUserCancel)
        End Sub

        Private Function GetNetworkCredentials(userName As String, password As String) As ICredentials
            If (userName Is Nothing) Then
                userName = ""
            End If
            If (password Is Nothing) Then
                password = ""
            End If
            If ((userName = "") And (password = "")) Then
                Return Nothing
            End If
            Return New NetworkCredential(userName, password)
        End Function

        Private Function GetUri(address As String) As Uri
            Dim uri As Uri
            Try
                uri = New Uri(address)
            Catch exception As UriFormatException
                Dim placeHolders As String() = New String() {address}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("address", "Network_InvalidUriString", placeHolders)
            End Try
            Return uri
        End Function

        Private Sub NetworkAvailabilityChangedHandler(state As Object)
            Dim isAvailable As Boolean = Me.IsAvailable
            If (Me.m_Connected <> isAvailable) Then
                Me.m_Connected = isAvailable
                Me.raise_NetworkAvailabilityChanged(Me, New NetworkAvailableEventArgs(isAvailable))
            End If
        End Sub

        Private Sub OS_NetworkAvailabilityChangedListener(sender As Object, e As EventArgs)
            Dim syncObject As Object = Me.m_SyncObject
            ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
            SyncLock syncObject
                Me.m_SynchronizationContext.Post(Me.m_NetworkAvailabilityChangedCallback, Nothing)
            End SyncLock
        End Sub

        Public Function Ping(hostNameOrAddress As String) As Boolean
            Return Me.Ping(hostNameOrAddress, &H3E8)
        End Function

        Public Function Ping(address As Uri) As Boolean
            If (address Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("address")
            End If
            Return Me.Ping(address.Host, &H3E8)
        End Function

        Public Function Ping(hostNameOrAddress As String, timeout As Integer) As Boolean
            If Not Me.IsAvailable Then
                Throw ExceptionUtils.GetInvalidOperationException("Network_NetworkNotAvailable", New String(0 - 1) {})
            End If
            Return (New Ping().Send(hostNameOrAddress, timeout, Me.PingBuffer).Status = IPStatus.Success)
        End Function

        Public Function Ping(address As Uri, timeout As Integer) As Boolean
            If (address Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("address")
            End If
            Return Me.Ping(address.Host, timeout)
        End Function

        Public Sub UploadFile(sourceFileName As String, address As String)
            Me.UploadFile(sourceFileName, address, "", "", False, &H186A0)
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As Uri)
            Me.UploadFile(sourceFileName, address, "", "", False, &H186A0)
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As String, userName As String, password As String)
            Me.UploadFile(sourceFileName, address, userName, password, False, &H186A0)
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As Uri, userName As String, password As String)
            Me.UploadFile(sourceFileName, address, userName, password, False, &H186A0)
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As Uri, networkCredentials As ICredentials, showUI As Boolean, connectionTimeout As Integer)
            Me.UploadFile(sourceFileName, address, networkCredentials, showUI, connectionTimeout, UICancelOption.ThrowException)
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As String, userName As String, password As String, showUI As Boolean, connectionTimeout As Integer)
            Me.UploadFile(sourceFileName, address, userName, password, showUI, connectionTimeout, UICancelOption.ThrowException)
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As Uri, networkCredentials As ICredentials, showUI As Boolean, connectionTimeout As Integer, onUserCancel As UICancelOption)
            sourceFileName = FileSystem.NormalizeFilePath(sourceFileName, "sourceFileName")
            If Not File.Exists(sourceFileName) Then
                Dim args As String() = New String() {sourceFileName}
                Throw New FileNotFoundException(Utils.GetResourceString("IO_FileNotFound_Path", args))
            End If
            If (connectionTimeout <= 0) Then
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("connectionTimeout", "Network_BadConnectionTimeout", New String(0 - 1) {})
            End If
            If (address Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("address")
            End If
            Using extended As WebClientExtended = New WebClientExtended
                extended.Timeout = connectionTimeout
                If (Not networkCredentials Is Nothing) Then
                    extended.Credentials = networkCredentials
                End If
                Dim dialog As ProgressDialog = Nothing
                If (showUI AndAlso Environment.UserInteractive) Then
                    dialog = New ProgressDialog
                    Dim textArray2 As String() = New String() {sourceFileName}
                    dialog.Text = Utils.GetResourceString("ProgressDialogUploadingTitle", textArray2)
                    Dim textArray3 As String() = New String() {sourceFileName, address.AbsolutePath}
                    dialog.LabelText = Utils.GetResourceString("ProgressDialogUploadingLabel", textArray3)
                End If
                Call New WebClientCopy(extended, dialog).UploadFile(sourceFileName, address)
                If ((showUI AndAlso Environment.UserInteractive) AndAlso ((onUserCancel = UICancelOption.ThrowException) And dialog.UserCanceledTheDialog)) Then
                    Throw New OperationCanceledException
                End If
            End Using
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As Uri, userName As String, password As String, showUI As Boolean, connectionTimeout As Integer)
            Me.UploadFile(sourceFileName, address, userName, password, showUI, connectionTimeout, UICancelOption.ThrowException)
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As String, userName As String, password As String, showUI As Boolean, connectionTimeout As Integer, onUserCancel As UICancelOption)
            If (String.IsNullOrEmpty(address) OrElse (address.Trim = "")) Then
                Throw ExceptionUtils.GetArgumentNullException("address")
            End If
            Dim uri As Uri = Me.GetUri(address.Trim)
            If (Path.GetFileName(uri.AbsolutePath) = "") Then
                Throw ExceptionUtils.GetInvalidOperationException("Network_UploadAddressNeedsFilename", New String(0 - 1) {})
            End If
            Me.UploadFile(sourceFileName, uri, userName, password, showUI, connectionTimeout, onUserCancel)
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As Uri, userName As String, password As String, showUI As Boolean, connectionTimeout As Integer, onUserCancel As UICancelOption)
            Dim networkCredentials As ICredentials = Me.GetNetworkCredentials(userName, password)
            Me.UploadFile(sourceFileName, address, networkCredentials, showUI, connectionTimeout, onUserCancel)
        End Sub


        ' Properties
        Public ReadOnly Property IsAvailable As Boolean
            Get
                Return NetworkInterface.GetIsNetworkAvailable
            End Get
        End Property

        Private ReadOnly Property PingBuffer As Byte()
            Get
                If (Me.m_PingBuffer Is Nothing) Then
                    Me.m_PingBuffer = New Byte(&H20  - 1) {}
                    Dim index As Integer = 0
                    Do
                        Me.m_PingBuffer(index) = Convert.ToByte((&H61 + (index Mod &H17)), CultureInfo.InvariantCulture)
                        index += 1
                    Loop While (index <= &H1F)
                End If
                Return Me.m_PingBuffer
            End Get
        End Property


        ' Fields
        Private Const BUFFER_SIZE As Integer = &H20
        Private Const DEFAULT_PASSWORD As String = ""
        Private Const DEFAULT_PING_TIMEOUT As Integer = &H3E8
        Private Const DEFAULT_TIMEOUT As Integer = &H186A0
        Private Const DEFAULT_USERNAME As String = ""
        Private m_Connected As Boolean
        Private m_NetworkAvailabilityChangedCallback As SendOrPostCallback
        Private m_NetworkAvailabilityEventHandlers As ArrayList
        Private m_PingBuffer As Byte()
        Private m_SynchronizationContext As SynchronizationContext
        Private m_SyncObject As Object = New Object
    End Class
End Namespace

