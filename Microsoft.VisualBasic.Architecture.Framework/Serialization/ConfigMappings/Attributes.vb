#Region "Microsoft.VisualBasic::67ee4739e6cacf49df11c85c7663c75a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Serialization\ConfigMappings\Attributes.vb"

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

Imports System.Reflection

Namespace Serialization

    ''' <summary>
    ''' 这个属性或者方法不会被用于映射
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Method, AllowMultiple:=False, Inherited:=False)>
    Public Class MappingsIgnored : Inherits Attribute
    End Class

    ''' <summary>
    ''' 不会使用系统自带的映射方法进行映射
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class UseCustomMapping : Inherits Attribute
    End Class

    Public Structure NodeMapping

        ''' <summary>
        ''' 映射的文本文件源
        ''' </summary>
        ''' <remarks></remarks>
        Dim Source As PropertyInfo
        ''' <summary>
        ''' 映射操作的目标数据模型
        ''' </summary>
        ''' <remarks></remarks>
        Dim Mapping As PropertyInfo
        ''' <summary>
        ''' 从源映射到数据模型的类型转换
        ''' </summary>
        ''' <remarks></remarks>
        Dim SourceToMappingCasting As __StringTypeCaster
        ''' <summary>
        ''' 从数据模型映射到源的类型转换
        ''' </summary>
        ''' <remarks></remarks>
        Dim MappingToSourceCasting As StringTypeCastHandler

        Public Overrides Function ToString() As String
            Return Source.Name & "   --->  " & Mapping.PropertyType.FullName
        End Function
    End Structure
End Namespace
