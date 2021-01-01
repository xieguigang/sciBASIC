#Region "Microsoft.VisualBasic::9e217abf5ba3f2929fef576bfb42c6e8, Microsoft.VisualBasic.Core\Data\Abstract.vb"

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
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataSourceModel

    Public Enum PropertyAccess As Byte
        NotSure = 0
        Readable = 2
        Writeable = 4
        ReadWrite = Readable And Writeable
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

#If netcore5 = 1 Then

    ''' <summary>
    ''' 用于与.NET Framework之中的ScriptIgnore属性标记兼容的一个对象
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class ScriptIgnoreAttribute : Inherits Attribute
    End Class
#End If
End Namespace
