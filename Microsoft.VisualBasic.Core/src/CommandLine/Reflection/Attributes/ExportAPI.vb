#Region "Microsoft.VisualBasic::1f254ab55e59d9a14fa0d33a4589045f, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Attributes\ExportAPI.vb"

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

    '   Total Lines: 65
    '    Code Lines: 19 (29.23%)
    ' Comment Lines: 41 (63.08%)
    '    - Xml Docs: 95.12%
    ' 
    '   Blank Lines: 5 (7.69%)
    '     File Size: 2.67 KB


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

Namespace CommandLine.Reflection

    ''' <summary>
    ''' A command object that with a specific name.
    ''' </summary>
    ''' <remarks>(一个具有特定名称命令执行对象)</remarks>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class ExportAPIAttribute : Inherits Attribute
        Implements IExportAPI

        ''' <summary>
        ''' The name of the commandline object.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>(这个命令的名称)</remarks>
        Public ReadOnly Property Name As String Implements IExportAPI.Name
        ''' <summary>
        ''' Something detail of help information.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>(详细的帮助信息)</remarks>
        <Obsolete> Public Property Info As String Implements IExportAPI.Info
        ''' <summary>
        ''' The usage of this command.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>(这个命令的用法，本属性仅仅是一个助记符，当用户没有编写任何的使用方法信息的时候才会使用本属性的值)</remarks>
        <Obsolete> Public Property Usage As String Implements IExportAPI.Usage
        ''' <summary>
        ''' A example that to useing this command.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>(对这个命令的使用示例，本属性仅仅是一个助记符，当用户没有编写任何示例信息的时候才会使用本属性的值，
        ''' 在编写帮助示例的时候，需要编写出包括命令开关名称的完整的例子)</remarks>
        <Obsolete> Public Property Example As String Implements IExportAPI.Example

        ''' <summary>
        ''' You are going to define a available export api 
        ''' for you application to another language or 
        ''' scripting program environment.
        ''' 
        ''' </summary>
        ''' <param name="Name">
        ''' The name of the commandline object or you define 
        ''' the exported API name here.
        ''' </param>
        ''' <remarks>(定义一个命令行程序之中可以使用的命令)</remarks>
        ''' 
        <DebuggerStepThrough>
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
