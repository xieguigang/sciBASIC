#Region "Microsoft.VisualBasic::ad72cf3e497acf943146a1af33e522c6, Microsoft.VisualBasic.Core\src\Serialization\Abstract.vb"

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

    '   Total Lines: 29
    '    Code Lines: 7 (24.14%)
    ' Comment Lines: 17 (58.62%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (17.24%)
    '     File Size: 1.17 KB


    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization

    ''' <summary>
    ''' 数据类型转换方法的句柄对象
    ''' </summary>
    ''' <param name="data">源之中的数据，由于源是一个TEXT格式的数据文件，故而这里的数据类型为字符串，通过本句柄对象可以将字符串数据映射为其他的复杂数据类型</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Delegate Function IStringParser(data As String) As Object
    Public Delegate Function IStringParser(Of T)(data As String) As T
    Public Delegate Function IObjectBuilder(data As String, schema As Type) As Object

    ''' <summary>
    ''' 将目标对象序列化为文本字符串的字符串构造方法
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Delegate Function IStringBuilder(data As Object) As String

    ''' <summary>
    ''' delegate for generic cast object to string
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>

    Public Delegate Function IToString(Of T)(obj As T) As String

End Namespace
