#Region "Microsoft.VisualBasic::a8251d66048cbc47fdba172bdaeb7f54, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\xConsole\xConsole.vb"

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

    '     Module xConsole
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CheckNewVersion, ClearInput, ClosestConsoleColor, ConvertHexStringToByteArray, getColor
    '                   GetConsoleWindow, (+2 Overloads) Implode, ParseLine, Print, ReadKeys
    '                   ReadLine, RetrieveLinkerTimestamp, SetWindowPos
    ' 
    '         Sub: __checkUpdates, CheckforUpdates, ClearInput, (+2 Overloads) CoolWrite, CoolWriteLine
    '              Credits, ListFonts, RestoreColors, SetFont, SetIcon
    '              SetWindowPos, Wait, Write, (+3 Overloads) WriteLine
    '         Class CoolWriteSettings
    ' 
    '             Properties: CoolWriting, CoolWritingDelay, CWRDDelay
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class Comparer
    ' 
    '             Constructor: (+3 Overloads) Sub New
    '             Function: Find
    ' 
    '         Class Spinner
    ' 
    '             Constructor: (+2 Overloads) Sub New
    ' 
    '             Function: Turn
    ' 
    '             Sub: Break, Run, RunTask
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' *	xConsole Source Code v 0.3.1
' *	Created by TheTrigger { overpowered.it } { thetriggersoft[at]]gmail[dot]com }
' *		23/05/2014
'

Imports System.Drawing
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Xml
Imports Microsoft.VisualBasic.Language
Imports sys = System.Math

Namespace Terminal

    ''' <summary>
    ''' Allows you to color and animate the console. ~ overpowered.it ~ TheTrigger - 💸
    ''' </summary>
    ''' <remarks>http://www.codeproject.com/Tips/626856/xConsole-Project</remarks>
    Public Module xConsole

#Region "STATIC COSTRUCTOR .."

        Sub New()
            If System.Diagnostics.Debugger.IsAttached Then
                Try
                    If CheckForUpdatesEnabled = True Then
                        Call __checkUpdates()
                    End If
                Catch generatedExceptionName As Exception
                    Call App.LogException(generatedExceptionName)
                End Try
            End If
        End Sub

        Private Sub __checkUpdates()
            Dim key As Microsoft.Win32.RegistryKey =
            Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", True).CreateSubKey("OverPowered")

            If key.GetValue("UpdateLastCheck") Is Nothing Then
                key.SetValue("UpdateLastCheck", 0)
            End If

            If CInt(key.GetValue("UpdateLastCheck")) < DateTime.Now.DayOfYear - 30 Then
                key.SetValue("UpdateLastCheck", DateTime.Now.DayOfYear)
                xConsole.CheckforUpdates()
            End If

            Call key.Close()
        End Sub

#End Region

#Region "COOL WRITING ✌"

        Public NotInheritable Class CoolWriteSettings
            Private Sub New()
            End Sub
            ''' <summary>
            ''' Gradual typing the output into console
            ''' </summary>
            Public Shared Property CoolWriting As Boolean = False

            ''' <summary>
            ''' Write speed
            ''' </summary>
            Public Shared Property CoolWritingDelay As Integer = 8

            ''' <summary>
            ''' Set the delay when write a new line or dots. (Default = 200).
            ''' </summary>
            Public Shared Property CWRDDelay As Integer = 280
        End Class

        ''' <summary>
        ''' Gradual output animation 👍👍
        ''' </summary>
        ''' <param name="obj">The object to convert</param>
        Public Sub CoolWrite(obj As Object)
            CoolWrite(String.Format("{0}", obj))
        End Sub

        ''' <summary>
        ''' Gradual output animation 👍👍
        ''' </summary>
        ''' <param name="format">The input string</param>
        ''' <param name="args"></param>
        Public Sub CoolWrite(format As String, ParamArray args As Object())
            My.AddToQueue(Sub()
                              Dim old As Boolean = CoolWriteSettings.CoolWriting
                              CoolWriteSettings.CoolWriting = True
                              Call Print(String.Format(format, args))
                              CoolWriteSettings.CoolWriting = old
                          End Sub)
        End Sub

        ''' <summary>
        ''' Gradual output animation
        ''' </summary>
        ''' <param name="format">The input string</param>
        ''' <param name="args">Arguments</param>
        Public Sub CoolWriteLine(format As String, ParamArray args As Object())
            Call CoolWrite(format & NEW_LINE, args)
        End Sub

        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
#End Region

#Region "WRITE LINE ✏"

        ''' <summary>
        ''' Allows you to write in the console-output with custom colors, followed by the current line terminator
        ''' </summary>
        Public Sub WriteLine()
            Write(NEW_LINE)
        End Sub

        ''' <summary>
        ''' Allows you to write in the console-output with custom colors, followed by the current line terminator
        ''' </summary>
        ''' <param name="obj">The object to convert</param>
        Public Sub WriteLine(obj As Object)
            Write(String.Format("{0}", obj) & NEW_LINE)
        End Sub

        ''' <summary>
        ''' Allows you to write in the console-output with custom colors, followed by the current line terminator
        ''' </summary>
        ''' <param name="format">The input string</param>
        ''' <param name="args">Arguments</param>
        Public Sub WriteLine(format As String, ParamArray args As Object())
            Write(format & NEW_LINE, args)
        End Sub

        ''' <summary>
        ''' Allows you to write in the console-output with custom colors
        ''' </summary>
        ''' <param name="format">The input string</param>
        ''' <param name="args">Arguments</param>
        Public Sub Write(format As String, ParamArray args As Object())
            My.InnerQueue.AddToQueue(Sub() Print(String.Format(format, args)))
        End Sub

        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
