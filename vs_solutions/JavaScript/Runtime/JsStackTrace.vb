#Region "Microsoft.VisualBasic::b67c1e409a3d4f52bd8e17c6a05f3d28, vs_solutions\JavaScript\Runtime\JsStackTrace.vb"

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

    '     Class JsStackFrame
    '
    '         Properties: Column, DisplayName, FunctionName, IsMarker, LineNumber
    '                   TruncatedFrames
    '
    '         Sub: MoveTo, Sync
    '
    '         Function: Clone
    '
    '     Module JsStackTrace
    '
    '         Properties: CurrentFrame, Depth, Frames, StackTraceLimit
    '
    '         Function: Capture, CaptureCount, Format, FormatFrame, FormatWithHeader
    '                   JsLocation, PushFrame, SingleFrame
    '
    '         Sub: PopFrame, Reset, UpdatePosition
    '
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Text
Imports Diagnostics = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime

    ''' <summary>
    ''' One JavaScript call frame, stored as a
    ''' <see cref="Diagnostics.StackFrame"/> (the engine reuses the platform stack frame
    ''' object) extended with the two things the JS world needs but the base class
    ''' does not carry: the JS function name and the source column.
    ''' <para>
    ''' The position is kept in integer fields (<see cref="LineNumber"/> /
    ''' <see cref="Column"/>) because the interpreter stamps the position on every
    ''' executed statement — converting to text there would allocate on the hot path.
    ''' <see cref="Sync"/> pushes the integers into the inherited
    ''' <see cref="Diagnostics.StackFrame.Line"/> string, and is called when the frame
    ''' is cloned for a captured stack.
    ''' </para>
    ''' </summary>
    Public Class JsStackFrame
        Inherits Diagnostics.StackFrame

        ''' <summary>Frame name of anonymous functions and of the top level script.</summary>
        Public Const AnonymousFunction As String = "<anonymous>"

        ''' <summary>
        ''' Placeholder frame used when no interpreter frame exists (the generated VB.NET
        ''' code path, which does not run through <c>Interpreter.InvokeBody</c>).
        ''' </summary>
        Public Const GeneratedFunction As String = "<generated>"

        ' mutable: the interpreter stamps the executing statement position on the hot path
        Private _line As Integer
        Private ReadOnly _truncated As Integer

        ''' <summary>JS function name of this frame.</summary>
        Public ReadOnly Property FunctionName As String

        ''' <summary>Source column (1 based, 0 when unknown).</summary>
        Public Property Column As Integer

        ''' <summary>Source line (1 based, 0 when unknown) — integer view of <see cref="Diagnostics.StackFrame.Line"/>.</summary>
        Public ReadOnly Property LineNumber As Integer
            Get
                Return _line
            End Get
        End Property

        ''' <summary>True for the synthetic "... (N frames truncated)" marker frame.</summary>
        Public ReadOnly Property IsMarker As Boolean
            Get
                Return _truncated > 0
            End Get
        End Property

        ''' <summary>Number of frames dropped from the captured stack (marker frame only).</summary>
        Public ReadOnly Property TruncatedFrames As Integer
            Get
                Return _truncated
            End Get
        End Property

        Friend Sub New(name As String, file As String, line As Integer, column As Integer)
            Me.FunctionName = If(String.IsNullOrEmpty(name), AnonymousFunction, name)
            Me.Column = System.Math.Max(0, column)
            _line = System.Math.Max(0, line)
            Me.File = If(file, "")

            ' the base Method ctor splits on '.' which would corrupt JS names like "a.b"
            Me.Method = New Diagnostics.Method With {
                .Method = Me.FunctionName,
                .[Module] = "",
                .[Namespace] = ""
            }

            Call Sync()
        End Sub

        ''' <summary>Truncation marker frame (see <see cref="JsStackTrace.MaxCaptureDepth"/>).</summary>
        Friend Sub New(truncated As Integer)
            _truncated = System.Math.Max(1, truncated)
            Me.FunctionName = "... (" & _truncated & " frames truncated)"
            Me.File = ""
            Me.Method = New Diagnostics.Method With {
                .Method = Me.FunctionName,
                .[Module] = "",
                .[Namespace] = ""
            }

            Call Sync()
        End Sub

        ''' <summary>Name shown in the Node style rendering.</summary>
        Public ReadOnly Property DisplayName As String
            Get
                Return If(String.IsNullOrEmpty(FunctionName), AnonymousFunction, FunctionName)
            End Get
        End Property

        ''' <summary>
        ''' Move the frame to the statement being executed: two integer writes, no
        ''' allocation (this runs for every executed statement).
        ''' </summary>
        Friend Sub MoveTo(line As Integer, column As Integer)
            _line = System.Math.Max(0, line)
            Me.Column = System.Math.Max(0, column)
        End Sub

        ''' <summary>Publish the integer position as the inherited text fields.</summary>
        Friend Sub Sync()
            Me.Line = If(IsMarker, "", _line.ToString(CultureInfo.InvariantCulture))
        End Sub

        ''' <summary>Snapshot of this frame, so a captured stack never mutates afterwards.</summary>
        Friend Function Clone() As JsStackFrame
            If IsMarker Then Return New JsStackFrame(_truncated)

            Return New JsStackFrame(FunctionName, File, _line, Column)
        End Function

    End Class

    ''' <summary>
    ''' Per-thread JavaScript call stack of the interpreter, rendered in the Node
    ''' style (<c>    at name (file:line:col)</c>).
    ''' <para>
    ''' The interpreter pushes a frame per invoked function body
    ''' (<c>Interpreter.InvokeBody</c>) and stamps the executing statement position on
    ''' the innermost frame, so an error captured anywhere carries the real JS
    ''' location. Errors freeze <see cref="Capture"/> at construction time, therefore
    ''' <c>e.stack</c> stays correct even after the frames have been popped
    ''' (<c>try { ... } catch (e) { e.stack }</c>).
    ''' </para>
    ''' <para>
    ''' The state is <c>&lt;ThreadStatic&gt;</c> (independent engines on different
    ''' threads never mix frames). <see cref="Reset"/> is called when a script run
    ''' starts, so consecutive runs in the same thread start from an empty stack.
    ''' </para>
    ''' </summary>
    Public Module JsStackTrace

        ''' <summary>Default of <see cref="StackTraceLimit"/> (Node uses 10).</summary>
        Public Const DefaultLimit As Integer = 10

        ''' <summary>Absolute cap on materialised frames, used when the limit is disabled.</summary>
        Public Const MaxCaptureDepth As Integer = 500

        ' ThreadStatic fields cannot carry an initialiser, so a sentinel marks "unset"
        Private Const Unset As Integer = Integer.MinValue

        ' module members are implicitly shared, so no Shared modifier here
        <ThreadStatic>
        Private _frames As List(Of JsStackFrame)

        <ThreadStatic>
        Private _limit As Integer

        ''' <summary>Live frame stack of this thread (root frame first, innermost last).</summary>
        Public ReadOnly Property Frames As List(Of JsStackFrame)
            Get
                If _frames Is Nothing Then _frames = New List(Of JsStackFrame)()

                Return _frames
            End Get
        End Property

        ''' <summary>Number of live frames.</summary>
        Public ReadOnly Property Depth As Integer
            Get
                If _frames Is Nothing Then Return 0

                Return _frames.Count
            End Get
        End Property

        ''' <summary>Innermost frame, or Nothing when the stack is empty.</summary>
        Public ReadOnly Property CurrentFrame As JsStackFrame
            Get
                If _frames Is Nothing OrElse _frames.Count = 0 Then Return Nothing

                Return _frames(_frames.Count - 1)
            End Get
        End Property

        ''' <summary>
        ''' Node's <c>Error.stackTraceLimit</c>: maximum number of frames captured into
        ''' a stack; <c>&lt;= 0</c> captures everything (up to <see cref="MaxCaptureDepth"/>).
        ''' </summary>
        Public Property StackTraceLimit As Integer
            Get
                If _limit = Unset Then Return DefaultLimit

                Return _limit
            End Get
            Set(value As Integer)
                _limit = value
            End Set
        End Property

        ' ===================== interpreter hooks =====================

        ''' <summary>Push a frame for an invoked function body.</summary>
        Public Function PushFrame(name As String, Optional file As String = Nothing,
                                  Optional line As Integer = 0, Optional column As Integer = 0) As JsStackFrame

            Dim frame As New JsStackFrame(name, file, line, column)

            Frames.Add(frame)

            Return frame
        End Function

        ''' <summary>Pop the innermost frame (silently ignored when the stack is empty).</summary>
        Public Sub PopFrame()
            If _frames Is Nothing Then Return
            If _frames.Count = 0 Then Return

            _frames.RemoveAt(_frames.Count - 1)
        End Sub

        ''' <summary>
        ''' Stamp the statement being executed on the innermost frame (hot path: two
        ''' integer writes).
        ''' </summary>
        Public Sub UpdatePosition(line As Integer, column As Integer)
            Dim frame = CurrentFrame

            If frame Is Nothing Then Return

            frame.MoveTo(line, column)
        End Sub

        ''' <summary>
        ''' Freeze the current stack as a snapshot list, innermost frame first, honouring
        ''' <see cref="StackTraceLimit"/>; never returns Nothing.
        ''' </summary>
        Public Function Capture() As List(Of JsStackFrame)
            Dim captured As New List(Of JsStackFrame)()
            Dim frameCount = Depth

            If frameCount = 0 Then Return captured

            Dim take As Integer = CaptureCount(frameCount)

            For i = 0 To take - 1
                captured.Add(_frames(frameCount - 1 - i).Clone())
            Next

            ' tell the reader that the stack was cut short
            If take < frameCount Then captured.Add(New JsStackFrame(frameCount - take))

            Return captured
        End Function

        Private Function CaptureCount(frameCount As Integer) As Integer
            Dim frameLimit = StackTraceLimit

            If frameLimit <= 0 Then frameLimit = MaxCaptureDepth

            Return System.Math.Min(System.Math.Min(frameCount, frameLimit), MaxCaptureDepth)
        End Function

        ''' <summary>
        ''' A one frame stack for a location known without an interpreter stack — a syntax
        ''' error reported by the parser, for instance, which hosts render as
        ''' <c>SyntaxError</c> plus its position.
        ''' </summary>
        Public Function SingleFrame(name As String, file As String, line As Integer, column As Integer) As List(Of JsStackFrame)
            Return New List(Of JsStackFrame) From {New JsStackFrame(name, file, line, column)}
        End Function

        ''' <summary>Clear the frame stack (start of a script run).</summary>
        Public Sub Reset()
            If _frames IsNot Nothing Then _frames.Clear()

            _limit = Unset
        End Sub

        ' ===================== node style rendering =====================

        ''' <summary>
        ''' Render frames the way Node does, one per line, innermost first:
        ''' <c>    at name (file:line:col)</c>. An empty stack renders the
        ''' <c>&lt;generated&gt;</c> placeholder frame.
        ''' </summary>
        Public Function Format(frames As List(Of JsStackFrame)) As String
            If frames Is Nothing OrElse frames.Count = 0 Then
                Return "    at " & JsStackFrame.GeneratedFunction
            End If

            Dim sb As New StringBuilder()

            For Each frame In frames
                If sb.Length > 0 Then sb.Append(vbLf)

                sb.Append(FormatFrame(frame))
            Next

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Node style two part stack: <c>"Name: message"</c> followed by the frames.
        ''' </summary>
        Public Function FormatWithHeader(name As String, message As String, frames As List(Of JsStackFrame)) As String
            Dim text = If(message, "")

            If text.Length = 0 Then
                text = If(String.IsNullOrEmpty(name), "Error", name)
            Else
                text = If(String.IsNullOrEmpty(name), text, name & ": " & text)
            End If

            Return text & vbLf & Format(frames)
        End Function

        Private Function FormatFrame(frame As Diagnostics.StackFrame) As String
            Dim js = TryCast(frame, JsStackFrame)

            If js IsNot Nothing Then
                If js.IsMarker Then Return "    ... (" & js.TruncatedFrames & " frames truncated)"

                Return "    at " & js.DisplayName & " (" & JsLocation(js.File, js.LineNumber, js.Column) & ")"
            End If

            ' a plain platform StackFrame (host supplied): best effort
            Dim name = If(frame.Method IsNot Nothing, frame.Method.Method, Nothing)
            Dim line As Integer = 0

            Call Integer.TryParse(If(frame.Line, ""), NumberStyles.Integer, CultureInfo.InvariantCulture, line)

            Return "    at " & If(String.IsNullOrEmpty(name), JsStackFrame.AnonymousFunction, name) &
                   " (" & JsLocation(frame.File, line, 0) & ")"
        End Function

        Private Function JsLocation(file As String, line As Integer, column As Integer) As String
            Dim source = If(String.IsNullOrEmpty(file), JsStackFrame.AnonymousFunction, file)

            If line <= 0 Then Return source

            Return source & ":" & line.ToString(CultureInfo.InvariantCulture) &
                   ":" & System.Math.Max(1, column).ToString(CultureInfo.InvariantCulture)
        End Function

    End Module

End Namespace
