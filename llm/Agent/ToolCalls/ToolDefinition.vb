#Region "Microsoft.VisualBasic::17b9ec4fca6f8a8fc4bce54115b379b7, llm\Agent\ToolCalls\ToolDefinition.vb"

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

    '   Total Lines: 22
    '    Code Lines: 11 (50.00%)
    ' Comment Lines: 7 (31.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (18.18%)
    '     File Size: 1.03 KB


    '     Class ToolDefinition
    ' 
    '         Properties: Description, Handler, Name, Schema
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Agent.ToolCalls

    ''' <summary>一个已注册的工具：名称 + 说明 + 参数 schema + 真实实现。</summary>
    Public Class ToolDefinition

        ''' <summary>Name the model uses to call the tool.</summary>
        Public Property Name As String
        ''' <summary>Description of the tool shown to the model.</summary>
        Public Property Description As String
        ''' <summary>JSON schema describing the arguments accepted by the tool.</summary>
        Public Property Schema As JsonSchema
        ''' <summary>真实实现：接收解析后的参数表，返回结果字符串。</summary>
        Public Property Handler As Func(Of Dictionary(Of String, String), String)

        ''' <summary>Returns a short description of the tool.</summary>
        ''' <returns>A text of the form <c>name - description</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"{Name} - {Description}"
        End Function

    End Class
End Namespace