#End Region

#Region "OTHER 🔦"

        ''' <summary>
        ''' (php-like) Implode function
        ''' </summary>
        ''' <param name="args">The list input</param>
        ''' <param name="delimiter">Delimiter</param>
        ''' <param name="start">Index offset</param>
        ''' <returns>Imploded string</returns>
        Public Function Implode(args As List(Of String), Optional delimiter As String = " ", Optional start As Integer = 0) As String
            Dim text As String = String.Empty
            For i As Integer = start To args.Count - 1
                text += args(i) & (If((i = args.Count - 1), String.Empty, delimiter))
            Next
            Return text
        End Function

        ''' <summary>
        ''' (php-like) Implode a List of strings
        ''' </summary>
        ''' <param name="args">The list input</param>
        ''' <param name="start">Index offset</param>
        ''' <returns>Imploded string</returns>
        Public Function Implode(args As List(Of String), Optional start As Integer = 0) As String
            Return Implode(args, Nothing, start)
        End Function

        ''' <summary>
        ''' Just wait. in milliseconds
        ''' </summary>
        ''' <param name="time"></param>
        Public Sub Wait(time As Integer)
            My.InnerQueue.AddToQueue(Sub() Thread.Sleep(time))
        End Sub

        ''' <summary>
        ''' Restore default colors
        ''' </summary>
        Public Sub RestoreColors()
            Console.ResetColor()
            FONT_COLOR = Console.ForegroundColor
            BACKGROUND_COLOR = Console.BackgroundColor
        End Sub

        ''' <summary>
        ''' Show credits
        ''' </summary>
        Public Sub Credits()
            WriteLine(vbCr & vbLf)
            WriteLine(vbCr & vbTab & vbTab & "^8╒^r≡^7=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=^r≡^8╕^!")
            WriteLine(vbCr & vbTab & vbTab & "^y│" & vbTab & "^3Created by^!:" & vbTab & "^8TheTrigger^!" & vbTab & vbTab & "^y│")
            WriteLine(vbCr & vbTab & vbTab & "^y│" & vbTab & "^3WebSite^!:" & vbTab & "^8overpowered.it^!" & vbTab & vbTab & "^y│")
            WriteLine(vbCr & vbTab & vbTab & "^y│" & vbTab & "^3Version^!:" & vbTab & "^g{0} ^!" & vbTab & vbTab & "^y│", MyASM.Version)
            WriteLine(vbCr & vbTab & vbTab & "^y│" & vbTab & "^3Build Date^!:" & vbTab & "^y{0}" & vbTab & vbTab & "^y│^!.", RetrieveLinkerTimestamp().ToShortDateString())
            WriteLine(vbCr & vbTab & vbTab & "^8╘^r■^7=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=═=^r■^8╛^!``")
            WriteLine(vbCr & vbLf)
        End Sub

#End Region

#Region "USER INPUT 🔣"

        ''' <summary>
        ''' Read the line, then parse it.
        ''' </summary>
        ''' <param name="ClearInput">Clear the buffer input</param>
        ''' <returns>Return a List of strings</returns>
        Public Function ReadLine(Optional Clearinput As Boolean = True) As List(Of String)
            My.InnerQueue.WaitQueue()

            If Clearinput Then
                xConsole.ClearInput()
            End If

            Dim sReadLine As String = Console.ReadLine()

            Return ParseLine(sReadLine)
        End Function

        ''' <summary>
        ''' Give back a ConsoleKeyInfo list
        ''' </summary>
        ''' <param name="Return"></param>
        ''' <returns></returns>
        Public Function ClearInput([Return] As Boolean) As List(Of ConsoleKeyInfo)
            Dim CKI As New List(Of ConsoleKeyInfo)()
            While Console.KeyAvailable
                CKI.Add(Console.ReadKey(False))
            End While
            Return CKI
        End Function

        ''' <summary>
        ''' Clear the user input in stack
        ''' </summary>
        Public Sub ClearInput()
            While Console.KeyAvailable
                Console.ReadKey(True)
            End While
        End Sub

        ''' <summary>
        ''' Parse the input string
        ''' </summary>
        ''' <param name="s">Input string</param>
        ''' <returns></returns>
        Public Function ParseLine(s As String) As List(Of String)
            Dim args As List(Of String) = Nothing

            If String.IsNullOrWhiteSpace(s) Then
                args = New List(Of String)() From {
                    String.Empty
                }
                Return args
            End If

            ' If even index
            ' Split the item
            ' Keep the entire item
            args = s.Split(""""c).Select(Function(element, index) If(index Mod 2 = 0, element.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries), New String() {element})).SelectMany(Function(element) element).AsList()

            If args.Count = 0 Then
                args.Add(String.Empty)
            End If
            Return args
        End Function

        ''' <summary>
        ''' Read EACH keys from the buffer input (visible and hidden chars)
        ''' </summary>
        ''' <param name="ClearInput">Clear the buffer input</param>
        ''' <returns>string with all chars</returns>
        Public Function ReadKeys(Optional Clearinput As Boolean = True) As String
            My.InnerQueue.WaitQueue()

            If Clearinput Then
                xConsole.ClearInput()
            End If

            Dim key As New Value(Of Char)
            Dim result As New List(Of Char)()
            While AscW(key = Console.ReadKey(True).KeyChar) >= 0 AndAlso
                AscW(+key) <> 13
                Call result.Add(key.value)
            End While

            Return String.Join("", result)
        End Function

        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
