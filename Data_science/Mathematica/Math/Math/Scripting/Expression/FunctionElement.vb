#Region "Microsoft.VisualBasic::590161e2ef3b7a402a315c2257f5ef6d, Data_science\Mathematica\Math\Math\Scripting\Expression\FunctionElement.vb"

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

    '   Total Lines: 24
    '    Code Lines: 16 (66.67%)
    ' Comment Lines: 3 (12.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 635 B


    '     Class FunctionElement
    ' 
    '         Properties: lambda, name, parameters
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace Scripting.MathExpression

    ''' <summary>
    ''' 将用户定义的函数持久化的保存在XML文件之中所使用到的格式
    ''' </summary>
    Public Class FunctionElement

        <XmlAttribute>
        Public Property name As String
        <XmlAttribute>
        Public Property parameters As String()
        <XmlText>
        Public Property lambda As String

        Public Overrides Function ToString() As String
            Return $"function({parameters.JoinBy(", ")}) {{
    return {lambda};
}}"
        End Function

    End Class
End Namespace
