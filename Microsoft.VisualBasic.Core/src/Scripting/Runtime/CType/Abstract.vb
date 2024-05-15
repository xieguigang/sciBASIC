#Region "Microsoft.VisualBasic::1771c61acba8743dfc6cd73f25951ffe, Microsoft.VisualBasic.Core\src\Scripting\Runtime\CType\Abstract.vb"

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

    '   Total Lines: 23
    '    Code Lines: 6
    ' Comment Lines: 13
    '   Blank Lines: 4
    '     File Size: 667 B


    '     Interface IParser
    ' 
    '         Function: ToString, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.Runtime

    ''' <summary>
    ''' Custom user object parser
    ''' </summary>
    Public Interface IParser

        ''' <summary>
        ''' 将目标对象序列化为文本字符串
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Function ToString(obj As Object) As String

        ''' <summary>
        ''' 从Csv文件之中所读取出来的字符串之中解析出目标对象
        ''' </summary>
        ''' <param name="content"></param>
        ''' <returns></returns>
        Function TryParse(content As String) As Object

    End Interface
End Namespace