#End Region

#Region "THE MAGIC-WRITER 👻"

        ''' <summary>
        ''' Convert input to color
        ''' </summary>
        ''' <param name="s">Input string</param>
        ''' <returns>👽👽👽👾</returns>
        Private Function getColor(s As String, Optional ForeC As ConsoleColor? = Nothing, Optional BackC As ConsoleColor? = Nothing) As ConsoleColor
            Dim FC As ConsoleColor = If(ForeC, FONT_COLOR)
            Dim BC As ConsoleColor = If(BackC, BACKGROUND_COLOR)

            If String.IsNullOrWhiteSpace(s) Then
                Return FC
            End If

            Dim Type As Char = s(0)
            s = s.TrimStart(New Char() {"^"c, "*"c})
            Dim i As Integer = -1

            ' RGB case
            If s.Length = 3 Then
                s = String.Format("{0}{0}{1}{1}{2}{2}", s(0), s(1), s(2))
                Dim colors As Byte() = ConvertHexStringToByteArray(s)
                Dim cc = ClosestConsoleColor(colors(0), colors(1), colors(2))
                Return cc

                ' Single Char
            ElseIf s.Length = 1 OrElse s.Length = 2 Then

                ' INT CASE
                If Integer.TryParse(s, i) Then
                    Return CType(If((i < 0 OrElse i > 16), 1, i), ConsoleColor)
                Else
                    ' Char case
                    Select Case s.ToLower()
                        Case "!", "-"
                            ' Restore color
                            Return If((Type = "^"c), FC, BC)

                        Case "."
                            ' Random color
                            Dim c As Integer = 0
                            Dim cList = [Enum].GetNames(GetType(ConsoleColor))
                            Do
                                c = (RDN.[Next](0, cList.Length - 1))
                            Loop While c = CInt(If((Type = "*"c), Console.ForegroundColor, Console.BackgroundColor))

                            Return CType(c, ConsoleColor)

                        Case "w"
                            Return ConsoleColor.White
                        Case "z"
                            Return ConsoleColor.Black
                        Case "y"
                            Return ConsoleColor.Yellow
                        Case "g"
                            Return ConsoleColor.Green
                        Case "r"
                            Return ConsoleColor.Red
                        Case "b"
                            Return ConsoleColor.Blue
                        Case "c"
                            Return ConsoleColor.Cyan
                        Case "m"
                            Return ConsoleColor.Magenta
                        Case Else
                    End Select
                End If
            End If
            Return Console.ForegroundColor
        End Function

        Const c As String = vbCr & vbLf & ".,:;!?"
        'string pattern = @"[\\]{0,1}(?:[\^|\*|°]{1})(?:[0-9a-fA-F]{3}|[0-9]{1,2}|[a-zA-Z!\.]{1})";
        Const pattern As String = "[\\]{0,1}(?:[\^|\*]{1})(?:[0-9A-F]{3}|[0-9]{1,2}|[a-zA-Z!\.]{1})"

        ''' <summary>
        ''' The Parser
        ''' </summary>
        ''' <param name="input">Input string</param>
        Private Function Print(input As String) As Integer
            If String.IsNullOrWhiteSpace(input) Then
                Return 0
            End If

            ' temp var for restore previous color
            Dim Fore As ConsoleColor = Console.ForegroundColor
            Dim Back As ConsoleColor = Console.BackgroundColor
            Dim matches As MatchCollection = Regex.Matches(input, pattern)
            Dim substrings As String() = Regex.Split(input, pattern)

            Dim i As Integer = 0, StringSize As Integer = 0

            For Each [sub] As String In substrings
                StringSize += [sub].Count()
                If CoolWriteSettings.CoolWriting Then
                    For j As Integer = 0 To [sub].Length - 1
                        Console.Write([sub](j))

                        If c.Contains([sub](j)) AndAlso (j < [sub].Length - 1 AndAlso [sub](j + 1) <> [sub](j)) Then
                            Thread.Sleep(CoolWriteSettings.CWRDDelay)
                        Else
                            Thread.Sleep(CoolWriteSettings.CoolWritingDelay)
                        End If
                    Next
                Else
                    Console.Write([sub])
                End If

                If i < matches.Count Then
                    Dim Type As Char = matches(i).Groups(0).Value(0)
                    Select Case Type
                        Case "*"c
                            Console.BackgroundColor = getColor(matches(i).Groups(0).Value)
                        Case "^"c
                            Console.ForegroundColor = getColor(matches(i).Groups(0).Value)
                        Case Else
                            Console.Write("{0}", matches(i).Groups(0).Value.TrimStart("\"c))
                    End Select
                End If

                i += 1
            Next

            If ClearColorsAtEnd Then
                Console.BackgroundColor = Fore
                Console.ForegroundColor = Back
            End If

            Return StringSize
        End Function


        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
#End Region

#Region "COMPARER 💻"

        ''' <summary>
        ''' This can compute the input then return back the most appropriate word.
        ''' </summary>
        Public Class Comparer

            ''' <summary>
            ''' This is the word to find
            ''' </summary>
            Public Word As String = String.Empty

            ''' <summary>
            ''' Descrizione
            ''' </summary>
            Public Description As String = String.Empty

            ''' <summary>
            ''' Init to 0!
            ''' </summary>
            Private Points As Integer = 0

            ''' <summary>
            ''' Initliaze a new instance
            ''' </summary>
            ''' <param name="w">The word to find</param>
            ''' <param name="p">It's should be 0</param>
            Public Sub New(w As String, p As Integer)
                Word = w
                Points = p
            End Sub

            ''' <summary>
            ''' Initliaze a new instance
            ''' </summary>
            ''' <param name="w">The word to find</param>
            Public Sub New(w As String)
                Word = w
                Points = 0
            End Sub

            ''' <summary>
            ''' Initliaze a new instance
            ''' </summary>
            ''' <param name="w">The word to find</param>
            ''' <param name="desc">Description (do nothing)</param>
            Public Sub New(w As String, desc As String)
                Word = w
                Description = desc
            End Sub

            ''' <summary>
            ''' Find a word from an input abbreviation (es n > name)
            ''' </summary>
            ''' <returns></returns>
            Public Overloads Shared Function Find(abbr As String, ByRef Words As List(Of Comparer)) As String
                Dim Result As String = String.Empty
                Dim Best As Integer = 0
                Dim c As Integer = 0

                While Words.Count > c AndAlso Words(c) IsNot Nothing
                    Dim word = Words(c)

                    If abbr = word.Word Then
                        Result = abbr
                        Exit While
                    End If

                    For i As Integer = 0 To abbr.Length - 1
                        If abbr.Length < word.Word.Length AndAlso abbr(i) = word.Word(i) Then
                            word.Points += 1
                        Else
                            word.Points = 0
                            Exit For
                        End If
                    Next

                    If word.Points > Best Then
                        Best = word.Points
                    End If


                    c += 1
                End While
                ' End while
                Dim n As Integer = 0
                For Each word As Comparer In Words
                    If word.Points = Best AndAlso word.Points > 0 Then
                        Result = word.Word
                        If System.Threading.Interlocked.Increment(n) > 1 Then
                            Result = String.Empty
                        End If
                    End If
                Next

                Return Result
            End Function
        End Class

        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
        '////////////////////////////////////////////////////////////////////////////////////////////////
#End Region

#Region "SPINNER ⌛"

        ''' <summary>
        ''' A list of spinners for your console ❤
        ''' </summary>
        Public Class Spinner
            Private counter As Integer = 0
            Private c As Char()

            ''' <summary>
            ''' List of available spinners (you can add new)
            ''' </summary>
            Public Spinners As New List(Of Char())() From {
                New Char() {"-"c, "\"c, "|"c, "/"c},
                New Char() {"▄"c, "■"c, "▀"c, "■"c},
                New Char() {"╔"c, "╗"c, "╝"c, "╚"c},
                New Char() {"."c, "·"c, "`"c, "·"c},
                New Char() {"1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "0"c}
            }

            ''' <summary>
            ''' looplooplooplooplooplooplooplooplooploop[...]
            ''' </summary>
            Private inLoop As Boolean = True

            ''' <summary>
            ''' The base string for spinning. {0} will display the spinner. COLOR is SUPPORTED! 🆒
            ''' </summary>
            Public SpinText As String = "{0} ^gLoading^!..."

            ''' <summary>
            ''' Initialize the spinner
            ''' </summary>
            ''' <param name="i">Index of the spinner to use</param>
            ''' <param name="txt">Base string. `{0} show the spinner`</param>
            Public Sub New(Optional i As Integer = 0, Optional txt As String = Nothing)
                If Not String.IsNullOrWhiteSpace(txt) Then
                    SpinText = txt
                End If
                c = If((Spinners.Count > i), Spinners(i), Spinners(0))
            End Sub

            ''' <summary>
            ''' Initialize a custom spinner
            ''' </summary>
            ''' <param name="spinner">Set a custom spinner, no size limit.</param>
            Public Sub New(spinner As Char())
                c = spinner
            End Sub

            ''' <summary>
            ''' Breaks the spinner
            ''' </summary>
            Public Sub Break()
                inLoop = False
            End Sub

            Private StringSize As Integer = 0

            ''' <summary>
            ''' Turn the spin!
            ''' </summary>
            ''' <param name="time">Waiting time. Default 130 ms</param>
            ''' <returns>False if it has been stopped</returns>
            ''' <example>while(spinner.Turn());</example>
            Public Function Turn(Optional time As Integer = 130) As Boolean
                Dim [loop] = inLoop

                If [loop] Then
                    counter += 1
                    Dim wr As String = String.Format(SpinText, c(counter Mod c.Length), "wtf?")
                    My.InnerQueue.AddToQueue(Sub()
                                                 StringSize = Print(wr)
                                                 Dim left As Integer = Console.CursorLeft - StringSize
                                                 If left < 0 Then
                                                     left = 0
                                                 End If
                                                 Console.SetCursorPosition(left, Console.CursorTop)
                                             End Sub)

                    Thread.Sleep(time)
                Else
                    My.InnerQueue.AddToQueue(Sub()
                                                 Dim pos As New Point(Console.CursorLeft, Console.CursorTop)
                                                 Console.Write(New String(" "c, StringSize))
                                                 Console.SetCursorPosition(pos.X, pos.Y)
                                             End Sub)
                End If

                Console.CursorVisible = Not [loop]

                Return [loop]
            End Function

            Public Sub Run(Optional speed As Integer = 130)
                Do While inLoop
                    Call Turn(speed)
                    Call Thread.Sleep(10)
                Loop
            End Sub

            Public Sub RunTask(Optional speed As Integer = 130)
                Call Parallel.RunTask(Sub() Call Run(speed))
            End Sub
        End Class

#End Region

#Region "SET FONT / ICON ⚠"

        ''' <summary>
        ''' Show list of fonts
        ''' </summary>
        Public Sub ListFonts()
            Dim fonts = Helpers.ConsoleFonts

            For f As Integer = 0 To fonts.Length - 1
                Console.WriteLine("{0}: X={1}, Y={2}", fonts(f).Index, fonts(f).SizeX, fonts(f).SizeY)
            Next

        End Sub

        ''' <summary>
        ''' Change console font
        ''' </summary>
        ''' <param name="i"></param>
        Public Sub SetFont(Optional i As UInteger = 6)
            Helpers.SetConsoleFont(i)
        End Sub

        Public Sub SetIcon(icon As Icon)
            Helpers.SetConsoleIcon(icon)
        End Sub

