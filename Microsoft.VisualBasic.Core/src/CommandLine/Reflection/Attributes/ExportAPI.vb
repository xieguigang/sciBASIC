#Region "Microsoft.VisualBasic::a5951f352564f26c5da0f962913f208b, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Attributes\ExportAPI.vb"

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

    '     Class ExportAPIAttribute
    ' 
    '         Properties: Example, Info, Name, Usage
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace CommandLine.Reflection

    ''' <summary>
    ''' A command object that with a specific name.(一个具有特定名称命令执行对象)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class ExportAPIAttribute : Inherits Attribute
        Implements IExportAPI

        ''' <summary>
        ''' The name of the commandline object.(这个命令的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name As String Implements IExportAPI.Name
        ''' <summary>
        ''' Something detail of help information.(详细的帮助信息)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete> Public Property Info As String Implements IExportAPI.Info
        ''' <summary>
        ''' The usage of this command.(这个命令的用法，本属性仅仅是一个助记符，当用户没有编写任何的使用方法信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete> Public Property Usage As String Implements IExportAPI.Usage
        ''' <summary>
        ''' A example that to useing this command.
        ''' (对这个命令的使用示例，本属性仅仅是一个助记符，当用户没有编写任何示例信息的时候才会使用本属性的值，
        ''' 在编写帮助示例的时候，需要编写出包括命令开关名称的完整的例子)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete> Public Property Example As String Implements IExportAPI.Example

        ''' <summary>
        ''' You are going to define a available export api for you application to another language or scripting program environment.
        ''' (定义一个命令行程序之中可以使用的命令)
        ''' </summary>
        ''' <param name="Name">The name of the commandline object or you define the exported API name here.(这个命令的名称)</param>
        ''' <remarks></remarks>
        Sub New(Name As String)
            _Name = Name
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class
End Namespace
