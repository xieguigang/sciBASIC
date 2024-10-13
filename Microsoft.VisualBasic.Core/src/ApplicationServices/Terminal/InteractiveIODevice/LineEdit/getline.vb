#Region "Microsoft.VisualBasic::6f9e818f184df24fdc0a0bbadf4e9288, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\LineEdit\getline.vb"

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


    ' Code Statistics:

    '   Total Lines: 1251
    '    Code Lines: 714 (57.07%)
    ' Comment Lines: 360 (28.78%)
    '    - Xml Docs: 68.06%
    ' 
    '   Blank Lines: 177 (14.15%)
    '     File Size: 45.77 KB


    '     Class LineEditor
    ' 
    '         Properties: HeuristicsMode, LineCount, TabAtStartCompletes
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Edit, HeuristicAutoComplete, TextToRenderPos, TextToScreenPos, WordBackward
    '                   WordForward
    ' 
    '         Sub: CmdBackspace, CmdBackwardWord, CmdDebug, CmdDeleteBackword, CmdDeleteChar
    '              CmdDeleteWord, CmdDone, CmdDown, CmdEnd, CmdForwardWord
    '              CmdHistoryNext, CmdHistoryPrev, CmdHome, CmdKillToEOF, CmdLeft
    '              CmdRefresh, CmdReverseSearch, CmdRight, CmdTabOrComplete, CmdUp
    '              CmdYank, Complete, ComputeRendered, EditLoop, ForceCursor
    '              GetUnixConsoleReset, HandleChar, HideCompletions, HistoryUpdateLine, InitText
    '              InsertChar, InsertTextAtCursor, InterruptEdit, Render, RenderAfter
    '              RenderFrom, ReverseSearch, SaveHistory, SearchAppend, SetPrompt
    '              SetSearchPrompt, SetText, ShowCompletions, UpdateCompletionWindow, UpdateCursor
    '              UpdateHomeRow
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' getline.cs: A command line editor
'
' Authors:
'   Miguel de Icaza (miguel@microsoft.com)
'
' Copyright 2008 Novell, Inc.
' Copyright 2016 Xamarin Inc
' Copyright 2017 Microsoft
'
' Completion wanted:
'
'   * Enable bash-like completion window the window as an option for non-GUI people?
'
'   * Continue completing when Backspace is used?
'
'   * Should we keep the auto-complete on "."?
'
'   * Completion produces an error if the value is not resolvable, we should hide those errors
'
' Dual-licensed under the terms of the MIT X11 license or the
' Apache License 2.0
'
' USE -define:DEMO to build this as a standalone file and test it
'
' TODO:
'    Enter an error (a = 1);  Notice how the prompt is in the wrong line
'		This is caused by Stderr not being tracked by System.Console.
'    Completion support
'    Why is Thread.Interrupt not working?   Currently I resort to Abort which is too much.
'
' Limitations in System.Console:
'    Console needs SIGWINCH support of some sort
'    Console needs a way of updating its position after things have been written
'    behind its back (P/Invoke puts for example).
'    System.Console needs to get the DELETE character, and report accordingly.
'
' Bug:
'   About 8 lines missing, type "Con<TAB>" and not enough lines are inserted at the bottom.
' 
'
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.Language.UnixBash
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports std = System.Math

