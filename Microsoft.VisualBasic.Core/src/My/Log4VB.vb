#Region "Microsoft.VisualBasic::08b579f7d7b37d1b1530b714278fd0fe, Microsoft.VisualBasic.Core\src\My\Log4VB.vb"

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

    '   Total Lines: 186
    '    Code Lines: 111 (59.68%)
    ' Comment Lines: 51 (27.42%)
    '    - Xml Docs: 92.16%
    ' 
    '   Blank Lines: 24 (12.90%)
    '     File Size: 7.70 KB


    '     Module Log4VB
    ' 
    '         Function: getColor, getLogger, Print
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

        Public Function getLogger(level As MSG_TYPES) As LoggingDriver
            Select Case level
                Case MSG_TYPES.DEBUG : Return redirectDebug
                Case MSG_TYPES.ERR : Return redirectError
                Case MSG_TYPES.INF : Return redirectInfo
                Case MSG_TYPES.WRN : Return redirectWarning
                Case Else
                    Return Nothing
            End Select
        End Function

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
        ''' 头部和消息字符串都是放在一个task之中进行输出的
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
        ''' 输出的终端消息带有指定的终端颜色色彩
        ''' </summary>
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
            Call System.Diagnostics.Debug.Write(msg)
#End If
        End Sub

        ''' <summary>
        ''' 输出的终端消息带有指定的终端颜色色彩
        ''' </summary>
        <Extension>
        Public Sub Println(msg$, Optional color As ConsoleColor = ConsoleColor.White, Optional background As ConsoleColor = -1)
            If Mute Then
                Return
            End If

            If ForceSTDError Then
                Console.Error.WriteLine(msg)
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

#If DEBUG Then
            Call System.Diagnostics.Debug.WriteLine(msg)
#End If
        End Sub
    End Module
End Namespace
