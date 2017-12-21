#Region "Microsoft.VisualBasic::5dc34b90e4cc94e531907979f16b71ba, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Interpreters\Abstract.vb"

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

Namespace CommandLine

    ''' <summary>
    ''' 假若所传入的命令行的name是文件路径，解释器就会执行这个函数指针
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Delegate Function __ExecuteFile(path As String, args As CommandLine) As Integer
    ''' <summary>
    ''' 假若所传入的命令行是空的，就会执行这个函数指针
    ''' </summary>
    ''' <returns></returns>
    Public Delegate Function __ExecuteEmptyCLI() As Integer

    ''' <summary>
    ''' 假若查找不到命令的话，执行这个函数
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Delegate Function __ExecuteNotFound(args As CommandLine) As Integer

End Namespace
