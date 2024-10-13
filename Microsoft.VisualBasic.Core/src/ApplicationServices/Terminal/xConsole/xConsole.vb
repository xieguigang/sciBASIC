#Region "Microsoft.VisualBasic::db3b8e9543d16ef36a97631e72ad620e, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\xConsole\xConsole.vb"

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

    '   Total Lines: 517
    '    Code Lines: 285 (55.13%)
    ' Comment Lines: 152 (29.40%)
    '    - Xml Docs: 80.92%
    ' 
    '   Blank Lines: 80 (15.47%)
    '     File Size: 20.61 KB


    '     Module xConsole
    ' 
    '         Properties: ClearColorsAtEnd
    ' 
    '         Function: ClearInput, ClosestConsoleColor, ConvertHexStringToByteArray, getColor, GetConsoleWindow
    '                   (+2 Overloads) Implode, ParseLine, Print, ReadKeys, ReadLine
    '                   SetWindowPos
    ' 
    '         Sub: ClearInput, (+2 Overloads) CoolWrite, CoolWriteLine, Credits, ListFonts
    '              RestoreColors, SetFont, SetIcon, SetWindowPos, Wait
    '              Write, (+3 Overloads) WriteLine
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
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.Language
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

Namespace ApplicationServices.Terminal.xConsole

    ''' <summary>
    ''' Allows you to color and animate the console. ~ overpowered.it ~ TheTrigger - 💸
    ''' </summary>
    ''' <remarks>http://www.codeproject.com/Tips/626856/xConsole-Project</remarks>
    <HideModuleName> Public Module xConsole

#Region "COOL WRITING ✌"
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
            WriteLine(vbCr & vbTab & vbTab & "^y│" & vbTab & "^3Build Date^!:" & vbTab & "^y{0}" & vbTab & vbTab & "^y│^!.", Assembly.GetCallingAssembly().RetrieveLinkerTimestamp().ToShortDateString())
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
                Call result.Add(key.Value)
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
                                c = (randf.seeds.Next(0, cList.Length - 1))
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
        Friend Function Print(input As String) As Integer
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

#If NET48 Then
        Public Sub SetIcon(icon As Icon)
            Helpers.SetConsoleIcon(icon)
        End Sub
#End If

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
            SetWindowPos(MyConsole, 0, x, y, 0, 0, 1)
        End Sub
#End Region

#Region "SOME VARS 🔧"

        ''' <summary>
        ''' My ASM FILE
        ''' </summary>
        Dim MyASM As AssemblyName = Assembly.GetAssembly(GetType(xConsole)).GetName()
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
        ''' Clear colors automatically at the end of each Writeline. (Default = false);
        ''' </summary>
        Public Property ClearColorsAtEnd As Boolean = False

#End Region

#Region "THIRD PART ✅"

        ''' <summary>
        ''' Convert rgb color to ConsoleColor. From stackoverflow
        ''' </summary>
        Public Function ClosestConsoleColor(r As Byte, g As Byte, b As Byte) As ConsoleColor
            Dim ret As ConsoleColor = 0
            Dim rr As Double = r, gg As Double = g, bb As Double = b, delta As Double = Double.MaxValue

            For Each cc As ConsoleColor In [Enum].GetValues(GetType(ConsoleColor))
                Dim n = [Enum].GetName(GetType(ConsoleColor), cc)
                Dim c = Color.FromName(If(n = "DarkYellow", "Orange", n))
                Dim t = stdNum.Pow(c.R - rr, 2.0) + stdNum.Pow(c.G - gg, 2.0) + stdNum.Pow(c.B - bb, 2.0)
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
    End Module
End Namespace
