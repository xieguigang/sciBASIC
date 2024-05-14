#Region "Microsoft.VisualBasic::8451dda2f14b33c39f39f5b1dc2195ea, Microsoft.VisualBasic.Core\src\Scripting\TokenIcer\CodeSpan.vb"

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

    '   Total Lines: 31
    '    Code Lines: 11
    ' Comment Lines: 15
    '   Blank Lines: 5
    '     File Size: 906 B


    '     Class CodeSpan
    ' 
    '         Properties: line, start, stops
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace Scripting.TokenIcer

    ''' <summary>
    ''' 目标Token对象在原始代码文本之中的定位位置
    ''' </summary>
    Public Class CodeSpan

        ''' <summary>
        ''' 源代码中的起始位置 
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property start As Integer
        ''' <summary>
        ''' 源代码中的结束位置
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property stops As Integer
        ''' <summary>
        ''' 在代码文本的行号
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property line As Integer

        Public Overrides Function ToString() As String
            Return $"[{start}, {[stops]}] at line {line}"
        End Function

    End Class
End Namespace