#End Region

#Region "Windows Positions 📍"

#Region "DLL IMPORT"
        <DllImport("user32.dll", EntryPoint:="SetWindowPos")>
        Private Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As Integer, x As Integer, Y As Integer, cx As Integer, cy As Integer,
            wFlags As Integer) As IntPtr
        End Function
        <DllImport("kernel32.dll", ExactSpelling:=True)>
        Private Function GetConsoleWindow() As IntPtr
        End Function
        Private MyConsole As IntPtr = GetConsoleWindow()
#End Region

        ''' <summary>
        ''' Set new window position
        ''' </summary>
        Public Sub SetWindowPos(x As Integer, y As Integer)
            SetWindowPos(MyConsole, 0, x, y, 0, 0,
                1)
        End Sub
#End Region

#Region "SOME VARS 🔧"

        ''' <summary>
        ''' My ASM FILE
        ''' </summary>
        Dim MyASM As AssemblyName = Assembly.GetAssembly(GetType(xConsole)).GetName()

        ''' <summary>
        ''' Random number Generator
        ''' </summary>
        Dim RDN As New Random()

        ''' <summary>
        ''' This value is used when restoring the colors of the console.
        ''' </summary>
        Dim FONT_COLOR As ConsoleColor = Console.ForegroundColor
        ''' <summary>
        ''' This value is used when restoring the colors of the console.
        ''' </summary>
        Dim BACKGROUND_COLOR As ConsoleColor = Console.BackgroundColor

        ''' <summary>
        ''' Default line terminator
        ''' </summary>
        ReadOnly NEW_LINE As String = Environment.NewLine

        ''' <summary>
        ''' Check for updates every 7days. False to disable. (Default = true);
        ''' </summary>
        Public CheckForUpdatesEnabled As Boolean = True

        ''' <summary>
        ''' Clear colors automatically at the end of each Writeline. (Default = false);
        ''' </summary>
        Public ClearColorsAtEnd As Boolean = False


