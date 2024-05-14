#Region "Microsoft.VisualBasic::3dbae4711a9e602a3a9daf2f1771469d, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Logging\LogFile\LogEntry.vb"

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

    '   Total Lines: 44
    '    Code Lines: 28
    ' Comment Lines: 9
    '   Blank Lines: 7
    '     File Size: 1.46 KB


    '     Structure LogEntry
    ' 
    '         Properties: [object], level, message, time
    ' 
    '         Function: FormatMessage, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Debugging.Logging

    ''' <summary>
    ''' 一条记录日志对象
    ''' </summary>
    Public Structure LogEntry

        Public Property message As String
        Public Property [object] As String
        Public Property level As MSG_TYPES
        Public Property time As Date

        ''' <summary>
        ''' 生成日志文档之中的一行记录数据
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim obj As String = [object].TrimNewLine
            Dim str As String

            If message.Contains(vbCr) OrElse message.Contains(vbLf) Then
                ' 多行模式
                str = $"[{time.ToString}][{level.ToString}][{[obj]}]{vbCrLf}{message}"
            Else
                ' 单行模式
                str = $"[{time.ToString}][{level.ToString}][{[obj]}] {message}"
            End If

            Return str
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FormatMessage(header$, message$, level As MSG_TYPES) As String
            Return New LogEntry With {
                .message = message,
                .[object] = header,
                .level = level,
                .time = Now
            }.ToString
        End Function
    End Structure
End Namespace
