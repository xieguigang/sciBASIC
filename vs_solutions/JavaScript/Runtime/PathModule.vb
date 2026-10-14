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

        Friend Shared ReadOnly Sep As String = System.IO.Path.DirectorySeparatorChar.ToString()
        Friend Shared ReadOnly Delim As String = System.IO.Path.PathSeparator.ToString()

        Public Shared Function MakeModule() As JsModule
            Dim props As New System.Collections.Generic.Dictionary(Of String, Object) From {
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
                Case "isAbsolute" : Return IsAbsolute(args)
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
        Private Shared Function SplitSegments(p As String) As System.Collections.Generic.List(Of String)
            Dim parts = p.Split(New Char() {System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries)
            Dim segs As New System.Collections.Generic.List(Of String)()
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

        Private Shared Function Assemble(segs As System.Collections.Generic.List(Of String), absolute As Boolean) As String
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
            Dim out As New System.Collections.Generic.List(Of String)()
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
            Dim all As New System.Collections.Generic.List(Of String)()
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
            Dim segs As New System.Collections.Generic.List(Of String)()
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
            Dim baseN = System.IO.Path.GetFileName(p.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar))
            Dim ext = If(args.Length > 1 AndAlso Arg(args, 1) IsNot JsRuntime.Undef, JsRuntime.JsStr(Arg(args, 1)), "")
            If ext <> "" AndAlso baseN.EndsWith(ext) Then
                baseN = baseN.Substring(0, baseN.Length - ext.Length)
            End If
            Return baseN
        End Function

        Private Shared Function Dirname(args As Object()) As String
            Dim p = JsRuntime.JsStr(Arg(args, 0))
            If p Is Nothing OrElse p.Length = 0 Then Return "."
            ' Node preserves the input separator style: it only trims trailing
            ' separators and chops at the last / or \ (GetDirectoryName collapses to "\").
            Dim endIdx = p.Length
            While endIdx > 0 AndAlso (p(endIdx - 1) = "\"c OrElse p(endIdx - 1) = "/"c)
                endIdx -= 1
            End While
            If endIdx = 0 Then Return If(IsAbs(p), Sep, ".")
            Dim lastSep = -1
            For i = endIdx - 1 To 0 Step -1
                If p(i) = "\"c OrElse p(i) = "/"c Then
                    lastSep = i
                    Exit For
                End If
            Next
            If lastSep = -1 Then
                ' no separator at all → e.g. "C:foo" is drive-relative → "."
                If endIdx = 2 AndAlso Char.IsLetter(p(0)) AndAlso p(1) = ":"c Then Return p.Substring(0, 2) & Sep
                Return "."
            End If
            Dim dir = p.Substring(0, lastSep)
            If dir.Length = 0 Then Return Sep
            Return dir
        End Function

        Private Shared Function Extname(args As Object()) As String
            Dim p = JsRuntime.JsStr(Arg(args, 0))
            Return System.IO.Path.GetExtension(p)
        End Function

        Private Shared Function IsAbsolute(args As Object()) As Boolean
            Return IsAbs(JsRuntime.JsStr(Arg(args, 0)))
        End Function

        Private Shared Function Relative(fromPath As String, toPath As String) As String
            Try
                Return System.IO.Path.GetRelativePath(fromPath, toPath).Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar)
            Catch
                Return toPath
            End Try
        End Function

        Private Shared Function Parse(args As Object()) As System.Collections.Generic.Dictionary(Of String, Object)
            Dim p = JsRuntime.JsStr(Arg(args, 0))
            Dim root = ""
            ' Drive letter form: "C:\..." -> root "C:\", "C:foo" -> root "C:"
            If p.Length >= 2 AndAlso Char.IsLetter(p(0)) AndAlso p(1) = ":"c Then
                If p.Length >= 3 AndAlso (p(2) = "\"c OrElse p(2) = "/"c) Then
                    root = p.Substring(0, 3)
                Else
                    root = p.Substring(0, 2)
                End If
            ElseIf p.StartsWith("\\") Then
                ' UNC: \\server\share\... -> root "\\server\share"
                Dim idx = p.IndexOf("\"c, 2)
                If idx > 2 Then
                    Dim idx2 = p.IndexOf("\"c, idx + 1)
                    If idx2 > idx Then
                        root = p.Substring(0, idx2)
                    Else
                        root = p
                    End If
                Else
                    root = p
                End If
            ElseIf p.StartsWith(Sep) Then
                root = Sep
            End If
            ' Use the full path so the drive prefix is preserved in dir.
            Dim baseN = System.IO.Path.GetFileName(p.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar))
            Dim ext = System.IO.Path.GetExtension(baseN)
            Dim nm = If(ext <> "" AndAlso baseN.Length > ext.Length, baseN.Substring(0, baseN.Length - ext.Length), baseN)
            Dim dir = System.IO.Path.GetDirectoryName(p)
            If dir Is Nothing Then dir = If(root <> "", root, ".")
            Dim d As New System.Collections.Generic.Dictionary(Of String, Object) From {
                {"root", root},
                {"dir", dir},
                {"base", baseN},
                {"ext", ext},
                {"name", nm}
            }
            Return d
        End Function

        Private Shared Function FormatPath(obj As Object) As String
            Dim d = DirectCast(obj, System.Collections.Generic.Dictionary(Of String, Object))
            Dim dir = StrField(d, "dir")
            Dim baseN = StrField(d, "base")
            If baseN = "" Then
                Dim nm = StrField(d, "name")
                Dim ext = StrField(d, "ext")
                baseN = nm & ext
            End If
            If dir = "" OrElse dir = "." Then Return baseN
            Return If(dir.EndsWith(Sep), dir & baseN, dir & Sep & baseN)
        End Function

        Private Shared Function StrField(d As System.Collections.Generic.Dictionary(Of String, Object), key As String) As String
            Dim v As Object = Nothing
            If d.TryGetValue(key, v) AndAlso v IsNot Nothing AndAlso v IsNot JsRuntime.Undef Then Return JsRuntime.JsStr(v)
            Return ""
        End Function

    End Class

End Namespace
