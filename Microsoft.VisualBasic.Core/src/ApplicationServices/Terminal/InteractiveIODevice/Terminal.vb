#Region "Microsoft.VisualBasic::128194f05d6e03b61d02480ee6ef8a80, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\Terminal.vb"

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

    '   Total Lines: 1083
    '    Code Lines: 366
    ' Comment Lines: 626
    '   Blank Lines: 91
    '     File Size: 60.22 KB


    '     Class Terminal
    ' 
    '         Properties: [Error], [In], BackgroundColor, BufferHeight, BufferWidth
    '                     CapsLock, CursorLeft, CursorSize, CursorTop, CursorVisible
    '                     ForegroundColor, InputEncoding, IsErrorRedirected, IsInputRedirected, IsOutputRedirected
    '                     KeyAvailable, LargestWindowHeight, LargestWindowWidth, NumberLock, Out
    '                     OutputEncoding, Title, TreatControlCAsInput, WindowHeight, WindowLeft
    '                     WindowTop, WindowWidth
    ' 
    '         Function: (+2 Overloads) OpenStandardError, (+2 Overloads) OpenStandardInput, (+2 Overloads) OpenStandardOutput, Read, (+2 Overloads) ReadKey
    '                   ReadLine
    ' 
    '         Sub: Clear, (+2 Overloads) MoveBufferArea, ResetColor, SetBufferSize, SetCursorPosition
    '              SetError, SetIn, SetOut, SetWindowPosition, SetWindowSize
    '              (+17 Overloads) Write, (+18 Overloads) WriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.STDIO__

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' Represents the standard input, output, and error streams for console applications. 
    ''' (交互式的命令行终端)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Terminal : Implements STDIO__.IConsole, IShellDevice

#Region "Console Member Inherits Details"

#Region "Public Methods"

        ''' <summary>
        ''' Clears the console buffer and corresponding console window of display information.
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Clear() Implements STDIO__.IConsole.Clear
            Call Console.Clear()
        End Sub

        ''' <summary>
        ''' Copies a specified source area of the screen buffer to a specified destination area.
        ''' </summary>
        ''' <param name="sourceLeft">The leftmost column of the source area.</param>
        ''' <param name="sourceTop">The topmost row of the source area.</param>
        ''' <param name="sourceWidth">The number of columns in the source area.</param>
        ''' <param name="sourceHeight">The number of rows in the source area.</param>
        ''' <param name="targetLeft">The leftmost column of the destination area.</param>
        ''' <param name="targetTop">The topmost row of the destination area.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">One or more of the parameters is less than zero.-or- sourceLeft or targetLeft is greater than or equal to System.Console.BufferWidth.-or- sourceTop or targetTop is greater than or equal to System.Console.BufferHeight.-or- sourceTop + sourceHeight is greater than or equal to System.Console.BufferHeight.-or- sourceLeft + sourceWidth is greater than or equal to System.Console.BufferWidth.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub MoveBufferArea(sourceLeft As Integer, sourceTop As Integer, sourceWidth As Integer, sourceHeight As Integer, targetLeft As Integer, targetTop As Integer)
            Call Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop)
        End Sub

        ''' <summary>
        ''' Copies a specified source area of the screen buffer to a specified destination area.
        ''' </summary>
        ''' <param name="sourceLeft">The leftmost column of the source area.</param>
        ''' <param name="sourceTop">The topmost row of the source area.</param>
        ''' <param name="sourceWidth">The number of columns in the source area.</param>
        ''' <param name="sourceHeight">The number of rows in the source area.</param>
        ''' <param name="targetLeft">The leftmost column of the destination area.</param>
        ''' <param name="targetTop">The topmost row of the destination area.</param>
        ''' <param name="sourceChar">The character used to fill the source area.</param>
        ''' <param name="sourceForeColor">The foreground color used to fill the source area.</param>
        ''' <param name="sourceBackColor">The background color used to fill the source area.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">One or more of the parameters is less than zero.-or- sourceLeft or targetLeft is greater than or equal to System.Console.BufferWidth.-or- sourceTop or targetTop is greater than or equal to System.Console.BufferHeight.-or- sourceTop + sourceHeight is greater than or equal to System.Console.BufferHeight.-or- sourceLeft + sourceWidth is greater than or equal to System.Console.BufferWidth.</exception>
        ''' <exception cref="System.ArgumentException">One or both of the color parameters is not a member of the System.ConsoleColor enumeration.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub MoveBufferArea(sourceLeft As Integer, sourceTop As Integer, sourceWidth As Integer, sourceHeight As Integer, targetLeft As Integer, targetTop As Integer, sourceChar As Char, sourceForeColor As System.ConsoleColor, sourceBackColor As System.ConsoleColor)
            Call Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, sourceChar, sourceForeColor, sourceBackColor)
        End Sub

        ''' <summary>
        ''' Sets the foreground and background console colors to their defaults.
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub ResetColor()
            Call Console.ResetColor()
        End Sub

        ''' <summary>
        ''' Sets the height and width of the screen buffer area to the specified values.
        ''' </summary>
        ''' <param name="width">The width of the buffer area measured in columns.</param>
        ''' <param name="height">The height of the buffer area measured in rows.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">height or width is less than or equal to zero.-or- height or width is greater than or equal to System.Int16.MaxValue.-or- width is less than System.Console.WindowLeft + System.Console.WindowWidth.-or- height is less than System.Console.WindowTop + System.Console.WindowHeight.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub SetBufferSize(width As Integer, height As Integer)
            Call Console.SetBufferSize(width, height)
        End Sub

        ''' <summary>
        ''' Sets the position of the cursor.
        ''' </summary>
        ''' <param name="left">The column position of the cursor.</param>
        ''' <param name="top">The row position of the cursor.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">left or top is less than zero.-or- left is greater than or equal to System.Console.BufferWidth.-or- top is greater than or equal to System.Console.BufferHeight.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub SetCursorPosition(left As Integer, top As Integer)
            Call Console.SetCursorPosition(left, top)
        End Sub

        ''' <summary>
        ''' Sets the System.Console.Error property to the specified System.IO.TextWriter object.
        ''' </summary>
        ''' <param name="newError">A stream that is the new standard error output.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentNullException">newError is null.</exception>
        ''' <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        Public Sub SetError(newError As System.IO.TextWriter)
            Call Console.SetError(newError)
        End Sub

        ''' <summary>
        ''' Sets the System.Console.In property to the specified System.IO.TextReader object.
        ''' </summary>
        ''' <param name="newIn">A stream that is the new standard input.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentNullException">newIn is null.</exception>
        ''' <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        Public Sub SetIn(newIn As System.IO.TextReader)
            Call Console.SetIn(newIn)
        End Sub

        ''' <summary>
        ''' Sets the System.Console.Out property to the specified System.IO.TextWriter object.
        ''' </summary>
        ''' <param name="newOut">A stream that is the new standard output.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentNullException">newOut is null.</exception>
        ''' <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        Public Sub SetOut(newOut As System.IO.TextWriter)
            Call Console.SetOut(newOut)
        End Sub

        ''' <summary>
        ''' Sets the position of the console window relative to the screen buffer.
        ''' </summary>
        ''' <param name="left">The column position of the upper left corner of the console window.</param>
        ''' <param name="top">The row position of the upper left corner of the console window.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">left or top is less than zero.-or- left + System.Console.WindowWidth is greater than System.Console.BufferWidth.-or- top + System.Console.WindowHeight is greater than System.Console.BufferHeight.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub SetWindowPosition(left As Integer, top As Integer)
            Call Console.SetWindowPosition(left, top)
        End Sub

        ''' <summary>
        ''' Sets the height and width of the console window to the specified values.
        ''' </summary>
        ''' <param name="width">The width of the console window measured in columns.</param>
        ''' <param name="height">The height of the console window measured in rows.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">width or height is less than or equal to zero.-or- width plus System.Console.WindowLeft or height plus System.Console.WindowTop is greater than or equal to System.Int16.MaxValue. -or-width or height is greater than the largest possible window width or height for the current screen resolution and console font.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub SetWindowSize(width As Integer, height As Integer)
            Call Console.SetWindowSize(width, height)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified Boolean value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As Boolean)
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the specified array of Unicode characters to the standard output stream.
        ''' </summary>
        ''' <param name="buffer">A Unicode character array.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(buffer() As Char)
            Call Console.Write(buffer)
        End Sub

        ''' <summary>
        ''' Writes the specified subarray of Unicode characters to the standard output stream.
        ''' </summary>
        ''' <param name="buffer">An array of Unicode characters.</param>
        ''' <param name="index">The starting position in buffer.</param>
        ''' <param name="count">The number of characters to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentNullException">buffer is null.</exception>
        ''' <exception cref="System.ArgumentOutOfRangeException">index or count is less than zero.</exception>
        ''' <exception cref="System.ArgumentException">index plus count specify a position that is not within buffer.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(buffer() As Char, index As Integer, count As Integer)
            Call Console.Write(buffer, index, count)
        End Sub

        ''' <summary>
        ''' Writes the specified Unicode character value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As Char)
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified System.Decimal value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As Decimal)
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified double-precision floating-point value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As Double)
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified 32-bit signed integer value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As Integer)
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified 64-bit signed integer value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As Long)
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified object to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write, or null.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As Object)
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified single-precision floating-point value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As Single)
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the specified string value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As String) Implements STDIO__.IConsole.Write, IShellDevice.SetPrompt
            Call Console.Write(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified object to the standard output stream using the specified format information.
        ''' </summary>
        ''' <param name="format">A composite format string (see Remarks).</param>
        ''' <param name="arg0">An object to write using format.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.ArgumentNullException">format is null.</exception>
        ''' <exception cref="System.FormatException">The format specification in format is invalid.</exception>
        Public Sub Write(format As String, arg0 As Object)
            Call Console.Write(format, arg0)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified objects to the standard output stream using the specified format information.
        ''' </summary>
        ''' <param name="format">A composite format string (see Remarks).</param>
        ''' <param name="arg0">The first object to write using format.</param>
        ''' <param name="arg1">The second object to write using format.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.ArgumentNullException">format is null.</exception>
        ''' <exception cref="System.FormatException">The format specification in format is invalid.</exception>
        Public Sub Write(format As String, arg0 As Object, arg1 As Object)
            Call Console.Write(format, arg0, arg1)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified objects to the standard output stream using the specified format information.
        ''' </summary>
        ''' <param name="format">A composite format string (see Remarks).</param>
        ''' <param name="arg0">The first object to write using format.</param>
        ''' <param name="arg1">The second object to write using format.</param>
        ''' <param name="arg2">The third object to write using format.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.ArgumentNullException">format is null.</exception>
        ''' <exception cref="System.FormatException">The format specification in format is invalid.</exception>
        Public Sub Write(format As String, arg0 As Object, arg1 As Object, arg2 As Object)
            Call Console.Write(format, arg0, arg1, arg2)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified array of objects to the standard output stream using the specified format information.
        ''' </summary>
        ''' <param name="format">A composite format string (see Remarks).</param>
        ''' <param name="args">An array of objects to write using format.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.ArgumentNullException">format or arg is null.</exception>
        ''' <exception cref="System.FormatException">The format specification in format is invalid.</exception>
        Public Sub Write(format As String, ParamArray args() As Object) Implements STDIO__.IConsole.WriteLine
            Call Console.WriteLine(format, args)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified 32-bit unsigned integer value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As UInteger)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified 64-bit unsigned integer value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub Write(value As ULong)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the current line terminator to the standard output stream.
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(str As String) Implements STDIO__.IConsole.WriteLine
            Call Console.WriteLine(str)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified Boolean value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As Boolean)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the specified array of Unicode characters, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="buffer">A Unicode character array.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(buffer() As Char)
            Call Console.WriteLine(buffer)
        End Sub

        ''' <summary>
        ''' Writes the specified subarray of Unicode characters, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="buffer">An array of Unicode characters.</param>
        ''' <param name="index">The starting position in buffer.</param>
        ''' <param name="count">The number of characters to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentNullException">buffer is null.</exception>
        ''' <exception cref="System.ArgumentOutOfRangeException">index or count is less than zero.</exception>
        ''' <exception cref="System.ArgumentException">index plus count specify a position that is not within buffer.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(buffer() As Char, index As Integer, count As Integer)
            Call Console.WriteLine(buffer, index, count)
        End Sub

        ''' <summary>
        ''' Writes the specified Unicode character, followed by the current line terminator, value to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As Char)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified System.Decimal value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As Decimal)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified double-precision floating-point value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As Double)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified 32-bit signed integer value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As Integer)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified 64-bit signed integer value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As Long)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified object, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As Object)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified single-precision floating-point value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As Single)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the specified string value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Overridable Sub WriteLine() Implements STDIO__.IConsole.WriteLine
            Call Console.WriteLine()
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified object, followed by the current line terminator, to the standard output stream using the specified format information.
        ''' </summary>
        ''' <param name="format">A composite format string (see Remarks).</param>
        ''' <param name="arg0">An object to write using format.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.ArgumentNullException">format is null.</exception>
        ''' <exception cref="System.FormatException">The format specification in format is invalid.</exception>
        Public Sub WriteLine(format As String, arg0 As Object)
            Call Console.WriteLine(format, arg0)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified objects, followed by the current line terminator, to the standard output stream using the specified format information.
        ''' </summary>
        ''' <param name="format">A composite format string (see Remarks).</param>
        ''' <param name="arg0">The first object to write using format.</param>
        ''' <param name="arg1">The second object to write using format.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.ArgumentNullException">format is null.</exception>
        ''' <exception cref="System.FormatException">The format specification in format is invalid.</exception>
        Public Sub WriteLine(format As String, arg0 As Object, arg1 As Object)
            Call Console.WriteLine(format, arg0, arg1)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified objects, followed by the current line terminator, to the standard output stream using the specified format information.
        ''' </summary>
        ''' <param name="format">A composite format string (see Remarks).</param>
        ''' <param name="arg0">The first object to write using format.</param>
        ''' <param name="arg1">The second object to write using format.</param>
        ''' <param name="arg2">The third object to write using format.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.ArgumentNullException">format is null.</exception>
        ''' <exception cref="System.FormatException">The format specification in format is invalid.</exception>
        Public Sub WriteLine(format As String, arg0 As Object, arg1 As Object, arg2 As Object)
            Call Console.WriteLine(format, arg0, arg1, arg2)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified array of objects, followed by the current line terminator, to the standard output stream using the specified format information.
        ''' </summary>
        ''' <param name="format">A composite format string (see Remarks).</param>
        ''' <param name="arg">An array of objects to write using format.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.ArgumentNullException">format or arg is null.</exception>
        ''' <exception cref="System.FormatException">The format specification in format is invalid.</exception>
        Public Sub WriteLine(format As String, ParamArray arg() As Object)
            Call Console.WriteLine(format, arg)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified 32-bit unsigned integer value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As UInteger)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified 64-bit unsigned integer value, followed by the current line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="value">The value to write.</param>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Sub WriteLine(value As ULong)
            Call Console.WriteLine(value)
        End Sub

        ''' <summary>
        ''' Acquires the standard error stream.
        ''' </summary>
        ''' <returns>The standard error stream.</returns>
        ''' <remarks></remarks>
        Public Function OpenStandardError() As System.IO.Stream
            Return Console.OpenStandardError
        End Function

        ''' <summary>
        ''' Acquires the standard error stream, which is set to a specified buffer size.
        ''' </summary>
        ''' <param name="bufferSize">The internal stream buffer size.</param>
        ''' <returns>The standard error stream.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">bufferSize is less than or equal to zero.</exception>
        Public Function OpenStandardError(bufferSize As Integer) As System.IO.Stream
            Return Console.OpenStandardError(bufferSize)
        End Function

        ''' <summary>
        ''' Acquires the standard input stream.
        ''' </summary>
        ''' <returns>The standard input stream.</returns>
        ''' <remarks></remarks>
        Public Function OpenStandardInput() As System.IO.Stream
            Return Console.OpenStandardInput
        End Function

        ''' <summary>
        ''' Acquires the standard input stream, which is set to a specified buffer size.
        ''' </summary>
        ''' <param name="bufferSize">The internal stream buffer size.</param>
        ''' <returns>The standard input stream.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">bufferSize is less than or equal to zero.</exception>
        Public Function OpenStandardInput(bufferSize As Integer) As System.IO.Stream
            Return Console.OpenStandardInput(bufferSize)
        End Function

        ''' <summary>
        ''' Acquires the standard output stream.
        ''' </summary>
        ''' <returns>The standard output stream.</returns>
        ''' <remarks></remarks>
        Public Function OpenStandardOutput() As System.IO.Stream
            Return Console.OpenStandardOutput()
        End Function

        ''' <summary>
        ''' Acquires the standard output stream, which is set to a specified buffer size.
        ''' </summary>
        ''' <param name="bufferSize">The internal stream buffer size.</param>
        ''' <returns>The standard output stream.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">bufferSize is less than or equal to zero.</exception>
        Public Function OpenStandardOutput(bufferSize As Integer) As System.IO.Stream
            Return Console.OpenStandardOutput(bufferSize)
        End Function

        ''' <summary>
        ''' Reads the next character from the standard input stream.
        ''' </summary>
        ''' <returns>The next character from the input stream, or negative one (-1) if there are currently no more characters to be read.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Overridable Function Read() As Integer Implements STDIO__.IConsole.Read
            Return Console.Read
        End Function

        ''' <summary>
        ''' Obtains the next character or function key pressed by the user. The pressed key is displayed in the console window.
        ''' </summary>
        ''' <returns>A System.ConsoleKeyInfo object that describes the System.ConsoleKey constant and Unicode character, if any, that correspond to the pressed console key. The System.ConsoleKeyInfo object also describes, in a bitwise combination of System.ConsoleModifiers values, whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously with the console key.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.InvalidOperationException">The System.Console.In property is redirected from some stream other than the console.</exception>
        Public Overridable Function ReadKey() As System.ConsoleKeyInfo Implements STDIO__.IConsole.ReadKey
            Return Console.ReadKey
        End Function

        ''' <summary>
        ''' Obtains the next character or function key pressed by the user. The pressed key is optionally displayed in the console window.
        ''' </summary>
        ''' <param name="intercept">Determines whether to display the pressed key in the console window. true to not display the pressed key; otherwise, false.</param>
        ''' <returns>A System.ConsoleKeyInfo object that describes the System.ConsoleKey constant and Unicode character, if any, that correspond to the pressed console key. The System.ConsoleKeyInfo object also describes, in a bitwise combination of System.ConsoleModifiers values, whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously with the console key.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.InvalidOperationException">The System.Console.In property is redirected from some stream other than the console.</exception>
        Public Overridable Function ReadKey(intercept As Boolean) As System.ConsoleKeyInfo
            Return Console.ReadKey(intercept)
        End Function

        ''' <summary>
        ''' Reads the next line of characters from the standard input stream.
        ''' </summary>
        ''' <returns>The next line of characters from the input stream, or null if no more lines are available.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
        ''' <exception cref="System.ArgumentOutOfRangeException">The number of characters in the next line of characters is greater than System.Int32.MaxValue.</exception>
        Public Overridable Function ReadLine() As String Implements STDIO__.IConsole.ReadLine, IShellDevice.ReadLine
            Return Console.ReadLine
        End Function
