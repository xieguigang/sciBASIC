#Region "Microsoft.VisualBasic::95d9fddb60c4f587436b4d047c7e8975, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Serialization\Abstract.vb"

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

Namespace Serialization

    ''' <summary>
    ''' 数据类型转换方法的句柄对象
    ''' </summary>
    ''' <param name="data">源之中的数据，由于源是一个TEXT格式的数据文件，故而这里的数据类型为字符串，通过本句柄对象可以将字符串数据映射为其他的复杂数据类型</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Delegate Function IStringParser(data As String) As Object
    ''' <summary>
    ''' 将目标对象序列化为文本字符串的字符串构造方法
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Delegate Function IStringBuilder(data As Object) As String
End Namespace
