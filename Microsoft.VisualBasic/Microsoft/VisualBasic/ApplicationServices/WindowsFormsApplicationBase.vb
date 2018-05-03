Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.Devices
Imports Microsoft.Win32.SafeHandles
Imports System
Imports System.Collections
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Net
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Tcp
Imports System.Runtime.Remoting.Messaging
Imports System.Security
Imports System.Security.AccessControl
Imports System.Security.Permissions
Imports System.Security.Principal
Imports System.Threading
Imports System.Timers
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.ApplicationServices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class WindowsFormsApplicationBase
        Inherits ConsoleApplicationBase
        ' Events
        Public Custom Event NetworkAvailabilityChanged As NetworkAvailableEventHandler
            AddHandler(value As NetworkAvailableEventHandler)
                Dim networkAvailChangeLock As Object = Me.m_NetworkAvailChangeLock
                ObjectFlowControl.CheckForSyncLockOnValueType(networkAvailChangeLock)
                SyncLock networkAvailChangeLock
                    If (Me.m_NetworkAvailabilityEventHandlers Is Nothing) Then
                        Me.m_NetworkAvailabilityEventHandlers = New ArrayList
                    End If
                    Me.m_NetworkAvailabilityEventHandlers.Add(value)
                    Me.m_TurnOnNetworkListener = True
                    If ((Me.m_NetworkObject Is Nothing) And Me.m_FinishedOnInitilaize) Then
                        Me.m_NetworkObject = New Network
                        AddHandler Me.m_NetworkObject.NetworkAvailabilityChanged, New NetworkAvailableEventHandler(AddressOf Me.NetworkAvailableEventAdaptor)
                    End If
                End SyncLock
            End AddHandler
            RemoveHandler(value As NetworkAvailableEventHandler)
                If ((Not Me.m_NetworkAvailabilityEventHandlers Is Nothing) AndAlso (Me.m_NetworkAvailabilityEventHandlers.Count > 0)) Then
                    Me.m_NetworkAvailabilityEventHandlers.Remove(value)
                    If (Me.m_NetworkAvailabilityEventHandlers.Count = 0) Then
                        RemoveHandler Me.m_NetworkObject.NetworkAvailabilityChanged, New NetworkAvailableEventHandler(AddressOf Me.NetworkAvailableEventAdaptor)
                        If (Not Me.m_NetworkObject Is Nothing) Then
                            Me.m_NetworkObject.DisconnectListener()
                            Me.m_NetworkObject = Nothing
                        End If
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
                            Try
                                If (Not current Is Nothing) Then
                                    current.Invoke(sender, e)
                                End If
                                Continue Do
                            Catch exception As Exception
                                If Not Me.OnUnhandledException(New UnhandledExceptionEventArgs(True, exception)) Then
                                    Throw
                                End If
                                Continue Do
                            End Try
                        Loop
                    Finally
                        If TypeOf enumerator Is IDisposable Then
                            TryCast(enumerator, IDisposable).Dispose()
                        End If
                    End Try
                End If
            End RaiseEvent
        End Event

        Public Event Shutdown As ShutdownEventHandler

        Public Event Startup As StartupEventHandler

        Public Event StartupNextInstance As StartupNextInstanceEventHandler
        Public Custom Event UnhandledException As UnhandledExceptionEventHandler
            AddHandler(value As UnhandledExceptionEventHandler)
                If (Me.m_UnhandledExceptionHandlers Is Nothing) Then
                    Me.m_UnhandledExceptionHandlers = New ArrayList
                End If
                Me.m_UnhandledExceptionHandlers.Add(value)
                If (Me.m_UnhandledExceptionHandlers.Count = 1) Then
                    AddHandler Application.ThreadException, New ThreadExceptionEventHandler(AddressOf Me.OnUnhandledExceptionEventAdaptor)
                End If
            End AddHandler
            RemoveHandler(value As UnhandledExceptionEventHandler)
                If ((Not Me.m_UnhandledExceptionHandlers Is Nothing) AndAlso (Me.m_UnhandledExceptionHandlers.Count > 0)) Then
                    Me.m_UnhandledExceptionHandlers.Remove(value)
                    If (Me.m_UnhandledExceptionHandlers.Count = 0) Then
                        RemoveHandler Application.ThreadException, New ThreadExceptionEventHandler(AddressOf Me.OnUnhandledExceptionEventAdaptor)
                    End If
                End If
            End RemoveHandler
            RaiseEvent(sender As Object, e As UnhandledExceptionEventArgs)
                If (Not Me.m_UnhandledExceptionHandlers Is Nothing) Then
                    Dim enumerator As IEnumerator
                    Me.m_ProcessingUnhandledExceptionEvent = True
                    Try
                        enumerator = Me.m_UnhandledExceptionHandlers.GetEnumerator
                        Do While enumerator.MoveNext
                            Dim current As UnhandledExceptionEventHandler = DirectCast(enumerator.Current, UnhandledExceptionEventHandler)
                            If (Not current Is Nothing) Then
                                current.Invoke(sender, e)
                            End If
                        Loop
                    Finally
                        If TypeOf enumerator Is IDisposable Then
                            TryCast(enumerator, IDisposable).Dispose()
                        End If
                    End Try
                    Me.m_ProcessingUnhandledExceptionEvent = False
                End If
            End RaiseEvent
        End Event

        ' Methods
        Public Sub New()
            Me.New(AuthenticationMode.Windows)
        End Sub

        <SecuritySafeCritical>
        Public Sub New(authenticationMode As AuthenticationMode)
            Me.m_MinimumSplashExposure = &H7D0
            Me.m_SplashLock = New Object
            Me.m_NetworkAvailChangeLock = New Object
            Me.m_Ok2CloseSplashScreen = True
            Me.ValidateAuthenticationModeEnumValue(authenticationMode, "authenticationMode")
            If (authenticationMode = AuthenticationMode.Windows) Then
                Try
                    Thread.CurrentPrincipal = New WindowsPrincipal(WindowsIdentity.GetCurrent)
                Catch exception As SecurityException
                End Try
            End If
            Me.m_AppContext = New WinFormsAppContext(Me)
            Call New UIPermission(UIPermissionWindow.AllWindows).Assert
            Me.m_AppSyncronizationContext = AsyncOperationManager.SynchronizationContext
            AsyncOperationManager.SynchronizationContext = New WindowsFormsSynchronizationContext
            PermissionSet.RevertAssert()
        End Sub

        Private Sub DisplaySplash()
            If (Not Me.m_SplashTimer Is Nothing) Then
                Me.m_SplashTimer.Enabled = True
            End If
            Application.Run(Me.m_SplashScreen)
        End Sub

        Private Sub DoApplicationModel()
            Dim eventArgs As New StartupEventArgs(MyBase.CommandLineArgs)
            If Not Debugger.IsAttached Then
                Try
                    If (Me.OnInitialize(MyBase.CommandLineArgs) AndAlso Me.OnStartup(eventArgs)) Then
                        Me.OnRun()
                        Me.OnShutdown()
                    End If
                Catch exception As Exception
                    If Me.m_ProcessingUnhandledExceptionEvent Then
                        Throw
                    End If
                    If Not Me.OnUnhandledException(New UnhandledExceptionEventArgs(True, exception)) Then
                        Throw
                    End If
                End Try
            ElseIf (Me.OnInitialize(MyBase.CommandLineArgs) AndAlso Me.OnStartup(eventArgs)) Then
                Me.OnRun()
                Me.OnShutdown()
            End If
        End Sub

        Public Sub DoEvents()
            Application.DoEvents()
        End Sub

        <SecurityCritical>
        Private Function GetApplicationInstanceID(Entry As Assembly) As String
            Dim set1 As New PermissionSet(PermissionState.None)
            set1.AddPermission(New FileIOPermission(PermissionState.Unrestricted))
            set1.AddPermission(New SecurityPermission(SecurityPermissionFlag.UnmanagedCode))
            set1.Assert()
            Dim typeLibGuidForAssembly As Guid = Marshal.GetTypeLibGuidForAssembly(Entry)
            Dim strArray As String() = Entry.GetName.Version.ToString.Split(Conversions.ToCharArrayRankOne("."))
            PermissionSet.RevertAssert()
            Return (typeLibGuidForAssembly.ToString & strArray(0) & "." & strArray(1))
        End Function

        <EditorBrowsable(EditorBrowsableState.Advanced), SecuritySafeCritical>
        Protected Sub HideSplashScreen()
            Dim splashLock As Object = Me.m_SplashLock
            ObjectFlowControl.CheckForSyncLockOnValueType(splashLock)
            SyncLock splashLock
                If (Not Me.MainForm Is Nothing) Then
                    Call New UIPermission(UIPermissionWindow.AllWindows).Assert
                    Me.MainForm.Activate()
                    PermissionSet.RevertAssert()
                End If
                If ((Not Me.m_SplashScreen Is Nothing) AndAlso Not Me.m_SplashScreen.IsDisposed) Then
                    Dim method As DisposeDelegate = New DisposeDelegate(AddressOf Me.m_SplashScreen.Dispose)
                    Me.m_SplashScreen.Invoke(method)
                    Me.m_SplashScreen = Nothing
                End If
            End SyncLock
        End Sub

        Private Sub MainFormLoadingDone(sender As Object, e As EventArgs)
            RemoveHandler Me.MainForm.Load, New EventHandler(AddressOf Me.MainFormLoadingDone)
            Do While Not Me.m_Ok2CloseSplashScreen
                Me.DoEvents()
            Loop
            Me.HideSplashScreen()
        End Sub

        Private Sub MinimumSplashExposureTimeIsUp(sender As Object, e As ElapsedEventArgs)
            If (Not Me.m_SplashTimer Is Nothing) Then
                Me.m_SplashTimer.Dispose
                Me.m_SplashTimer = Nothing
            End If
            Me.m_Ok2CloseSplashScreen = True
        End Sub

        Private Sub NetworkAvailableEventAdaptor(sender As Object, e As NetworkAvailableEventArgs)
            Me.raise_NetworkAvailabilityChanged(sender, e)
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Overridable Sub OnCreateMainForm()
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Overridable Sub OnCreateSplashScreen()
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced), STAThread>
        Protected Overridable Function OnInitialize(commandLineArgs As ReadOnlyCollection(Of String)) As Boolean
            If Me.m_EnableVisualStyles Then
                Application.EnableVisualStyles()
            End If
            If (Not commandLineArgs.Contains("/nosplash") AndAlso Not MyBase.CommandLineArgs.Contains("-nosplash")) Then
                Me.ShowSplashScreen()
            End If
            Me.m_FinishedOnInitilaize = True
            Return True
        End Function

        <SecuritySafeCritical, EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Overridable Sub OnRun()
            If (Me.MainForm Is Nothing) Then
                Me.OnCreateMainForm()

                If (Me.MainForm Is Nothing) Then
                    Throw New NoStartupFormException
                End If
                AddHandler Me.MainForm.Load, New EventHandler(AddressOf Me.MainFormLoadingDone)
            End If
            Try
                Application.Run(Me.m_AppContext)
            Finally
                If (Not Me.m_NetworkObject Is Nothing) Then
                    Me.m_NetworkObject.DisconnectListener()
                End If
                If (Not Me.m_FirstInstanceSemaphore Is Nothing) Then
                    Me.m_FirstInstanceSemaphore.Close()
                    Me.m_FirstInstanceSemaphore = Nothing
                End If
                AsyncOperationManager.SynchronizationContext = Me.m_AppSyncronizationContext
                Me.m_AppSyncronizationContext = Nothing
            End Try
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Overridable Sub OnShutdown()
            Dim shutdownEvent As ShutdownEventHandler = Me.ShutdownEvent
            If (Not shutdownEvent Is Nothing) Then
                shutdownEvent.Invoke(Me, EventArgs.Empty)
            End If
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Overridable Function OnStartup(eventArgs As StartupEventArgs) As Boolean
            eventArgs.Cancel = False
            If (Me.m_TurnOnNetworkListener And (Me.m_NetworkObject Is Nothing)) Then
                Me.m_NetworkObject = New Network
                AddHandler Me.m_NetworkObject.NetworkAvailabilityChanged, New NetworkAvailableEventHandler(AddressOf Me.NetworkAvailableEventAdaptor)
            End If
            Dim startupEvent As StartupEventHandler = Me.StartupEvent
            If (Not startupEvent Is Nothing) Then
                startupEvent.Invoke(Me, eventArgs)
            End If
            Return Not eventArgs.Cancel
        End Function

        <EditorBrowsable(EditorBrowsableState.Advanced), SecuritySafeCritical>
        Protected Overridable Sub OnStartupNextInstance(eventArgs As StartupNextInstanceEventArgs)
            Dim startupNextInstanceEvent As StartupNextInstanceEventHandler = Me.StartupNextInstanceEvent
            If (Not startupNextInstanceEvent Is Nothing) Then
                startupNextInstanceEvent.Invoke(Me, eventArgs)
            End If
            Call New UIPermission(UIPermissionWindow.AllWindows).Assert
            If (eventArgs.BringToForeground AndAlso (Not Me.MainForm Is Nothing)) Then
                If (Me.MainForm.WindowState = FormWindowState.Minimized) Then
                    Me.MainForm.WindowState = FormWindowState.Normal
                End If
                Me.MainForm.Activate()
            End If
        End Sub

        Private Sub OnStartupNextInstanceMarshallingAdaptor(args As Object)
            Me.OnStartupNextInstance(New StartupNextInstanceEventArgs(DirectCast(args, ReadOnlyCollection(Of String)), True))
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Overridable Function OnUnhandledException(e As UnhandledExceptionEventArgs) As Boolean
            If ((Not Me.m_UnhandledExceptionHandlers Is Nothing) AndAlso (Me.m_UnhandledExceptionHandlers.Count > 0)) Then
                Me.raise_UnhandledException(Me, e)
                If e.ExitApplication Then
                    Application.Exit()
                End If
                Return True
            End If
            Return False
        End Function

        Private Sub OnUnhandledExceptionEventAdaptor(sender As Object, e As ThreadExceptionEventArgs)
            Me.OnUnhandledException(New UnhandledExceptionEventArgs(True, e.Exception))
        End Sub

        <SecurityCritical>
        Private Function ReadUrlFromMemoryMappedFile() As String
            Dim str2 As String
            Using handle As SafeFileHandle = UnsafeNativeMethods.OpenFileMapping(4, False, Me.m_MemoryMappedID)
                If handle.IsInvalid Then
                    Return Nothing
                End If
                Using handle2 As SafeMemoryMappedViewOfFileHandle = UnsafeNativeMethods.MapViewOfFile(handle.DangerousGetHandle, 4, 0, 0, UIntPtr.Zero)
                    If handle2.IsInvalid Then
                        Throw ExceptionUtils.GetWin32Exception("AppModel_CantGetMemoryMappedFile", New String(0 - 1) {})
                    End If
                    str2 = Marshal.PtrToStringUni(handle2.DangerousGetHandle)
                End Using
            End Using
            Return str2
        End Function

        <SecurityCritical>
        Private Function RegisterChannel(ChannelType As ChannelType, ChannelIsSecure As Boolean) As IChannel
            Dim set1 As New PermissionSet(PermissionState.None)
            set1.AddPermission(New SecurityPermission((SecurityPermissionFlag.ControlPrincipal Or (SecurityPermissionFlag.SerializationFormatter Or SecurityPermissionFlag.UnmanagedCode))))
            set1.AddPermission(New SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "127.0.0.1", 0))
            set1.AddPermission(New EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME"))
            set1.AddPermission(New SecurityPermission(SecurityPermissionFlag.RemotingConfiguration))
            set1.Assert()
            Dim properties As IDictionary = New Hashtable(3)
            properties.Add("bindTo", "127.0.0.1")
            properties.Add("port", 0)
            properties.Add("name", String.Empty)
            If ChannelIsSecure Then
                properties.Add("secure", True)
                properties.Add("tokenimpersonationlevel", TokenImpersonationLevel.Impersonation)
                properties.Add("impersonate", True)
            End If
            Dim chnl As IChannel = Nothing
            If (ChannelType = ChannelType.Server) Then
                chnl = New TcpServerChannel(properties, Nothing)
            Else
                chnl = New TcpClientChannel(properties, Nothing)
            End If
            ChannelServices.RegisterChannel(chnl, ChannelIsSecure)
            PermissionSet.RevertAssert()
            Return chnl
        End Function

        <MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical>
        Public Sub Run(commandLine As String())
            MyBase.InternalCommandLine = New ReadOnlyCollection(Of String)(commandLine)
            If Not Me.IsSingleInstance Then
                Me.DoApplicationModel()
            Else
                Dim flag2 As Boolean
                Dim applicationInstanceID As String = Me.GetApplicationInstanceID(Assembly.GetCallingAssembly)
                Me.m_MemoryMappedID = (applicationInstanceID & "Map")
                Dim name As String = (applicationInstanceID & "Event")
                Dim str3 As String = (applicationInstanceID & "Event2")
                Me.m_StartNextInstanceCallback = New SendOrPostCallback(AddressOf Me.OnStartupNextInstanceMarshallingAdaptor)
                Call New SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert
                Dim left As String = WindowsIdentity.GetCurrent.Name
                Dim channelIsSecure As Boolean = (Operators.CompareString(left, "", False) > 0)
                CodeAccessPermission.RevertAssert()

                If channelIsSecure Then
                    Dim rule As New EventWaitHandleAccessRule(left, EventWaitHandleRights.FullControl, AccessControlType.Allow)
                    Dim eventSecurity As New EventWaitHandleSecurity
                    Call New SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert
                    eventSecurity.AddAccessRule(rule)
                    CodeAccessPermission.RevertAssert()
                    Me.m_FirstInstanceSemaphore = New EventWaitHandle(False, EventResetMode.ManualReset, name, flag2, eventSecurity)
                    Dim createdNew As Boolean = False
                    Me.m_MessageRecievedSemaphore = New EventWaitHandle(False, EventResetMode.AutoReset, str3, createdNew, eventSecurity)
                Else
                    Me.m_FirstInstanceSemaphore = New EventWaitHandle(False, EventResetMode.ManualReset, name, flag2)
                    Me.m_MessageRecievedSemaphore = New EventWaitHandle(False, EventResetMode.AutoReset, str3)
                End If
                If flag2 Then
                    Try
                        Dim uRI As String = (applicationInstanceID & ".rem")
                        Call New SecurityPermission(SecurityPermissionFlag.RemotingConfiguration).Assert
                        RemotingServices.Marshal(New RemoteCommunicator(Me, Me.m_MessageRecievedSemaphore), uRI)
                        CodeAccessPermission.RevertAssert()
                        Dim uRL As String = DirectCast(Me.RegisterChannel(ChannelType.Server, channelIsSecure), TcpServerChannel).GetUrlsForUri(uRI)(0)
                        Me.WriteUrlToMemoryMappedFile(uRL)
                        Me.m_FirstInstanceSemaphore.Set()
                        Me.DoApplicationModel()
                        Return
                    Finally
                        If (Not Me.m_MessageRecievedSemaphore Is Nothing) Then
                            Me.m_MessageRecievedSemaphore.Close()
                        End If
                        If (Not Me.m_FirstInstanceSemaphore Is Nothing) Then
                            Me.m_FirstInstanceSemaphore.Close()
                        End If
                        If ((Not Me.m_FirstInstanceMemoryMappedFileHandle Is Nothing) AndAlso Not Me.m_FirstInstanceMemoryMappedFileHandle.IsInvalid) Then
                            Me.m_FirstInstanceMemoryMappedFileHandle.Close()
                        End If
                    End Try
                End If
                If Not Me.m_FirstInstanceSemaphore.WaitOne(&H9C4, False) Then
                    Throw New CantStartSingleInstanceException
                End If
                Me.RegisterChannel(ChannelType.Client, channelIsSecure)
                Dim url As String = Me.ReadUrlFromMemoryMappedFile
                If (url Is Nothing) Then
                    Throw New CantStartSingleInstanceException
                End If
                Dim set1 As New PermissionSet(PermissionState.None)
                set1.AddPermission(New SecurityPermission((SecurityPermissionFlag.ControlPrincipal Or (SecurityPermissionFlag.SerializationFormatter Or SecurityPermissionFlag.UnmanagedCode))))
                set1.AddPermission(New DnsPermission(PermissionState.Unrestricted))
                set1.AddPermission(New SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "127.0.0.1", -1))
                set1.AddPermission(New EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME"))
                set1.Assert()
                DirectCast(RemotingServices.Connect(GetType(RemoteCommunicator), url), RemoteCommunicator).RunNextInstance(MyBase.CommandLineArgs)
                PermissionSet.RevertAssert()

                If Not Me.m_MessageRecievedSemaphore.WaitOne(&H9C4, False) Then
                    Throw New CantStartSingleInstanceException
                End If
            End If
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Sub ShowSplashScreen()
            If Not Me.m_DidSplashScreen Then
                Me.m_DidSplashScreen = True
                If (Me.m_SplashScreen Is Nothing) Then
                    Me.OnCreateSplashScreen()
                End If
                If (Not Me.m_SplashScreen Is Nothing) Then
                    If (Me.m_MinimumSplashExposure > 0) Then
                        Me.m_Ok2CloseSplashScreen = False
                        Me.m_SplashTimer = New Timer(CDbl(Me.m_MinimumSplashExposure))
                        AddHandler Me.m_SplashTimer.Elapsed, New ElapsedEventHandler(AddressOf Me.MinimumSplashExposureTimeIsUp)
                        Me.m_SplashTimer.AutoReset = False
                    Else
                        Me.m_Ok2CloseSplashScreen = True
                    End If
                    Call New Thread(New ThreadStart(AddressOf Me.DisplaySplash)).Start
                End If
            End If
        End Sub

        Private Sub ValidateAuthenticationModeEnumValue(value As AuthenticationMode, paramName As String)
            If ((value < AuthenticationMode.Windows) OrElse (value > AuthenticationMode.ApplicationDefined)) Then
                Throw New InvalidEnumArgumentException(paramName, CInt(value), GetType(AuthenticationMode))
            End If
        End Sub

        Private Sub ValidateShutdownModeEnumValue(value As ShutdownMode, paramName As String)
            If ((value < ShutdownMode.AfterMainFormCloses) OrElse (value > ShutdownMode.AfterAllFormsClose)) Then
                Throw New InvalidEnumArgumentException(paramName, CInt(value), GetType(ShutdownMode))
            End If
        End Sub

        <SecurityCritical>
        Private Sub WriteUrlToMemoryMappedFile(URL As String)
            Dim hFile As New HandleRef(Nothing, New IntPtr(-1))
            Using security_attributes As SECURITY_ATTRIBUTES = New SECURITY_ATTRIBUTES
                Dim flag As Boolean
                security_attributes.bInheritHandle = False
                Try
                    Call New SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert
                    flag = NativeMethods.ConvertStringSecurityDescriptorToSecurityDescriptor("D:(A;;GA;;;CO)(A;;GR;;;AU)", 1, security_attributes.lpSecurityDescriptor, IntPtr.Zero)
                    CodeAccessPermission.RevertAssert()
                Catch exception As EntryPointNotFoundException
                    security_attributes.lpSecurityDescriptor = IntPtr.Zero
                Catch exception2 As DllNotFoundException
                    security_attributes.lpSecurityDescriptor = IntPtr.Zero
                End Try
                If Not flag Then
                    security_attributes.lpSecurityDescriptor = IntPtr.Zero
                End If
                Me.m_FirstInstanceMemoryMappedFileHandle = UnsafeNativeMethods.CreateFileMapping(hFile, security_attributes, 4, 0, ((URL.Length + 1) * 2), Me.m_MemoryMappedID)
                If Me.m_FirstInstanceMemoryMappedFileHandle.IsInvalid Then
                    Throw ExceptionUtils.GetWin32Exception("AppModel_CantGetMemoryMappedFile", New String(0 - 1) {})
                End If
            End Using
            Using handle As SafeMemoryMappedViewOfFileHandle = UnsafeNativeMethods.MapViewOfFile(Me.m_FirstInstanceMemoryMappedFileHandle.DangerousGetHandle, 2, 0, 0, UIntPtr.Zero)
                If handle.IsInvalid Then
                    Throw ExceptionUtils.GetWin32Exception("AppModel_CantGetMemoryMappedFile", New String(0 - 1) {})
                End If
                Dim source As Char() = URL.ToCharArray
                Marshal.Copy(source, 0, handle.DangerousGetHandle, source.Length)
            End Using
        End Sub


        ' Properties
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property ApplicationContext As ApplicationContext
            Get
                Return Me.m_AppContext
            End Get
        End Property

        Protected Property EnableVisualStyles As Boolean
            Get
                Return Me.m_EnableVisualStyles
            End Get
            Set(value As Boolean)
                Me.m_EnableVisualStyles = value
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Property IsSingleInstance As Boolean
            Get
                Return Me.m_IsSingleInstance
            End Get
            Set(value As Boolean)
                Me.m_IsSingleInstance = value
            End Set
        End Property

        Protected Property MainForm As Form
            Get
                Return Interaction.IIf(Of Form)((Me.m_AppContext IsNot Nothing), Me.m_AppContext.MainForm, Nothing)
            End Get
            Set(value As Form)
                If (value Is Nothing) Then
                    Dim placeHolders As String() = New String() {"MainForm"}
                    Throw ExceptionUtils.GetArgumentNullException("MainForm", "General_PropertyNothing", placeHolders)
                End If
                If (value Is Me.m_SplashScreen) Then
                    Throw New ArgumentException(Utils.GetResourceString("AppModel_SplashAndMainFormTheSame"))
                End If
                Me.m_AppContext.MainForm = value
            End Set
        End Property

        Public Property MinimumSplashScreenDisplayTime As Integer
            Get
                Return Me.m_MinimumSplashExposure
            End Get
            Set(value As Integer)
                Me.m_MinimumSplashExposure = value
            End Set
        End Property

        Public ReadOnly Property OpenForms As FormCollection
            Get
                Return Application.OpenForms
            End Get
        End Property

        Private ReadOnly Property RunNextInstanceDelegate As SendOrPostCallback
            Get
                Return Me.m_StartNextInstanceCallback
            End Get
        End Property

        Public Property SaveMySettingsOnExit As Boolean
            Get
                Return Me.m_SaveMySettingsOnExit
            End Get
            Set(value As Boolean)
                Me.m_SaveMySettingsOnExit = value
            End Set
        End Property

        Protected Friend Property ShutdownStyle As ShutdownMode
            Get
                Return Me.m_ShutdownStyle
            End Get
            Set(value As ShutdownMode)
                Me.ValidateShutdownModeEnumValue(value, "value")
                Me.m_ShutdownStyle = value
            End Set
        End Property

        Public Property SplashScreen As Form
            Get
                Return Me.m_SplashScreen
            End Get
            Set(value As Form)
                If ((Not value Is Nothing) AndAlso (value Is Me.m_AppContext.MainForm)) Then
                    Throw New ArgumentException(Utils.GetResourceString("AppModel_SplashAndMainFormTheSame"))
                End If
                Me.m_SplashScreen = value
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Shared ReadOnly Property UseCompatibleTextRendering As Boolean
            Get
                Return False
            End Get
        End Property


        ' Fields
        Private Const ATTACH_TIMEOUT As Integer = &H9C4
        Private Const HOST_NAME As String = "127.0.0.1"
        Private m_AppContext As WinFormsAppContext
        Private m_AppSyncronizationContext As SynchronizationContext
        Private m_DidSplashScreen As Boolean
        Private m_EnableVisualStyles As Boolean
        Private m_FinishedOnInitilaize As Boolean
        <SecurityCritical>
        Private m_FirstInstanceMemoryMappedFileHandle As SafeFileHandle
        Private m_FirstInstanceSemaphore As EventWaitHandle
        Private m_IsSingleInstance As Boolean
        Private m_MemoryMappedID As String
        Private m_MessageRecievedSemaphore As EventWaitHandle
        Private m_MinimumSplashExposure As Integer
        Private m_NetworkAvailabilityEventHandlers As ArrayList
        Private m_NetworkAvailChangeLock As Object
        Private m_NetworkObject As Network
        Private m_Ok2CloseSplashScreen As Boolean
        Private m_ProcessingUnhandledExceptionEvent As Boolean
        Private m_SaveMySettingsOnExit As Boolean
        Private m_ShutdownStyle As ShutdownMode
        Private m_SplashLock As Object
        Private m_SplashScreen As Form
        Private m_SplashTimer As Timer
        Private m_StartNextInstanceCallback As SendOrPostCallback
        Private m_TurnOnNetworkListener As Boolean
        Private m_UnhandledExceptionHandlers As ArrayList
        Private Const SECOND_INSTANCE_TIMEOUT As Integer = &H9C4

        ' Nested Types
        Private Enum ChannelType As Byte
            ' Fields
            Client = 1
            Server = 0
        End Enum

        Private Delegate Sub DisposeDelegate()

        Private Class RemoteCommunicator
            Inherits MarshalByRefObject
            ' Methods
            <SecurityCritical>
            Friend Sub New(appObject As WindowsFormsApplicationBase, ConnectionMadeSemaphore As EventWaitHandle)
                Call New SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert
                Me.m_OriginalUser = WindowsIdentity.GetCurrent
                CodeAccessPermission.RevertAssert()
                Me.m_AsyncOp = AsyncOperationManager.CreateOperation(Nothing)
                Me.m_StartNextInstanceDelegate = appObject.RunNextInstanceDelegate
                Me.m_ConnectionMadeSemaphore = ConnectionMadeSemaphore
            End Sub

            <SecurityCritical>
            Public Overrides Function InitializeLifetimeService() As Object
                Return Nothing
            End Function

            <SecuritySafeCritical, OneWay>
            Public Sub RunNextInstance(Args As ReadOnlyCollection(Of String))
                Call New SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert
                If Not (Not Me.m_OriginalUser.User Is WindowsIdentity.GetCurrent.User) Then
                    Me.m_ConnectionMadeSemaphore.Set()
                    CodeAccessPermission.RevertAssert()
                    Me.m_AsyncOp.Post(Me.m_StartNextInstanceDelegate, Args)
                End If
            End Sub


            ' Fields
            Private m_AsyncOp As AsyncOperation
            Private m_ConnectionMadeSemaphore As EventWaitHandle
            Private m_OriginalUser As WindowsIdentity
            Private m_StartNextInstanceDelegate As SendOrPostCallback
        End Class

        Private Class WinFormsAppContext
            Inherits ApplicationContext
            ' Methods
            Public Sub New(App As WindowsFormsApplicationBase)
                Me.m_App = App
            End Sub

            <SecuritySafeCritical>
            Protected Overrides Sub OnMainFormClosed(sender As Object, e As EventArgs)
                If (Me.m_App.ShutdownStyle = ShutdownMode.AfterMainFormCloses) Then
                    MyBase.OnMainFormClosed(sender, e)
                Else
                    Call New UIPermission(UIPermissionWindow.AllWindows).Assert
                    Dim openForms As FormCollection = Application.OpenForms
                    PermissionSet.RevertAssert()

                    If (openForms.Count > 0) Then
                        MyBase.MainForm = openForms.Item(0)
                    Else
                        MyBase.OnMainFormClosed(sender, e)
                    End If
                End If
            End Sub


            ' Fields
            Private m_App As WindowsFormsApplicationBase
        End Class
    End Class
End Namespace