#End Region

#Region "Public Property"

        ''' <summary>
        ''' Gets or sets the background color of the console.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A System.ConsoleColor that specifies the background color of the console; that is, the color that appears behind each character. The default is black.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentException">The color specified in a set operation is not a valid Color.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property BackgroundColor As System.ConsoleColor Implements STDIO__.IConsole.BackgroundColor
            Get
                Return Console.BackgroundColor
            End Get
            Set(value As System.ConsoleColor)
                Console.BackgroundColor = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the height of the buffer area.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current height, in rows, of the buffer area.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">The value in a set operation is less than or equal to zero.-or- The value in a set operation is greater than or equal to System.Int16.MaxValue.-or- The value in a set operation is less than System.Console.WindowTop + System.Console.WindowHeight.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property BufferHeight As Integer
            Get
                Return Console.BufferHeight
            End Get
            Set(value As Integer)
                Console.BufferHeight = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the width of the buffer area.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current width, in columns, of the buffer area.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">The value in a set operation is less than or equal to zero.-or- The value in a set operation is greater than or equal to System.Int16.MaxValue.-or- The value in a set operation is less than System.Console.WindowLeft + System.Console.WindowWidth.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property BufferWidth As Integer
            Get
                Return Console.BufferWidth
            End Get
            Set(value As Integer)
                Console.BufferWidth = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the CAPS LOCK keyboard toggle is turned on or turned off.
        ''' </summary>
        ''' <value></value>
        ''' <returns>true if CAPS LOCK is turned on; false if CAPS LOCK is turned off.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CapsLock As Boolean
            Get
                Return Console.CapsLock
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the column position of the cursor within the buffer area.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current position, in columns, of the cursor.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">The value in a set operation is less than zero.-or- The value in a set operation is greater than or equal to System.Console.BufferWidth.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property CursorLeft As Integer
            Get
                Return Console.CursorLeft
            End Get
            Set(value As Integer)
                Console.CursorLeft = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the height of the cursor within a character cell.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The size of the cursor expressed as a percentage of the height of a character cell. The property value ranges from 1 to 100.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">The value specified in a set operation is less than 1 or greater than 100.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property CursorSize As Integer
            Get
                Return Console.CursorSize
            End Get
            Set(value As Integer)
                Console.CursorSize = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the row position of the cursor within the buffer area.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current position, in rows, of the cursor.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">The value in a set operation is less than zero.-or- The value in a set operation is greater than or equal to System.Console.BufferHeight.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property CursorTop As Integer
            Get
                Return Console.CursorTop
            End Get
            Set(value As Integer)
                Console.CursorTop = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the cursor is visible.
        ''' </summary>
        ''' <value></value>
        ''' <returns>true if the cursor is visible; otherwise, false.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property CursorVisible As Boolean
            Get
                Return Console.CursorVisible
            End Get
            Set(value As Boolean)
                Console.CursorVisible = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the standard error output stream.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A System.IO.TextWriter that represents the standard error output stream.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property [Error] As System.IO.TextWriter
            Get
                Return Console.Error
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the foreground color of the console.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A System.ConsoleColor that specifies the foreground color of the console; that is, the color of each character that is displayed. The default is gray.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentException">The color specified in a set operation is not a valid Color.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property ForegroundColor As System.ConsoleColor Implements STDIO__.IConsole.ForegroundColor
            Get
                Return Console.ForegroundColor
            End Get
            Set(value As System.ConsoleColor)
                Console.ForegroundColor = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the standard input stream.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A System.IO.TextReader that represents the standard input stream.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property [In] As System.IO.TextReader
            Get
                Return Console.In
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the encoding the console uses to read input.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The encoding used to read console input.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentNullException">The property value in a set operation is null.</exception>
        ''' <exception cref="System.IO.IOException">An error occurred during the execution of this operation.</exception>
        ''' <exception cref="System.Security.SecurityException">Your application does not have permission to perform this operation.</exception>
        Public Property InputEncoding As System.Text.Encoding
            Get
                Return Console.InputEncoding
            End Get
            Set(value As System.Text.Encoding)
                Console.InputEncoding = value
            End Set
        End Property

