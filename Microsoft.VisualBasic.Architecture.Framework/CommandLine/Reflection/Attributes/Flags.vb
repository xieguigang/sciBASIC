#Region "Microsoft.VisualBasic::4565cc5984461eb3f502b68af5485968, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\Attributes\Flags.vb"

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

Imports System.ComponentModel

Namespace CommandLine.Reflection

    Public Enum PipelineTypes

        undefined
        ''' <summary>
        ''' This argument can accept the std_out from upstream app as input
        ''' </summary>
        <Description("(This argument can accept the std_out from upstream app as input)")>
        std_in
        ''' <summary>
        ''' This argument can output data to std_out
        ''' </summary>
        <Description("(This argument can output data to std_out)")>
        std_out
    End Enum
End Namespace
