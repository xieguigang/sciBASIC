#Region "Microsoft.VisualBasic::0227155966e2957e1f85d9a8b216bb68, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\CLITypes.vb"

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

Namespace CommandLine.Reflection

    ''' <summary>
    ''' The data type enumeration of the target optional parameter switch.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum CLITypes
        ''' <summary>
        ''' String.(对于指定为字符串类型的参数，在调用的时候回自动调用<see cref="Extensions.CLIToken"/>函数)
        ''' </summary>
        ''' <remarks></remarks>
        [String]
        ''' <summary>
        ''' Int
        ''' </summary>
        ''' <remarks></remarks>
        [Integer]
        ''' <summary>
        ''' Real
        ''' </summary>
        ''' <remarks></remarks>
        [Double]
        ''' <summary>
        ''' This is a flag value, if this flag presents in the CLI, then this named Boolean value is TRUE, otherwise is FALSE.
        ''' </summary>
        [Boolean]
        ''' <summary>
        ''' File path, is equals most string.(对于指定为路径类型的参数值，在生成命令行的时候会自动调用<see cref="CLIPath"/>函数)
        ''' </summary>
        File
    End Enum
End Namespace
