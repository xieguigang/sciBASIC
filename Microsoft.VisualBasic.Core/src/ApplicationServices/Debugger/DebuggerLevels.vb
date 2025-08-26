#Region "Microsoft.VisualBasic::70fd68fd2eafbadf80da2922e0ec0b7d, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\DebuggerLevels.vb"

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

'   Total Lines: 28
'    Code Lines: 9 (32.14%)
' Comment Lines: 18 (64.29%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 1 (3.57%)
'     File Size: 837 B


'     Enum DebuggerLevels
' 
'         [Error], [On], All, Off, Warning
' 
'  
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' 默认的参数值是<see cref="DebuggerLevels.On"/>
    ''' </summary>
    Public Enum DebuggerLevels As Integer
        ''' <summary>
        ''' 是否输出调试信息有程序代码来控制，这个是默认的参数
        ''' </summary>
        [On] = -1
        ''' <summary>
        ''' 不会输出任务调试信息
        ''' </summary>
        Off = Integer.MaxValue
        ''' <summary>
        ''' 强制覆盖掉<see cref="[On]"/>的设置，输出所有类型的信息
        ''' </summary>
        All = 0
        Info
        Debug
        ''' <summary>
        ''' 只会输出警告或者错误类型的信息
        ''' </summary>
        Warning
        ''' <summary>
        ''' 只会输出错误类型的信息
        ''' </summary>
        [Error]
    End Enum
End Namespace
