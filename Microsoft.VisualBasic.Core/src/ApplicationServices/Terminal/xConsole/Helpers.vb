#Region "Microsoft.VisualBasic::e7239aabbc1881247e1ab3069dbca90e, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\xConsole\Helpers.vb"

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

    '   Total Lines: 78
    '    Code Lines: 56 (71.79%)
    ' Comment Lines: 6 (7.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (20.51%)
    '     File Size: 2.67 KB


    '     Module Helpers
    ' 
    '         Function: SetConsoleFont, (+2 Overloads) SetConsoleIcon
    '         Enum StdHandle
    ' 
    ' 
    ' 
    ' 
    '         Structure ConsoleFont
    ' 
    '             Properties: ConsoleFonts, ConsoleFontsCount
    ' 
    '             Function: GetConsoleFontInfo, GetNumberOfConsoleFonts, GetStdHandle, SetConsoleFont
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.InteropServices

Namespace ApplicationServices.Terminal.xConsole

    ''' <summary>
    ''' Console helper. http://blogs.microsoft.co.il/blogs/pavely/archive/2009/07/23/changing-console-fonts.aspx
    ''' </summary>
    Public Module Helpers

        <DllImport("kernel32")>
        Public Function SetConsoleIcon(hIcon As IntPtr) As Boolean
        End Function

#If NET48 Then
        Public Function SetConsoleIcon(icon As Icon) As Boolean
            Return SetConsoleIcon(icon.Handle)
        End Function
#End If

        <DllImport("kernel32")>
        Public Function SetConsoleFont(hOutput As IntPtr, index As UInteger) As Boolean
        End Function

        Private Enum StdHandle
            OutputHandle = -11
        End Enum

        <DllImport("kernel32")>
        Private Function GetStdHandle(index As StdHandle) As IntPtr
        End Function

        Public Function SetConsoleFont(index As UInteger) As Boolean
            Return SetConsoleFont(GetStdHandle(StdHandle.OutputHandle), index)
        End Function

        <DllImport("kernel32")>
        Private Function GetConsoleFontInfo(hOutput As IntPtr,
                                            <MarshalAs(UnmanagedType.Bool)> bMaximize As Boolean,
                                            count As UInteger,
                                            <MarshalAs(UnmanagedType.LPArray), Out> fonts As ConsoleFont()) As Boolean
        End Function

        <DllImport("kernel32")>
        Private Function GetNumberOfConsoleFonts() As UInteger
        End Function

        Public ReadOnly Property ConsoleFontsCount() As UInteger
            Get
                Return GetNumberOfConsoleFonts()
            End Get
        End Property

        Public ReadOnly Property ConsoleFonts() As ConsoleFont()
            Get
                Dim fonts As ConsoleFont() = New ConsoleFont(GetNumberOfConsoleFonts() - 1) {}
                If fonts.Length > 0 Then
                    GetConsoleFontInfo(GetStdHandle(StdHandle.OutputHandle), False, CUInt(fonts.Length), fonts)
                End If
                Return fonts
            End Get
        End Property

#Region "HELPER"

        ''' <summary>
        ''' http://blogs.microsoft.co.il/blogs/pavely/archive/2009/07/23/changing-console-fonts.aspx
        ''' </summary>
        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure ConsoleFont
            Public Index As UInteger
            Public SizeX As Short, SizeY As Short
        End Structure

#End Region

    End Module
End Namespace
