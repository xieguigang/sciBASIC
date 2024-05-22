#Region "Microsoft.VisualBasic::1982150d07994b6fc4934d13ea614a9e, mime\text%html\jQuery\Extensions.vb"

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
    '    Code Lines: 18 (62.07%)
    ' Comment Lines: 7 (24.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (13.79%)
    '     File Size: 1.09 KB


    '     Module Extensions
    ' 
    '         Function: TagName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Html.XmlMeta

Namespace HTML.jQuery

    Public Module Extensions

        ''' <summary>
        ''' 如果定义了<see cref="XmlTypeAttribute"/>，则优先使用这个属性之中的<see cref="XmlTypeAttribute.TypeName"/>
        ''' 作为文档元素标签的名字，没有找到的话，则直接使用``Class Name``来作为文档元素标签的名字
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="element"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TagName(Of T As Node)(element As T) As String
            Dim type As Type = GetType(T)
            Dim xmlType As XmlTypeAttribute = type.GetCustomAttribute(Of XmlTypeAttribute)

            If xmlType Is Nothing Then
                Return type.Name
            Else
                Return xmlType.TypeName
            End If
        End Function
    End Module
End Namespace