#If NET_40 = 0 Then

        ''' <summary>
        ''' Gets a value that indicates whether the error output stream has been redirected from the standard error stream.
        ''' </summary>
        ''' <value></value>
        ''' <returns>true if error output is redirected; otherwise, false.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsErrorRedirected As Boolean
            Get
                Return Console.IsErrorRedirected
            End Get
        End Property

        ''' <summary>
        ''' Gets a value that indicates whether input has been redirected from the standard input stream.
        ''' </summary>
        ''' <value></value>
        ''' <returns>true if input is redirected; otherwise, false.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsInputRedirected As Boolean
            Get
                Return Console.IsInputRedirected
            End Get
        End Property

        ''' <summary>
        ''' Gets a value that indicates whether output has been redirected from the standard output stream.
        ''' </summary>
        ''' <value></value>
        ''' <returns>true if output is redirected; otherwise, false.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsOutputRedirected As Boolean
            Get
                Return Console.IsOutputRedirected
            End Get
        End Property
#End If

        ''' <summary>
        ''' Gets a value indicating whether a key press is available in the input stream.
        ''' </summary>
        ''' <value></value>
        ''' <returns>true if a key press is available; otherwise, false.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        ''' <exception cref="System.InvalidOperationException">Standard input is redirected to a file instead of the keyboard.</exception>
        Public ReadOnly Property KeyAvailable As Boolean
            Get
                Return Console.KeyAvailable
            End Get
        End Property

        ''' <summary>
        ''' Gets the largest possible number of console window rows, based on the current font and screen resolution.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The height of the largest possible console window measured in rows.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LargestWindowHeight As Integer
            Get
                Return Console.LargestWindowHeight
            End Get
        End Property

        ''' <summary>
        ''' Gets the largest possible number of console window columns, based on the current font and screen resolution.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The width of the largest possible console window measured in columns.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LargestWindowWidth As Integer
            Get
                Return Console.LargestWindowWidth
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the NUM LOCK keyboard toggle is turned on or turned off.
        ''' </summary>
        ''' <value></value>
        ''' <returns>true if NUM LOCK is turned on; false if NUM LOCK is turned off.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NumberLock As Boolean
            Get
                Return Console.NumberLock
            End Get
        End Property

        ''' <summary>
        ''' Gets the standard output stream.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A System.IO.TextWriter that represents the standard output stream.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Out As System.IO.TextWriter
            Get
                Return Console.Out
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the encoding the console uses to write output.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The encoding used to write console output.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentNullException">The property value in a set operation is null.</exception>
        ''' <exception cref="System.IO.IOException">An error occurred during the execution of this operation.</exception>
        ''' <exception cref="System.Security.SecurityException">Your application does not have permission to perform this operation.</exception>
        Public Property OutputEncoding As System.Text.Encoding
            Get
                Return Console.OutputEncoding
            End Get
            Set(value As System.Text.Encoding)
                Console.OutputEncoding = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the title to display in the console title bar.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The string to be displayed in the title bar of the console. The maximum length of the title string is 24500 characters.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.InvalidOperationException">In a get operation, the retrieved title is longer than 24500 characters.</exception>
        ''' <exception cref="System.ArgumentOutOfRangeException">In a set operation, the specified title is longer than 24500 characters.</exception>
        ''' <exception cref="System.ArgumentNullException">In a set operation, the specified title is null.</exception>
        ''' <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        Public Property Title As String
            Get
                Return Console.Title
            End Get
            Set(value As String)
                Console.Title = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the combination of the System.ConsoleModifiers.Control modifier key and System.ConsoleKey.C console key (Ctrl+C) is treated as ordinary input or as an interruption that is handled by the operating system.
        ''' </summary>
        ''' <value></value>
        ''' <returns>true if Ctrl+C is treated as ordinary input; otherwise, false.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.IO.IOException">Unable to get or set the input mode of the console input buffer.</exception>
        Public Property TreatControlCAsInput As Boolean
            Get
                Return Console.TreatControlCAsInput
            End Get
            Set(value As Boolean)
                Console.TreatControlCAsInput = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the height of the console window area.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The height of the console window measured in rows.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">The value of the System.Console.WindowWidth property or the value of the System.Console.WindowHeight property is less than or equal to 0.-or-The value of the System.Console.WindowHeight property plus the value of the System.Console.WindowTop property is greater than or equal to System.Int16.MaxValue.-or-The value of the System.Console.WindowWidth property or the value of the System.Console.WindowHeight property is greater than the largest possible window width or height for the current screen resolution and console font.</exception>
        ''' <exception cref="System.IO.IOException">Error reading or writing information.</exception>
        Public Property WindowHeight As Integer
            Get
                Return Console.WindowHeight
            End Get
            Set(value As Integer)
                Console.WindowHeight = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the leftmost position of the console window area relative to the screen buffer.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The leftmost console window position measured in columns.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">In a set operation, the value to be assigned is less than zero.-or-As a result of the assignment, System.Console.WindowLeft plus System.Console.WindowWidth would exceed System.Console.BufferWidth.</exception>
        ''' <exception cref="System.IO.IOException">Error reading or writing information.</exception>
        Public Property WindowLeft As Integer
            Get
                Return Console.WindowLeft
            End Get
            Set(value As Integer)
                Console.WindowLeft = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the top position of the console window area relative to the screen buffer.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The uppermost console window position measured in rows.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">In a set operation, the value to be assigned is less than zero.-or-As a result of the assignment, System.Console.WindowTop plus System.Console.WindowHeight would exceed System.Console.BufferHeight.</exception>
        ''' <exception cref="System.IO.IOException">Error reading or writing information.</exception>
        Public Property WindowTop As Integer
            Get
                Return Console.WindowTop
            End Get
            Set(value As Integer)
                Console.WindowTop = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the width of the console window.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The width of the console window measured in columns.</returns>
        ''' <remarks></remarks>
        ''' <exception cref="System.ArgumentOutOfRangeException">The value of the System.Console.WindowWidth property or the value of the System.Console.WindowHeight property is less than or equal to 0.-or-The value of the System.Console.WindowHeight property plus the value of the System.Console.WindowTop property is greater than or equal to System.Int16.MaxValue.-or-The value of the System.Console.WindowWidth property or the value of the System.Console.WindowHeight property is greater than the largest possible window width or height for the current screen resolution and console font.</exception>
        ''' <exception cref="System.IO.IOException">Error reading or writing information.</exception>
        Public Property WindowWidth As Integer Implements STDIO__.IConsole.WindowWidth
            Get
                Return Console.WindowWidth
            End Get
            Set(value As Integer)
                Console.WindowWidth = value
            End Set
        End Property

#End Region

        ''' <summary>
        ''' Occurs when the System.ConsoleModifiers.Control modifier key (Ctrl) and either the System.ConsoleKey.C console key (C) or the Break key are pressed simultaneously (Ctrl+C or Ctrl+Break).
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event CancelKeyPress(sender As Object, e As System.ConsoleCancelEventArgs)
        Public Event Tab() Implements IConsole.Tab
#End Region
    End Class
End Namespace