#End Region

#Region "THIRD PART ✅"
        ''' <summary>
        ''' Convert rgb color to ConsoleColor. From stackoverflow
        ''' </summary>
        Private Function ClosestConsoleColor(r As Byte, g As Byte, b As Byte) As ConsoleColor
            Dim ret As ConsoleColor = 0
            Dim rr As Double = r, gg As Double = g, bb As Double = b, delta As Double = Double.MaxValue

            For Each cc As ConsoleColor In [Enum].GetValues(GetType(ConsoleColor))
                Dim n = [Enum].GetName(GetType(ConsoleColor), cc)
                Dim c = Color.FromName(If(n = "DarkYellow", "Orange", n))
                Dim t = sys.Pow(c.R - rr, 2.0) + sys.Pow(c.G - gg, 2.0) + sys.Pow(c.B - bb, 2.0)
                If t = 0.0 Then
                    Return cc
                End If
                If t < delta Then
                    delta = t
                    ret = cc
                End If
            Next
            Return ret
        End Function


        ''' <summary>
        ''' Linker Timestamp
        ''' </summary>
        Private Function RetrieveLinkerTimestamp() As DateTime
            ' from stackoverflow
            Dim filePath As String = System.Reflection.Assembly.GetCallingAssembly().Location
            Const c_PeHeaderOffset As Integer = 60
            Const c_LinkerTimestampOffset As Integer = 8
            Dim b As Byte() = New Byte(2047) {}
            Dim s As System.IO.Stream = Nothing

            Try
                s = New System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read)
                s.Read(b, 0, 2048)
            Finally
                If s IsNot Nothing Then
                    s.Close()
                End If
            End Try

            Dim i As Integer = System.BitConverter.ToInt32(b, c_PeHeaderOffset)
            Dim secondsSince1970 As Integer = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset)
            Dim dt As New DateTime(1970, 1, 1, 0, 0, 0)
            dt = dt.AddSeconds(secondsSince1970)
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours)
            Return dt
        End Function


        ''' <summary>
        ''' Convert String to byte array
        ''' </summary>
        ''' <param name="hexString"></param>
        ''' <returns></returns>
        Private Function ConvertHexStringToByteArray(hexString As String) As Byte()
            ' from stackoverflow
            If hexString.Length Mod 2 <> 0 Then
                Throw New ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString))
            End If

            Dim HexAsBytes As Byte() = New Byte(hexString.Length \ 2 - 1) {}
            For index As Integer = 0 To HexAsBytes.Length - 1
                Dim byteValue As String = hexString.Substring(index * 2, 2)
                HexAsBytes(index) = Byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            Next

            Return HexAsBytes
        End Function

#End Region

#Region "CHECK FOR NEW VERSION ♻"

        Public Sub CheckforUpdates()
            CheckForUpdatesEnabled = False

            ' New version
            ' BACK TO THE FUTURE!? wooooooooo
            ' up to date
            Call New Thread(Sub()
                                Thread.Sleep(10)
                                Dim data = CheckNewVersion()
                                If Not String.IsNullOrWhiteSpace(data("url").ToString()) Then
                                    xConsole.WriteLine("[^mxConsole^!] ^6Current Ver: ^y{0}^! / ^6Latest: ^y{1}^!", MyASM.Version, data("ver"))
                                    Dim compared As Integer = MyASM.Version.CompareTo(data("ver"))
                                    If compared = -1 Then
                                        xConsole.WriteLine("[^mxConsole^!] ^gNew version available!^!")
                                        xConsole.WriteLine("[^mxConsole^!]^11 Download/info page: ^w{0}^!", data("url"))
                                    ElseIf compared = 1 Then
                                        xConsole.WriteLine("[^mxConsole^!] *y^r>>>^6" & vbNullChar & "BACK TO THE ^rFUTURE!^!*!")
                                    Else
                                        xConsole.WriteLine("[^mxConsole^!] ^gUp to date! :)^!")
                                    End If
                                Else
                                    xConsole.WriteLine("[^mxConsole^!] ^rCan not check for updates :/^!")
                                End If

                            End Sub).Start()
        End Sub

        Private Function CheckNewVersion() As Dictionary(Of String, Object)
            Dim reader As XmlTextReader = Nothing
            Dim data As New Dictionary(Of String, Object)()
            data.Add("ver", New Version())
            data.Add("url", String.Empty)

            Try
                Dim xmlURL As String = "http://trigger.overpowered.it/xConsole/curr_version.xml"
                reader = New XmlTextReader(xmlURL)
                xConsole.WriteLine("[^mxConsole^!] ^6Checking for newer version...^!")

                reader.MoveToContent()

                Dim elementName As String = ""

                If (reader.NodeType = XmlNodeType.Element) AndAlso (reader.Name = "data") Then
                    While reader.Read()
                        If reader.NodeType = XmlNodeType.Element Then
                            elementName = reader.Name
                        Else
                            If (reader.NodeType = XmlNodeType.Text) AndAlso (reader.HasValue) Then
                                Select Case elementName
                                    Case "version"
                                        data("ver") = New Version(reader.Value)


                                    Case "url"
                                        data("url") = reader.Value


                                End Select
                            End If
                        End If
                    End While
                End If

            Catch generatedExceptionName As Exception
            Finally
                If reader IsNot Nothing Then
                    reader.Close()
                End If
            End Try

            Return data
        End Function
#End Region
    End Module
End Namespace
