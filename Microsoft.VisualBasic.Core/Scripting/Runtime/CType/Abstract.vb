#Region "Microsoft.VisualBasic::64314a2cb94b30754f0a699b7ba63153, ..\sciBASIC#\Microsoft.VisualBasic.Core\Scripting\Runtime\CType\Abstract.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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
        ''' <param name="cell$"></param>
        ''' <returns></returns>
        Function TryParse(cell$) As Object
    End Interface
End Namespace
