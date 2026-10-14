Imports System.IO

Namespace Runtime

    ''' <summary>
    ''' Node.js <c>path</c> module (OS-specific variant matching the host platform):
    ''' <c>join/resolve/basename/dirname/extname/normalize/parse/isAbsolute</c> plus
    ''' the <c>sep</c> / <c>delimiter</c> properties.
    ''' </summary>
    Public NotInheritable Class PathModule

        Private Sub New()
        End Sub

        Friend Shared ReadOnly Sep As String = Path.DirectorySeparatorChar.ToString()
        Friend Shared ReadOnly Delim As String = Path.PathSeparator.ToString()

        Public Shared Function MakeModule() As JsModule
            Dim props As New Dictionary(Of String, Object) From {
                {"sep", Sep},
                {"delimiter", Delim}
            }
            Return New JsModule("path", props)
        End Function

        Public Shared Function Dispatch(method As String, args As Object()) As Object
            Select Case method
                Case "join" : Return Join(args)
                Case "resolve" : Return Resolve(args)
                Case "basename" : Return Basename(args)
                Case "dirname" : Return Dirname(args)
                Case "extname" : Return Extname(args)
                Case "normalize" : Return Normalize(JsRuntime.JsStr(Arg(args, 0)))
                Case "isAbsolute" : Return IsAbsolute(JsRuntime.JsStr(Arg(args, 0)))
                Case "parse" : Return Parse(args)
                Case "format" : Return FormatPath(Arg(args, 0))
                Case "relative" : Return Relative(JsRuntime.JsStr(Arg(args, 0)), JsRuntime.JsStr(Arg(args, 1)))
                Case "sep" : Return Sep
                Case "delimiter" : Return Delim
                Case Else : Throw JsRuntimeException.TypeError("path." & method & " is not a function")
            End Select
        End Function

        Private Shared Function Arg(args As Object(), i As Integer) As Object
            If i < args.Length Then Return args(i)
            Return JsRuntime.Undef
        End Function

        ' Split on either separator so scripts written with '/' work on Windows too.
        Private Shared Function SplitSegments(p As String) As List(Of String)
            Dim parts = p.Split(New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries)
            Dim segs As New List(Of String)()
            For Each part In parts : segs.Add(part) : Next
            Return segs
        End Function

        Private Shared Function IsAbs(p As String) As Boolean
            If p Is Nothing OrElse p.Length = 0 Then Return False
            If p.StartsWith(Sep) Then Return True
            ' Windows drive letter, e.g. C:\ or C:/
            If p.Length >= 2 AndAlso Char.IsLetter(p(0)) AndAlso p(1) = ":"c Then Return True
            Return False
        End Function

        Private Shared Function Assemble(segs As List(Of String), absolute As Boolean) As String
            If segs.Count = 0 Then Return If(absolute, Sep, ".")
            Return If(absolute, Sep, "") & String.Join(Sep, segs)
        End Function

        ' Node-style normalize: process . and .., keep leading separator.
        Private Shared Function Normalize(p As String) As String
            If p Is Nothing OrElse p.Length = 0 Then Return "."
            Dim absolute = IsAbs(p)
            Dim root = ""
            If p.Length >= 2 AndAlso Char.IsLetter(p(0)) AndAlso p(1) = ":"c Then
                root = p.Substring(0, 2)
                p = p.Substring(2)
                absolute = True
            End If
            Dim segs = SplitSegments(p)
            Dim out As New List(Of String)()
            For Each s In segs
                If s = "." Then Continue For
                If s = ".." Then
                    If out.Count > 0 AndAlso out(out.Count - 1) <> ".." Then
                        out.RemoveAt(out.Count - 1)
                    ElseIf Not absolute Then
                        out.Add("..")
                    End If
                Else
                    out.Add(s)
                End If
            Next
            If root <> "" Then Return root & (If(out.Count = 0, Sep, Sep & String.Join(Sep, out)))
            Return Assemble(out, absolute)
        End Function

        Private Shared Function Join(args As Object()) As String
            Dim all As New List(Of String)()
            Dim absolute = False
            For i = 0 To args.Length - 1
                Dim part = JsRuntime.JsStr(args(i))
                If part Is Nothing OrElse part.Length = 0 Then Continue For
                If i = 0 AndAlso IsAbs(part) Then absolute = True
                For Each s In SplitSegments(part) : all.Add(s) : Next
            Next
            Return Normalize(String.Join(Sep, all))
        End Function

        Private Shared Function Resolve(args As Object()) As String
            Dim segs As New List(Of String)()
            Dim absolute = False
            For i = args.Length - 1 To 0 Step -1
                Dim part = JsRuntime.JsStr(args(i))
                If part Is Nothing OrElse part.Length = 0 Then Continue For
                If IsAbs(part) Then
                    segs.InsertRange(0, SplitSegments(part))
                    absolute = True
                    Exit For
                End If
                segs.InsertRange(0, SplitSegments(part))
            Next
            If Not absolute Then
                For Each s In SplitSegments(ProcessModule.Cwd) : segs.Insert(0, s) : Next
                absolute = IsAbs(ProcessModule.Cwd)
            End If
            Return Normalize(String.Join(Sep, segs))
        End Function

        Private Shared Function Basename(args As Object()) As String
            Dim p = JsRuntime.JsStr(Arg(args, 0))
            Dim baseName = Path.GetFileName(p.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            Dim ext = If(args.Length > 1 AndAlso Arg(args, 1) IsNot JsRuntime.Undef, JsRuntime.JsStr(Arg(args, 1)), "")
            If ext <> "" AndAlso baseName.EndsWith(ext) Then
                baseName = baseName.Substring(0, baseName.Length - ext.Length)
            End If
            Return baseName
        End Function

        Private Shared Function Dirname(args As Object()) As String
            Dim p = JsRuntime.JsStr(Arg(args, 0))
            If p Is Nothing OrElse p.Length = 0 Then Return "."
            Dim d = Path.GetDirectoryName(p)
            If d Is Nothing OrElse d.Length = 0 Then Return If(IsAbs(p), Sep, ".")
            Return d
        End Function

        Private Shared Function Extname(args As Object()) As String
            Dim p = JsRuntime.JsStr(Arg(args, 0))
            Return Path.GetExtension(p)
        End Function

        Private Shared Function IsAbsolute(args As Object()) As Boolean
            Return IsAbs(JsRuntime.JsStr(Arg(args, 0)))
        End Function

        Private Shared Function Relative(fromPath As String, toPath As String) As String
            Try
                Return Path.GetRelativePath(fromPath, toPath).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            Catch
                Return toPath
            End Try
        End Function

        Private Shared Function Parse(args As Object()) As Dictionary(Of String, Object)
            Dim p = JsRuntime.JsStr(Arg(args, 0))
            Dim root = ""
            Dim work = p
            If work.Length >= 2 AndAlso Char.IsLetter(work(0)) AndAlso work(1) = ":"c Then
                root = work.Substring(0, 2)
                work = work.Substring(2)
            ElseIf work.StartsWith(Sep) Then
                root = Sep
            End If
            Dim baseName = Path.GetFileName(work.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            Dim ext = Path.GetExtension(baseName)
            Dim nm = If(ext <> "" AndAlso baseName.Length > ext.Length, baseName.Substring(0, baseName.Length - ext.Length), baseName)
            Dim dir = Path.GetDirectoryName(work)
            If dir Is Nothing Then dir = If(root <> "", root, ".")
            Dim d As New Dictionary(Of String, Object) From {
                {"root", root},
                {"dir", dir},
                {"base", baseName},
                {"ext", ext},
                {"name", nm}
            }
            Return d
        End Function

        Private Shared Function FormatPath(obj As Object) As String
            Dim d = DirectCast(obj, Dictionary(Of String, Object))
            Dim dir = StrField(d, "dir")
            Dim baseName = StrField(d, "base")
            If baseName = "" Then
                Dim nm = StrField(d, "name")
                Dim ext = StrField(d, "ext")
                baseName = nm & ext
            End If
            If dir = "" OrElse dir = "." Then Return baseName
            Return If(dir.EndsWith(Sep), dir & baseName, dir & Sep & baseName)
        End Function

        Private Shared Function StrField(d As Dictionary(Of String, Object), key As String) As String
            Dim v As Object = Nothing
            If d.TryGetValue(key, v) AndAlso v IsNot Nothing AndAlso v IsNot JsRuntime.Undef Then Return JsRuntime.JsStr(v)
            Return ""
        End Function

    End Class

End Namespace
