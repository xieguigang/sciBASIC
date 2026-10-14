Imports System.IO

Namespace Runtime

    ''' <summary>
    ''' Node.js <c>fs</c> module: synchronous methods (<c>*Sync</c>), callback-style async
    ''' (<c>readFile(path, cb)</c>) and promise-style (<c>fs.promises.*</c>, await-able).
    ''' <para>
    ''' All operations are backed by <see cref="System.IO"/>. Text reads default to a
    ''' <see cref="BufferInstance"/> (matching Node), or decode to a string when an encoding
    ''' is given. Errors are mapped to a <see cref="JsFsError"/> carrying a Node-like
    ''' <c>code</c> (ENOENT / EACCES / EISDIR / EEXIST / EIO).
    ''' </para>
    ''' <para>
    ''' The engine has no event loop, so async callbacks and promise settlements are queued
    ''' as microtasks — they run after the current synchronous code, matching Node's ordering.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class FsModule

        Private Sub New()
        End Sub

        Public Shared Function MakeModule() As JsModule
            Dim constants As New System.Collections.Generic.Dictionary(Of String, Object) From {
                {"F_OK", 0.0}, {"R_OK", 4.0}, {"W_OK", 2.0}, {"X_OK", 1.0}
            }
            Dim promises = New JsModule("fs.promises")
            Dim props As New System.Collections.Generic.Dictionary(Of String, Object) From {
                {"constants", constants},
                {"promises", promises}
            }
            Return New JsModule("fs", props)
        End Function

        ' ===================== sync + async-callback dispatch =====================

        Public Shared Function Dispatch(method As String, args As Object()) As Object
            Select Case method
                Case "readFileSync" : Return SyncReadFile(args)
                Case "writeFileSync" : Return SyncWriteFile(args)
                Case "existsSync" : Return ExistsSync(args)
                Case "readdirSync" : Return SyncReaddir(args)
                Case "mkdirSync" : Return SyncMkdir(args)
                Case "rmdirSync" : SyncRmdir(args) : Return JsRuntime.Undef
                Case "statSync" : Return SyncStat(args, True)
                Case "lstatSync" : Return SyncStat(args, False)
                Case "unlinkSync" : SyncUnlink(args) : Return JsRuntime.Undef
                Case "appendFileSync" : SyncAppend(args) : Return JsRuntime.Undef
                Case "copyFileSync" : SyncCopy(args) : Return JsRuntime.Undef
                Case "renameSync" : SyncRename(args) : Return JsRuntime.Undef
                Case "readFile" : Return AsyncReadFile(args)
                Case "writeFile" : Return AsyncWriteFile(args)
                Case "readdir" : Return AsyncReaddir(args)
                Case "mkdir" : Return AsyncMkdir(args)
                Case "rmdir", "rm" : Return AsyncRmdir(args)
                Case "stat" : Return AsyncStat(args, True)
                Case "lstat" : Return AsyncStat(args, False)
                Case "unlink" : Return AsyncUnlink(args)
                Case "appendFile" : Return AsyncAppend(args)
                Case "copyFile" : Return AsyncCopy(args)
                Case "rename" : Return AsyncRename(args)
                Case "access" : Return AsyncAccess(args)
                Case Else : Throw JsRuntimeException.TypeError("fs." & method & " is not a function")
            End Select
        End Function

        Public Shared Function DispatchPromises(method As String, args As Object()) As Object
            Select Case method
                Case "readFile" : Return PromiseReadFile(args)
                Case "writeFile" : Return PromiseWriteFile(args)
                Case "readdir" : Return PromiseReaddir(args)
                Case "mkdir" : Return PromiseMkdir(args)
                Case "rmdir", "rm" : Return PromiseRmdir(args)
                Case "stat" : Return PromiseStat(args, True)
                Case "lstat" : Return PromiseStat(args, False)
                Case "unlink" : Return PromiseUnlink(args)
                Case "appendFile" : Return PromiseAppend(args)
                Case "copyFile" : Return PromiseCopy(args)
                Case "rename" : Return PromiseRename(args)
                Case "access" : Return PromiseAccess(args)
                Case Else : Throw JsRuntimeException.TypeError("fs.promises." & method & " is not a function")
            End Select
        End Function

        Public Shared Function DispatchStats(target As FsStats, method As String, args As Object()) As Object
            Select Case method
                Case "isFile" : Return target.IsFile
                Case "isDirectory" : Return target.IsDir
                Case "isBlockDevice", "isCharacterDevice", "isSymbolicLink", "isFIFO", "isSocket" : Return False
                Case Else : Throw JsRuntimeException.TypeError("stats." & method & " is not a function")
            End Select
        End Function

        ' ===================== arg parsing helpers =====================

        Private Shared Sub Extract(args As Object(), requiredCount As Integer, ByRef cb As Object, ByRef opts As Object)
            cb = Nothing
            If args.Length > 0 AndAlso JsRuntime.IsJsFunction(args(args.Length - 1)) Then cb = args(args.Length - 1)
            Dim posCount = args.Length - If(cb Is Nothing, 0, 1)
            opts = If(posCount > requiredCount, args(posCount - 1), JsRuntime.Undef)
        End Sub

        Private Shared Function Arg(args As Object(), i As Integer) As Object
            If i < args.Length Then Return args(i)
            Return JsRuntime.Undef
        End Function

        Private Shared Function Str(args As Object(), i As Integer) As String
            Dim v = Arg(args, i)
            If v Is Nothing OrElse v Is JsRuntime.Undef Then Return ""
            Return JsRuntime.JsStr(v)
        End Function

        ' ===================== core operations =====================

        Private Shared Function ResolvePath(pth As String) As String
            If String.IsNullOrEmpty(pth) Then Return pth
            If System.IO.Path.IsPathRooted(pth) Then Return pth
            Return System.IO.Path.Combine(ProcessModule.Cwd, pth)
        End Function

        ' read: returns encoding string or Nothing (Nothing => Buffer)
        Private Shared Function ReadEncoding(opt As Object) As String
            If opt Is Nothing OrElse opt Is JsRuntime.Undef Then Return Nothing
            If TypeOf opt Is String Then
                Dim e = CStr(opt)
                If e = "" OrElse e = "buffer" Then Return Nothing
                Return e
            End If
            If JsRuntime.IsJsObject(opt) Then
                Dim d = DirectCast(opt, System.Collections.Generic.Dictionary(Of String, Object))
                Dim v As Object = Nothing
                If d.TryGetValue("encoding", v) AndAlso v IsNot Nothing AndAlso v IsNot JsRuntime.Undef Then
                    Dim e = JsRuntime.JsStr(v)
                    If e = "" OrElse e = "buffer" Then Return Nothing
                    Return e
                End If
                Return Nothing
            End If
            Return Nothing
        End Function

        ' write: returns encoding string used when data is a string (default utf8)
        Private Shared Function WriteEncoding(opt As Object) As String
            If opt Is Nothing OrElse opt Is JsRuntime.Undef Then Return "utf8"
            If TypeOf opt Is String Then
                Dim e = CStr(opt)
                If e = "" OrElse e = "buffer" Then Return "utf8"
                Return e
            End If
            If JsRuntime.IsJsObject(opt) Then
                Dim d = DirectCast(opt, System.Collections.Generic.Dictionary(Of String, Object))
                Dim v As Object = Nothing
                If d.TryGetValue("encoding", v) AndAlso v IsNot Nothing AndAlso v IsNot JsRuntime.Undef Then Return JsRuntime.JsStr(v)
            End If
            Return "utf8"
        End Function

        Private Shared Function IsRecursive(opt As Object) As Boolean
            If opt Is Nothing OrElse opt Is JsRuntime.Undef OrElse Not JsRuntime.IsJsObject(opt) Then Return False
            Dim d = DirectCast(opt, System.Collections.Generic.Dictionary(Of String, Object))
            Dim v As Object = Nothing
            If d.TryGetValue("recursive", v) Then Return JsRuntime.JsTruthy(v)
            Return False
        End Function

        Private Shared Function CoreReadFile(pth As String, enc As String) As Object
            Dim bytes = System.IO.File.ReadAllBytes(pth)
            If enc Is Nothing Then Return New BufferInstance(bytes)
            Return BufferModule.Decode(enc, bytes, 0, bytes.Length)
        End Function

        Private Shared Sub CoreWriteFile(pth As String, data As Object, enc As String)
            Dim bytes As Byte()
            If TypeOf data Is BufferInstance Then
                bytes = DirectCast(data, BufferInstance).Data
            Else
                bytes = BufferModule.Encode(enc, JsRuntime.JsStr(data))
            End If
            System.IO.File.WriteAllBytes(pth, bytes)
        End Sub

        Private Shared Function CoreReaddir(pth As String) As System.Collections.Generic.List(Of Object)
            Dim dir = ResolvePath(pth)
            Dim names As New System.Collections.Generic.List(Of Object)()
            For Each f In System.IO.Directory.GetFiles(dir) : names.Add(System.IO.Path.GetFileName(f)) : Next
            For Each d In System.IO.Directory.GetDirectories(dir) : names.Add(System.IO.Path.GetFileName(d)) : Next
            Return names
        End Function

        Private Shared Sub CoreMkdir(pth As String, recursive As Boolean)
            Dim full = ResolvePath(pth)
            If System.IO.Directory.Exists(full) Then
                If Not recursive Then Throw New JsRuntimeException(FsError("EEXIST", "file already exists", "mkdir", full))
                Return
            End If
            If Not recursive Then
                Dim parent = System.IO.Path.GetDirectoryName(full)
                If parent IsNot Nothing AndAlso parent.Length > 0 AndAlso Not System.IO.Directory.Exists(parent) Then
                    Throw New JsRuntimeException(FsError("ENOENT", "no such file or directory", "mkdir", full))
                End If
            End If
            System.IO.Directory.CreateDirectory(full)
        End Sub

        Private Shared Sub CoreRmdir(pth As String)
            Dim full = ResolvePath(pth)
            If System.IO.Directory.Exists(full) Then
                System.IO.Directory.Delete(full, False)
            ElseIf System.IO.File.Exists(full) Then
                Throw New JsRuntimeException(FsError("ENOTDIR", "not a directory", "rmdir", full))
            Else
                Throw New JsRuntimeException(FsError("ENOENT", "no such file or directory", "rmdir", full))
            End If
        End Sub

        Private Shared Function CoreStat(pth As String, follow As Boolean) As FsStats
            Dim full = ResolvePath(pth)
            If System.IO.File.Exists(full) Then
                Dim fi = New System.IO.FileInfo(full)
                Return New FsStats With {
                    .Size = fi.Length, .IsFile = True, .IsDir = False,
                    .MtimeMs = ToMs(fi.LastWriteTimeUtc), .CtimeMs = ToMs(fi.CreationTimeUtc),
                    .AtimeMs = ToMs(fi.LastAccessTimeUtc), .BirthtimeMs = ToMs(fi.CreationTimeUtc),
                    .Mode = &H8000, .Dev = 1
                }
            ElseIf System.IO.Directory.Exists(full) Then
                Dim di = New System.IO.DirectoryInfo(full)
                Return New FsStats With {
                    .Size = 0, .IsFile = False, .IsDir = True,
                    .MtimeMs = ToMs(di.LastWriteTimeUtc), .CtimeMs = ToMs(di.CreationTimeUtc),
                    .AtimeMs = ToMs(di.LastAccessTimeUtc), .BirthtimeMs = ToMs(di.CreationTimeUtc),
                    .Mode = &H4000, .Dev = 1
                }
            End If
            Throw New JsRuntimeException(FsError("ENOENT", "no such file or directory", "stat", full))
        End Function

        Private Shared Sub CoreUnlink(pth As String)
            Dim full = ResolvePath(pth)
            If System.IO.File.Exists(full) Then
                System.IO.File.Delete(full)
            ElseIf System.IO.Directory.Exists(full) Then
                Throw New JsRuntimeException(FsError("EISDIR", "illegal operation on a directory", "unlink", full))
            Else
                Throw New JsRuntimeException(FsError("ENOENT", "no such file or directory", "unlink", full))
            End If
        End Sub

        Private Shared Sub CoreAppend(pth As String, data As Object, enc As String)
            Dim bytes As Byte()
            If TypeOf data Is BufferInstance Then
                bytes = DirectCast(data, BufferInstance).Data
            Else
                bytes = BufferModule.Encode(enc, JsRuntime.JsStr(data))
            End If
            Using fs = New System.IO.FileStream(ResolvePath(pth), System.IO.FileMode.Append, System.IO.FileAccess.Write)
                fs.Write(bytes, 0, bytes.Length)
            End Using
        End Sub

        Private Shared Sub CoreCopy(src As String, dest As String)
            System.IO.File.Copy(ResolvePath(src), ResolvePath(dest), True)
        End Sub

        Private Shared Sub CoreRename(oldP As String, newP As String)
            Dim s = ResolvePath(oldP), d = ResolvePath(newP)
            If System.IO.File.Exists(s) Then
                System.IO.File.Move(s, d, True)
            ElseIf System.IO.Directory.Exists(s) Then
                System.IO.Directory.Move(s, d)
            Else
                Throw New JsRuntimeException(FsError("ENOENT", "no such file or directory", "rename", s))
            End If
        End Sub

        Private Shared Function ToMs(dt As System.DateTime) As Double
            Return (dt - New System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds
        End Function

        ' ===================== sync wrappers =====================

        Private Shared Function SyncReadFile(args As Object()) As Object
            Dim pth = Str(args, 0), enc = ReadEncoding(Arg(args, 1))
            Try : Return CoreReadFile(ResolvePath(pth), enc)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "open", pth))
            End Try
        End Function

        Private Shared Function SyncWriteFile(args As Object()) As Object
            Dim pth = Str(args, 0), data = Arg(args, 1), enc = WriteEncoding(Arg(args, 2))
            Try : CoreWriteFile(ResolvePath(pth), data, enc)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "open", pth))
            End Try
            Return JsRuntime.Undef
        End Function

        Private Shared Function ExistsSync(args As Object()) As Boolean
            Dim full = ResolvePath(Str(args, 0))
            Return System.IO.File.Exists(full) OrElse System.IO.Directory.Exists(full)
        End Function

        Private Shared Function SyncReaddir(args As Object()) As Object
            Dim pth = Str(args, 0)
            Try : Return CoreReaddir(pth)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "scandir", pth))
            End Try
        End Function

        Private Shared Function SyncMkdir(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 1, cb, opts)
            Dim pth = Str(args, 0), rec = IsRecursive(opts)
            Try : CoreMkdir(pth, rec)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "mkdir", pth))
            End Try
            Return JsRuntime.Undef
        End Function

        Private Shared Sub SyncRmdir(args As Object())
            Dim pth = Str(args, 0)
            Try : CoreRmdir(pth)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "rmdir", pth))
            End Try
        End Sub

        Private Shared Function SyncStat(args As Object(), follow As Boolean) As Object
            Dim pth = Str(args, 0)
            Try : Return CoreStat(pth, follow)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "stat", pth))
            End Try
        End Function

        Private Shared Sub SyncUnlink(args As Object())
            Dim pth = Str(args, 0)
            Try : CoreUnlink(pth)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "unlink", pth))
            End Try
        End Sub

        Private Shared Sub SyncAppend(args As Object())
            Dim pth = ResolvePath(Str(args, 0)), data = Arg(args, 1), enc = WriteEncoding(Arg(args, 2))
            Try : CoreAppend(pth, data, enc)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "open", pth))
            End Try
        End Sub

        Private Shared Sub SyncCopy(args As Object())
            Dim s = Str(args, 0), d = Str(args, 1)
            Try : CoreCopy(s, d)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "copyfile", s))
            End Try
        End Sub

        Private Shared Sub SyncRename(args As Object())
            Dim s = Str(args, 0), d = Str(args, 1)
            Try : CoreRename(s, d)
            Catch rex As JsRuntimeException : Throw
            Catch ex As Exception : Throw New JsRuntimeException(FsError(ex, "rename", s))
            End Try
        End Sub

        ' ===================== async (callback) wrappers =====================

        Private Shared Function AsyncReadFile(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 1, cb, opts)
            Dim pth = Str(args, 0), enc = ReadEncoding(opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.readFile requires a callback")
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             Dim data = CoreReadFile(ResolvePath(pth), enc)
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef, data})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "open", pth), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncWriteFile(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 2, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.writeFile requires a callback")
            Dim pth = Str(args, 0), data = Arg(args, 1), enc = WriteEncoding(opts)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             CoreWriteFile(ResolvePath(pth), data, enc)
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "open", pth), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncReaddir(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 1, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.readdir requires a callback")
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef, CoreReaddir(pth)})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "scandir", pth), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncMkdir(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 1, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.mkdir requires a callback")
            Dim pth = Str(args, 0), rec = IsRecursive(opts)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             CoreMkdir(pth, rec)
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "mkdir", pth), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncRmdir(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 1, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.rmdir requires a callback")
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             CoreRmdir(pth)
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "rmdir", pth), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncStat(args As Object(), follow As Boolean) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 1, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.stat requires a callback")
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef, CoreStat(pth, follow)})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "stat", pth), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncUnlink(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 1, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.unlink requires a callback")
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             CoreUnlink(pth)
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "unlink", pth), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncAppend(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 2, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.appendFile requires a callback")
            Dim pth = ResolvePath(Str(args, 0)), data = Arg(args, 1), enc = WriteEncoding(opts)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             CoreAppend(pth, data, enc)
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "open", pth), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncCopy(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 2, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.copyFile requires a callback")
            Dim s = Str(args, 0), d = Str(args, 1)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             CoreCopy(s, d)
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "copyfile", s), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncRename(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 2, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.rename requires a callback")
            Dim s = Str(args, 0), d = Str(args, 1)
            JsRuntime.QueueMicrotask(Sub()
                                         Try
                                             CoreRename(s, d)
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef})
                                         Catch rex As JsRuntimeException
                                             JsRuntime.JsInvoke(cb, New Object() {rex.Payload, JsRuntime.Undef})
                                         Catch ex As Exception
                                             JsRuntime.JsInvoke(cb, New Object() {FsError(ex, "rename", s), JsRuntime.Undef})
                                         End Try
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        Private Shared Function AsyncAccess(args As Object()) As Object
            Dim cb As Object = Nothing, opts As Object = Nothing
            Extract(args, 1, cb, opts)
            If cb Is Nothing Then Throw JsRuntimeException.TypeError("fs.access requires a callback")
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Dim full = ResolvePath(pth)
                                         If System.IO.File.Exists(full) OrElse System.IO.Directory.Exists(full) Then
                                             JsRuntime.JsInvoke(cb, New Object() {JsRuntime.Undef})
                                         Else
                                             JsRuntime.JsInvoke(cb, New Object() {FsError("ENOENT", "no such file or directory", "access", full), JsRuntime.Undef})
                                         End If
                                     End Sub)
            Return JsRuntime.Undef
        End Function

        ' ===================== promise wrappers =====================

        Private Shared Function PromiseReadFile(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0), enc = ReadEncoding(Arg(args, 1))
            JsRuntime.QueueMicrotask(Sub()
                                         Try : pr.Fulfill(CoreReadFile(ResolvePath(pth), enc))
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "open", pth))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseWriteFile(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0), data = Arg(args, 1), enc = WriteEncoding(Arg(args, 2))
            JsRuntime.QueueMicrotask(Sub()
                                         Try : CoreWriteFile(ResolvePath(pth), data, enc) : pr.Fulfill(JsRuntime.Undef)
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "open", pth))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseReaddir(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Try : pr.Fulfill(CoreReaddir(pth))
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "scandir", pth))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseMkdir(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0), rec = IsRecursive(Arg(args, 1))
            JsRuntime.QueueMicrotask(Sub()
                                         Try : CoreMkdir(pth, rec) : pr.Fulfill(JsRuntime.Undef)
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "mkdir", pth))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseRmdir(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Try : CoreRmdir(pth) : pr.Fulfill(JsRuntime.Undef)
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "rmdir", pth))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseStat(args As Object(), follow As Boolean) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Try : pr.Fulfill(CoreStat(pth, follow))
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "stat", pth))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseUnlink(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Try : CoreUnlink(pth) : pr.Fulfill(JsRuntime.Undef)
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "unlink", pth))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseAppend(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0), data = Arg(args, 1), enc = WriteEncoding(Arg(args, 2))
            JsRuntime.QueueMicrotask(Sub()
                                         Try : CoreAppend(ResolvePath(pth), data, enc) : pr.Fulfill(JsRuntime.Undef)
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "open", pth))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseCopy(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim s = Str(args, 0), d = Str(args, 1)
            JsRuntime.QueueMicrotask(Sub()
                                         Try : CoreCopy(s, d) : pr.Fulfill(JsRuntime.Undef)
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "copyfile", s))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseRename(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim s = Str(args, 0), d = Str(args, 1)
            JsRuntime.QueueMicrotask(Sub()
                                         Try : CoreRename(s, d) : pr.Fulfill(JsRuntime.Undef)
                                         Catch rex As JsRuntimeException : pr.Reject(rex.Payload)
                                         Catch ex As Exception : pr.Reject(FsError(ex, "rename", s))
                                         End Try
                                     End Sub)
            Return pr
        End Function

        Private Shared Function PromiseAccess(args As Object()) As Object
            Dim pr = New JsPromise()
            Dim pth = Str(args, 0)
            JsRuntime.QueueMicrotask(Sub()
                                         Dim full = ResolvePath(pth)
                                         If System.IO.File.Exists(full) OrElse System.IO.Directory.Exists(full) Then
                                             pr.Fulfill(JsRuntime.Undef)
                                         Else
                                             pr.Reject(FsError("ENOENT", "no such file or directory", "access", full))
                                         End If
                                     End Sub)
            Return pr
        End Function

        ' ===================== error mapping =====================

        Private Shared Function FsError(ex As Exception, syscall As String, pth As String) As JsFsError
            Dim code = "EIO"
            If TypeOf ex Is System.IO.FileNotFoundException OrElse TypeOf ex Is System.IO.DirectoryNotFoundException Then
                code = "ENOENT"
            ElseIf TypeOf ex Is System.UnauthorizedAccessException Then
                code = "EACCES"
            ElseIf TypeOf ex Is System.IO.IOException Then
                Dim full = ResolvePath(pth)
                If System.IO.Directory.Exists(full) Then code = "EISDIR" Else code = "EIO"
            End If
            Return FsError(code, MapDesc(code), syscall, pth)
        End Function

        Private Shared Function FsError(code As String, desc As String, syscall As String, pth As String) As JsFsError
            Dim msg = code & ": " & desc & ", " & syscall & " '" & pth & "'"
            Return New JsFsError(msg, code, syscall, pth)
        End Function

        Private Shared Function MapDesc(code As String) As String
            Select Case code
                Case "ENOENT" : Return "no such file or directory"
                Case "EACCES" : Return "permission denied"
                Case "EISDIR" : Return "illegal operation on a directory"
                Case "EEXIST" : Return "file already exists"
                Case Else : Return "unknown error"
            End Select
        End Function

    End Class

    ''' <summary>fs.Stats object: size plus isFile()/isDirectory() predicates.</summary>
    Public Class FsStats
        Public Size As Long
        Public IsFile As Boolean
        Public IsDir As Boolean
        Public MtimeMs As Double
        Public CtimeMs As Double
        Public AtimeMs As Double
        Public BirthtimeMs As Double
        Public Mode As Integer
        Public Dev As Integer
    End Class

    ''' <summary>
    ''' Node-style fs error carrying a <c>code</c> (ENOENT, EACCES, ...), <c>syscall</c>
    ''' and <c>path</c>. Printed as <c>"Error: &lt;code&gt;: &lt;desc&gt;, &lt;syscall&gt; '&lt;path&gt;'"</c>
    ''' and exposes <c>e.code</c> / <c>e.syscall</c> / <c>e.path</c> to scripts.
    ''' </summary>
    Public Class JsFsError
        Inherits JsError

        Public ReadOnly Code As String
        Public ReadOnly Syscall As String
        Public ReadOnly Path As String

        Public Sub New(message As String, code As String, syscall As String, pth As String)
            MyBase.New("Error", message)
            Me.Code = code
            Me.Syscall = syscall
            Me.Path = pth
        End Sub

    End Class

End Namespace
