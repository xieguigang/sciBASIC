#Region "Microsoft.VisualBasic::7cb4c9de3c3a481d4d8a749b0cc0156f, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ConsolePasswordInput.vb"

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

    '   Total Lines: 467
    '    Code Lines: 190 (40.69%)
    ' Comment Lines: 231 (49.46%)
    '    - Xml Docs: 27.27%
    ' 
    '   Blank Lines: 46 (9.85%)
    '     File Size: 24.09 KB


    '     Module Constants
    ' 
    ' 
    ' 
    '     Structure uCharUnion
    ' 
    ' 
    ' 
    '     Structure KEY_EVENT_RECORD
    ' 
    ' 
    ' 
    '     Structure COORD
    ' 
    ' 
    ' 
    '     Structure MOUSE_EVENT_RECORD
    ' 
    ' 
    ' 
    '     Structure WINDOW_BUFFER_SIZE_RECORD
    ' 
    ' 
    ' 
    '     Structure MENU_EVENT_RECORD
    ' 
    ' 
    ' 
    '     Structure FOCUS_EVENT_RECORD
    ' 
    ' 
    ' 
    '     Structure EventUnion
    ' 
    ' 
    ' 
    '     Structure INPUT_RECORD
    ' 
    ' 
    ' 
    '     Class ConsolePasswordInput
    ' 
    '         Function: FlushConsoleInputBuffer, GetConsoleMode, GetLastError, GetStdHandle, ReadConsoleInput
    '                   SetConsoleMode, WriteConsole, WriteConsoleOutputCharacter
    '         Delegate Function
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: FocusEventProc, KeyEventProc, MenuEventProc, MouseEventProc, WindowBufferSizeEventProc
    ' 
    '             Sub: PasswordInput
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports System.Collections
Imports Microsoft.VisualBasic.Language

