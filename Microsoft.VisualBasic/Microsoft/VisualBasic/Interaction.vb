Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.Win32
Imports System
Imports System.Collections
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic
    <StandardModule> _
    Public NotInheritable Class Interaction
        ' Methods
        <SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
        Public Shared Sub AppActivate(ProcessId As Integer)
            Dim num As Integer
            Dim window As IntPtr = NativeMethods.GetWindow(NativeMethods.GetDesktopWindow, 5)
            Do While (window <> IntPtr.Zero)
                SafeNativeMethods.GetWindowThreadProcessId(window, num)
                If (((num = ProcessId) AndAlso SafeNativeMethods.IsWindowEnabled(window)) AndAlso SafeNativeMethods.IsWindowVisible(window)) Then
                    Exit Do
                End If
                window = NativeMethods.GetWindow(window, 2)
            Loop
            If (window = IntPtr.Zero) Then
                window = NativeMethods.GetWindow(NativeMethods.GetDesktopWindow, 5)
                Do While (window <> IntPtr.Zero)
                    SafeNativeMethods.GetWindowThreadProcessId(window, num)
                    If (num = ProcessId) Then
                        Exit Do
                    End If
                    window = NativeMethods.GetWindow(window, 2)
                Loop
            End If
            If (window = IntPtr.Zero) Then
                Dim args As String() = New String() {Conversions.ToString(ProcessId)}
                Throw New ArgumentException(Utils.GetResourceString("ProcessNotFound", args))
            End If
            Interaction.AppActivateHelper(window)
        End Sub

        <SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
        Public Shared Sub AppActivate(Title As String)
            Dim lpClassName As String = Nothing
            Dim hWnd As IntPtr = NativeMethods.FindWindow(lpClassName, Title)
            If (hWnd = IntPtr.Zero) Then
                Dim strA As String = String.Empty
                Dim lpString As New StringBuilder(&H1FF)
                Dim length As Integer = Strings.Len(Title)
                hWnd = NativeMethods.GetWindow(NativeMethods.GetDesktopWindow, 5)
                Do While (hWnd <> IntPtr.Zero)
                    strA = lpString.ToString
                    If ((NativeMethods.GetWindowText(hWnd, lpString, lpString.Capacity) >= length) AndAlso (String.Compare(strA, 0, Title, 0, length, StringComparison.OrdinalIgnoreCase) = 0)) Then
                        Exit Do
                    End If
                    hWnd = NativeMethods.GetWindow(hWnd, 2)
                Loop
                If (hWnd = IntPtr.Zero) Then
                    hWnd = NativeMethods.GetWindow(NativeMethods.GetDesktopWindow, 5)
                    Do While (hWnd <> IntPtr.Zero)
                        strA = lpString.ToString
                        If ((NativeMethods.GetWindowText(hWnd, lpString, lpString.Capacity) >= length) AndAlso (String.Compare(Strings.Right(strA, length), 0, Title, 0, length, StringComparison.OrdinalIgnoreCase) = 0)) Then
                            Exit Do
                        End If
                        hWnd = NativeMethods.GetWindow(hWnd, 2)
                    Loop
                End If
            End If
            If (hWnd = IntPtr.Zero) Then
                Dim args As String() = New String() {Title}
                Throw New ArgumentException(Utils.GetResourceString("ProcessNotFound", args))
            End If
            Interaction.AppActivateHelper(hWnd)
        End Sub

        <SecuritySafeCritical>
        Private Shared Sub AppActivateHelper(hwndApp As IntPtr)
            Dim num As Integer
            Try
                Call New UIPermission(UIPermissionWindow.AllWindows).Demand()
            Catch exception As Exception
                Throw exception
            End Try
            If (Not SafeNativeMethods.IsWindowEnabled(hwndApp) OrElse Not SafeNativeMethods.IsWindowVisible(hwndApp)) Then
                Dim window As IntPtr = NativeMethods.GetWindow(hwndApp, 0)
                Do While (window <> IntPtr.Zero)
                    If (NativeMethods.GetWindow(window, 4) = hwndApp) Then
                        If (SafeNativeMethods.IsWindowEnabled(window) AndAlso SafeNativeMethods.IsWindowVisible(window)) Then
                            Exit Do
                        End If
                        hwndApp = window
                        window = NativeMethods.GetWindow(hwndApp, 0)
                    End If
                    window = NativeMethods.GetWindow(window, 2)
                Loop
                If (window = IntPtr.Zero) Then
                    Throw New ArgumentException(Utils.GetResourceString("ProcessNotFound"))
                End If
                hwndApp = window
            End If
            NativeMethods.AttachThreadInput(0, SafeNativeMethods.GetWindowThreadProcessId(hwndApp, num), 1)
            NativeMethods.SetForegroundWindow(hwndApp)
            NativeMethods.SetFocus(hwndApp)
            NativeMethods.AttachThreadInput(0, SafeNativeMethods.GetWindowThreadProcessId(hwndApp, num), 0)
        End Sub

        <SecuritySafeCritical>
        Public Shared Sub Beep()
            Try
                Call New UIPermission(UIPermissionWindow.SafeSubWindows).Demand()
            Catch exception As SecurityException
                Try
                    Call New UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand()
                Catch exception2 As SecurityException
                    Return
                End Try
            End Try
            UnsafeNativeMethods.MessageBeep(0)
        End Sub

        Public Shared Function CallByName(ObjectRef As Object, ProcName As String, UseCallType As CallType, ParamArray Args As Object()) As Object
            Select Case UseCallType
                Case CallType.Method
                    Return LateBinding.InternalLateCall(ObjectRef, Nothing, ProcName, args, Nothing, Nothing, False)
                Case CallType.Get
                    Return LateBinding.LateGet(ObjectRef, Nothing, ProcName, args, Nothing, Nothing)
                Case CallType.Let, CallType.Set
                    Dim objType As Type = Nothing
                    LateBinding.InternalLateSet(ObjectRef, objType, ProcName, args, Nothing, False, UseCallType)
                    Return Nothing
            End Select
            Dim args As String() = New String() {"CallType"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
        End Function

        Private Shared Sub CheckPathComponent(s As String)
            If ((s Is Nothing) OrElse (s.Length = 0)) Then
                Throw New ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty"))
            End If
        End Sub

        Public Shared Function Choose(Index As Double, ParamArray Choice As Object()) As Object
            Dim index As Integer = CInt(Math.Round(CDbl((Conversion.Fix(index) - 1))))
            If (Choice.Rank <> 1) Then
                Dim args As String() = New String() {"Choice"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", args))
            End If
            If ((index < 0) OrElse (index > Choice.GetUpperBound(0))) Then
                Return Nothing
            End If
            Return Choice(index)
        End Function

        <SecuritySafeCritical>
        Public Shared Function Command() As String
            Call New EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Demand()

            If (Interaction.m_CommandLine Is Nothing) Then
                Dim index As Integer
                Dim commandLine As String = Environment.CommandLine
                If ((commandLine Is Nothing) OrElse (commandLine.Length = 0)) Then
                    Return ""
                End If
                Dim length As Integer = Environment.GetCommandLineArgs(0).Length
                Do
                    index = commandLine.IndexOf(""""c, index)
                    If ((index >= 0) AndAlso (index <= length)) Then
                        commandLine = commandLine.Remove(index, 1)
                    End If
                Loop While ((index >= 0) AndAlso (index <= length))
                If ((index = 0) OrElse (index > commandLine.Length)) Then
                    Interaction.m_CommandLine = ""
                Else
                    Interaction.m_CommandLine = Strings.LTrim(commandLine.Substring(length))
                End If
            End If
            Return Interaction.m_CommandLine
        End Function

        <SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt), SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
        Public Shared Function CreateObject(ProgId As String, Optional ServerName As String = "") As Object
            Dim obj2 As Object
            If (ProgId.Length = 0) Then
                Throw ExceptionUtils.VbMakeException(&H1AD)
            End If
            If ((ServerName Is Nothing) OrElse (ServerName.Length = 0)) Then
                ServerName = Nothing
            ElseIf (String.Compare(Environment.MachineName, ServerName, StringComparison.OrdinalIgnoreCase) = 0) Then
                ServerName = Nothing
            End If
            Try
                Dim typeFromProgID As Type
                If (ServerName Is Nothing) Then
                    typeFromProgID = Type.GetTypeFromProgID(ProgId)
                Else
                    typeFromProgID = Type.GetTypeFromProgID(ProgId, ServerName, True)
                End If
                obj2 = Activator.CreateInstance(typeFromProgID)
            Catch exception As COMException
                If (exception.ErrorCode = -2147023174) Then
                    Throw ExceptionUtils.VbMakeException(&H1CE)
                End If
                Throw ExceptionUtils.VbMakeException(&H1AD)
            Catch exception2 As StackOverflowException
                Throw exception2
            Catch exception3 As OutOfMemoryException
                Throw exception3
            Catch exception4 As ThreadAbortException
                Throw exception4
            Catch exception5 As Exception
                Throw ExceptionUtils.VbMakeException(&H1AD)
            End Try
            Return obj2
        End Function

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Shared Sub DeleteSetting(AppName As String, Optional Section As String = Nothing, Optional Key As String = Nothing)
            Dim key2 As RegistryKey = Nothing
            Interaction.CheckPathComponent(AppName)
            Dim subkey As String = Interaction.FormRegKey(AppName, Section)
            Try
                Dim currentUser As RegistryKey = Registry.CurrentUser
                If (Information.IsNothing(Key) OrElse (Key.Length = 0)) Then
                    currentUser.DeleteSubKeyTree(subkey)
                Else
                    key2 = currentUser.OpenSubKey(subkey, True)
                    If (key2 Is Nothing) Then
                        Dim args As String() = New String() {"Section"}
                        Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
                    End If
                    key2.DeleteValue(Key)
                End If
            Catch exception As Exception
                Throw exception
            Finally
                If (Not key2 Is Nothing) Then
                    key2.Close()
                End If
            End Try
        End Sub

        <SecuritySafeCritical>
        Public Shared Function Environ(Expression As Integer) As String
            If ((Expression <= 0) OrElse (Expression > &HFF)) Then
                Dim args As String() = New String() {"Expression"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_Range1toFF1", args))
            End If
            If (Interaction.m_SortedEnvList Is Nothing) Then
                Dim environSyncObject As Object = Interaction.m_EnvironSyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(environSyncObject)
                SyncLock environSyncObject
                    If (Interaction.m_SortedEnvList Is Nothing) Then
                        Call New EnvironmentPermission(PermissionState.Unrestricted).Assert()
                        Interaction.m_SortedEnvList = New SortedList(Environment.GetEnvironmentVariables)
                        PermissionSet.RevertAssert()
                    End If
                End SyncLock
            End If
            If (Expression > Interaction.m_SortedEnvList.Count) Then
                Return ""
            End If
            Dim pathList As String = Interaction.m_SortedEnvList.GetKey((Expression - 1)).ToString
            Dim str3 As String = Interaction.m_SortedEnvList.GetByIndex((Expression - 1)).ToString
            Call New EnvironmentPermission(EnvironmentPermissionAccess.Read, pathList).Demand()
            Return (pathList & "=" & str3)
        End Function

        Public Shared Function Environ(Expression As String) As String
            Expression = Strings.Trim(Expression)
            If (Expression.Length = 0) Then
                Dim args As String() = New String() {"Expression"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Return Environment.GetEnvironmentVariable(Expression)
        End Function

        Private Shared Function FormRegKey(sApp As String, sSect As String) As String
            If (Information.IsNothing(sApp) OrElse (sApp.Length = 0)) Then
                Return "Software\VB and VBA Program Settings"
            End If
            If (Information.IsNothing(sSect) OrElse (sSect.Length = 0)) Then
                Return ("Software\VB and VBA Program Settings\" & sApp)
            End If
            Return ("Software\VB and VBA Program Settings\" & sApp & "\" & sSect)
        End Function

        Public Shared Function GetAllSettings(AppName As String, Section As String) As String(0 To .,0 To .)
            Interaction.CheckPathComponent(AppName)
            Interaction.CheckPathComponent(Section)
            Dim name As String = Interaction.FormRegKey(AppName, Section)
            Dim key As RegistryKey = Registry.CurrentUser.OpenSubKey(name)
            If (key Is Nothing) Then
                Return Nothing
            End If
            Dim strArray(,) As String(0 To .,0 To .) = Nothing
            Try
                If (key.ValueCount = 0) Then
                    Return strArray
                End If
                Dim valueNames As String() = key.GetValueNames
                Dim upperBound As Integer = valueNames.GetUpperBound(0)
                Dim strArray3(,) As String(0 To .,0 To .) = New String((upperBound + 1) - 1, 2 - 1) {}
                Dim num2 As Integer = upperBound
                Dim i As Integer = 0
                Do While (i <= num2)
                    Dim str2 As String = valueNames(i)
                    strArray3(i, 0) = str2
                    Dim obj2 As Object = key.GetValue(str2)
                    If ((Not obj2 Is Nothing) AndAlso TypeOf obj2 Is String) Then
                        strArray3(i, 1) = obj2.ToString
                    End If
                    i += 1
                Loop
                strArray = strArray3
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception4 As Exception
            Finally
                key.Close()
            End Try
            Return strArray
        End Function

        <SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt), SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
        Public Shared Function GetObject(Optional PathName As String = Nothing, Optional [Class] As String = Nothing) As Object
            Dim activeObject As IPersistFile
            If (Strings.Len([Class]) = 0) Then
                Try
                    Return Marshal.BindToMoniker(PathName)
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception18 As Exception
                    Throw ExceptionUtils.VbMakeException(&H1AD)
                End Try
            End If
            If (PathName Is Nothing) Then
                Try
                    Return Marshal.GetActiveObject([Class])
                Catch exception4 As StackOverflowException
                    Throw exception4
                Catch exception5 As OutOfMemoryException
                    Throw exception5
                Catch exception6 As ThreadAbortException
                    Throw exception6
                Catch exception22 As Exception
                    Throw ExceptionUtils.VbMakeException(&H1AD)
                End Try
            End If
            If (Strings.Len(PathName) = 0) Then
                Try
                    Return Activator.CreateInstance(Type.GetTypeFromProgID([Class]))
                Catch exception7 As StackOverflowException
                    Throw exception7
                Catch exception8 As OutOfMemoryException
                    Throw exception8
                Catch exception9 As ThreadAbortException
                    Throw exception9
                Catch exception26 As Exception
                    Throw ExceptionUtils.VbMakeException(&H1AD)
                End Try
            End If
            Try
                activeObject = DirectCast(Marshal.GetActiveObject([Class]), IPersistFile)
            Catch exception10 As StackOverflowException
                Throw exception10
            Catch exception11 As OutOfMemoryException
                Throw exception11
            Catch exception12 As ThreadAbortException
                Throw exception12
            Catch exception30 As Exception
                Throw ExceptionUtils.VbMakeException(&H1B0)
            End Try
            Try
                activeObject.Load(PathName, 0)
            Catch exception13 As StackOverflowException
                Throw exception13
            Catch exception14 As OutOfMemoryException
                Throw exception14
            Catch exception15 As ThreadAbortException
                Throw exception15
            Catch exception34 As Exception
                Throw ExceptionUtils.VbMakeException(&H1AD)
            End Try
            Return activeObject
        End Function

        Public Shared Function GetSetting(AppName As String, Section As String, Key As String, Optional [Default] As String = "") As String
            Dim key As RegistryKey = Nothing
            Dim obj2 As Object
            Interaction.CheckPathComponent(AppName)
            Interaction.CheckPathComponent(Section)
            Interaction.CheckPathComponent(key)
            If ([Default] Is Nothing) Then
                [Default] = ""
            End If
            Dim name As String = Interaction.FormRegKey(AppName, Section)
            Try
                key = Registry.CurrentUser.OpenSubKey(name)
                If (key Is Nothing) Then
                    Return [Default]
                End If
                obj2 = key.GetValue(key, [Default])
            Finally
                If (Not key Is Nothing) Then
                    key.Close()
                End If
            End Try
            If (obj2 Is Nothing) Then
                Return Nothing
            End If
            If Not TypeOf obj2 Is String Then
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
            End If
            Return CStr(obj2)
        End Function

        Private Shared Function GetTitleFromAssembly(CallingAssembly As Assembly) As String
            Try
                Return CallingAssembly.GetName.Name
            Catch exception As SecurityException
                Dim fullName As String = CallingAssembly.FullName
                Dim index As Integer = fullName.IndexOf(","c)
                If (index >= 0) Then
                    Return fullName.Substring(0, index)
                End If
                Return ""
            End Try
        End Function

        Public Shared Function IIf(Expression As Boolean, TruePart As Object, FalsePart As Object) As Object
            If Expression Then
                Return TruePart
            End If
            Return FalsePart
        End Function

        Friend Shared Function IIf(Of T)(Condition As Boolean, TruePart As T, FalsePart As T) As T
            If Condition Then
                Return TruePart
            End If
            Return FalsePart
        End Function

        <MethodImpl(MethodImplOptions.NoInlining), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.UI)>
        Public Shared Function InputBox(Prompt As String, Optional Title As String = "", Optional DefaultResponse As String = "", Optional XPos As Integer = -1, Optional YPos As Integer = -1) As String
            Dim parentWindow As IWin32Window = Nothing
            Dim vBHost As IVbHost = HostServices.VBHost
            If (Not vBHost Is Nothing) Then
                parentWindow = vBHost.GetParentWindow
            End If
            If (Title.Length = 0) Then
                If (vBHost Is Nothing) Then
                    Title = Interaction.GetTitleFromAssembly(Assembly.GetCallingAssembly)
                Else
                    Title = vBHost.GetWindowTitle
                End If
            End If
            If (Thread.CurrentThread.GetApartmentState <> ApartmentState.STA) Then
                Dim handler As New InputBoxHandler(Prompt, Title, DefaultResponse, XPos, YPos, parentWindow)
                Dim thread1 As New Thread(New ThreadStart(AddressOf handler.StartHere))
                thread1.Start()
                thread1.Join()

                If (Not handler.Exception Is Nothing) Then
                    Throw handler.Exception
                End If
                Return handler.Result
            End If
            Return Interaction.InternalInputBox(Prompt, Title, DefaultResponse, XPos, YPos, parentWindow)
        End Function

        Private Shared Sub InsertNumber(ByRef Buffer As String, Num As Long, Spaces As Long)
            Dim expression As String = Conversions.ToString(Num)
            Interaction.InsertSpaces(Buffer, (Spaces - Strings.Len(expression)))
            Buffer = (Buffer & expression)
        End Sub

        Private Shared Sub InsertSpaces(ByRef Buffer As String, Spaces As Long)
            Do While (Spaces > 0)
                Buffer = (Buffer & " ")
                Spaces = (Spaces - 1)
            Loop
        End Sub

        Private Shared Function InternalInputBox(Prompt As String, Title As String, DefaultResponse As String, XPos As Integer, YPos As Integer, ParentWindow As IWin32Window) As String
            Dim box1 As New VBInputBox(Prompt, Title, DefaultResponse, XPos, YPos)
            box1.ShowDialog(ParentWindow)
            Dim output As String = box1.Output
            box1.Dispose()
            Return output
        End Function

        <MethodImpl(MethodImplOptions.NoInlining), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.UI)>
        Public Shared Function MsgBox(Prompt As Object, Optional Buttons As MsgBoxStyle = 0, Optional Title As Object = Nothing) As MsgBoxResult
            Dim text As String = Nothing
            Dim titleFromAssembly As String
            Dim owner As IWin32Window = Nothing
            Dim vBHost As IVbHost = HostServices.VBHost
            If (Not vBHost Is Nothing) Then
                owner = vBHost.GetParentWindow
            End If
            If ((((Buttons And 15) > MsgBoxStyle.RetryCancel) OrElse ((Buttons And 240) > MsgBoxStyle.Information)) OrElse ((Buttons And &HF00) > MsgBoxStyle.DefaultButton3)) Then
                Buttons = MsgBoxStyle.ApplicationModal
            End If
            Try
                If (Not Prompt Is Nothing) Then
                    [text] = CStr(Conversions.ChangeType(Prompt, GetType(String)))
                End If
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception9 As Exception
                Dim args As String() = New String() {"Prompt", "String"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", args))
            End Try
            Try
                If (Title Is Nothing) Then
                    If (vBHost Is Nothing) Then
                        titleFromAssembly = Interaction.GetTitleFromAssembly(Assembly.GetCallingAssembly)
                    Else
                        titleFromAssembly = vBHost.GetWindowTitle
                    End If
                Else
                    titleFromAssembly = Conversions.ToString(Title)
                End If
            Catch exception4 As StackOverflowException
                Throw exception4
            Catch exception5 As OutOfMemoryException
                Throw exception5
            Catch exception6 As ThreadAbortException
                Throw exception6
            Catch exception13 As Exception
                Dim textArray2 As String() = New String() {"Title", "String"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", textArray2))
            End Try
            Return DirectCast(MessageBox.Show(owner, [text], titleFromAssembly, (DirectCast(Buttons, MessageBoxButtons) And DirectCast(15, MessageBoxButtons)), (DirectCast(Buttons, MessageBoxIcon) And DirectCast(240, MessageBoxIcon)), (DirectCast(Buttons, MessageBoxDefaultButton) And DirectCast(&HF00, MessageBoxDefaultButton)), (DirectCast(Buttons, MessageBoxOptions) And DirectCast(-4096, MessageBoxOptions))), MsgBoxResult)
        End Function

        Public Shared Function Partition(Number As Long, Start As Long, [Stop] As Long, Interval As Long) As String
            Dim num As Long
            Dim num2 As Long
            Dim flag As Boolean
            Dim flag2 As Boolean
            Dim buffer As String = Nothing
            Dim num3 As Long
            If (Start < 0) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If ([Stop] <= Start) Then
                Dim textArray2 As String() = New String() {"Stop"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
            End If
            If (Interval < 1) Then
                Dim textArray3 As String() = New String() {"Interval"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3))
            End If
            If (Number < Start) Then
                num2 = (Start - 1)
                flag2 = True
            ElseIf (Number > [Stop]) Then
                num = ([Stop] + 1)
                flag = True
            ElseIf (Interval = 1) Then
                num = Number
                num2 = Number
            Else
                num = ((((Number - Start) / Interval) * Interval) + Start)
                num2 = ((num + Interval) - 1)
                If (num2 > [Stop]) Then
                    num2 = [Stop]
                End If
                If (num < Start) Then
                    num = Start
                End If
            End If
            Dim expression As String = Conversions.ToString(CLng(([Stop] + 1)))
            Dim str3 As String = Conversions.ToString(CLng((Start - 1)))
            If (Strings.Len(expression) > Strings.Len(str3)) Then
                num3 = Strings.Len(expression)
            Else
                num3 = Strings.Len(str3)
            End If
            If flag2 Then
                expression = Conversions.ToString(num2)
                If (num3 < Strings.Len(expression)) Then
                    num3 = Strings.Len(expression)
                End If
            End If
            If flag2 Then
                Interaction.InsertSpaces(buffer, num3)
            Else
                Interaction.InsertNumber(buffer, num, num3)
            End If
            buffer = (buffer & ":")
            If flag Then
                Interaction.InsertSpaces(buffer, num3)
                Return buffer
            End If
            Interaction.InsertNumber(buffer, num2, num3)
            Return buffer
        End Function

        Public Shared Sub SaveSetting(AppName As String, Section As String, Key As String, Setting As String)
            Interaction.CheckPathComponent(AppName)
            Interaction.CheckPathComponent(Section)
            Interaction.CheckPathComponent(key)
            Dim subkey As String = Interaction.FormRegKey(AppName, Section)
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(subkey)
            If (key Is Nothing) Then
                Dim args As String() = New String() {subkey}
                Throw New ArgumentException(Utils.GetResourceString("Interaction_ResKeyNotCreated1", args))
            End If
            Try
                key.SetValue(key, Setting)
            Catch exception As Exception
                Throw exception
            Finally
                key.Close()
            End Try
        End Sub

        <SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
        Public Shared Function Shell(PathName As String, Optional Style As AppWinStyle = 2, Optional Wait As Boolean = False, Optional Timeout As Integer = -1) As Integer
            Dim num As Integer
            Dim lpStartupInfo As New STARTUPINFO
            Dim lpProcessInformation As New PROCESS_INFORMATION
            Dim hHandle As New LateInitSafeHandleZeroOrMinusOneIsInvalid
            Dim invalid2 As New LateInitSafeHandleZeroOrMinusOneIsInvalid
            Dim num3 As Integer = 0
            Try
                Call New UIPermission(UIPermissionWindow.AllWindows).Demand()
            Catch exception As Exception
                Throw exception
            End Try
            If (PathName Is Nothing) Then
                Dim args As String() = New String() {"Pathname"}
                Throw New NullReferenceException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
            End If
            If ((Style < AppWinStyle.Hide) OrElse (Style > DirectCast(9, AppWinStyle))) Then
                Dim textArray2 As String() = New String() {"Style"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
            End If
            NativeMethods.GetStartupInfo(lpStartupInfo)
            Try
                Dim num2 As Integer
                lpStartupInfo.dwFlags = 1
                lpStartupInfo.wShowWindow = CShort(Style)
                RuntimeHelpers.PrepareConstrainedRegions()

                Try
                Finally
                    num2 = NativeMethods.CreateProcess(Nothing, PathName, Nothing, Nothing, False, &H20, New IntPtr, Nothing, lpStartupInfo, lpProcessInformation)
                    If (num2 = 0) Then
                        num3 = Marshal.GetLastWin32Error
                    End If
                    If ((lpProcessInformation.hProcess <> IntPtr.Zero) AndAlso (lpProcessInformation.hProcess <> NativeTypes.INVALID_HANDLE)) Then
                        hHandle.InitialSetHandle(lpProcessInformation.hProcess)
                    End If
                    If ((lpProcessInformation.hThread <> IntPtr.Zero) AndAlso (lpProcessInformation.hThread <> NativeTypes.INVALID_HANDLE)) Then
                        invalid2.InitialSetHandle(lpProcessInformation.hThread)
                    End If
                End Try
                Try
                    If (num2 <> 0) Then
                        If Wait Then
                            If (NativeMethods.WaitForSingleObject(hHandle, Timeout) = 0) Then
                                Return 0
                            End If
                            Return lpProcessInformation.dwProcessId
                        End If
                        NativeMethods.WaitForInputIdle(hHandle, &H2710)
                        Return lpProcessInformation.dwProcessId
                    End If
                    If (num3 = 5) Then
                        Throw ExceptionUtils.VbMakeException(70)
                    End If
                    Throw ExceptionUtils.VbMakeException(&H35)
                Finally
                    hHandle.Close
                    invalid2.Close
                End Try
            Finally
                lpStartupInfo.Dispose
            End Try
            Return num
        End Function

        Public Shared Function Switch(ParamArray VarExpr As Object()) As Object
            If (VarExpr Is Nothing) Then
                Return Nothing
            End If
            Dim length As Integer = VarExpr.Length
            Dim index As Integer = 0
            If ((length Mod 2) = 0) Then
                Do While (length > 0)
                    If Conversions.ToBoolean(VarExpr(index)) Then
                        Return VarExpr((index + 1))
                    End If
                    index = (index + 2)
                    length = (length - 2)
                Loop
                Return Nothing
            End If
            Dim args As String() = New String() {"VarExpr"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
        End Function


        ' Fields
        Private Shared m_CommandLine As String
        Private Shared m_EnvironSyncObject As Object = New Object
        Private Shared m_SortedEnvList As SortedList

        ' Nested Types
        Private NotInheritable Class InputBoxHandler
            ' Methods
            Public Sub New(Prompt As String, Title As String, DefaultResponse As String, XPos As Integer, YPos As Integer, ParentWindow As IWin32Window)
                Me.m_Prompt = Prompt
                Me.m_Title = Title
                Me.m_DefaultResponse = DefaultResponse
                Me.m_XPos = XPos
                Me.m_YPos = YPos
                Me.m_ParentWindow = ParentWindow
            End Sub

            <STAThread>
            Public Sub StartHere()
                Try
                    Me.m_Result = Interaction.InternalInputBox(Me.m_Prompt, Me.m_Title, Me.m_DefaultResponse, Me.m_XPos, Me.m_YPos, Me.m_ParentWindow)
                Catch exception As Exception
                    Me.m_Exception = exception
                End Try
            End Sub


            ' Properties
            Friend ReadOnly Property Exception As Exception
                Get
                    Return Me.m_Exception
                End Get
            End Property

            Public ReadOnly Property Result As String
                Get
                    Return Me.m_Result
                End Get
            End Property


            ' Fields
            Private m_DefaultResponse As String
            Private m_Exception As Exception
            Private m_ParentWindow As IWin32Window
            Private m_Prompt As String
            Private m_Result As String
            Private m_Title As String
            Private m_XPos As Integer
            Private m_YPos As Integer
        End Class

        <ComVisible(True), Guid("0000010B-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
        Private Interface IPersistFile
            Sub GetClassID(ByRef pClassID As Guid)
            Sub IsDirty()
            Sub Load(pszFileName As String, dwMode As Integer)
            Sub Save(pszFileName As String, fRemember As Integer)
            Sub SaveCompleted(pszFileName As String)
            Function GetCurFile() As String
        End Interface
    End Class
End Namespace

