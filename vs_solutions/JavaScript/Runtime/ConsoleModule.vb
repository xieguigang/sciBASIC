#Region "Microsoft.VisualBasic::e3d5a7c21b8f4a069d3e6f18c74b2a05, vs_solutions\JavaScript\Runtime\ConsoleModule.vb"

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

    '     Class ConsoleModule
    '
    '         Properties: Counters, IndentLevel, Marks, Timers
    '
    '         Function: AppendValue, ArgLabel, Assertion, Clear, Count, CountReset
    '                   Dispatch, Elapsed, Emit, Format, FormatTemplate, Group
    '                   GroupEnd, HasText, LevelPrefix, MakeConsole, Profile
    '                   SingleSpan, Slice, SpansText, Table, Time, TimeEnd, TimeLog
    '                   TimeStamp, Trace
    '
    '         Sub: ApplyIndent, Reset
    '
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics
Imports System.Globalization
Imports System.Text

Namespace Runtime

    ''' <summary>
    ''' The JavaScript <c>console</c> object, following
    ''' <see href="https://developer.mozilla.org/zh-CN/docs/Web/API/console">the MDN
    ''' console reference</see>: logging at several levels, printf style format
    ''' specifiers, <c>console.table</c>, grouping with indentation, counters,
    ''' timers, assertions and tracing.
    ''' <para>
    ''' Mirrors the structure of <see cref="FsModule"/>: <see cref="MakeConsole"/>
    ''' builds the singleton object and <see cref="Dispatch"/> is the single method
    ''' table. Unknown members raise a <c>TypeError</c> instead of silently printing.
    ''' </para>
    ''' <para>
    ''' Counters / timers / group indentation are per-thread state of the shared
    ''' singleton, and are reset by <see cref="Reset"/> whenever a script engine
    ''' binds its sink (<see cref="JsRuntime.Io"/>), so consecutive runs never leak
    ''' state into each other.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class ConsoleModule

        Private Sub New()
        End Sub

        ' ===================== per-thread state =====================

        ' ThreadStatic fields cannot carry an initialiser, hence the lazy getters
        <ThreadStatic>
        Private Shared _counters As Dictionary(Of String, Double)

        <ThreadStatic>
        Private Shared _timers As Dictionary(Of String, Stopwatch)

        <ThreadStatic>
        Private Shared _marks As List(Of String)

        <ThreadStatic>
        Private Shared _indent As Integer

        Private Shared ReadOnly Property Counters As Dictionary(Of String, Double)
            Get
                If _counters Is Nothing Then _counters = New Dictionary(Of String, Double)(StringComparer.Ordinal)

                Return _counters
            End Get
        End Property

        Private Shared ReadOnly Property Timers As Dictionary(Of String, Stopwatch)
            Get
                If _timers Is Nothing Then _timers = New Dictionary(Of String, Stopwatch)(StringComparer.Ordinal)

                Return _timers
            End Get
        End Property

        ''' <summary>
        ''' Non-printing console marks: <c>profile</c> / <c>profileEnd</c> events and
        ''' <c>timeStamp</c> records (a text console has no timeline to write into).
        ''' </summary>
        Public Shared ReadOnly Property Marks As List(Of String)
            Get
                If _marks Is Nothing Then _marks = New List(Of String)()

                Return _marks
            End Get
        End Property

        ''' <summary>Current <c>console.group</c> indentation (two spaces per level).</summary>
        Public Shared ReadOnly Property IndentLevel As Integer
            Get
                Return _indent
            End Get
        End Property

        ''' <summary>Forget counters, timers, marks and group indentation of this thread.</summary>
        Public Shared Sub Reset()
            If _counters IsNot Nothing Then _counters.Clear()
            If _timers IsNot Nothing Then _timers.Clear()
            If _marks IsNot Nothing Then _marks.Clear()

            _indent = 0
        End Sub

        ' ===================== module entry points =====================

        ''' <summary>Build the <c>console</c> singleton installed as a JS global.</summary>
        Public Shared Function MakeConsole() As NativeObject
            Return New NativeObject("console")
        End Function

        ''' <summary>Method table of the <c>console</c> object.</summary>
        Public Shared Function Dispatch(method As String, args As Object()) As Object
            Select Case method
                Case "log" : Return Emit("log", Format(args))
                Case "debug" : Return Emit("debug", Format(args))
                Case "info" : Return Emit("info", Format(args))
                Case "warn" : Return Emit("warn", Format(args))
                Case "error" : Return Emit("error", Format(args))
                Case "dir", "dirxml" : Return Emit("log", Format(args))
                Case "trace" : Return Trace(args)
                Case "assert" : Return Assertion(args)
                Case "clear" : Return Clear()
                Case "count" : Return Count(args)
                Case "countReset" : Return CountReset(args)
                Case "group" : Return Group(args)
                Case "groupCollapsed" : Return Group(args)
                Case "groupEnd" : Return GroupEnd(args)
                Case "table" : Return Table(args)
                Case "time" : Return Time(args)
                Case "timeEnd" : Return TimeEnd(args)
                Case "timeLog" : Return TimeLog(args)
                Case "profile" : Return Profile(args, True)
                Case "profileEnd" : Return Profile(args, False)
                Case "timeStamp" : Return TimeStamp(args)
                Case Else : Throw JsRuntimeException.TypeError("console." & method & " is not a function")
            End Select
        End Function

        ' ===================== logging =====================

        ''' <summary>
        ''' printf style formatting of one console call into styled fragments:
        ''' <c>%s</c> (string), <c>%d</c> / <c>%i</c> (integer), <c>%f</c> (float),
        ''' <c>%o</c> / <c>%O</c> / <c>%j</c> (object), <c>%c</c> (CSS style) and
        ''' <c>%%</c> (literal percent).
        ''' <para>
        ''' Only the first argument is scanned for specifiers; a specifier without a
        ''' matching argument and an unknown specifier both stay literal, while the
        ''' remaining arguments are appended space separated (Node semantics).
        ''' </para>
        ''' </summary>
        Public Shared Function Format(args As Object()) As List(Of ConsoleSpan)
            Dim spans As New List(Of ConsoleSpan)

            If args Is Nothing OrElse args.Length = 0 Then
                spans.Add(New ConsoleSpan(""))
                Return spans
            End If

            Dim consumed As Integer = 0

            If TypeOf args(0) Is String Then
                consumed = FormatTemplate(CStr(args(0)), args, spans)
            Else
                spans.Add(New ConsoleSpan(JsRuntime.JsDisplay(args(0))))
            End If

            For i = 1 + consumed To args.Length - 1
                AppendValue(spans, JsRuntime.JsDisplay(args(i)))
            Next

            Return spans
        End Function

        Private Shared Function FormatTemplate(template As String, args As Object(), spans As List(Of ConsoleSpan)) As Integer
            Dim sb As New StringBuilder()
            Dim css As String = Nothing
            Dim consumed As Integer = 0
            Dim i As Integer = 0

            Do While i < template.Length
                If template(i) <> "%"c OrElse i = template.Length - 1 Then
                    sb.Append(template(i))
                    i += 1
                    Continue Do
                End If

                Dim specifier = template(i + 1)
                Dim argumentIndex = 1 + consumed
                Dim hasArgument = argumentIndex < args.Length
                Dim value As Object = Nothing

                Select Case specifier
                    Case "s"c
                        If Not hasArgument Then
                            sb.Append("%s")
                            i += 2
                            Continue Do
                        End If

                        value = JsRuntime.JsStr(args(argumentIndex))
                        consumed += 1
                    Case "d"c, "i"c
                        If Not hasArgument Then
                            sb.Append("%" & specifier)
                            i += 2
                            Continue Do
                        End If

                        value = JsRuntime.JsStr(System.Math.Truncate(JsRuntime.JsNum(args(argumentIndex))))
                        consumed += 1
                    Case "f"c
                        If Not hasArgument Then
                            sb.Append("%f")
                            i += 2
                            Continue Do
                        End If

                        value = JsRuntime.JsStr(JsRuntime.JsNum(args(argumentIndex)))
                        consumed += 1
                    Case "o"c, "O"c, "j"c
                        If Not hasArgument Then
                            sb.Append("%" & specifier)
                            i += 2
                            Continue Do
                        End If

                        value = JsRuntime.JsDisplay(args(argumentIndex))
                        consumed += 1
                    Case "c"c
                        If Not hasArgument Then
                            ' no style argument: keep the specifier out of the text
                            i += 2
                            Continue Do
                        End If

                        ' a %c opens a new styled fragment
                        If sb.Length > 0 Then
                            spans.Add(New ConsoleSpan(sb.ToString(), css))
                            sb.Clear()
                        End If

                        css = JsRuntime.JsStr(args(argumentIndex))
                        consumed += 1
                        i += 2
                        Continue Do
                    Case "%"c
                        sb.Append("%"c)
                        i += 2
                        Continue Do
                    Case Else
                        ' unknown specifier: emit the percent and rescan the next char
                        sb.Append("%"c)
                        i += 1
                        Continue Do
                End Select

                sb.Append(DirectCast(value, String))
                i += 2
            Loop

            If sb.Length > 0 Then spans.Add(New ConsoleSpan(sb.ToString(), css))

            Return consumed
        End Function

        ''' <summary>Append an extra argument, separated by a space from the text so far.</summary>
        Private Shared Sub AppendValue(spans As List(Of ConsoleSpan), text As String)
            If spans.Count = 0 Then
                spans.Add(New ConsoleSpan(text))
                Return
            End If

            Dim last = spans(spans.Count - 1)
            Dim gap = If(HasText(spans), " ", "")

            last.Text = If(last.Text, "") & gap & text
        End Sub

        Private Shared Function HasText(spans As List(Of ConsoleSpan)) As Boolean
            For Each span In spans
                If Not String.IsNullOrEmpty(span.Text) Then Return True
            Next

            Return False
        End Function

        ''' <summary>
        ''' Hand the fragments to the sink: applies the current group indentation and
        ''' the level tag of <c>console.warn</c> / <c>console.error</c>.
        ''' </summary>
        Private Shared Function Emit(level As String, spans As List(Of ConsoleSpan)) As Object
            Dim prefix = LevelPrefix(level)

            If prefix.Length > 0 Then
                ' the indentation is applied afterwards so the tag stays closest to the text
                If spans.Count = 0 Then
                    spans.Add(New ConsoleSpan(prefix))
                Else
                    spans(0).Text = prefix & If(spans(0).Text, "")
                End If
            End If

            If _indent > 0 Then ApplyIndent(spans, New String(" "c, _indent * 2))

            Dim message As New ConsoleMessage(level)

            message.Spans.AddRange(spans)
            JsRuntime.Io.WriteMessage(message)

            Return JsRuntime.Undef
        End Function

        Private Shared Sub ApplyIndent(spans As List(Of ConsoleSpan), indent As String)
            Dim first = True

            For Each span In spans
                Dim text = If(span.Text, "")

                If text.Length = 0 Then Continue For

                text = text.Replace(vbLf, vbLf & indent)

                If first Then
                    text = indent & text
                    first = False
                End If

                span.Text = text
            Next
        End Sub

        Private Shared Function LevelPrefix(level As String) As String
            Select Case LCase(If(level, ""))
                Case "warn" : Return "[warn] "
                Case "error" : Return "[error] "
                Case Else : Return ""
            End Select
        End Function

        Private Shared Function SingleSpan(text As String) As List(Of ConsoleSpan)
            Return New List(Of ConsoleSpan) From {New ConsoleSpan(text)}
        End Function

        ' ===================== trace / assert =====================

        ''' <summary>
        ''' <c>console.trace</c>: the optional label followed by the live JS stack of the
        ''' interpreter, rendered Node style (<c>    at fn (file:line:col)</c>). The
        ''' innermost frame is the function containing this call, pointing at the call
        ''' line; a stack with no interpreter frame (generated code) shows the
        ''' <c>&lt;generated&gt;</c> placeholder.
        ''' </summary>
        Private Shared Function Trace(args As Object()) As Object
            Dim spans = Format(args)
            Dim sb As New StringBuilder()

            sb.Append("Trace")

            If HasText(spans) Then
                sb.Append(": ")
                sb.Append(SpansText(spans))
            End If

            sb.Append(vbLf)
            sb.Append(JsStackTrace.Format(JsStackTrace.Capture()))

            Return Emit("trace", SingleSpan(sb.ToString()))
        End Function

        Private Shared Function Assertion(args As Object()) As Object
            Dim condition = If(args IsNot Nothing AndAlso args.Length > 0, args(0), JsRuntime.Undef)

            If JsRuntime.JsTruthy(condition) Then Return JsRuntime.Undef

            Dim spans As List(Of ConsoleSpan)

            If args.Length > 1 Then
                spans = Format(Slice(args, 1))
                spans(0).Text = "Assertion failed: " & If(spans(0).Text, "")
            Else
                spans = SingleSpan("Assertion failed")
            End If

            Return Emit("assert", spans)
        End Function

        ' ===================== counters =====================

        Private Shared Function Count(args As Object()) As Object
            Dim label = ArgLabel(args, 0, "default")
            Dim total As Double = 0

            Counters.TryGetValue(label, total)
            total += 1
            Counters(label) = total

            Return Emit("count", SingleSpan(label & ": " & JsRuntime.JsStr(total)))
        End Function

        Private Shared Function CountReset(args As Object()) As Object
            Dim label = ArgLabel(args, 0, "default")

            If Not Counters.Remove(label) Then
                Return Emit("warn", SingleSpan("Count for '" & label & "' does not exist"))
            End If

            Return JsRuntime.Undef
        End Function

        ' ===================== groups =====================

        Private Shared Function Group(args As Object()) As Object
            Call Emit("group", Format(args))

            _indent += 1

            Return JsRuntime.Undef
        End Function

        Private Shared Function GroupEnd(args As Object()) As Object
            If _indent > 0 Then
                _indent -= 1
                Return JsRuntime.Undef
            End If

            Return Emit("warn", SingleSpan("console.groupEnd() without a matching console.group()"))
        End Function

        ' ===================== timers =====================

        Private Shared Function Time(args As Object()) As Object
            Dim label = ArgLabel(args, 0, "default")

            If Timers.ContainsKey(label) Then
                Return Emit("warn", SingleSpan("Timer '" & label & "' already exists"))
            End If

            Timers(label) = Stopwatch.StartNew()

            Return JsRuntime.Undef
        End Function

        Private Shared Function TimeEnd(args As Object()) As Object
            Dim label = ArgLabel(args, 0, "default")
            Dim timer As Stopwatch = Nothing

            If Not Timers.TryGetValue(label, timer) Then
                Return Emit("warn", SingleSpan("Timer '" & label & "' does not exist"))
            End If

            timer.Stop()
            Timers.Remove(label)

            Return Emit("time", SingleSpan(label & ": " & Elapsed(timer)))
        End Function

        Private Shared Function TimeLog(args As Object()) As Object
            Dim label = ArgLabel(args, 0, "default")
            Dim timer As Stopwatch = Nothing

            If Not Timers.TryGetValue(label, timer) Then
                Return Emit("warn", SingleSpan("Timer '" & label & "' does not exist"))
            End If

            Dim head = label & ": " & Elapsed(timer)

            If args.Length > 1 Then
                Dim spans = Format(Slice(args, 1))

                spans(0).Text = head & " " & If(spans(0).Text, "")
                Return Emit("time", spans)
            End If

            Return Emit("time", SingleSpan(head))
        End Function

        Private Shared Function Elapsed(timer As Stopwatch) As String
            Return timer.Elapsed.TotalMilliseconds.ToString("0.###", CultureInfo.InvariantCulture) & "ms"
        End Function

        ' ===================== profiling marks =====================

        Private Shared Function Profile(args As Object(), started As Boolean) As Object
            Dim label = ArgLabel(args, 0, "")
            Dim entry = If(started, "profile", "profileEnd") & If(label.Length > 0, ":" & label, "")

            Marks.Add(entry)

            Return JsRuntime.Undef
        End Function

        Private Shared Function TimeStamp(args As Object()) As Object
            Dim label = ArgLabel(args, 0, "")

            Marks.Add("timeStamp" & If(label.Length > 0, ":" & label, "") & "@" & Date.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture))

            Return JsRuntime.Undef
        End Function

        ' ===================== table =====================

        Private Shared Function Table(args As Object()) As Object
            Dim data = If(args IsNot Nothing AndAlso args.Length > 0, args(0), JsRuntime.Undef)
            Dim columns As List(Of String) = Nothing

            If args.Length > 1 AndAlso TypeOf args(1) Is List(Of Object) Then
                columns = DirectCast(args(1), List(Of Object)).Select(AddressOf JsRuntime.JsStr).ToList()
            End If

            Dim rendered = ConsoleTableFormatter.Render(data, columns)

            If rendered Is Nothing Then
                ' scalar / unsuitable input falls back to the console.log behaviour
                Return Emit("log", Format(args))
            End If

            Return Emit("table", SingleSpan(rendered))
        End Function

        ' ===================== helpers =====================

        Private Shared Function Clear() As Object
            JsRuntime.Io.Clear()

            Return JsRuntime.Undef
        End Function

        ''' <summary>Read a string argument (defaulting when omitted or nullish).</summary>
        Private Shared Function ArgLabel(args As Object(), index As Integer, fallback As String) As String
            If args Is Nothing OrElse index >= args.Length Then Return fallback

            Dim value = args(index)

            If value Is Nothing OrElse value Is JsRuntime.Undef Then Return fallback

            Dim text = JsRuntime.JsStr(value)

            Return If(text.Length = 0, fallback, text)
        End Function

        ''' <summary>A sub-range of the argument list, as its own argument array.</summary>
        Private Shared Function Slice(args As Object(), start As Integer) As Object()
            If args Is Nothing OrElse start >= args.Length Then Return New Object() {}

            Dim result(args.Length - start - 1) As Object

            Call Array.Copy(args, start, result, 0, result.Length)

            Return result
        End Function

        ''' <summary>Fragments flattened back to plain text.</summary>
        Private Shared Function SpansText(spans As List(Of ConsoleSpan)) As String
            Dim sb As New StringBuilder()

            For Each span In spans
                sb.Append(span.Text)
            Next

            Return sb.ToString()
        End Function

    End Class

End Namespace