Namespace ApplicationServices.Terminal.Utility

    ''' <summary>
    ''' Constants used with PInvoke methods
    ''' </summary>
    ''' <remarks></remarks>
    Module Constants
        ' Standard input, output, and error
        Public Const STD_INPUT_HANDLE As Integer = -10
        Public Const STD_OUTPUT_HANDLE As Integer = -11
        Public Const STD_ERROR_HANDLE As Integer = -12

        '  Input Mode flags.
        Public Const ENABLE_WINDOW_INPUT As Integer = &H8
        Public Const ENABLE_MOUSE_INPUT As Integer = &H10

        ''' <summary>
        ''' EventType flags.
        ''' </summary>
        Public Const KEY_EVENT As Integer = &H1
        ''' <summary>
        ''' Event contains key event record
        ''' </summary>
        Public Const MOUSE_EVENT As Integer = &H2
        ''' <summary>
        ''' Event contains mouse event record
        ''' </summary>
        Public Const WINDOW_BUFFER_SIZE_EVENT As Integer = &H4
        ''' <summary>
        ''' Event contains window change event record
        ''' </summary>
        Public Const MENU_EVENT As Integer = &H8
        ''' <summary>
        ''' Event contains menu event record
        ''' </summary>
        Public Const FOCUS_EVENT As Integer = &H10
        ''' <summary>
        ''' Event contains focus change
        ''' Returned by GetStdHandle when an error occurs
        ''' </summary>
        Public ReadOnly INVALID_HANDLE_VALUE As New IntPtr(-1)
    End Module

    ''' <summary>
    ''' ' Struct uChar is meant to support the Windows Console API's uChar union.
    ''' ' Unions do not exist in the pure .NET world. We have to use the regular
    ''' ' C# struct and the StructLayout and FieldOffset Attributes to preserve
    ''' ' the memory layout of the unmanaged union.
    ''' '
    ''' ' We specify the "LayoutKind.Explicit" value for the StructLayout attribute
    ''' ' to specify that every field of the struct uChar is marked with a byte offset.
    ''' '
    ''' ' This byte offset is specified by the FieldOffsetAttribute and it indicates
    ''' ' the number of bytes between the beginning of the struct in memory and the
    ''' ' beginning of the field.
    ''' '
    ''' ' As you can see in the struct uChar (below), the fields "UnicodeChar"
    ''' ' and "AsciiChar" have been marked as being of offset 0. This is the only
    ''' ' way that an unmanaged C/C++ union can be represented in C#.
    ''' '
    ''' </summary>
    ''' <remarks></remarks>
    <StructLayout(LayoutKind.Explicit)>
    Friend Structure uCharUnion
        <FieldOffset(0)>
        Friend UnicodeChar As UShort
        <FieldOffset(0)>
        Friend AsciiChar As Byte
    End Structure

    ''' <summary>
    ''' ' The struct KEY_EVENT_RECORD is used to report keyboard input events
    ''' ' in a console INPUT_RECORD structure.
    ''' '
    ''' ' Internally, it uses the structure uChar which is treated as a union
    ''' ' in the unmanaged world.
    ''' '
    ''' </summary>
    ''' <remarks></remarks>
    <StructLayout(LayoutKind.Sequential, Pack:=8)>
    Friend Structure KEY_EVENT_RECORD
        Friend bKeyDown As Integer
        Friend wRepeatCount As UShort
        Friend wVirtualKeyCode As UShort
        Friend wVirtualScanCode As UShort
        Friend uchar As uCharUnion
        Friend dwControlKeyState As UInteger
    End Structure

    ' The other stuctures are not used within our application.
    Friend Structure COORD
        Friend X As Short
        Friend Y As Short
    End Structure

    Friend Structure MOUSE_EVENT_RECORD
        Friend dwMousePosition As COORD
        Friend dwButtonState As UInteger
        Friend dwControlKeyState As UInteger
        Friend dwEventFlags As UInteger
    End Structure

    Friend Structure WINDOW_BUFFER_SIZE_RECORD
        Friend dwSize As COORD
    End Structure

    Friend Structure MENU_EVENT_RECORD
        Friend dwCommandId As UInteger
    End Structure

    Friend Structure FOCUS_EVENT_RECORD
        Friend bSetFocus As Boolean
    End Structure

    ' The EventUnion struct is also treated as a union in the unmanaged world.
    ' We therefore use the StructLayoutAttribute and the FieldOffsetAttribute.
    <StructLayout(LayoutKind.Explicit)>
    Friend Structure EventUnion
        <FieldOffset(0)>
        Friend KeyEvent As KEY_EVENT_RECORD
        <FieldOffset(0)>
        Friend MouseEvent As MOUSE_EVENT_RECORD
        <FieldOffset(0)>
        Friend WindowBufferSizeEvent As WINDOW_BUFFER_SIZE_RECORD
        <FieldOffset(0)>
        Friend MenuEvent As MENU_EVENT_RECORD
        <FieldOffset(0)>
        Friend FocusEvent As FOCUS_EVENT_RECORD
    End Structure

    ' The INPUT_RECORD structure is used within our application
    ' to capture console input data.
    Friend Structure INPUT_RECORD
        Friend EventType As UShort
        Friend [Event] As EventUnion
    End Structure

    ''' <summary>
    ''' Summary description for ConsolePasswordInput.
    ''' </summary>
    ''' <remarks>
    ''' .NET Console Password Input By Masking Keyed-In Characters
    ''' http://www.codeproject.com/Articles/8110/NET-Console-Password-Input-By-Masking-Keyed-In-Ch
    ''' </remarks>
    Public NotInheritable Class ConsolePasswordInput
        ' This class requires alot of imported functions from Kernel32.dll.

        ' ReadConsoleInput() is used to read data from a console input buffer and then remove it from the buffer.
        ' We will be relying heavily on this function.
        <DllImport("Kernel32.DLL", EntryPoint:="ReadConsoleInputW", CallingConvention:=CallingConvention.StdCall)>
        Private Shared Function ReadConsoleInput(hConsoleInput As IntPtr, <Out> lpBuffer As INPUT_RECORD(), nLength As UInteger, ByRef lpNumberOfEventsRead As UInteger) As Boolean
        End Function

        ' The GetStdHandle() function retrieves a handle for the standard input, standard output, or standard
        ' error device, depending on its input parameter.
        ' Handles returned by GetStdHandle() can be used by applications that need to read from or write
        ' to the console. We will be using the handle returned by GetStdHandle() to call the various
        ' Console APIs.
        ' Note that although handles are integers by default, we will be using the managed type IntPtr
        ' to represent the unmanaged world's HANDLE types. This is the recommended practice as expounded
        ' in the documentation.
        <DllImport("Kernel32.DLL", EntryPoint:="GetStdHandle", CallingConvention:=CallingConvention.StdCall)>
        Public Shared Function GetStdHandle(nStdHandle As Integer) As IntPtr
        End Function

        ' The GetConsoleMode() function retrieves the current input mode of a console's input buffer
        ' or the current output mode of a console screen buffer.
        ' A console consists of an input buffer and one or more screen buffers. The mode of a console
        ' buffer determines how the console behaves during input or output (I/O) operations.
        ' One set of flag constants is used with input handles, and another set is used with screen buffer
        ' (output) handles.
        ' Setting the output modes of one screen buffer does not affect the output modes of other
        ' screen buffers.
        ' We shall be retrieving the mode of our console during password input in order to temporarily
        ' modify the console mode. Later, after retrieving the required password, we will need to restore
        ' the original console mode.
        <DllImport("Kernel32.DLL", EntryPoint:="GetConsoleMode", CallingConvention:=CallingConvention.StdCall)>
        Public Shared Function GetConsoleMode(hConsoleHandle As IntPtr, ByRef Mode As Integer) As Boolean
        End Function

        ' The SetConsoleMode() function sets the input mode of a console's input buffer or the output mode
        ' of a console screen buffer.
        ' We will be calling this API before the end of our password processing function to restore the
        ' previous console mode.
        <DllImport("Kernel32.DLL", EntryPoint:="SetConsoleMode", CallingConvention:=CallingConvention.StdCall)>
        Public Shared Function SetConsoleMode(hConsoleHandle As IntPtr, Mode As Integer) As Boolean
        End Function

        ' GetLastError() is a useful Win32 API to determine the cause of a problem when something went wrong.
        <DllImport("Kernel32.DLL", EntryPoint:="GetLastError", CallingConvention:=CallingConvention.StdCall)>
        Public Shared Function GetLastError() As UInteger
        End Function

        ' The WriteConsole() function writes a character string to a console screen buffer beginning
        ' at the current cursor location.
        ' We will be using this API to write '*'s to the screen in place of a password character.
        ' handle to screen buffer
        ' write buffer
        ' number of characters to write
        ' number of characters written
        <DllImport("Kernel32.DLL", EntryPoint:="WriteConsoleW", CallingConvention:=CallingConvention.StdCall)>
        Public Shared Function WriteConsole(hConsoleOutput As IntPtr, lpBuffer As String, nNumberOfCharsToWrite As UInteger, ByRef lpNumberOfCharsWritten As UInteger, lpReserved As IntPtr) As Boolean
            ' reserved
        End Function

        ' Not used in this application but declared here for possible future use.
        <DllImport("Kernel32.DLL", EntryPoint:="FlushConsoleInputBuffer", CallingConvention:=CallingConvention.StdCall)>
        Public Shared Function FlushConsoleInputBuffer(hConsoleInput As IntPtr) As Boolean
        End Function

        ' Not used in this application but declared here for possible future use.
        ' handle to screen buffer
        ' characters
        ' number of characters to write
        ' first cell coordinates
        <DllImport("Kernel32.DLL", EntryPoint:="WriteConsoleOutputCharacterW", CallingConvention:=CallingConvention.StdCall)>
        Private Shared Function WriteConsoleOutputCharacter(hConsoleOutput As IntPtr, lpCharacter As String, nLength As UInteger, dwWriteCoord As COORD, ByRef lpNumberOfCharsWritten As UInteger) As Boolean
            ' number of cells written
        End Function

        ' Declare a delegate to encapsulate a console event handler function.
        ' All event handler functions must return a boolean value indicating whether
        ' the password processing function should continue to read in another console
        ' input record (via ReadConsoleInput() API).
        ' Returning a true indicates continue.
        ' Returning a false indicates don't continue.
        Friend Delegate Function ConsoleInputEvent(input_record As INPUT_RECORD, ByRef strBuildup As String) As Boolean
        ' Std console input and output handles.
        Protected hStdin As IntPtr = IntPtr.Zero
        Protected hStdout As IntPtr = IntPtr.Zero
        ' Used to set and reset console modes.
        Protected dwSaveOldMode As Integer = 0
        Protected dwMode As Integer = 0
        ' Counter used to detect how many characters have been typed in.
        Protected iCounter As i32 = 0
        ' Hashtable to store console input event handler functions.
        Protected htCodeLookup As Hashtable
        ' Used to indicate the maximum number of characters for a password. 20 is the default.
        Protected iMaxNumberOfCharacters As Integer

        Const strOutput As String = "*"

        ' Event handler to handle a keyboard event.
        ' We use this function to accumulate characters typed into the console and build
        ' up the password this way.
        ' All event handler functions must return a boolean value indicating whether
        ' the password processing function should continue to read in another console
        ' input record (via ReadConsoleInput() API).
        ' Returning a true indicates continue.
        ' Returning a false indicates don't continue.
        Private Function KeyEventProc(input_record As INPUT_RECORD, ByRef strBuildup As String) As Boolean
            ' From the INPUT_RECORD, extract the KEY_EVENT_RECORD structure.
            Dim ker As KEY_EVENT_RECORD = input_record.[Event].KeyEvent

            ' We process only during the keydown event.
            If ker.bKeyDown <> 0 Then
                Dim intptr As New IntPtr(0)
                ' This is to simulate a NULL handle value.
                Dim ch As Char = ChrW(ker.uchar.UnicodeChar)
                ' Get the current character pressed.
                Dim dwNumberOfCharsWritten As UInteger = 0

                ' The character string that will be displayed on the console screen.
                ' If we have received a Carriage Return character, we exit.
                If ch = CChar(ControlChars.Cr) Then
                    Return False
                Else
                    If AscW(ch) > 0 Then
                        ' The typed in key must represent a character and must not be a control ley (e.g. SHIFT, ALT, CTRL, etc)
                        ' A regular (non Carriage-Return character) is typed in...

                        ' We first display a '*' on the screen...
                        ' handle to screen buffer
                        ' write buffer
                        ' number of characters to write
                        ' number of characters written
                        ' reserved
                        WriteConsole(hStdout, strOutput, 1, dwNumberOfCharsWritten, intptr)

                        ' We build up our password string...
                        Dim strConcat As New String(ch, 1)

                        ' by appending each typed in character at the end of strBuildup.
                        strBuildup += strConcat

                        If ++iCounter < iMaxNumberOfCharacters Then
                            ' Adding 1 to iCounter still makes iCounter less than MaxNumberOfCharacters.
                            ' This means that the total number of characters collected so far (this is
                            ' equal to iCounter, by the way) is less than MaxNumberOfCharacters.
                            ' We can carry on.
                            Return True
                        Else
                            ' If, by adding 1 to iCounter makes iCounter greater than MaxNumberOfCharacters,
                            ' it means that we have already collected MaxNumberOfCharacters number of characters
                            ' inside strBuildup. We must exit now.
                            Return False
                        End If
                    End If
                End If
            End If

            ' The keydown state is false, we allow further characters to be typed in...
            Return True
        End Function

        ' All event handler functions must return a boolean value indicating whether
        ' the password processing function should continue to read in another console
        ' input record (via ReadConsoleInput() API).
        ' Returning a true indicates continue.
        ' Returning a false indicates don't continue.
        Private Function MouseEventProc(input_record As INPUT_RECORD, ByRef strBuildup As String) As Boolean
            ' Since our Mouse Event Handler does not intend to do anything,
            ' we simply return a true to indicate to the password processing
            ' function to readin another console input record.
            Return True
        End Function

        ' All event handler functions must return a boolean value indicating whether
        ' the password processing function should continue to read in another console
        ' input record (via ReadConsoleInput() API).
        ' Returning a true indicates continue.
        ' Returning a false indicates don't continue.
        Private Function WindowBufferSizeEventProc(input_record As INPUT_RECORD, ByRef strBuildup As String) As Boolean
            ' Since our Window Buffer Size Event Handler does not intend to do anything,
            ' we simply return a true to indicate to the password processing
            ' function to readin another console input record.
            Return True
        End Function

        ' All event handler functions must return a boolean value indicating whether
        ' the password processing function should continue to read in another console
        ' input record (via ReadConsoleInput() API).
        ' Returning a true indicates continue.
        ' Returning a false indicates don't continue.
        Private Function MenuEventProc(input_record As INPUT_RECORD, ByRef strBuildup As String) As Boolean
            ' Since our Menu Event Handler does not intend to do anything,
            ' we simply return a true to indicate to the password processing
            ' function to readin another console input record.
            Return True
        End Function

        ' All event handler functions must return a boolean value indicating whether
        ' the password processing function should continue to read in another console
        ' input record (via ReadConsoleInput() API).
        ' Returning a true indicates continue.
        ' Returning a false indicates don't continue.
        Private Function FocusEventProc(input_record As INPUT_RECORD, ByRef strBuildup As String) As Boolean
            ' Since our Focus Event Handler does not intend to do anything,
            ' we simply return a true to indicate to the password processing
            ' function to readin another console input record.
            Return True
        End Function

        ' Public constructor.
        ' Here, we prepare our hashtable of console input event handler functions.
        Public Sub New()
            htCodeLookup = New Hashtable()
            ' Note well that we must cast Constant.* event numbers to ushort's.
            ' This is because Constants.*_EVENT have been declared as of type int.
            ' We could have, of course, declare Constants.*_EVENT to be of type ushort
            ' but I deliberately declared them as ints to show the importance of
            ' types in C#.
            Call htCodeLookup.Add(DirectCast(CUShort(Constants.KEY_EVENT), Object), New ConsoleInputEvent(AddressOf KeyEventProc))
            Call htCodeLookup.Add(DirectCast(CUShort(Constants.MOUSE_EVENT), Object), New ConsoleInputEvent(AddressOf MouseEventProc))
            Call htCodeLookup.Add(DirectCast(CUShort(Constants.WINDOW_BUFFER_SIZE_EVENT), Object), New ConsoleInputEvent(AddressOf WindowBufferSizeEventProc))
            Call htCodeLookup.Add(DirectCast(CUShort(Constants.MENU_EVENT), Object), New ConsoleInputEvent(AddressOf MenuEventProc))
            Call htCodeLookup.Add(DirectCast(CUShort(Constants.FOCUS_EVENT), Object), New ConsoleInputEvent(AddressOf FocusEventProc))
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="refPasswordToBuild"></param>
        ''' <param name="iMaxNumberOfCharactersSet">The password max length limits.</param>
        ''' <remarks></remarks>
        Public Sub PasswordInput(ByRef refPasswordToBuild As String, iMaxNumberOfCharactersSet As Integer)
            Dim irInBuf As INPUT_RECORD() = New INPUT_RECORD(127) {}
            ' Define an array of 128 INPUT_RECORD structs.
            Dim cNumRead As UInteger = 0
            Dim bContinueLoop As Boolean = True
            ' Used to indicate whether to continue our ReadConsoleInput() loop.
            ' Reset character counter.
            iCounter = 0

            ' Initialize hStdin.
            If hStdin = CType(0, IntPtr) Then
                hStdin = GetStdHandle(Constants.STD_INPUT_HANDLE)
                If hStdin = Constants.INVALID_HANDLE_VALUE Then
                    Return
                End If
            End If

            ' Initialize hStdout.
            If hStdout = CType(0, IntPtr) Then
                hStdout = GetStdHandle(Constants.STD_OUTPUT_HANDLE)
                If hStdout = Constants.INVALID_HANDLE_VALUE Then
                    Return
                End If
            End If

            ' Retrieve the current console mode.
            If GetConsoleMode(hStdin, dwSaveOldMode) = False Then
                Return
            End If

            ' Set the current console mode to enable window input and mouse input.
            ' This is not necessary for our password processing application.
            ' This is set only for demonstration purposes.
            '
            ' By setting ENABLE_WINDOW_INPUT into the console mode, user interactions
            ' that change the size of the console screen buffer are reported in the
            ' console's input buffer. Information about this event can be read from
            ' the input buffer by our application using the ReadConsoleInput function.
            '
            ' By setting ENABLE_MOUSE_INPUT into the console mode, if the mouse pointer
            ' is within the borders of the console window and the window has the
            ' keyboard focus, mouse events generated by mouse movement and button presses
            ' are placed in the input buffer. Information about this event can be read from
            ' the input buffer by our application using the ReadConsoleInput function.
            dwMode = Constants.ENABLE_WINDOW_INPUT Or Constants.ENABLE_MOUSE_INPUT
            If SetConsoleMode(hStdin, dwMode) = False Then
                Return
            End If

            ' To safeguard against invalid values, we stipulate that only if iMaxNumberOfCharactersSet
            ' is greater than zero do we set MaxNumberOfCharacters equal to it.
            ' Otherwise, MaxNumberOfCharacters is set to 20 by default.
            ' An alternative to setting MaxNumberOfCharacters to a default value is to throw an exception.
            If iMaxNumberOfCharactersSet > 0 Then
                iMaxNumberOfCharacters = iMaxNumberOfCharactersSet
            Else
                ' We could throw an exception here if we want to.
                iMaxNumberOfCharacters = 20
            End If

            ' Main loop to collect characters typed into the console.
            While bContinueLoop = True
                ' input buffer handle
                ' buffer to read into
                ' size of read buffer
                ' number of records read
                If ReadConsoleInput(hStdin, irInBuf, 128, cNumRead) = True Then
                    ' Dispatch the events to the appropriate handler.
                    For i As UInteger = 0 To CType(cNumRead - 1, UInteger)
                        ' Lookup the hashtable for the appropriate handler function... courtesy of Derek Kiong !
                        Dim cie_handler As ConsoleInputEvent = DirectCast(htCodeLookup(DirectCast(irInBuf(CInt(i)).EventType, Object)), ConsoleInputEvent)

                        ' Note well that htCodeLookup may not have the handler for the current event,
                        ' so check first for a null value in cie_handler.
                        If cie_handler IsNot Nothing Then
                            ' Invoke the handler.
                            bContinueLoop = cie_handler(irInBuf(CInt(i)), refPasswordToBuild)
                        End If
                    Next
                End If
            End While

            ' Restore the previous mode before we exit.
            Call SetConsoleMode(hStdin, dwSaveOldMode)
            Call Console.WriteLine()
        End Sub
    End Class
End Namespace
