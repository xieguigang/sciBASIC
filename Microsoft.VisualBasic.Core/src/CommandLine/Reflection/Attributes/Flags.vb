#Region "Microsoft.VisualBasic::e6ec7bef4c88e217265d0a358da1fd20, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Attributes\Flags.vb"

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

    '   Total Lines: 19
    '    Code Lines: 10 (52.63%)
    ' Comment Lines: 6 (31.58%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (15.79%)
    '     File Size: 583 B


    '     Enum PipelineTypes
    ' 
    '         std_in, std_out, undefined
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace CommandLine.Reflection

    Public Enum PipelineTypes As Byte

        undefined
        ''' <summary>
        ''' This argument can accept the std_out from upstream app as input
        ''' </summary>
        <Description("(This argument can accept the ``std_out`` from upstream app as input)")>
        std_in
        ''' <summary>
        ''' This argument can output data to std_out
        ''' </summary>
        <Description("(This argument can output data to ``std_out``)")>
        std_out
    End Enum
End Namespace
