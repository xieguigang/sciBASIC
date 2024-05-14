#Region "Microsoft.VisualBasic::9a96eebe53ccc7e496293a9b3e915a52, Microsoft.VisualBasic.Core\src\My\Log4VB.vb"

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

    '   Total Lines: 175
    '    Code Lines: 101
    ' Comment Lines: 51
    '   Blank Lines: 23
    '     File Size: 7.21 KB


    '     Module Log4VB
    ' 
    '         Function: getColor, Print
    ' 
    '         Sub: Print, Println
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal

Namespace My

    ''' <summary>
    ''' VB.NET <see cref="Console"/> log framework.
    ''' </summary>
    Public Module Log4VB

        ''' <summary>
        ''' Additional user log file drivers
        ''' </summary>
        Friend ReadOnly logs As New List(Of LoggingDriver)

        ''' <summary>
        ''' ``<see cref="MSG_TYPES"/> -> <see cref="ConsoleColor"/>``
        ''' </summary>
        ReadOnly DebuggerTagColors As New Dictionary(Of Integer, ConsoleColor) From {
            {MSG_TYPES.DEBUG, ConsoleColor.DarkGreen},
            {MSG_TYPES.ERR, ConsoleColor.Red},
            {MSG_TYPES.INF, ConsoleColor.Blue},
            {MSG_TYPES.WRN, ConsoleColor.Yellow}
        }

        ''' <summary>
        ''' LoggingDriver(header$, message$, level As MSG_TYPES)
        ''' </summary>
        Public redirectWarning As LoggingDriver
        ''' <summary>
        ''' LoggingDriver(header$, message$, level As MSG_TYPES)
        ''' </summary>
        Public redirectError As LoggingDriver
        ''' <summary>
        ''' LoggingDriver(header$, message$, level As MSG_TYPES)
        ''' </summary>
        Public redirectDebug As LoggingDriver
        ''' <summary>
        ''' LoggingDriver(header$, message$, level As MSG_TYPES)
        ''' </summary>
        Public redirectInfo As LoggingDriver

        ''' <summary>
        ''' Translate <see cref="MSG_TYPES"/> to <see cref="ConsoleColor"/>
        ''' </summary>
        ''' <param name="level"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension>
        Private Function getColor(level As Integer) As ConsoleColor
            Return If(DebuggerTagColors.ContainsKey(level), DebuggerTagColors(level), CType(level, ConsoleColor))
        End Function

        ''' <summary>
        ''' 头部和消息字符串都是放在一个task之中进行输出的，<see cref="xConsole"/>的输出也是和内部的debugger输出使用的同一个消息线程
        ''' </summary>
        ''' <param name="header"></param>
        ''' <param name="msg"></param>
        ''' <param name="msgColor"></param>
        ''' <param name="level"><see cref="ConsoleColor"/> or <see cref="MSG_TYPES"/></param>
        Public Function Print(header$, msg$, msgColor As ConsoleColor, level As Integer) As Boolean
            My.InnerQueue.AddToQueue(
                Sub()
                    ' 2018-12-14
                    ' 替换meta chars可能会导致windows下的路径显示出现bug
                    ' msg = msg.ReplaceMetaChars

                    If ForceSTDError Then
                        Call Console.Error.WriteLine($"[{header}]{msg}")
                    Else
                        Dim cl As ConsoleColor = Console.ForegroundColor
                        Dim headColor As ConsoleColor = getColor(level)

                        If msgColor = headColor Then
                            Console.ForegroundColor = headColor
                            Console.WriteLine($"[{header}]{msg}")
                            Console.ForegroundColor = cl
                        Else
                            Call Console.Write("[")
                            Console.ForegroundColor = headColor
                            Call Console.Write(header)
                            Console.ForegroundColor = cl
                            Call Console.Write("]")

                            Call Println(msg, msgColor)
                        End If
                    End If
                End Sub)

            For Each driver As LoggingDriver In logs
                Call driver(header, msg, level)
            Next

            Return False
        End Function

        ''' <summary>
        ''' 输出的终端消息带有指定的终端颜色色彩，当<see cref="UsingxConsole"/>为True的时候，
        ''' <paramref name="msg"/>参数之中的文本字符串兼容<see cref="xConsole"/>语法，
        ''' 而<paramref name="color"/>将会被<see cref="xConsole"/>覆盖而不会起作用
        ''' </summary>
        ''' <param name="msg">兼容<see cref="xConsole"/>语法</param>
        ''' <param name="color">当<see cref="UsingxConsole"/>参数为True的时候，这个函数参数将不会起作用</param>
        <Extension>
        Public Sub Print(msg$,
                         Optional color As ConsoleColor = ConsoleColor.White,
                         Optional background As ConsoleColor = -1)

            If Mute Then
                Return
            End If

            If ForceSTDError Then
                Console.Error.Write(msg)
            Else
                Dim cl As ConsoleColor = Console.ForegroundColor
                Dim bg As ConsoleColor = Console.BackgroundColor

                If background >= 0 Then
                    Console.BackgroundColor = background
                End If

                Console.ForegroundColor = color
                Console.Write(msg)
                Console.ForegroundColor = cl
                Console.BackgroundColor = bg
            End If

#If DEBUG Then
            Call Debug.Write(msg)
#End If
        End Sub

        ''' <summary>
        ''' 输出的终端消息带有指定的终端颜色色彩，当<see cref="UsingxConsole"/>为True的时候，
        ''' <paramref name="msg"/>参数之中的文本字符串兼容<see cref="xConsole"/>语法，
        ''' 而<paramref name="color"/>将会被<see cref="xConsole"/>覆盖而不会起作用
        ''' </summary>
        ''' <param name="msg">兼容<see cref="xConsole"/>语法</param>
        ''' <param name="color">当<see cref="UsingxConsole"/>参数为True的时候，这个函数参数将不会起作用</param>
        <Extension>
        Public Sub Println(msg$, Optional color As ConsoleColor = ConsoleColor.White, Optional background As ConsoleColor = -1)
            If Mute Then
                Return
            End If

            If ForceSTDError Then
                Console.Error.WriteLine(msg)
            Else
                If UsingxConsole AndAlso App.IsMicrosoftPlatform Then
                    Call xConsole.CoolWrite(msg)
                Else
                    ' 使用传统的输出输出方法
                    Dim cl As ConsoleColor = Console.ForegroundColor
                    Dim bg As ConsoleColor = Console.BackgroundColor

                    If background >= 0 Then
                        Console.BackgroundColor = background
                    End If

                    Console.ForegroundColor = color
                    Console.WriteLine(msg)
                    Console.ForegroundColor = cl
                    Console.BackgroundColor = bg
                End If
            End If

#If DEBUG Then
            Call Debug.WriteLine(msg)
#End If
        End Sub
    End Module
End Namespace
