Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions
Imports System.Text
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.Logging
    <ComVisible(False)> _
    Public Class FileLogTraceListener
        Inherits TraceListener
        ' Methods
        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
        Public Sub New()
            Me.New("FileLogTraceListener")
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(name As String)
            MyBase.New(name)
            Me.m_Location = LogFileLocation.LocalUserApplicationDirectory
            Me.m_AutoFlush = False
            Me.m_Append = True
            Me.m_IncludeHostName = False
            Me.m_DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.DiscardMessages
            Me.m_BaseFileName = Path.GetFileNameWithoutExtension(Application.ExecutablePath)
            Me.m_LogFileDateStamp = LogFileCreationScheduleOption.None
            Me.m_MaxFileSize = &H4C4B40
            Me.m_ReserveDiskSpace = &H989680
            Me.m_Delimiter = ChrW(9)
            Me.m_Encoding = Encoding.UTF8
            Me.m_CustomLocation = Application.UserAppDataPath
            Me.m_Day = DateAndTime.Now.Date
            Me.m_Days = 0
            Me.m_FirstDayOfWeek = FileLogTraceListener.GetFirstDayOfWeek(DateAndTime.Now.Date)
            Me.m_PropertiesSet = New BitArray(12, False)
            Me.m_SupportedAttributes = New String() {"append", "Append", "autoflush", "AutoFlush", "autoFlush", "basefilename", "BaseFilename", "baseFilename", "BaseFileName", "baseFileName", "customlocation", "CustomLocation", "customLocation", "delimiter", "Delimiter", "diskspaceexhaustedbehavior", "DiskSpaceExhaustedBehavior", "diskSpaceExhaustedBehavior", "encoding", "Encoding", "includehostname", "IncludeHostName", "includeHostName", "location", "Location", "logfilecreationschedule", "LogFileCreationSchedule", "logFileCreationSchedule", "maxfilesize", "MaxFileSize", "maxFileSize", "reservediskspace", "ReserveDiskSpace", "reserveDiskSpace"}
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Public Overrides Sub Close()
            Me.Dispose(True)
        End Sub

        Private Sub CloseCurrentStream()
            If (Not Me.m_Stream Is Nothing) Then
                Dim streams As Object = FileLogTraceListener.m_Streams
                SyncLock streams
                    If (Not Me.m_Stream Is Nothing) Then
                        Me.m_Stream.CloseStream()

                        If Not Me.m_Stream.IsInUse Then
                            FileLogTraceListener.m_Streams.Remove(Me.m_FullFileName.ToUpper(CultureInfo.InvariantCulture))
                        End If
                        Me.m_Stream = Nothing
                    End If
                End SyncLock
            End If
        End Sub

        Private Function DayChanged() As Boolean
            Return (DateTime.Compare(Me.m_Day.AddDays(CDbl(Me.m_Days)), DateAndTime.Now.Date) > 0)
        End Function

        <SecurityCritical>
        Private Sub DemandWritePermission()
            Dim directoryName As String = Path.GetDirectoryName(Me.LogFileName)
            Call New FileIOPermission(FileIOPermissionAccess.Write, directoryName).Demand()
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                Me.CloseCurrentStream()
            End If
        End Sub

        Private Sub EnsureStreamIsOpen()
            If (Me.m_Stream Is Nothing) Then
                Me.m_Stream = Me.GetStream
            End If
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Public Overrides Sub Flush()
            If (Not Me.m_Stream Is Nothing) Then
                Me.m_Stream.Flush()
            End If
        End Sub

        Private Function GetFileEncoding(fileName As String) As Encoding
            If File.Exists(fileName) Then
                Dim reader As StreamReader = Nothing
                Try
                    reader = New StreamReader(fileName, Me.Encoding, True)
                    If (reader.BaseStream.Length > 0) Then
                        reader.ReadLine()
                        Return reader.CurrentEncoding
                    End If
                Finally
                    If (Not reader Is Nothing) Then
                        reader.Close()
                    End If
                End Try
            End If
            Return Nothing
        End Function

        Private Shared Function GetFirstDayOfWeek(checkDate As DateTime) As DateTime
            Return checkDate.AddDays(CDbl((0 - checkDate.DayOfWeek))).Date
        End Function

        <SecuritySafeCritical>
        Private Function GetFreeDiskSpace() As Long
            Dim num3 As Long
            Dim num4 As Long
            Dim pathRoot As String = Path.GetPathRoot(Path.GetFullPath(Me.FullLogFileName))
            Dim userSpaceFree As Long = -1

            Call New FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathRoot).Demand()

            If (Not UnsafeNativeMethods.GetDiskFreeSpaceEx(pathRoot, userSpaceFree, num3, num4) OrElse (userSpaceFree <= -1)) Then
                Throw ExceptionUtils.GetWin32Exception("ApplicationLog_FreeSpaceError", New String(0 - 1) {})
            End If
            Return userSpaceFree
        End Function

        <SecuritySafeCritical>
        Private Function GetStream() As ReferencedStream
            Dim num As Integer = 0
            Dim stream2 As ReferencedStream = Nothing
            Dim fullPath As String = Path.GetFullPath((Me.LogFileName & ".log"))
            Do While ((stream2 Is Nothing) AndAlso (num < &H7FFFFFFF))
                Dim str2 As String
                If (num = 0) Then
                    str2 = Path.GetFullPath((Me.LogFileName & ".log"))
                Else
                    str2 = Path.GetFullPath((Me.LogFileName & "-" & num.ToString(CultureInfo.InvariantCulture) & ".log"))
                End If
                Dim key As String = str2.ToUpper(CultureInfo.InvariantCulture)
                Dim streams As Object = FileLogTraceListener.m_Streams
                SyncLock streams
                    If FileLogTraceListener.m_Streams.ContainsKey(key) Then
                        stream2 = FileLogTraceListener.m_Streams.Item(key)
                        If Not stream2.IsInUse Then
                            FileLogTraceListener.m_Streams.Remove(key)
                            stream2 = Nothing
                        Else
                            If Me.Append Then
                                Call New FileIOPermission(FileIOPermissionAccess.Write, str2).Demand()
                                stream2.AddReference()
                                Me.m_FullFileName = str2
                                Return stream2
                            End If
                            num += 1
                            stream2 = Nothing
                            Continue Do
                        End If
                    End If
                    Dim fileEncoding As Encoding = Me.Encoding
                    Try
                        If Me.Append Then
                            fileEncoding = Me.GetFileEncoding(str2)
                            If (fileEncoding Is Nothing) Then
                                fileEncoding = Me.Encoding
                            End If
                        End If
                        Dim stream As New StreamWriter(str2, Me.Append, fileEncoding)
                        stream2 = New ReferencedStream(stream)
                        stream2.AddReference()
                        FileLogTraceListener.m_Streams.Add(key, stream2)
                        Me.m_FullFileName = str2
                        Return stream2
                    Catch exception As IOException
                    End Try
                    num += 1
                    Continue Do
                End SyncLock
            Loop
            Dim placeHolders As String() = New String() {fullPath}
            Throw ExceptionUtils.GetInvalidOperationException("ApplicationLog_ExhaustedPossibleStreamNames", placeHolders)
        End Function

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Protected Overrides Function GetSupportedAttributes() As String()
            Return Me.m_SupportedAttributes
        End Function

        Private Sub HandleDateChange()
            If (Me.LogFileCreationSchedule = LogFileCreationScheduleOption.Daily) Then
                If Me.DayChanged Then
                    Me.m_Days = DateAndTime.Now.Date.Subtract(Me.m_Day).Days
                    Me.CloseCurrentStream()
                End If
            ElseIf ((Me.LogFileCreationSchedule = LogFileCreationScheduleOption.Weekly) AndAlso Me.WeekChanged) Then
                Me.CloseCurrentStream()
            End If
        End Sub

        Private Function ResourcesAvailable(newEntrySize As Long) As Boolean
            If ((Me.ListenerStream.FileSize + newEntrySize) > Me.MaxFileSize) Then
                If (Me.DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.ThrowException) Then
                    Throw New InvalidOperationException(Utils.GetResourceString("ApplicationLog_FileExceedsMaximumSize"))
                End If
                Return False
            End If
            If ((Me.GetFreeDiskSpace - newEntrySize) < Me.ReserveDiskSpace) Then
                If (Me.DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.ThrowException) Then
                    Throw New InvalidOperationException(Utils.GetResourceString("ApplicationLog_ReservedSpaceEncroached"))
                End If
                Return False
            End If
            Return True
        End Function

        Private Shared Function StackToString(stack As Stack) As String
            Dim enumerator As IEnumerator = Nothing
            Dim length As Integer = ", ".Length
            Dim builder As New StringBuilder
            Try
                enumerator = stack.GetEnumerator
                Do While enumerator.MoveNext
                    builder.Append((enumerator.Current.ToString & ", "))
                Loop
            Finally
                If TypeOf enumerator Is IDisposable Then
                    TryCast(enumerator, IDisposable).Dispose()
                End If
            End Try
            builder.Replace("""", """""")
            If (builder.Length >= length) Then
                builder.Remove((builder.Length - length), length)
            End If
            Return ("""" & builder.ToString & """")
        End Function

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Public Overrides Sub TraceData(eventCache As TraceEventCache, source As String, eventType As TraceEventType, id As Integer, data As Object)
            Dim message As String = ""
            If (Not data Is Nothing) Then
                message = data.ToString
            End If
            Me.TraceEvent(eventCache, source, eventType, id, message)
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Public Overrides Sub TraceData(eventCache As TraceEventCache, source As String, eventType As TraceEventType, id As Integer, ParamArray data As Object())
            Dim builder As New StringBuilder
            If (Not data Is Nothing) Then
                Dim num As Integer = (data.Length - 1)
                Dim num2 As Integer = num
                Dim i As Integer = 0
                Do While (i <= num2)
                    builder.Append(data(i).ToString)
                    If (i <> num) Then
                        builder.Append(Me.Delimiter)
                    End If
                    i += 1
                Loop
            End If
            Me.TraceEvent(eventCache, source, eventType, id, builder.ToString)
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Public Overrides Sub TraceEvent(eventCache As TraceEventCache, source As String, eventType As TraceEventType, id As Integer, message As String)
            If ((MyBase.Filter Is Nothing) OrElse MyBase.Filter.ShouldTrace(eventCache, source, eventType, id, message, Nothing, Nothing, Nothing)) Then
                Dim builder As New StringBuilder
                builder.Append((source & Me.Delimiter))
                builder.Append(([Enum].GetName(GetType(TraceEventType), eventType) & Me.Delimiter))
                builder.Append((id.ToString(CultureInfo.InvariantCulture) & Me.Delimiter))
                builder.Append(message)
                If ((MyBase.TraceOutputOptions And TraceOptions.Callstack) = TraceOptions.Callstack) Then
                    builder.Append((Me.Delimiter & eventCache.Callstack))
                End If
                If ((MyBase.TraceOutputOptions And TraceOptions.LogicalOperationStack) = TraceOptions.LogicalOperationStack) Then
                    builder.Append((Me.Delimiter & FileLogTraceListener.StackToString(eventCache.LogicalOperationStack)))
                End If
                If ((MyBase.TraceOutputOptions And TraceOptions.DateTime) = TraceOptions.DateTime) Then
                    builder.Append((Me.Delimiter & eventCache.DateTime.ToString("u", CultureInfo.InvariantCulture)))
                End If
                If ((MyBase.TraceOutputOptions And TraceOptions.ProcessId) = TraceOptions.ProcessId) Then
                    builder.Append((Me.Delimiter & eventCache.ProcessId.ToString(CultureInfo.InvariantCulture)))
                End If
                If ((MyBase.TraceOutputOptions And TraceOptions.ThreadId) = TraceOptions.ThreadId) Then
                    builder.Append((Me.Delimiter & eventCache.ThreadId))
                End If
                If ((MyBase.TraceOutputOptions And TraceOptions.Timestamp) = TraceOptions.Timestamp) Then
                    builder.Append((Me.Delimiter & eventCache.Timestamp.ToString(CultureInfo.InvariantCulture)))
                End If
                If Me.IncludeHostName Then
                    builder.Append((Me.Delimiter & Me.HostName))
                End If
                Me.WriteLine(builder.ToString)
            End If
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Public Overrides Sub TraceEvent(eventCache As TraceEventCache, source As String, eventType As TraceEventType, id As Integer, format As String, ParamArray args As Object())
            Dim message As String = Nothing
            If (Not args Is Nothing) Then
                message = String.Format(CultureInfo.InvariantCulture, format, args)
            Else
                message = format
            End If
            Me.TraceEvent(eventCache, source, eventType, id, message)
        End Sub

        Private Sub ValidateDiskSpaceExhaustedOptionEnumValue(value As DiskSpaceExhaustedOption, paramName As String)
            If ((value < DiskSpaceExhaustedOption.ThrowException) OrElse (value > DiskSpaceExhaustedOption.DiscardMessages)) Then
                Throw New InvalidEnumArgumentException(paramName, CInt(value), GetType(DiskSpaceExhaustedOption))
            End If
        End Sub

        Private Sub ValidateLogFileCreationScheduleOptionEnumValue(value As LogFileCreationScheduleOption, paramName As String)
            If ((value < LogFileCreationScheduleOption.None) OrElse (value > LogFileCreationScheduleOption.Weekly)) Then
                Throw New InvalidEnumArgumentException(paramName, CInt(value), GetType(LogFileCreationScheduleOption))
            End If
        End Sub

        Private Sub ValidateLogFileLocationEnumValue(value As LogFileLocation, paramName As String)
            If ((value < LogFileLocation.TempDirectory) OrElse (value > LogFileLocation.Custom)) Then
                Throw New InvalidEnumArgumentException(paramName, CInt(value), GetType(LogFileLocation))
            End If
        End Sub

        Private Function WeekChanged() As Boolean
            Return (DateTime.Compare(Me.m_FirstDayOfWeek.Date, FileLogTraceListener.GetFirstDayOfWeek(DateAndTime.Now.Date)) > 0)
        End Function

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Public Overrides Sub Write(message As String)
            Try
                Me.HandleDateChange()
                Dim byteCount As Long = Me.Encoding.GetByteCount(message)
                If Me.ResourcesAvailable(byteCount) Then
                    Me.ListenerStream.Write(message)
                    If Me.AutoFlush Then
                        Me.ListenerStream.Flush()
                    End If
                End If
            Catch exception1 As Exception
                Me.CloseCurrentStream()
                Throw
            End Try
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Synchronization:=True)>
        Public Overrides Sub WriteLine(message As String)
            Try
                Me.HandleDateChange()
                Dim byteCount As Long = Me.Encoding.GetByteCount((message & ChrW(13) & ChrW(10)))
                If Me.ResourcesAvailable(byteCount) Then
                    Me.ListenerStream.WriteLine(message)
                    If Me.AutoFlush Then
                        Me.ListenerStream.Flush()
                    End If
                End If
            Catch exception1 As Exception
                Me.CloseCurrentStream()
                Throw
            End Try
        End Sub


        ' Properties
        Public Property Append As Boolean
            Get
                If (Not Me.m_PropertiesSet.Item(0) AndAlso MyBase.Attributes.ContainsKey("append")) Then
                    Me.Append = Convert.ToBoolean(MyBase.Attributes.Item("append"), CultureInfo.InvariantCulture)
                End If
                Return Me.m_Append
            End Get
            <SecuritySafeCritical>
            Set(value As Boolean)
                Me.DemandWritePermission()

                If (value <> Me.m_Append) Then
                    Me.CloseCurrentStream()
                End If
                Me.m_Append = value
                Me.m_PropertiesSet.Item(0) = True
            End Set
        End Property

        Public Property AutoFlush As Boolean
            Get
                If (Not Me.m_PropertiesSet.Item(1) AndAlso MyBase.Attributes.ContainsKey("autoflush")) Then
                    Me.AutoFlush = Convert.ToBoolean(MyBase.Attributes.Item("autoflush"), CultureInfo.InvariantCulture)
                End If
                Return Me.m_AutoFlush
            End Get
            <SecuritySafeCritical>
            Set(value As Boolean)
                Me.DemandWritePermission()
                Me.m_AutoFlush = value
                Me.m_PropertiesSet.Item(1) = True
            End Set
        End Property

        Public Property BaseFileName As String
            Get
                If (Not Me.m_PropertiesSet.Item(2) AndAlso MyBase.Attributes.ContainsKey("basefilename")) Then
                    Me.BaseFileName = MyBase.Attributes.Item("basefilename")
                End If
                Return Me.m_BaseFileName
            End Get
            Set(value As String)
                If (value = "") Then
                    Throw ExceptionUtils.GetArgumentNullException("value", "ApplicationLogBaseNameNull", New String(0 - 1) {})
                End If
                Path.GetFullPath(value)
                If (String.Compare(value, Me.m_BaseFileName, StringComparison.OrdinalIgnoreCase) <> 0) Then
                    Me.CloseCurrentStream()
                    Me.m_BaseFileName = value
                End If
                Me.m_PropertiesSet.Item(2) = True
            End Set
        End Property

        Public Property CustomLocation As String
            <SecuritySafeCritical>
            Get
                If (Not Me.m_PropertiesSet.Item(3) AndAlso MyBase.Attributes.ContainsKey("customlocation")) Then
                    Me.CustomLocation = MyBase.Attributes.Item("customlocation")
                End If
                Dim fullPath As String = Path.GetFullPath(Me.m_CustomLocation)
                Call New FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullPath).Demand()
                Return fullPath
            End Get
            Set(value As String)
                Dim fullPath As String = Path.GetFullPath(value)
                If Not Directory.Exists(fullPath) Then
                    Directory.CreateDirectory(fullPath)
                End If
                If ((Me.Location = LogFileLocation.Custom) And (String.Compare(fullPath, Me.m_CustomLocation, StringComparison.OrdinalIgnoreCase) > 0)) Then
                    Me.CloseCurrentStream()
                End If
                Me.Location = LogFileLocation.Custom
                Me.m_CustomLocation = fullPath
                Me.m_PropertiesSet.Item(3) = True
            End Set
        End Property

        Public Property Delimiter As String
            Get
                If (Not Me.m_PropertiesSet.Item(4) AndAlso MyBase.Attributes.ContainsKey("delimiter")) Then
                    Me.Delimiter = MyBase.Attributes.Item("delimiter")
                End If
                Return Me.m_Delimiter
            End Get
            Set(value As String)
                Me.m_Delimiter = value
                Me.m_PropertiesSet.Item(4) = True
            End Set
        End Property

        Public Property DiskSpaceExhaustedBehavior As DiskSpaceExhaustedOption
            Get
                If (Not Me.m_PropertiesSet.Item(5) AndAlso MyBase.Attributes.ContainsKey("diskspaceexhaustedbehavior")) Then
                    Dim converter As TypeConverter = TypeDescriptor.GetConverter(GetType(DiskSpaceExhaustedOption))
                    Me.DiskSpaceExhaustedBehavior = DirectCast(converter.ConvertFromInvariantString(MyBase.Attributes.Item("diskspaceexhaustedbehavior")), DiskSpaceExhaustedOption)
                End If
                Return Me.m_DiskSpaceExhaustedBehavior
            End Get
            <SecuritySafeCritical>
            Set(value As DiskSpaceExhaustedOption)
                Me.DemandWritePermission()
                Me.ValidateDiskSpaceExhaustedOptionEnumValue(value, "value")
                Me.m_DiskSpaceExhaustedBehavior = value
                Me.m_PropertiesSet.Item(5) = True
            End Set
        End Property

        Public Property Encoding As Encoding
            Get
                If (Not Me.m_PropertiesSet.Item(6) AndAlso MyBase.Attributes.ContainsKey("encoding")) Then
                    Me.Encoding = Encoding.GetEncoding(MyBase.Attributes.Item("encoding"))
                End If
                Return Me.m_Encoding
            End Get
            Set(value As Encoding)
                If (value Is Nothing) Then
                    Throw ExceptionUtils.GetArgumentNullException("value")
                End If
                Me.m_Encoding = value
                Me.m_PropertiesSet.Item(6) = True
            End Set
        End Property

        Public ReadOnly Property FullLogFileName As String
            <SecuritySafeCritical>
            Get
                Me.EnsureStreamIsOpen()
                Dim fullFileName As String = Me.m_FullFileName
                Call New FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullFileName).Demand()
                Return fullFileName
            End Get
        End Property

        Private ReadOnly Property HostName As String
            Get
                If (Me.m_HostName = "") Then
                    Me.m_HostName = Environment.MachineName
                End If
                Return Me.m_HostName
            End Get
        End Property

        Public Property IncludeHostName As Boolean
            Get
                If (Not Me.m_PropertiesSet.Item(7) AndAlso MyBase.Attributes.ContainsKey("includehostname")) Then
                    Me.IncludeHostName = Convert.ToBoolean(MyBase.Attributes.Item("includehostname"), CultureInfo.InvariantCulture)
                End If
                Return Me.m_IncludeHostName
            End Get
            <SecuritySafeCritical>
            Set(value As Boolean)
                Me.DemandWritePermission()
                Me.m_IncludeHostName = value
                Me.m_PropertiesSet.Item(7) = True
            End Set
        End Property

        Private ReadOnly Property ListenerStream As ReferencedStream
            Get
                Me.EnsureStreamIsOpen()
                Return Me.m_Stream
            End Get
        End Property

        Public Property Location As LogFileLocation
            Get
                If (Not Me.m_PropertiesSet.Item(8) AndAlso MyBase.Attributes.ContainsKey("location")) Then
                    Dim converter As TypeConverter = TypeDescriptor.GetConverter(GetType(LogFileLocation))
                    Me.Location = DirectCast(converter.ConvertFromInvariantString(MyBase.Attributes.Item("location")), LogFileLocation)
                End If
                Return Me.m_Location
            End Get
            Set(value As LogFileLocation)
                Me.ValidateLogFileLocationEnumValue(value, "value")
                If (Me.m_Location <> value) Then
                    Me.CloseCurrentStream()
                End If
                Me.m_Location = value
                Me.m_PropertiesSet.Item(8) = True
            End Set
        End Property

        Public Property LogFileCreationSchedule As LogFileCreationScheduleOption
            Get
                If (Not Me.m_PropertiesSet.Item(9) AndAlso MyBase.Attributes.ContainsKey("logfilecreationschedule")) Then
                    Dim converter As TypeConverter = TypeDescriptor.GetConverter(GetType(LogFileCreationScheduleOption))
                    Me.LogFileCreationSchedule = DirectCast(converter.ConvertFromInvariantString(MyBase.Attributes.Item("logfilecreationschedule")), LogFileCreationScheduleOption)
                End If
                Return Me.m_LogFileDateStamp
            End Get
            Set(value As LogFileCreationScheduleOption)
                Me.ValidateLogFileCreationScheduleOptionEnumValue(value, "value")
                If (value <> Me.m_LogFileDateStamp) Then
                    Me.CloseCurrentStream()
                    Me.m_LogFileDateStamp = value
                End If
                Me.m_PropertiesSet.Item(9) = True
            End Set
        End Property

        Private ReadOnly Property LogFileName As String
            Get
                Dim tempPath As String
                Select Case Me.Location
                    Case LogFileLocation.TempDirectory
                        tempPath = Path.GetTempPath
                        Exit Select
                    Case LogFileLocation.LocalUserApplicationDirectory
                        tempPath = Application.UserAppDataPath
                        Exit Select
                    Case LogFileLocation.CommonApplicationDirectory
                        tempPath = Application.CommonAppDataPath
                        Exit Select
                    Case LogFileLocation.ExecutableDirectory
                        tempPath = Path.GetDirectoryName(Application.ExecutablePath)
                        Exit Select
                    Case LogFileLocation.Custom
                        If (Me.CustomLocation <> "") Then
                            tempPath = Me.CustomLocation
                            Exit Select
                        End If
                        tempPath = Application.UserAppDataPath
                        Exit Select
                    Case Else
                        tempPath = Application.UserAppDataPath
                        Exit Select
                End Select
                Dim baseFileName As String = Me.BaseFileName
                Select Case Me.LogFileCreationSchedule
                    Case LogFileCreationScheduleOption.Daily
                        baseFileName = (baseFileName & "-" & DateAndTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                        Exit Select
                    Case LogFileCreationScheduleOption.Weekly
                        Me.m_FirstDayOfWeek = DateAndTime.Now.AddDays(CDbl((0 - DateAndTime.Now.DayOfWeek)))
                        baseFileName = (baseFileName & "-" & Me.m_FirstDayOfWeek.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                        Exit Select
                End Select
                Return Path.Combine(tempPath, baseFileName)
            End Get
        End Property

        Public Property MaxFileSize As Long
            Get
                If (Not Me.m_PropertiesSet.Item(10) AndAlso MyBase.Attributes.ContainsKey("maxfilesize")) Then
                    Me.MaxFileSize = Convert.ToInt64(MyBase.Attributes.Item("maxfilesize"), CultureInfo.InvariantCulture)
                End If
                Return Me.m_MaxFileSize
            End Get
            <SecuritySafeCritical>
            Set(value As Long)
                Me.DemandWritePermission()

                If (value < &H3E8) Then
                    Dim placeHolders As String() = New String() {"MaxFileSize"}
                    Throw ExceptionUtils.GetArgumentExceptionWithArgName("value", "ApplicationLogNumberTooSmall", placeHolders)
                End If
                Me.m_MaxFileSize = value
                Me.m_PropertiesSet.Item(10) = True
            End Set
        End Property

        Public Property ReserveDiskSpace As Long
            Get
                If (Not Me.m_PropertiesSet.Item(11) AndAlso MyBase.Attributes.ContainsKey("reservediskspace")) Then
                    Me.ReserveDiskSpace = Convert.ToInt64(MyBase.Attributes.Item("reservediskspace"), CultureInfo.InvariantCulture)
                End If
                Return Me.m_ReserveDiskSpace
            End Get
            <SecuritySafeCritical>
            Set(value As Long)
                Me.DemandWritePermission()

                If (value < 0) Then
                    Dim placeHolders As String() = New String() {"ReserveDiskSpace"}
                    Throw ExceptionUtils.GetArgumentExceptionWithArgName("value", "ApplicationLog_NegativeNumber", placeHolders)
                End If
                Me.m_ReserveDiskSpace = value
                Me.m_PropertiesSet.Item(11) = True
            End Set
        End Property


        ' Fields
        Private Const APPEND_INDEX As Integer = 0
        Private Const AUTOFLUSH_INDEX As Integer = 1
        Private Const BASEFILENAME_INDEX As Integer = 2
        Private Const CUSTOMLOCATION_INDEX As Integer = 3
        Private Const DATE_FORMAT As String = "yyyy-MM-dd"
        Private Const DEFAULT_NAME As String = "FileLogTraceListener"
        Private Const DELIMITER_INDEX As Integer = 4
        Private Const DISKSPACEEXHAUSTEDBEHAVIOR_INDEX As Integer = 5
        Private Const ENCODING_INDEX As Integer = 6
        Private Const FILE_EXTENSION As String = ".log"
        Private Const INCLUDEHOSTNAME_INDEX As Integer = 7
        Private Const KEY_APPEND As String = "append"
        Private Const KEY_APPEND_PASCAL As String = "Append"
        Private Const KEY_AUTOFLUSH As String = "autoflush"
        Private Const KEY_AUTOFLUSH_CAMEL As String = "autoFlush"
        Private Const KEY_AUTOFLUSH_PASCAL As String = "AutoFlush"
        Private Const KEY_BASEFILENAME As String = "basefilename"
        Private Const KEY_BASEFILENAME_CAMEL As String = "baseFilename"
        Private Const KEY_BASEFILENAME_CAMEL_ALT As String = "baseFileName"
        Private Const KEY_BASEFILENAME_PASCAL As String = "BaseFilename"
        Private Const KEY_BASEFILENAME_PASCAL_ALT As String = "BaseFileName"
        Private Const KEY_CUSTOMLOCATION As String = "customlocation"
        Private Const KEY_CUSTOMLOCATION_CAMEL As String = "customLocation"
        Private Const KEY_CUSTOMLOCATION_PASCAL As String = "CustomLocation"
        Private Const KEY_DELIMITER As String = "delimiter"
        Private Const KEY_DELIMITER_PASCAL As String = "Delimiter"
        Private Const KEY_DISKSPACEEXHAUSTEDBEHAVIOR As String = "diskspaceexhaustedbehavior"
        Private Const KEY_DISKSPACEEXHAUSTEDBEHAVIOR_CAMEL As String = "diskSpaceExhaustedBehavior"
        Private Const KEY_DISKSPACEEXHAUSTEDBEHAVIOR_PASCAL As String = "DiskSpaceExhaustedBehavior"
        Private Const KEY_ENCODING As String = "encoding"
        Private Const KEY_ENCODING_PASCAL As String = "Encoding"
        Private Const KEY_INCLUDEHOSTNAME As String = "includehostname"
        Private Const KEY_INCLUDEHOSTNAME_CAMEL As String = "includeHostName"
        Private Const KEY_INCLUDEHOSTNAME_PASCAL As String = "IncludeHostName"
        Private Const KEY_LOCATION As String = "location"
        Private Const KEY_LOCATION_PASCAL As String = "Location"
        Private Const KEY_LOGFILECREATIONSCHEDULE As String = "logfilecreationschedule"
        Private Const KEY_LOGFILECREATIONSCHEDULE_CAMEL As String = "logFileCreationSchedule"
        Private Const KEY_LOGFILECREATIONSCHEDULE_PASCAL As String = "LogFileCreationSchedule"
        Private Const KEY_MAXFILESIZE As String = "maxfilesize"
        Private Const KEY_MAXFILESIZE_CAMEL As String = "maxFileSize"
        Private Const KEY_MAXFILESIZE_PASCAL As String = "MaxFileSize"
        Private Const KEY_RESERVEDISKSPACE As String = "reservediskspace"
        Private Const KEY_RESERVEDISKSPACE_CAMEL As String = "reserveDiskSpace"
        Private Const KEY_RESERVEDISKSPACE_PASCAL As String = "ReserveDiskSpace"
        Private Const LOCATION_INDEX As Integer = 8
        Private Const LOGFILECREATIONSCHEDULE_INDEX As Integer = 9
        Private m_Append As Boolean
        Private m_AutoFlush As Boolean
        Private m_BaseFileName As String
        Private m_CustomLocation As String
        Private m_Day As DateTime
        Private m_Days As Integer
        Private m_Delimiter As String
        Private m_DiskSpaceExhaustedBehavior As DiskSpaceExhaustedOption
        Private m_Encoding As Encoding
        Private m_FirstDayOfWeek As DateTime
        Private m_FullFileName As String
        Private m_HostName As String
        Private m_IncludeHostName As Boolean
        Private m_Location As LogFileLocation
        Private m_LogFileDateStamp As LogFileCreationScheduleOption
        Private m_MaxFileSize As Long
        Private m_PropertiesSet As BitArray
        Private m_ReserveDiskSpace As Long
        Private m_Stream As ReferencedStream
        Private Shared m_Streams As Dictionary(Of String, ReferencedStream) = New Dictionary(Of String, ReferencedStream)
        Private m_SupportedAttributes As String()
        Private Const MAX_OPEN_ATTEMPTS As Integer = &H7FFFFFFF
        Private Const MAXFILESIZE_INDEX As Integer = 10
        Private Const MIN_FILE_SIZE As Integer = &H3E8
        Private Const PROPERTY_COUNT As Integer = 12
        Private Const RESERVEDISKSPACE_INDEX As Integer = 11
        Private Const STACK_DELIMITER As String = ", "

        ' Nested Types
        Friend Class ReferencedStream
            Implements IDisposable
            ' Methods
            Friend Sub New(stream As StreamWriter)
                Me.m_Stream = stream
            End Sub

            Friend Sub AddReference()
                Dim syncObject As Object = Me.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    Dim numRef As Integer
                    numRef = CInt(AddressOf Me.m_ReferenceCount) = (numRef + 1)
                End SyncLock
            End Sub

            Friend Sub CloseStream()
                Dim syncObject As Object = Me.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    Try
                        Dim numRef As Integer
                        numRef = CInt(AddressOf Me.m_ReferenceCount) = (numRef - 1)
                        Me.m_Stream.Flush()
                    Finally
                        If (Me.m_ReferenceCount <= 0) Then
                            Me.m_Stream.Close()
                            Me.m_Stream = Nothing
                        End If
                    End Try
                End SyncLock
            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                Me.Dispose(True)
                GC.SuppressFinalize(Me)
            End Sub

            Private Sub Dispose(disposing As Boolean)
                If (disposing AndAlso Not Me.m_Disposed) Then
                    If (Not Me.m_Stream Is Nothing) Then
                        Me.m_Stream.Close()
                    End If
                    Me.m_Disposed = True
                End If
            End Sub

            Protected Overrides Sub Finalize()
                Me.Dispose(False)
                MyBase.Finalize()
            End Sub

            Friend Sub Flush()
                Dim syncObject As Object = Me.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    Me.m_Stream.Flush()
                End SyncLock
            End Sub

            Friend Sub Write(message As String)
                Dim syncObject As Object = Me.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    Me.m_Stream.Write(message)
                End SyncLock
            End Sub

            Friend Sub WriteLine(message As String)
                Dim syncObject As Object = Me.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    Me.m_Stream.WriteLine(message)
                End SyncLock
            End Sub


            ' Properties
            Friend ReadOnly Property FileSize As Long
                Get
                    Return Me.m_Stream.BaseStream.Length
                End Get
            End Property

            Friend ReadOnly Property IsInUse As Boolean
                Get
                    Return Not (Me.m_Stream Is Nothing)
                End Get
            End Property


            ' Fields
            Private m_Disposed As Boolean = False
            Private m_ReferenceCount As Integer = 0
            Private m_Stream As StreamWriter
            Private m_SyncObject As Object = New Object
        End Class
    End Class
End Namespace

