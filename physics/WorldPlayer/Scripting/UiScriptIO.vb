#Region "Microsoft.VisualBasic::86b7c3c9c5af7c98e0ad367a4fbf5389, physics\WorldPlayer\Scripting\UiScriptIO.vb"

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
    '    Code Lines: 15 (53.57%)
    ' Comment Lines: 7 (25.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 887 B


    ' Class UiScriptIO
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: WriteError, WriteLine
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

''' <summary>
''' 把 JS 的 <c>console.log</c> 输出转发到宿主控制台面板，
''' 并做行数上限保护（超过 <see cref="MaxLines"/> 后从头滚动截断），
''' 避免高频打印把 UI 卡死。
''' </summary>
Public Class UiScriptIO
    Inherits ScriptIO

    ''' <summary>最多保留的行数。</summary>
    Public Const MaxLines As Integer = 500

    Private ReadOnly sink As Action(Of String, Boolean)

    Public Sub New(sink As Action(Of String, Boolean))
        Me.sink = sink
    End Sub

    Public Overrides Sub WriteLine(text As String)
        sink(text, False)
    End Sub

    ''' <summary>写一条错误行（宿主侧使用，红色高亮）。</summary>
    Public Sub WriteError(text As String)
        sink(text, True)
    End Sub
End Class

