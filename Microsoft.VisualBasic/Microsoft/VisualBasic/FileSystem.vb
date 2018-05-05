Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.CompilerServices

Namespace Microsoft.VisualBasic
    <StandardModule, SecurityCritical, HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public NotInheritable Class FileSystem
        ' Methods
        Private Shared Sub AddFileToList(oAssemblyData As AssemblyData, FileNumber As Integer, oFile As VB6File)
            If (oFile Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(&H33)
            End If
            oFile.OpenFile
            oAssemblyData.SetChannelObj(FileNumber, oFile)
        End Sub

        Public Shared Sub ChDir(Path As String)
            Path = Strings.RTrim(Path)
            If ((Path Is Nothing) OrElse (Path.Length = 0)) Then
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty")), &H34)
            End If
            If (Path = "\") Then
                Path = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory)
            End If
            Try
                Directory.SetCurrentDirectory(Path)
            Catch exception As FileNotFoundException
                Dim args As String() = New String() {Path}
                Throw ExceptionUtils.VbMakeException(New FileNotFoundException(Utils.GetResourceString("FileSystem_PathNotFound1", args)), &H4C)
            End Try
        End Sub

        Public Shared Sub ChDrive(Drive As Char)
            Drive = Char.ToUpper(Drive, CultureInfo.InvariantCulture)
            If ((Drive < "A"c) OrElse (Drive > "Z"c)) Then
                Dim args As String() = New String() {"Drive"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If Not FileSystem.UnsafeValidDrive(Drive) Then
                Dim textArray2 As String() = New String() {Conversions.ToString(Drive)}
                Throw ExceptionUtils.VbMakeException(New IOException(Utils.GetResourceString("FileSystem_DriveNotFound1", textArray2)), &H44)
            End If
            Directory.SetCurrentDirectory((Conversions.ToString(Drive) & Conversions.ToString(Path.VolumeSeparatorChar)))
        End Sub

        Public Shared Sub ChDrive(Drive As String)
            If ((Not Drive Is Nothing) AndAlso (Drive.Length <> 0)) Then
                FileSystem.ChDrive(Drive.Chars(0))
            End If
        End Sub

        Friend Shared Function CheckFileOpen(oAssemblyData As AssemblyData, sPath As String, NewFileMode As OpenModeTypes) As Boolean
            Dim num2 As Integer = &HFF
            Dim i As Integer = 1
            Do While (i <= num2)
                Dim channelOrNull As VB6File = FileSystem.GetChannelOrNull(oAssemblyData, i)
                If (Not channelOrNull Is Nothing) Then
                    Dim mode As OpenMode = channelOrNull.GetMode
                    If (String.Compare(sPath, channelOrNull.GetAbsolutePath, StringComparison.OrdinalIgnoreCase) = 0) Then
                        If (NewFileMode = OpenModeTypes.Any) Then
                            Return True
                        End If
                        If (((NewFileMode Or DirectCast(CInt(mode), OpenModeTypes)) <> OpenModeTypes.Input) AndAlso ((((NewFileMode Or DirectCast(CInt(mode), OpenModeTypes)) Or OpenModeTypes.Binary) Or OpenModeTypes.Random) <> (OpenModeTypes.Binary Or OpenModeTypes.Random))) Then
                            Return True
                        End If
                    End If
                End If
                i += 1
            Loop
            Return False
        End Function

        Private Shared Sub CheckInputCapable(oFile As VB6File)
            If Not oFile.CanInput Then
                Throw ExceptionUtils.VbMakeException(&H36)
            End If
        End Sub

        Friend Shared Sub CloseAllFiles(oAssemblyData As AssemblyData)
            Dim fileNumber As Integer = 1
            Do
                FileSystem.InternalCloseFile(oAssemblyData, fileNumber)
                fileNumber += 1
            Loop While (fileNumber <= &HFF)
        End Sub

        Friend Shared Sub CloseAllFiles(assem As Assembly)
            FileSystem.CloseAllFiles(ProjectData.GetProjectData.GetAssemblyData(assem))
        End Sub

        Public Shared Function CurDir() As String
            Return Directory.GetCurrentDirectory
        End Function

        Public Shared Function CurDir(Drive As Char) As String
            Drive = Char.ToUpper(Drive, CultureInfo.InvariantCulture)
            If ((Drive < "A"c) OrElse (Drive > "Z"c)) Then
                Dim args As String() = New String() {"Drive"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args)), &H44)
            End If
            Dim fullPath As String = Path.GetFullPath((Conversions.ToString(Drive) & Conversions.ToString(Path.VolumeSeparatorChar) & "."))
            If Not FileSystem.UnsafeValidDrive(Drive) Then
                Dim textArray2 As String() = New String() {Conversions.ToString(Drive)}
                Throw ExceptionUtils.VbMakeException(New IOException(Utils.GetResourceString("FileSystem_DriveNotFound1", textArray2)), &H44)
            End If
            Return fullPath
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function Dir() As String
            Return IOUtils.FindNextFile(Assembly.GetCallingAssembly)
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function Dir(PathName As String, Optional Attributes As FileAttribute = 0) As String
            If (attributes = FileAttribute.Volume) Then
                Dim lpVolumeNameBuffer As New StringBuilder(&H100)
                Dim lpRootPathName As String = Nothing
                If (PathName.Length > 0) Then
                    lpRootPathName = Path.GetPathRoot(PathName)
                    If (lpRootPathName.Chars((lpRootPathName.Length - 1)) <> Path.DirectorySeparatorChar) Then
                        lpRootPathName = (lpRootPathName & Conversions.ToString(Path.DirectorySeparatorChar))
                    End If
                End If
                Dim lpVolumeSerialNumber As Integer = 0
                Dim lpMaximumComponentLength As Integer = 0
                Dim lpFileSystemFlags As Integer = 0
                If (NativeMethods.GetVolumeInformation(lpRootPathName, lpVolumeNameBuffer, &H100, lpVolumeSerialNumber, lpMaximumComponentLength, lpFileSystemFlags, New IntPtr, 0) <> 0) Then
                    Return lpVolumeNameBuffer.ToString
                End If
                Return ""
            End If
            Dim attributes As FileAttributes = (DirectCast(attributes, FileAttributes) Or FileAttributes.Normal)
            Return IOUtils.FindFirstFile(Assembly.GetCallingAssembly, PathName, attributes)
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function EOF(FileNumber As Integer) As Boolean
            Return FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).EOF
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function FileAttr(FileNumber As Integer) As OpenMode
            Return FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).GetMode
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileClose(ParamArray FileNumbers As Integer())
            Try
                Dim callingAssembly As Assembly = Assembly.GetCallingAssembly
                Dim assemblyData As AssemblyData = ProjectData.GetProjectData.GetAssemblyData(callingAssembly)
                If ((FileNumbers Is Nothing) OrElse (FileNumbers.Length = 0)) Then
                    FileSystem.CloseAllFiles(assemblyData)
                Else
                    Dim upperBound As Integer = FileNumbers.GetUpperBound(0)
                    Dim i As Integer = 0
                    Do While (i <= upperBound)
                        FileSystem.InternalCloseFile(assemblyData, FileNumbers(i))
                        i += 1
                    Loop
                End If
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileCopy(Source As String, Destination As String)
            If ((Source Is Nothing) OrElse (Source.Length = 0)) Then
                Dim args As String() = New String() {"Source"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty1", args)), &H34)
            End If
            If ((Destination Is Nothing) OrElse (Destination.Length = 0)) Then
                Dim textArray2 As String() = New String() {"Destination"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty1", textArray2)), &H34)
            End If
            If FileSystem.PathContainsWildcards(Source) Then
                Dim textArray3 As String() = New String() {"Source"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3)), &H34)
            End If
            If FileSystem.PathContainsWildcards(Destination) Then
                Dim textArray4 As String() = New String() {"Destination"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray4)), &H34)
            End If
            Dim assemblyData As AssemblyData = ProjectData.GetProjectData.GetAssemblyData(Assembly.GetCallingAssembly)
            If FileSystem.CheckFileOpen(assemblyData, Destination, OpenModeTypes.Output) Then
                Dim textArray5 As String() = New String() {Destination}
                Throw ExceptionUtils.VbMakeException(New IOException(Utils.GetResourceString("FileSystem_FileAlreadyOpen1", textArray5)), &H37)
            End If
            If FileSystem.CheckFileOpen(assemblyData, Source, OpenModeTypes.Input) Then
                Dim textArray6 As String() = New String() {Source}
                Throw ExceptionUtils.VbMakeException(New IOException(Utils.GetResourceString("FileSystem_FileAlreadyOpen1", textArray6)), &H37)
            End If
            Try
                File.Copy(Source, Destination, True)
                File.SetAttributes(Destination, FileAttributes.Archive)
            Catch exception As FileNotFoundException
                Throw ExceptionUtils.VbMakeException(exception, &H35)
            Catch exception2 As IOException
                Throw ExceptionUtils.VbMakeException(exception2, &H37)
            Catch exception3 As Exception
                Throw exception3
            End Try
        End Sub

        Public Shared Function FileDateTime(PathName As String) As DateTime
            If FileSystem.PathContainsWildcards(PathName) Then
                Dim textArray1 As String() = New String() {"PathName"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray1)), &H34)
            End If
            If File.Exists(PathName) Then
                Return New FileInfo(PathName).LastWriteTime
            End If
            Dim args As String() = New String() {PathName}
            Throw New FileNotFoundException(Utils.GetResourceString("FileSystem_FileNotFound1", args))
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Boolean, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Byte, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Char, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As DateTime, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Decimal, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Double, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Short, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Integer, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Long, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Single, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As ValueType, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As String, Optional RecordNumber As Long = -1, Optional StringIsFixedLength As Boolean = False)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber, StringIsFixedLength)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGet(FileNumber As Integer, ByRef Value As Array, Optional RecordNumber As Long = -1, Optional ArrayIsDynamic As Boolean = False, Optional StringIsFixedLength As Boolean = False)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Get(Value, RecordNumber, ArrayIsDynamic, StringIsFixedLength)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileGetObject(FileNumber As Integer, ByRef Value As Object, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).GetObject(Value, RecordNumber, True)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        Public Shared Function FileLen(PathName As String) As Long
            If File.Exists(PathName) Then
                Return New FileInfo(PathName).Length
            End If
            Dim args As String() = New String() {PathName}
            Throw New FileNotFoundException(Utils.GetResourceString("FileSystem_FileNotFound1", args))
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileOpen(FileNumber As Integer, FileName As String, Mode As OpenMode, Optional Access As OpenAccess = -1, Optional Share As OpenShare = -1, Optional RecordLength As Integer = -1)
            Try
                FileSystem.ValidateMode(Mode)
                FileSystem.ValidateAccess(Access)
                FileSystem.ValidateShare(Share)
                If ((FileNumber < 1) OrElse (FileNumber > &HFF)) Then
                    Throw ExceptionUtils.VbMakeException(&H34)
                End If
                FileSystem.vbIOOpenFile(Assembly.GetCallingAssembly, FileNumber, FileName, Mode, Access, Share, RecordLength)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Boolean, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Byte, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Char, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As DateTime, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Decimal, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Double, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Short, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Integer, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Long, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Single, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As ValueType, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining), Obsolete("This member has been deprecated. Please use FilePutObject to write Object types, or coerce FileNumber and RecordNumber to Integer for writing non-Object types. http://go.microsoft.com/fwlink/?linkid=14202")>
        Public Shared Sub FilePut(FileNumber As Object, Value As Object, Optional RecordNumber As Object = -1)
            Throw New ArgumentException(Utils.GetResourceString("UseFilePutObject"))
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As String, Optional RecordNumber As Long = -1, Optional StringIsFixedLength As Boolean = False)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber, StringIsFixedLength)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePut(FileNumber As Integer, Value As Array, Optional RecordNumber As Long = -1, Optional ArrayIsDynamic As Boolean = False, Optional StringIsFixedLength As Boolean = False)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).Put(Value, RecordNumber, ArrayIsDynamic, StringIsFixedLength)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FilePutObject(FileNumber As Integer, Value As Object, Optional RecordNumber As Long = -1)
            Try
                FileSystem.ValidateGetPutRecordNumber(RecordNumber)
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber, (OpenModeTypes.Binary Or OpenModeTypes.Random)).PutObject(Value, RecordNumber, True)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub FileWidth(FileNumber As Integer, RecordWidth As Integer)
            FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).SetWidth(RecordWidth)
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function FreeFile() As Integer
            Dim callingAssembly As Assembly = Assembly.GetCallingAssembly
            Dim assemblyData As AssemblyData = ProjectData.GetProjectData.GetAssemblyData(callingAssembly)
            Dim lChannel As Integer = 1
            Do
                If (assemblyData.GetChannelObj(lChannel) Is Nothing) Then
                    Return lChannel
                End If
                lChannel += 1
            Loop While (lChannel <= &HFF)
            Throw ExceptionUtils.VbMakeException(&H43)
        End Function

        Public Shared Function GetAttr(PathName As String) As FileAttribute
            Dim anyOf As Char() = New Char() {"*"c, "?"c}
            If (PathName.IndexOfAny(anyOf) >= 0) Then
                Throw ExceptionUtils.VbMakeException(&H34)
            End If
            Dim info As New FileInfo(PathName)
            If info.Exists Then
                Return (DirectCast(info.Attributes, FileAttribute) And (FileAttribute.Archive Or (FileAttribute.Directory Or (FileAttribute.Volume Or (FileAttribute.System Or (FileAttribute.Hidden Or FileAttribute.ReadOnly))))))
            End If
            Dim info2 As New DirectoryInfo(PathName)
            If info2.Exists Then
                Return (DirectCast(info2.Attributes, FileAttribute) And (FileAttribute.Archive Or (FileAttribute.Directory Or (FileAttribute.Volume Or (FileAttribute.System Or (FileAttribute.Hidden Or FileAttribute.ReadOnly))))))
            End If
            If (Path.GetFileName(PathName).Length = 0) Then
                Throw ExceptionUtils.VbMakeException(&H34)
            End If
            Dim args As String() = New String() {PathName}
            Throw New FileNotFoundException(Utils.GetResourceString("FileSystem_FileNotFound1", args))
        End Function

        Friend Shared Function GetChannelObj(assem As Assembly, FileNumber As Integer) As VB6File
            Dim channelOrNull As VB6File = FileSystem.GetChannelOrNull(ProjectData.GetProjectData.GetAssemblyData(assem), FileNumber)
            If (channelOrNull Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(&H34)
            End If
            Return channelOrNull
        End Function

        Private Shared Function GetChannelOrNull(oAssemblyData As AssemblyData, FileNumber As Integer) As VB6File
            Return oAssemblyData.GetChannelObj(FileNumber)
        End Function

        Private Shared Function GetStream(assem As Assembly, FileNumber As Integer) As VB6File
            Return FileSystem.GetStream(assem, FileNumber, (OpenModeTypes.Binary Or (OpenModeTypes.Append Or (OpenModeTypes.Random Or (OpenModeTypes.Output Or OpenModeTypes.Input)))))
        End Function

        Private Shared Function GetStream(assem As Assembly, FileNumber As Integer, mode As OpenModeTypes) As VB6File
            If ((FileNumber < 1) OrElse (FileNumber > &HFF)) Then
                Throw ExceptionUtils.VbMakeException(&H34)
            End If
            Dim channelObj As VB6File = FileSystem.GetChannelObj(assem, FileNumber)
            If ((FileSystem.OpenModeTypesFromOpenMode(channelObj.GetMode) Or mode) = Not OpenModeTypes.Any) Then
                channelObj = Nothing
                Throw ExceptionUtils.VbMakeException(&H36)
            End If
            Return channelObj
        End Function

        Private Shared Function InitializeWriteDateFormatInfo() As DateTimeFormatInfo
            Return New DateTimeFormatInfo With {
                .DateSeparator = "-",
                .ShortDatePattern = "\#yyyy-MM-dd\#",
                .LongTimePattern = "\#HH:mm:ss\#",
                .FullDateTimePattern = "\#yyyy-MM-dd HH:mm:ss\#"
            }
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Boolean)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Byte)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Char)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As DateTime)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Decimal)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Double)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Short)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Integer)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Long)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Object)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As Single)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Input(FileNumber As Integer, ByRef Value As String)
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Input(Value)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function InputString(FileNumber As Integer, CharCount As Integer) As String
            Dim str As String
            Try
                If ((CharCount < 0) OrElse (CharCount > 1073741823.5)) Then
                    Dim args As String() = New String() {"CharCount"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
                End If
                Dim channelObj As VB6File = FileSystem.GetChannelObj(Assembly.GetCallingAssembly, FileNumber)
                channelObj.Lock
                Try
                    str = channelObj.InputString(CharCount)
                Finally
                    channelObj.Unlock
                End Try
            Catch exception As Exception
                Throw exception
            End Try
            Return str
        End Function

        Private Shared Sub InternalCloseFile(oAssemblyData As AssemblyData, FileNumber As Integer)
            If (FileNumber = 0) Then
                FileSystem.CloseAllFiles(oAssemblyData)
            Else
                Dim channelOrNull As VB6File = FileSystem.GetChannelOrNull(oAssemblyData, FileNumber)
                If (Not channelOrNull Is Nothing) Then
                    oAssemblyData.SetChannelObj(FileNumber, Nothing)
                    If (Not channelOrNull Is Nothing) Then
                        channelOrNull.CloseFile
                    End If
                End If
            End If
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Kill(PathName As String)
            Dim fileName As String
            Dim num As Integer
            Dim directoryName As String = Path.GetDirectoryName(PathName)
            If ((directoryName Is Nothing) OrElse (directoryName.Length = 0)) Then
                directoryName = Environment.CurrentDirectory
                fileName = PathName
            Else
                fileName = Path.GetFileName(PathName)
            End If
            Dim files As FileInfo() = New DirectoryInfo(directoryName).GetFiles(fileName)
            directoryName = (directoryName & Conversions.ToString(Path.PathSeparator))
            If (Not files Is Nothing) Then
                Dim upperBound As Integer = files.GetUpperBound(0)
                Dim i As Integer = 0
                Do While (i <= upperBound)
                    Dim info As FileInfo = files(i)
                    If ((info.Attributes And (FileAttributes.System Or FileAttributes.Hidden)) = 0) Then
                        fileName = info.FullName
                        If FileSystem.CheckFileOpen(ProjectData.GetProjectData.GetAssemblyData(Assembly.GetCallingAssembly), fileName, OpenModeTypes.Any) Then
                            Dim args As String() = New String() {fileName}
                            Throw ExceptionUtils.VbMakeException(New IOException(Utils.GetResourceString("FileSystem_FileAlreadyOpen1", args)), &H37)
                        End If
                        Try
                            File.Delete(fileName)
                            num += 1
                        Catch exception As IOException
                            Throw ExceptionUtils.VbMakeException(exception, &H37)
                        Catch exception2 As Exception
                            Throw exception2
                        End Try
                    End If
                    i += 1
                Loop
            End If
            If (num = 0) Then
                Dim textArray2 As String() = New String() {PathName}
                Throw New FileNotFoundException(Utils.GetResourceString("KILL_NoFilesFound1", textArray2))
            End If
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function LineInput(FileNumber As Integer) As String
            Dim stream As VB6File = FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber)
            FileSystem.CheckInputCapable(stream)
            If stream.EOF Then
                Throw ExceptionUtils.VbMakeException(&H3E)
            End If
            Return stream.LineInput
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function Loc(FileNumber As Integer) As Long
            Return FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).LOC
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Lock(FileNumber As Integer)
            FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Lock
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Lock(FileNumber As Integer, Record As Long)
            FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Lock(Record)
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Lock(FileNumber As Integer, FromRecord As Long, ToRecord As Long)
            FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Lock(FromRecord, ToRecord)
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function LOF(FileNumber As Integer) As Long
            Return FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).LOF
        End Function

        Public Shared Sub MkDir(Path As String)
            If ((Path Is Nothing) OrElse (Path.Length = 0)) Then
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty")), &H34)
            End If
            If Directory.Exists(Path) Then
                Throw ExceptionUtils.VbMakeException(&H4B)
            End If
            Directory.CreateDirectory(Path)
        End Sub

        Private Shared Function OpenModeTypesFromOpenMode(om As OpenMode) As OpenModeTypes
            If (om = OpenMode.Input) Then
                Return OpenModeTypes.Input
            End If
            If (om = OpenMode.Output) Then
                Return OpenModeTypes.Output
            End If
            If (om = OpenMode.Append) Then
                Return OpenModeTypes.Append
            End If
            If (om = OpenMode.Binary) Then
                Return OpenModeTypes.Binary
            End If
            If (om = OpenMode.Random) Then
                Return OpenModeTypes.Random
            End If
            If (om <> DirectCast(-1, OpenMode)) Then
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"), "om")
            End If
            Return OpenModeTypes.Any
        End Function

        Private Shared Function PathContainsWildcards(Path As String) As Boolean
            If (Path Is Nothing) Then
                Return False
            End If
            Return ((Path.IndexOf("*"c) <> -1) OrElse (Path.IndexOf("?"c) <> -1))
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Print(FileNumber As Integer, ParamArray Output As Object())
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Print(Output)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub PrintLine(FileNumber As Integer, ParamArray Output As Object())
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).PrintLine(Output)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Rename(OldPath As String, NewPath As String)
            Dim assemblyData As AssemblyData = ProjectData.GetProjectData.GetAssemblyData(Assembly.GetCallingAssembly)
            OldPath = FileSystem.VB6CheckPathname(assemblyData, OldPath, DirectCast(-1, OpenMode))
            NewPath = FileSystem.VB6CheckPathname(assemblyData, NewPath, DirectCast(-1, OpenMode))
            Call New FileIOPermission((FileIOPermissionAccess.Write Or FileIOPermissionAccess.Read), OldPath).Demand
            Call New FileIOPermission(FileIOPermissionAccess.Write, NewPath).Demand
            If (UnsafeNativeMethods.MoveFile(OldPath, NewPath) = 0) Then
                Select Case Marshal.GetLastWin32Error
                    Case 2
                        Throw ExceptionUtils.VbMakeException(&H35)
                    Case 12
                        Throw ExceptionUtils.VbMakeException(&H4B)
                    Case &H11
                        Throw ExceptionUtils.VbMakeException(&H4A)
                    Case 80, &HB7
                        Throw ExceptionUtils.VbMakeException(&H3A)
                End Select
                Throw ExceptionUtils.VbMakeException(5)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Reset()
            FileSystem.CloseAllFiles(Assembly.GetCallingAssembly)
        End Sub

        Public Shared Sub RmDir(Path As String)
            If ((Path Is Nothing) OrElse (Path.Length = 0)) Then
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty")), &H34)
            End If
            Try
                Directory.Delete(Path)
            Catch exception As DirectoryNotFoundException
                Throw ExceptionUtils.VbMakeException(exception, &H4C)
            Catch exception2 As StackOverflowException
                Throw exception2
            Catch exception3 As OutOfMemoryException
                Throw exception3
            Catch exception4 As ThreadAbortException
                Throw exception4
            Catch exception5 As Exception
                Throw ExceptionUtils.VbMakeException(exception5, &H4B)
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Function Seek(FileNumber As Integer) As Long
            Return FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Seek
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Seek(FileNumber As Integer, Position As Long)
            FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Seek(Position)
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub SetAttr(PathName As String, Attributes As FileAttribute)
            If ((PathName Is Nothing) OrElse (PathName.Length = 0)) Then
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty")), &H34)
            End If
            Dim callingAssembly As Assembly = Assembly.GetCallingAssembly
            FileSystem.VB6CheckPathname(ProjectData.GetProjectData.GetAssemblyData(callingAssembly), PathName, OpenMode.Input)
            If ((Attributes Or (FileAttribute.Archive Or (FileAttribute.System Or (FileAttribute.Hidden Or FileAttribute.ReadOnly)))) <> (FileAttribute.Archive Or (FileAttribute.System Or (FileAttribute.Hidden Or FileAttribute.ReadOnly)))) Then
                Dim args As String() = New String() {"Attributes"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Dim fileAttributes As FileAttributes = DirectCast(Attributes, FileAttributes)
            File.SetAttributes(PathName, fileAttributes)
        End Sub

        Public Shared Function SPC(Count As Short) As SpcInfo
            Dim info As SpcInfo
            If (Count < 1) Then
                Count = 0
            End If
            info.Count = Count
            Return info
        End Function

        Public Shared Function TAB() As TabInfo
            Dim info As TabInfo
            info.Column = -1
            Return info
        End Function

        Public Shared Function TAB(Column As Short) As TabInfo
            Dim info As TabInfo
            If (Column < 1) Then
                Column = 1
            End If
            info.Column = Column
            Return info
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Unlock(FileNumber As Integer)
            FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Unlock
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Unlock(FileNumber As Integer, Record As Long)
            FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Unlock(Record)
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Unlock(FileNumber As Integer, FromRecord As Long, ToRecord As Long)
            FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).Unlock(FromRecord, ToRecord)
        End Sub

        Private Shared Function UnsafeValidDrive(cDrive As Char) As Boolean
            Dim num As Integer = (cDrive - "A"c)
            Return ((UnsafeNativeMethods.GetLogicalDrives And CLng(Math.Round(Math.Pow(2, CDbl(num))))) > 0)
        End Function

        Private Shared Sub ValidateAccess(Access As OpenAccess)
            If (((Access <> OpenAccess.Default) AndAlso (Access <> OpenAccess.Read)) AndAlso ((Access <> OpenAccess.ReadWrite) AndAlso (Access <> OpenAccess.Write))) Then
                Dim args As String() = New String() {"Access"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
        End Sub

        Private Shared Sub ValidateGetPutRecordNumber(RecordNumber As Long)
            If ((RecordNumber < 1) AndAlso (RecordNumber <> -1)) Then
                Dim args As String() = New String() {"RecordNumber"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args)), &H3F)
            End If
        End Sub

        Private Shared Sub ValidateMode(Mode As OpenMode)
            If ((((Mode <> OpenMode.Input) AndAlso (Mode <> OpenMode.Output)) AndAlso ((Mode <> OpenMode.Random) AndAlso (Mode <> OpenMode.Append))) AndAlso (Mode <> OpenMode.Binary)) Then
                Dim args As String() = New String() {"Mode"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
        End Sub

        Private Shared Sub ValidateShare(Share As OpenShare)
            If ((((Share <> OpenShare.Default) AndAlso (Share <> OpenShare.Shared)) AndAlso ((Share <> OpenShare.LockRead) AndAlso (Share <> OpenShare.LockReadWrite))) AndAlso (Share <> OpenShare.LockWrite)) Then
                Dim args As String() = New String() {"Share"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
        End Sub

        Friend Shared Function VB6CheckPathname(oAssemblyData As AssemblyData, sPath As String, mode As OpenMode) As String
            If ((sPath.IndexOf("?"c) <> -1) OrElse (sPath.IndexOf("*"c) <> -1)) Then
                Dim args As String() = New String() {sPath}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidPathChars1", args))
            End If
            Dim fullName As String = New FileInfo(sPath).FullName
            If FileSystem.CheckFileOpen(oAssemblyData, fullName, FileSystem.OpenModeTypesFromOpenMode(mode)) Then
                Throw ExceptionUtils.VbMakeException(&H37)
            End If
            Return fullName
        End Function

        Private Shared Sub vbIOOpenFile(assem As Assembly, FileNumber As Integer, FileName As String, Mode As OpenMode, Access As OpenAccess, Share As OpenShare, RecordLength As Integer)
            Dim file As VB6File
            Dim assemblyData As AssemblyData = ProjectData.GetProjectData.GetAssemblyData(assem)
            If (Not FileSystem.GetChannelOrNull(assemblyData, FileNumber) Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(&H37)
            End If
            If ((FileName Is Nothing) OrElse (FileName.Length = 0)) Then
                Throw ExceptionUtils.VbMakeException(&H4B)
            End If
            FileName = New FileInfo(FileName).FullName
            If FileSystem.CheckFileOpen(assemblyData, FileName, FileSystem.OpenModeTypesFromOpenMode(Mode)) Then
                Throw ExceptionUtils.VbMakeException(&H37)
            End If
            If ((RecordLength <> -1) AndAlso (RecordLength <= 0)) Then
                Throw ExceptionUtils.VbMakeException(5)
            End If
            If (Mode = OpenMode.Binary) Then
                RecordLength = 1
            ElseIf (RecordLength = -1) Then
                If (Mode = OpenMode.Random) Then
                    RecordLength = &H80
                Else
                    RecordLength = &H200
                End If
            End If
            If (Share = OpenShare.Default) Then
                Share = OpenShare.LockReadWrite
            End If
            Select Case Mode
                Case OpenMode.Input
                    If ((Access <> OpenAccess.Read) AndAlso (Access <> OpenAccess.Default)) Then
                        Throw New ArgumentException(Utils.GetResourceString("FileSystem_IllegalInputAccess"))
                    End If
                    file = New VB6InputFile(FileName, Share)
                    Exit Select
                Case OpenMode.Output
                    If ((Access <> OpenAccess.Write) AndAlso (Access <> OpenAccess.Default)) Then
                        Throw New ArgumentException(Utils.GetResourceString("FileSystem_IllegalOutputAccess"))
                    End If
                    file = New VB6OutputFile(FileName, Share, False)
                    Exit Select
                Case OpenMode.Random
                    If (Access = OpenAccess.Default) Then
                        Access = OpenAccess.ReadWrite
                    End If
                    file = New VB6RandomFile(FileName, Access, Share, RecordLength)
                    Exit Select
                Case OpenMode.Append
                    If (((Access <> OpenAccess.Write) AndAlso (Access <> OpenAccess.ReadWrite)) AndAlso (Access <> OpenAccess.Default)) Then
                        Throw New ArgumentException(Utils.GetResourceString("FileSystem_IllegalAppendAccess"))
                    End If
                    file = New VB6OutputFile(FileName, Share, True)
                    Exit Select
                Case OpenMode.Binary
                    If (Access = OpenAccess.Default) Then
                        Access = OpenAccess.ReadWrite
                    End If
                    file = New VB6BinaryFile(FileName, Access, Share)
                    Exit Select
                Case Else
                    Throw ExceptionUtils.VbMakeException(&H33)
            End Select
            FileSystem.AddFileToList(assemblyData, FileNumber, file)
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub Write(FileNumber As Integer, ParamArray Output As Object())
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).WriteHelper(Output)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Shared Sub WriteLine(FileNumber As Integer, ParamArray Output As Object())
            Try
                FileSystem.GetStream(Assembly.GetCallingAssembly, FileNumber).WriteLineHelper(Output)
            Catch exception As Exception
                Throw exception
            End Try
        End Sub


        ' Fields
        Private Const A_ALLBITS As Integer = &H3F
        Private Const A_ARCH As Integer = &H20
        Private Const A_HIDDEN As Integer = 2
        Private Const A_NORMAL As Integer = 0
        Private Const A_RDONLY As Integer = 1
        Private Const A_SUBDIR As Integer = &H10
        Private Const A_SYSTEM As Integer = 4
        Private Const A_VOLID As Integer = 8
        Private Const ERROR_ACCESS_DENIED As Integer = 5
        Private Const ERROR_ALREADY_EXISTS As Integer = &HB7
        Private Const ERROR_BAD_NETPATH As Integer = &H35
        Private Const ERROR_FILE_EXISTS As Integer = 80
        Private Const ERROR_FILE_NOT_FOUND As Integer = 2
        Private Const ERROR_INVALID_ACCESS As Integer = 12
        Private Const ERROR_INVALID_PARAMETER As Integer = &H57
        Private Const ERROR_NOT_SAME_DEVICE As Integer = &H11
        Private Const ERROR_WRITE_PROTECT As Integer = &H13
        Friend Const FIRST_LOCAL_CHANNEL As Integer = 1
        Friend Const LAST_LOCAL_CHANNEL As Integer = &HFF
        Friend Shared ReadOnly m_WriteDateFormatInfo As DateTimeFormatInfo = FileSystem.InitializeWriteDateFormatInfo
        Friend Const sDateFormat As String = "d"
        Friend Const sDateTimeFormat As String = "F"
        Friend Const sTimeFormat As String = "T"

        ' Nested Types
        Friend Enum vbFileType
            ' Fields
            vbPrintFile = 0
            vbWriteFile = 1
        End Enum
    End Class
End Namespace

