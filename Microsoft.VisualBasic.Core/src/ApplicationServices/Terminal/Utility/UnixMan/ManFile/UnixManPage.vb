#Region "Microsoft.VisualBasic::0adad75b7efa70e95bbb85d67786cb8c, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\UnixMan\UnixManPage.vb"

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

'   Total Lines: 163
'    Code Lines: 98 (60.12%)
' Comment Lines: 46 (28.22%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 19 (11.66%)
'     File Size: 6.08 KB


'     Class Index
' 
'         Properties: [date], category, index, keyword, title
' 
'         Function: ToString
' 
'     Class UnixManPage
' 
'         Properties: AUTHOR, BUGS, COPYRIGHT, DESCRIPTION, DETAILS
'                     EXAMPLES, EXIT_STATUS, FILES, HISTORY, index
'                     LICENSE, NAME, OPTIONS, PROLOG, SEE_ALSO
'                     SYNOPSIS, VALUE, WARRANTY
' 
'         Function: (+2 Overloads) ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ApplicationServices.Terminal.Utility

    ''' <summary>
    ''' man手册页（manual pages，“手册”），是类UNIX系统最重要的手册工具。多数类UNIX都预装了它，这也包括Arch。使用man手册页的命令是：man。
    ''' </summary>
    Public Class UnixManPage

        Public Property index As ManIndex

        Public Property PROLOG As String

        ''' <summary>
        ''' 手册叙述对象名称，及简要描述。
        ''' </summary>
        ''' <returns></returns>
        Public Property NAME As String
        ''' <summary>
        ''' 命令参数格式，或者函数调用格式等。
        ''' </summary>
        ''' <returns></returns>
        Public Property SYNOPSIS As String
        ''' <summary>
        ''' 对叙述对象更加详细的描述。
        ''' </summary>
        ''' <returns></returns>
        Public Property DESCRIPTION As String
        Public Property VALUE As String
        Public Property DETAILS As String
        ''' <summary>
        ''' 由浅入深的使用示例。
        ''' </summary>
        ''' <returns></returns>
        Public Property EXAMPLES As String
        ''' <summary>
        ''' 命令行或者函数调用参数的意义。
        ''' </summary>
        ''' <returns></returns>
        Public Property OPTIONS As NamedValue(Of String)()
        ''' <summary>
        ''' 不同返回（退出）代码的含义。
        ''' </summary>
        ''' <returns></returns>
        Public Property EXIT_STATUS As String
        ''' <summary>
        ''' 与叙述对象相关的文件。
        ''' </summary>
        ''' <returns></returns>
        Public Property FILES As String
        ''' <summary>
        ''' 已知的bug。
        ''' </summary>
        ''' <returns></returns>
        Public Property BUGS As String
        ''' <summary>
        ''' 相关内容列表。
        ''' </summary>
        ''' <returns></returns>
        Public Property SEE_ALSO As String
        Public Property AUTHOR As String
        Public Property HISTORY As String
        Public Property COPYRIGHT As String
        Public Property LICENSE As String
        Public Property WARRANTY As String

        ''' <summary>
        ''' 生成man page的文本内容
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return ToString(Me, Nothing)
        End Function

        Public Shared Function Parse(text As String) As UnixManPage
            Return ManParser.ParseText(text)
        End Function

        Public Overloads Shared Function ToString(man As UnixManPage, Optional comments$ = ManWriter.default_comment_text) As String
            Return ManWriter.ToString(man, comments)
        End Function
    End Class
End Namespace