Namespace ApplicationServices.Terminal.LineEdit

    ''' <summary>
    ''' Interactive line editor.
    ''' </summary>
    ''' <remarks>
    ''' Interactive line editor.
    ''' 
    ''' > https://github.com/mono/LineEditor
    ''' 
    '''   <para>
    '''     LineEditor is an interative line editor for .NET applications that provides
    '''     editing capabilities for an input line with common editing capabilities and
    '''     navigation expected in modern application as well as history, incremental
    '''     search over the history, completion (both textual or visual) and various 
    '''     Emacs-like commands.
    '''   </para>
    '''   <para>
    '''     When you create your line editor, you can pass the name of your application, 
    '''     which will be used to load and save the history of commands entered by the user
    '''     for this particular application.    
    '''   </para>
    '''   <para>
    '''     
    '''   </para>
    '''   <example>
    '''     The following example shows how you can instantiate a line editor that
    '''     can provide code completion for some words when the user presses TAB
    '''     and how the user can edit them. 
    '''     <code>
    ''' LineEditor le = new LineEditor ("myshell") { HeuristicsMode = "csharp" };
    ''' le.AutoCompleteEvent += delegate (string line, int point){
    '''     string prefix = "";
    '''     var completions = new string [] { 
    '''         "One", "Two", "Three", "Four", "Five", 
    '''          "Six", "Seven", "Eight", "Nine", "Ten" 
    '''     };
    '''     return new Mono.Terminal.LineEditor.Completion(prefix, completions);
    ''' };
    ''' 		
    ''' string s;
    ''' 		
    ''' while ((s = le.Edit("shell> ", "")) != null)
    '''    Console.WriteLine("You typed: [{0}]", s);			}
    '''     </code>
    '''   </example>
    '''   <para>
    '''      Users can use the cursor keys to navigate both the text on the current
    '''      line, or move back and forward through the history of commands that have
    '''      been entered.   
    '''   </para>
    '''   <para>
    '''     The interactive commands and keybindings are inspired by the GNU bash and
    '''     GNU readline capabilities and follow the same concepts found there.
    '''   </para>
    '''   <para>
    '''      Copy and pasting works like bash, deleted words or regions are added to 
    '''      the kill buffer.   Repeated invocations of the same deleting operation will
    '''      append to the kill buffer (for example, repeatedly deleting words) and to
    '''      paste the results you would use the Control-y command (yank).
    '''   </para>
    '''   <para>
    '''      The history search capability is triggered when you press 
    '''      Control-r to start a reverse interactive-search
    '''      and start typing the text you are looking for, the edited line will
    '''      be updated with matches.  Typing control-r again will go to the next
    '''      match in history and so on.
    '''   </para>
    '''   <list type="table"> 
    '''     <listheader>
    '''       <term>Shortcut</term>
    '''       <description>Action performed</description>
    '''     </listheader>
    '''     <item>
    '''        <term>Left cursor, Control-b</term>
    '''        <description>
    '''          Moves the editing point left.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Right cursor, Control-f</term>
    '''        <description>
    '''          Moves the editing point right.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Alt-b</term>
    '''        <description>
    '''          Moves one word back.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Alt-f</term>
    '''        <description>
    '''          Moves one word forward.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Up cursor, Control-p</term>
    '''        <description>
    '''          Selects the previous item in the editing history.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Down cursor, Control-n</term>
    '''        <description>
    '''          Selects the next item in the editing history.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Home key, Control-a</term>
    '''        <description>
    '''          Moves the cursor to the beginning of the line.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>End key, Control-e</term>
    '''        <description>
    '''          Moves the cursor to the end of the line.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Delete, Control-d</term>
    '''        <description>
    '''          Deletes the character in front of the cursor.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Backspace</term>
    '''        <description>
    '''          Deletes the character behind the cursor.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Tab</term>
    '''        <description>
    '''           Triggers the completion and invokes the AutoCompleteEvent which gets
    '''           both the line contents and the position where the cursor is.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Control-k</term>
    '''        <description>
    '''          Deletes the text until the end of the line and replaces the kill buffer
    '''          with the deleted text.   You can paste this text in a different place by
    '''          using Control-y.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Control-l refresh</term>
    '''        <description>
    '''           Clears the screen and forces a refresh of the line editor, useful when
    '''           a background process writes to the console and garbles the contents of
    '''           the screen.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Control-r</term>
    '''        <description>
    '''          Initiates the reverse search in history.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Alt-backspace</term>
    '''        <description>
    '''           Deletes the word behind the cursor and adds it to the kill ring.  You 
    '''           can paste the contents of the kill ring with Control-y.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Alt-d</term>
    '''        <description>
    '''           Deletes the word above the cursor and adds it to the kill ring.  You 
    '''           can paste the contents of the kill ring with Control-y.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Control-y</term>
    '''        <description>
    '''           Pastes the content of the kill ring into the current position.
    '''        </description>
    '''     </item>
    '''     <item>
    '''        <term>Control-q</term>
    '''        <description>
    '''          Quotes the next input character, to prevent the normal processing of
    '''          key handling to take place.
    '''        </description>
    '''     </item>
    '''   </list>
    ''' </remarks>
    Public Class LineEditor

        ''' <summary>
        ''' The heuristics mode used by code completion.
        ''' </summary>
        ''' <remarks>
        '''    <para>
        '''      This controls the heuristics style used to show the code
        '''      completion popup as well as when to accept an entry.
        '''    </para>
        '''    <para>
        '''      The default value is null which requires the user to explicitly
        '''      use the TAB key to trigger a completion.    
        '''    </para>
        '''    <para>
        '''      Another possible value is "csharp" which will trigger auto-completion when a 
        '''      "." is entered.
        '''    </para>
        ''' </remarks>
        Public Property HeuristicsMode As Boolean

        'static StreamWriter log;

        ' The text being edited.
        Private text As StringBuilder

        ' The text as it is rendered (replaces (char)1 with ^A on display for example).
        Private rendered_text As StringBuilder

        ''' <summary>
        ''' The prompt specified, and the prompt shown to the user.
        ''' </summary>
        Private prompt As String
        Private shown_prompt As String

        ' The current cursor position, indexes into "text", for an index
        ' into rendered_text, use TextToRenderPos
        Private cursor As Integer

        ' The row where we started displaying data.
        Private home_row As Integer

        ''' <summary>
        ''' The maximum length that has been displayed on the screen
        ''' </summary>
        Private max_rendered As Integer

        ' If we are done editing, this breaks the interactive loop
        Private done As Boolean = False

        ' The thread where the Editing started taking place
        Private edit_thread As Thread

        ' Our object that tracks history
        Private history As HistoryType

        ' The contents of the kill buffer (cut/paste in Emacs parlance)
        Private kill_buffer As String = ""

        ' The string being searched for
        Private search As String
        Private last_search As String

        ' whether we are searching (-1= reverse; 0 = no; 1 = forward)
        Private searching As Integer

        ' The position where we found the match.
        Private match_at As Integer

        ' Used to implement the Kill semantics (multiple Alt-Ds accumulate)
        Private last_handler As KeyHandler

        ' If we have a popup completion, this is not null and holds the state.
        Private current_completion As CompletionState

        ' If this is set, it contains an escape sequence to reset the Unix colors to the ones that were used on startup
        Friend Shared unix_reset_colors As Byte()

        ' This contains a raw stream pointing to stdout, used to bypass the TermInfoDriver
        Friend Shared unix_raw_output As Stream

        ''' <summary>
        '''   Invoked when the user requests auto-completion using the tab character
        ''' </summary>
        ''' <remarks>
        '''    The result is null for no values found, an array with a single
        '''    string, in that case the string should be the text to be inserted
        '''    for example if the word at pos is "T", the result for a completion
        '''    of "ToString" should be "oString", not "ToString".
        '''
        '''    When there are multiple results, the result should be the full
        '''    text
        ''' </remarks>
        Public AutoCompleteEvent As AutoCompleteHandler

        ''' <summary>
        ''' max width of the auto-complete form window width in chars
        ''' </summary>
        Public MaxWidth As Integer = 50

        Private Shared handlers As Handler()

        Private ReadOnly isWindows As Boolean

        ''' <summary>
        ''' Initializes a new instance of the LineEditor, using the specified name for 
        ''' retrieving and storing the history.   The history will default to 10 entries.
        ''' </summary>
        ''' <param name="name">Prefix for storing the editing history.</param>
        Public Sub New(name As String)
            Me.New(name, 10)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the LineEditor, using the specified name for 
        ''' retrieving and storing the history.   
        ''' </summary>
        ''' <param name="name">Prefix for storing the editing history.</param>
        ''' <param name="histsize">Number of entries to store in the history file.</param>
        Public Sub New(name As String, histsize As Integer)
            ' Emacs keys
            handlers = New Handler() {
                New Handler(ConsoleKey.Home, AddressOf CmdHome),
                New Handler(ConsoleKey.End, AddressOf CmdEnd),
                New Handler(ConsoleKey.LeftArrow, AddressOf CmdLeft),
                New Handler(ConsoleKey.RightArrow, AddressOf CmdRight),
                New Handler(ConsoleKey.UpArrow, AddressOf CmdUp, resetCompletion:=False),
                New Handler(ConsoleKey.DownArrow, AddressOf CmdDown, resetCompletion:=False),
                New Handler(ConsoleKey.Enter, AddressOf CmdDone, resetCompletion:=False),
                New Handler(ConsoleKey.Backspace, AddressOf CmdBackspace, resetCompletion:=False),
                New Handler(ConsoleKey.Delete, AddressOf CmdDeleteChar),
                New Handler(ConsoleKey.Tab, AddressOf CmdTabOrComplete, resetCompletion:=False),
                Handler.Control("A"c, New KeyHandler(AddressOf CmdHome)),
                Handler.Control("E"c, New KeyHandler(AddressOf CmdEnd)),
                Handler.Control("B"c, New KeyHandler(AddressOf CmdLeft)),
                Handler.Control("F"c, New KeyHandler(AddressOf CmdRight)),
                Handler.Control("P"c, New KeyHandler(AddressOf CmdUp), resetCompletion:=False),
                Handler.Control("N"c, New KeyHandler(AddressOf CmdDown), resetCompletion:=False),
                Handler.Control("K"c, New KeyHandler(AddressOf CmdKillToEOF)),
                Handler.Control("Y"c, New KeyHandler(AddressOf CmdYank)),
                Handler.Control("D"c, New KeyHandler(AddressOf CmdDeleteChar)),
                Handler.Control("L"c, New KeyHandler(AddressOf CmdRefresh)),
                Handler.Control("R"c, New KeyHandler(AddressOf CmdReverseSearch)),
                Handler.Control("G"c, Sub()
                                          ' DEBUG
                                          'Handler.Control ('T', CmdDebug),

                                          ' quote
                                      End Sub),
                Handler.Alt("B"c, ConsoleKey.B, New KeyHandler(AddressOf CmdBackwardWord)),
                Handler.Alt("F"c, ConsoleKey.F, New KeyHandler(AddressOf CmdForwardWord)),
                Handler.Alt("D"c, ConsoleKey.D, New KeyHandler(AddressOf CmdDeleteWord)),
                Handler.Alt(ASCII.BS, ConsoleKey.Backspace, New KeyHandler(AddressOf CmdDeleteBackword)),
                Handler.Control("Q"c, Sub() HandleChar(Console.ReadKey(True).KeyChar))
            }

            rendered_text = New StringBuilder()
            text = New StringBuilder()
            history = New HistoryType(name, histsize)
            isWindows = App.IsMicrosoftPlatform

            Call GetUnixConsoleReset()
            'if (File.Exists ("log"))File.Delete ("log");
            'log = File.CreateText ("log"); 
        End Sub

        ' On Unix, there is a "default" color which is not represented by any colors in
        ' ConsoleColor and it is not possible to set is by setting the ForegroundColor or
        ' BackgroundColor properties, so we have to use the terminfo driver in Mono to
        ' fetch these values

        Private Sub GetUnixConsoleReset()
            '
            ' On Unix, we want to be able to reset the color for the pop-up completion
            '
            If isWindows Then Return

            ' Sole purpose of this call is to initialize the Terminfo driver
            Dim x = Console.CursorLeft

            Try
                Dim terminfo_driver = Type.GetType("System.ConsoleDriver")?.GetField("driver", BindingFlags.Static Or BindingFlags.NonPublic)?.GetValue(Nothing)
                If terminfo_driver Is Nothing Then Return

                Dim unix_reset_colors_str = TryCast((terminfo_driver?.GetType()?.GetField("origPair", BindingFlags.Instance Or BindingFlags.NonPublic))?.GetValue(terminfo_driver), String)

                If Not unix_reset_colors_str Is Nothing Then
                    unix_reset_colors = Encoding.UTF8.GetBytes(unix_reset_colors_str)
                End If

                unix_raw_output = Console.OpenStandardOutput()
            Catch e As Exception
                Console.WriteLine("Error: " & e.ToString())
            End Try
        End Sub


        Private Sub CmdDebug()
            history.Dump()
            Console.WriteLine()
            Render()
        End Sub

        Private Sub Render()
            Console.Write(shown_prompt)
            Console.Write(rendered_text)

            Dim max = std.Max(rendered_text.Length + shown_prompt.Length, max_rendered)

            For i As Integer = rendered_text.Length + shown_prompt.Length To max_rendered - 1
                Console.Write(" "c)
            Next
            max_rendered = shown_prompt.Length + rendered_text.Length

            ' Write one more to ensure that we always wrap around properly if we are at the
            ' end of a line.
            Console.Write(" "c)

            UpdateHomeRow(max)
        End Sub

        Private Sub UpdateHomeRow(screenpos As Integer)
            Dim lines As Integer = 1 + screenpos / Console.WindowWidth

            home_row = Console.CursorTop - (lines - 1)

            If home_row < 0 Then
                home_row = 0
            End If
        End Sub

        Private Sub RenderFrom(pos As Integer)
            Dim rpos = TextToRenderPos(pos)
            Dim i As Integer

            For i = rpos To rendered_text.Length - 1
                Console.Write(rendered_text(i))
            Next

            If shown_prompt.Length + rendered_text.Length > max_rendered Then
                max_rendered = shown_prompt.Length + rendered_text.Length
            Else
                Dim max_extra = max_rendered - shown_prompt.Length
                While i < max_extra
                    Console.Write(" "c)
                    i += 1
                End While
            End If
        End Sub

        Private Sub ComputeRendered()
            rendered_text.Length = 0

            For i As Integer = 0 To text.Length - 1
                Dim c As Integer = AscW(text(i))
                If c < 26 Then
                    If c = 9 Then
                        rendered_text.Append("    ")
                    Else
                        rendered_text.Append("^"c)
                        rendered_text.Append(ChrW(c + ASCII.A - 1))
                    End If
                Else
                    rendered_text.Append(ChrW(c))
                End If
            Next
        End Sub

        Private Function TextToRenderPos(pos As Integer) As Integer
            Dim p = 0

            For i As Integer = 0 To pos - 1
                Dim c As Integer

                c = AscW(text(i))

                If c < 26 Then
                    If c = 9 Then
                        p += 4
                    Else
                        p += 2
                    End If
                Else
                    p += 1
                End If
            Next

            Return p
        End Function

        Private Function TextToScreenPos(pos As Integer) As Integer
            Return shown_prompt.Length + TextToRenderPos(pos)
        End Function

        Private ReadOnly Property LineCount As Integer
            Get
                Return (shown_prompt.Length + rendered_text.Length) / Console.WindowWidth
            End Get
        End Property

        Private Sub ForceCursor(newpos As Integer)
            cursor = newpos

            Dim actual_pos = shown_prompt.Length + TextToRenderPos(cursor)
            Dim row As Integer = home_row + actual_pos / Console.WindowWidth
            Dim col = actual_pos Mod Console.WindowWidth

            If row >= Console.BufferHeight Then
                row = Console.BufferHeight - 1
            End If

            Console.SetCursorPosition(col, row)

            'log.WriteLine ("Going to cursor={0} row={1} col={2} actual={3} prompt={4} ttr={5} old={6}", newpos, row, col, actual_pos, prompt.Length, TextToRenderPos (cursor), cursor);
            'log.Flush ();
        End Sub

        Private Sub UpdateCursor(newpos As Integer)
            If cursor = newpos Then Return

            ForceCursor(newpos)
        End Sub

        Private Sub InsertChar(c As Char)
            Dim prev_lines = LineCount
            text = text.Insert(cursor, c)
            ComputeRendered()
            If prev_lines <> LineCount Then

                Console.SetCursorPosition(0, home_row)
                Render()
                ForceCursor(Interlocked.Increment(cursor))
            Else
                RenderFrom(cursor)
                ForceCursor(Interlocked.Increment(cursor))
                UpdateHomeRow(TextToScreenPos(cursor))
            End If
        End Sub

        Private Sub ShowCompletions(prefix As String, completions As String())
            ' Ensure we have space, determine window size
            Dim window_height As Integer = std.Min(completions.Length, Console.WindowHeight / 5)
            Dim target_line = Console.WindowHeight - window_height - 1
            If Not isWindows AndAlso Console.CursorTop > target_line Then
                Dim delta = Console.CursorTop - target_line
                Console.CursorLeft = 0
                Console.CursorTop = Console.WindowHeight - 1
                For i As Integer = 0 To delta + 1 - 1
                    For c As Integer = Console.WindowWidth To 1 Step -1
                        Console.Write(" ")
                    Next ' To debug use ("{0}", i%10);
                Next
                Console.CursorTop = target_line
                Console.CursorLeft = 0
                Render()
            End If

            Dim window_width = 12
            Dim plen = prefix.Length

            For Each s As String In completions
                window_width = std.Max(plen + s.Length, window_width)
            Next
            window_width = std.Min(window_width, MaxWidth)

            If current_completion Is Nothing Then
                Dim left = Console.CursorLeft - prefix.Length

                If left + window_width + 1 >= Console.WindowWidth Then left = Console.WindowWidth - window_width - 1

                current_completion = New CompletionState(left, Console.CursorTop + 1, window_width, window_height) With {
                    .Prefix = prefix,
                    .Completions = completions
                }
            Else
                current_completion.Prefix = prefix
                current_completion.Completions = completions
            End If
            current_completion.Show()
            Console.CursorLeft = 0
        End Sub

        Private Sub HideCompletions()
            If current_completion Is Nothing Then Return
            current_completion.Remove()
            current_completion = Nothing
        End Sub

        '
        ' Triggers the completion engine, if insertBestMatch is true, then this will
        ' insert the best match found, this behaves like the shell "tab" which will
        ' complete as much as possible given the options.
        '
        Private Sub Complete()
            Dim completion As Completion = AutoCompleteEvent(text.ToString(), cursor)
            Dim completions = completion.Result
            If completions Is Nothing Then
                HideCompletions()
                Return
            End If

            Dim ncompletions = completions.Length
            If ncompletions = 0 Then
                HideCompletions()
                Return
            End If

            If completions.Length = 1 Then
                InsertTextAtCursor(completions(0))
                HideCompletions()
            Else
                Dim last = -1

                For p = 0 To completions(0).Length - 1
                    Dim c = completions(0)(p)


                    For i As Integer = 1 To ncompletions - 1
                        If completions(i).Length < p Then
                            GoTo mismatch
                        End If

                        If completions(i)(p) <> c Then
                            GoTo mismatch
                        End If
                    Next
                    last = p
                Next
mismatch:
                Dim prefix = completion.Prefix
                If last <> -1 Then
                    InsertTextAtCursor(completions(0).Substring(0, last + 1))

                    ' Adjust the completions to skip the common prefix
                    prefix += completions(0).Substring(0, last + 1)
                    For i As Integer = 0 To completions.Length - 1
                        completions(i) = completions(i).Substring(last + 1)
                    Next
                End If
                ShowCompletions(prefix, completions)
                Render()
                ForceCursor(cursor)
            End If
        End Sub

        '
        ' When the user has triggered a completion window, this will try to update
        ' the contents of it.   The completion window is assumed to be hidden at this
        ' point
        ' 
        Private Sub UpdateCompletionWindow()
            If current_completion IsNot Nothing Then Throw New Exception("This method should only be called if the window has been hidden")

            Dim completion As Completion = AutoCompleteEvent(text.ToString(), cursor)
            Dim completions = completion.Result
            If completions Is Nothing Then Return

            Dim ncompletions = completions.Length
            If ncompletions = 0 Then Return

            ShowCompletions(completion.Prefix, completion.Result)
            Render()
            ForceCursor(cursor)
        End Sub


        '
        ' Commands
        '
        Private Sub CmdDone()
            If current_completion IsNot Nothing Then
                InsertTextAtCursor(current_completion.Current)
                HideCompletions()
                Return
            End If
            done = True
        End Sub

        Private Sub CmdTabOrComplete()
            Dim complete = False

            If AutoCompleteEvent IsNot Nothing Then
                If TabAtStartCompletes Then
                    complete = True
                Else
                    For i As Integer = 0 To cursor - 1
                        If Not Char.IsWhiteSpace(text(i)) Then
                            complete = True
                            Exit For
                        End If
                    Next
                End If

                If complete Then
                    Me.Complete()
                Else
                    Me.HandleChar(ASCII.HT)
                End If
            Else
                HandleChar("t"c)
            End If
        End Sub

        Private Sub CmdHome()
            UpdateCursor(0)
        End Sub

        Private Sub CmdEnd()
            UpdateCursor(text.Length)
        End Sub

        Private Sub CmdLeft()
            If cursor = 0 Then Return

            UpdateCursor(cursor - 1)
        End Sub

        Private Sub CmdBackwardWord()
            Dim p = WordBackward(cursor)
            If p = -1 Then Return
            UpdateCursor(p)
        End Sub

        Private Sub CmdForwardWord()
            Dim p = WordForward(cursor)
            If p = -1 Then Return
            UpdateCursor(p)
        End Sub

        Private Sub CmdRight()
            If cursor = text.Length Then Return

            UpdateCursor(cursor + 1)
        End Sub

        Private Sub RenderAfter(p As Integer)
            ForceCursor(p)
            RenderFrom(p)
            ForceCursor(cursor)
        End Sub

        Private Sub CmdBackspace()
            If cursor = 0 Then Return

            Dim completing = current_completion IsNot Nothing
            HideCompletions()

            text.Remove(Interlocked.Decrement(cursor), 1)
            ComputeRendered()
            RenderAfter(cursor)
            If completing Then UpdateCompletionWindow()
        End Sub

        Private Sub CmdDeleteChar()
            ' If there is no input, this behaves like EOF
            If text.Length = 0 Then
                done = True
                text = Nothing
                Console.WriteLine()
                Return
            End If

            If cursor = text.Length Then Return
            text.Remove(cursor, 1)
            ComputeRendered()
            RenderAfter(cursor)
        End Sub

        Private Function WordForward(p As Integer) As Integer
            If p >= text.Length Then Return -1

            Dim i = p
            If Char.IsPunctuation(text(p)) OrElse Char.IsSymbol(text(p)) OrElse Char.IsWhiteSpace(text(p)) Then
                While i < text.Length
                    If Char.IsLetterOrDigit(text(i)) Then Exit While
                    i += 1
                End While
                While i < text.Length
                    If Not Char.IsLetterOrDigit(text(i)) Then Exit While
                    i += 1
                End While
            Else
                While i < text.Length
                    If Not Char.IsLetterOrDigit(text(i)) Then Exit While
                    i += 1
                End While
            End If
            If i <> p Then Return i
            Return -1
        End Function

        Private Function WordBackward(p As Integer) As Integer
            If p = 0 Then Return -1

            Dim i = p - 1
            If i = 0 Then Return 0

            If Char.IsPunctuation(text(i)) OrElse Char.IsSymbol(text(i)) OrElse Char.IsWhiteSpace(text(i)) Then
                While i >= 0
                    If Char.IsLetterOrDigit(text(i)) Then Exit While
                    i -= 1
                End While
                While i >= 0
                    If Not Char.IsLetterOrDigit(text(i)) Then Exit While
                    i -= 1
                End While
            Else
                While i >= 0
                    If Not Char.IsLetterOrDigit(text(i)) Then Exit While
                    i -= 1
                End While
            End If
            i += 1

            If i <> p Then Return i

            Return -1
        End Function

        ReadOnly _cmdDeleteWord As KeyHandler = AddressOf Me.CmdDeleteWord
        ReadOnly _cmdDeleteBackword As KeyHandler = AddressOf Me.CmdDeleteBackword
        ReadOnly _cmdReverseSearch As KeyHandler = AddressOf Me.CmdReverseSearch

        Private Sub CmdDeleteWord()
            Dim pos = WordForward(cursor)

            If pos = -1 Then Return

            Dim k = text.ToString(cursor, pos - cursor)

            If last_handler Is _cmdDeleteWord Then
                kill_buffer = kill_buffer & k
            Else
                kill_buffer = k
            End If

            text.Remove(cursor, pos - cursor)
            ComputeRendered()
            RenderAfter(cursor)
        End Sub

        Private Sub CmdDeleteBackword()
            Dim pos = WordBackward(cursor)
            If pos = -1 Then Return

            Dim k = text.ToString(pos, cursor - pos)

            If last_handler Is _cmdDeleteBackword Then
                kill_buffer = k & kill_buffer
            Else
                kill_buffer = k
            End If

            text.Remove(pos, cursor - pos)
            ComputeRendered()
            RenderAfter(pos)
        End Sub

        '
        ' Adds the current line to the history if needed
        '
        Private Sub HistoryUpdateLine()
            history.Update(text.ToString())
        End Sub

        Private Sub CmdHistoryPrev()
            If Not history.PreviousAvailable() Then Return

            HistoryUpdateLine()

            SetText(history.Previous())
        End Sub

        Private Sub CmdHistoryNext()
            If Not history.NextAvailable() Then Return

            history.Update(text.ToString())
            SetText(history.Next())

        End Sub

        Private Sub CmdUp()
            If current_completion Is Nothing Then
                CmdHistoryPrev()
            Else
                current_completion.SelectPrevious()
            End If
        End Sub

        Private Sub CmdDown()
            If current_completion Is Nothing Then
                CmdHistoryNext()
            Else
                current_completion.SelectNext()
            End If
        End Sub

        Private Sub CmdKillToEOF()
            kill_buffer = text.ToString(cursor, text.Length - cursor)
            text.Length = cursor
            ComputeRendered()
            RenderAfter(cursor)
        End Sub

        Private Sub CmdYank()
            InsertTextAtCursor(kill_buffer)
        End Sub

        Private Sub InsertTextAtCursor(str As String)
            Dim prev_lines = LineCount
            text.Insert(cursor, str)
            ComputeRendered()
            If prev_lines <> LineCount Then
                Console.SetCursorPosition(0, home_row)
                Render()
                cursor += str.Length
                ForceCursor(cursor)
            Else
                RenderFrom(cursor)
                cursor += str.Length
                ForceCursor(cursor)
                UpdateHomeRow(TextToScreenPos(cursor))
            End If
        End Sub

        Private Sub SetSearchPrompt(s As String)
            SetPrompt("(reverse-i-search)`" & s & "': ")
        End Sub

        Private Sub ReverseSearch()
            Dim p As Integer

            If cursor = text.Length Then
                ' The cursor is at the end of the string

                p = text.ToString().LastIndexOf(search)
                If p <> -1 Then
                    match_at = p
                    cursor = p
                    ForceCursor(cursor)
                    Return
                End If
            Else
                ' The cursor is somewhere in the middle of the string
                Dim start = If(cursor = match_at, cursor - 1, cursor)
                If start <> -1 Then
                    p = text.ToString().LastIndexOf(search, start)
                    If p <> -1 Then
                        match_at = p
                        cursor = p
                        ForceCursor(cursor)
                        Return
                    End If
                End If
            End If

            ' Need to search backwards in history
            HistoryUpdateLine()
            Dim s = history.SearchBackward(search)
            If Not Equals(s, Nothing) Then
                match_at = -1
                SetText(s)
                ReverseSearch()
            End If
        End Sub

        Private Sub CmdReverseSearch()
            If searching = 0 Then
                match_at = -1
                last_search = search
                searching = -1
                search = ""
                SetSearchPrompt("")
            Else
                If Equals(search, "") Then
                    If Not Equals(last_search, "") AndAlso Not Equals(last_search, Nothing) Then
                        search = last_search
                        SetSearchPrompt(search)

                        ReverseSearch()
                    End If
                    Return
                End If
                ReverseSearch()
            End If
        End Sub

        Private Sub SearchAppend(c As Char)
            search = search & c.ToString()
            SetSearchPrompt(search)

            '
            ' If the new typed data still matches the current text, stay here
            '
            If cursor < text.Length Then
                Dim r = text.ToString(cursor, text.Length - cursor)
                If r.StartsWith(search) Then Return
            End If

            ReverseSearch()
        End Sub

        Private Sub CmdRefresh()
            Console.Clear()
            max_rendered = 0
            Render()
            ForceCursor(cursor)
        End Sub

        Private Sub InterruptEdit(sender As Object, a As ConsoleCancelEventArgs)
            ' Do not abort our program:
            a.Cancel = True

            Try
                ' Interrupt the editor
                Call edit_thread.Abort()
            Catch ex As Exception

            End Try
        End Sub

        '
        ' Implements heuristics to show the completion window based on the mode
        '
        Private Function HeuristicAutoComplete(wasCompleting As Boolean, insertedChar As Char) As Boolean
            If HeuristicsMode Then
                ' csharp heuristics
                If wasCompleting Then
                    If insertedChar = " "c Then
                        Return False
                    End If
                    Return True
                End If
                ' If we were not completing, determine if we want to now
                If insertedChar = "."c Then
                    ' Avoid completing for numbers "1.2" for example
                    If cursor > 1 AndAlso Char.IsDigit(text(cursor - 2)) Then
                        For p = cursor - 3 To 0 Step -1
                            Dim c = text(p)
                            If Char.IsDigit(c) Then Continue For
                            If c = "_"c Then Return True
                            If Char.IsLetter(c) OrElse Char.IsPunctuation(c) OrElse Char.IsSymbol(c) OrElse Char.IsControl(c) Then Return True
                        Next
                        Return False
                    End If
                    Return True
                End If
            End If

            Return False
        End Function

        Private Sub HandleChar(c As Char)
            If searching <> 0 Then
                SearchAppend(c)
            Else
                Dim completing = current_completion IsNot Nothing
                HideCompletions()

                InsertChar(c)
                If HeuristicAutoComplete(completing, c) Then UpdateCompletionWindow()
            End If
        End Sub

        Private Sub EditLoop()
            Dim cki As ConsoleKeyInfo

            While Not done
                Dim [mod] As ConsoleModifiers

                cki = Console.ReadKey(True)
                If cki.Key = ConsoleKey.Escape Then
                    If current_completion IsNot Nothing Then
                        HideCompletions()
                        Continue While
                    Else
                        cki = Console.ReadKey(True)

                        [mod] = ConsoleModifiers.Alt
                    End If
                Else
                    [mod] = cki.Modifiers
                End If

                Dim handled = False

                For Each handler In handlers
                    Dim t = handler.CKI

                    If t.Key = cki.Key AndAlso t.Modifiers = [mod] Then
                        handled = True
                        If handler.ResetCompletion Then HideCompletions()
                        handler.KeyHandler()
                        last_handler = handler.KeyHandler
                        Exit For
                    ElseIf t.KeyChar = cki.KeyChar AndAlso t.Key = ConsoleKey.Zoom Then
                        handled = True
                        If handler.ResetCompletion Then HideCompletions()

                        handler.KeyHandler()
                        last_handler = handler.KeyHandler
                        Exit For
                    End If
                Next
                If handled Then
                    If searching <> 0 Then
                        If last_handler IsNot _cmdReverseSearch Then
                            searching = 0
                            SetPrompt(prompt)
                        End If
                    End If
                    Continue While
                End If

                If cki.KeyChar <> ASCII.NUL Then
                    HandleChar(cki.KeyChar)
                End If
            End While
        End Sub

        Private Sub InitText(initial As String)
            text = New StringBuilder(initial)
            ComputeRendered()
            cursor = text.Length
            Render()
            ForceCursor(cursor)
        End Sub

        Private Sub SetText(newtext As String)
            Console.SetCursorPosition(0, home_row)
            InitText(newtext)
        End Sub

        Private Sub SetPrompt(newprompt As String)
            shown_prompt = newprompt
            Console.SetCursorPosition(0, home_row)
            Render()
            ForceCursor(cursor)
        End Sub

        Public Function Edit(ps1 As PS1, Optional initial As String = Nothing) As String
            Return Edit(ps1.ToString, initial)
        End Function

        ''' <summary>
        ''' Edit a line, and provides both a prompt and the initial contents to edit
        ''' </summary>
        ''' <returns>The edit.</returns>
        ''' <param name="prompt">Prompt shown to edit the line.</param>
        ''' <param name="initial">Initial contents, can be null.</param>
        Public Function Edit(prompt As String, initial As String) As String
            edit_thread = Thread.CurrentThread
            searching = 0
            AddHandler Console.CancelKeyPress, AddressOf InterruptEdit

            done = False
            history.CursorToEnd()
            max_rendered = 0

            Me.prompt = prompt
            shown_prompt = prompt
            InitText(initial)
            history.Append(initial)

            Do
                Try
                    EditLoop()
                Catch __unusedThreadAbortException1__ As ThreadAbortException
                    searching = 0
                    Call Thread.ResetAbort()
                    Console.WriteLine()
                    SetPrompt(prompt)
                    SetText("")
                End Try
            Loop While Not done
            Console.WriteLine()

            RemoveHandler Console.CancelKeyPress, AddressOf InterruptEdit

            If text Is Nothing Then
                history.Close()
                Return Nothing
            End If

            Dim result As String = text.ToString()
            If Not Equals(result, "") Then
                history.Accept(result)
            Else
                history.RemoveLast()
            End If

            Return result
        End Function

        ''' <summary>
        ''' Triggers the history to be written at this point, usually not necessary, history is saved on each line edited.
        ''' </summary>
        Public Sub SaveHistory()
            If history IsNot Nothing Then
                history.Close()
            End If
        End Sub

        ''' <summary>
        ''' Gets or sets a value indicating whether hitting the TAB key before any text exists triggers completion or inserts a "tab" character into the buffer.  This is useful to allow users to copy/paste code that might contain whitespace at the start and you want to preserve it.
        ''' </summary>
        ''' <value><c>true</c> if tab at start completes; otherwise, <c>false</c>.</value>
        Public Property TabAtStartCompletes As Boolean

    End Class
End Namespace
