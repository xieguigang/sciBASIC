#Region "Microsoft.VisualBasic::04e4724dc9ff7f3dbecf3f750acd4817, Microsoft.VisualBasic.Core\src\Data\Abstract.vb"

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

    '   Total Lines: 48
    '    Code Lines: 15 (31.25%)
    ' Comment Lines: 28 (58.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (10.42%)
    '     File Size: 1.62 KB


    '     Enum PropertyAccess
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class DataIgnoredAttribute
    ' 
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Class ScriptIgnoreAttribute
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' the data access
    ''' </summary>
    Public Enum PropertyAccess As Byte
        ''' <summary>
        ''' no data access
        ''' </summary>
        NotSure = 0
        ''' <summary>
        ''' data can be read from the clr object(get/readonly property)
        ''' </summary>
        Readable = 2
        ''' <summary>
        ''' data can be write to the clr object(set/writeonly property)
        ''' </summary>
        Writeable = 4
        ''' <summary>
        ''' data can be read and write to the clr object(get/set)
        ''' </summary>
        ReadWrite = Readable + Writeable
    End Enum

    ''' <summary>
    ''' 在数据框数据映射操作之中是否忽略掉这个属性或者方法？
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class DataIgnoredAttribute : Inherits Attribute
    End Class

    ''' <summary>
    ''' Class field reader
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="Tout"></typeparam>
    ''' <param name="[in]"></param>
    ''' <returns></returns>
    Public Delegate Function Projector(Of T, Tout)([in] As T) As Tout

    ''' <summary>
    ''' 用于与.NET Framework之中的ScriptIgnore属性标记兼容的一个对象
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class ScriptIgnoreAttribute : Inherits Attribute
    End Class

End Namespace
