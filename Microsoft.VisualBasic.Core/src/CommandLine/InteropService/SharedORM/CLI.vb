#Region "Microsoft.VisualBasic::fbedf0252652f4d03e94a8db319af729, Microsoft.VisualBasic.Core\src\CommandLine\InteropService\SharedORM\CLI.vb"

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

    '   Total Lines: 24
    '    Code Lines: 17 (70.83%)
    ' Comment Lines: 3 (12.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (16.67%)
    '     File Size: 835 B


    '     Class CLIAttribute
    ' 
    '         Function: ParseAssembly
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' 创建一个自定义标记来表示这个模块是一个CLI命令接口模块
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class)>
    Public Class CLIAttribute : Inherits Attribute

        Public Shared Function ParseAssembly(dll$) As Type
            Dim assembly As Assembly = Assembly.LoadFile(dll)
            Dim type As Type = LinqAPI.DefaultFirst(Of Type) _
 _
                () <= From t As Type
                      In EmitReflection.GetTypesHelper(assembly)
                      Where Not t.GetCustomAttribute(Of CLIAttribute) Is Nothing
                      Select t ' 

            Return type
        End Function
    End Class
End Namespace
