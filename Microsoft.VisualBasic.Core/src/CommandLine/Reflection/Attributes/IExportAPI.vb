#Region "Microsoft.VisualBasic::34ca51ba0a998a99b78db02aae4872d6, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Attributes\IExportAPI.vb"

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

    '   Total Lines: 34
    '    Code Lines: 8
    ' Comment Lines: 24
    '   Blank Lines: 2
    '     File Size: 1.37 KB


    '     Interface IExportAPI
    ' 
    '         Properties: Example, Info, Name, Usage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine.Reflection

    Public Interface IExportAPI

        ''' <summary>
        ''' The name of the commandline object.(这个命令的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Name As String
        ''' <summary>
        ''' Something detail of help information.(详细的帮助信息)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Info As String
        ''' <summary>
        ''' The usage of this command.(这个命令的用法，本属性仅仅是一个助记符，当用户没有编写任何的使用方法信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Usage As String
        ''' <summary>
        ''' A example that to useing this command.(对这个命令的使用示例，本属性仅仅是一个助记符，当用户没有编写任何示例信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Example As String
    End Interface
End Namespace
