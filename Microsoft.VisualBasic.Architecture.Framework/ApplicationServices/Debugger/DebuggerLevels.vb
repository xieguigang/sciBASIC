#Region "Microsoft.VisualBasic::6cb5e1b5aa3e0f39bd35d2acb3ebc42a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Debugger\DebuggerLevels.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' 默认的参数值是<see cref="DebuggerLevels.On"/>
    ''' </summary>
    Public Enum DebuggerLevels
        ''' <summary>
        ''' 是否输出调试信息有程序代码来控制，这个是默认的参数
        ''' </summary>
        [On]
        ''' <summary>
        ''' 不会输出任务调试信息
        ''' </summary>
        Off
        ''' <summary>
        ''' 强制覆盖掉<see cref="[On]"/>的设置，输出所有类型的信息
        ''' </summary>
        All
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
