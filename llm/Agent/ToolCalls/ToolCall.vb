#Region "Microsoft.VisualBasic::6dbc4fc9179d550c66f6dfa361e4e7e5, llm\Agent\ToolCalls\ToolCall.vb"

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

    '   Total Lines: 27
    '    Code Lines: 12 (44.44%)
    ' Comment Lines: 7 (25.93%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (29.63%)
    '     File Size: 1.10 KB


    '     Class ToolCall
    ' 
    '         Properties: Arguments, ArgumentsJson, Index, Name
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Agent.ToolCalls

    ''' <summary>一次工具调用的解析结果。</summary>
    Public Class ToolCall

        ''' <summary>被调用的工具名。</summary>
        Public Property Name As String

        ''' <summary>参数的原始 JSON 文本（可能带代码围栏之前的内容）。</summary>
        Public Property ArgumentsJson As String

        ''' <summary>解析后的参数键值对（值一律按字符串保存，由工具自己解释类型）。</summary>
        Public Property Arguments As New Dictionary(Of String, String)

        ''' <summary>第几个调用（0 基）。</summary>
        Public Property Index As Integer

        ''' <summary>Returns the call rendered in the textual protocol form.</summary>
        ''' <returns>A text of the form <c>name(arg=value, ...)</c>.</returns>
        Public Overrides Function ToString() As String
            Dim args = String.Join(", ", Arguments.Select(Function(kv) kv.Key & "=" & kv.Value))

            Return $"{Name}({args})"
        End Function

    End Class
End Namespace
