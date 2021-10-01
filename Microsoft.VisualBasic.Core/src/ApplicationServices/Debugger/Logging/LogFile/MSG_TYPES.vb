#Region "Microsoft.VisualBasic::11f876578ff4099060a3b739d3998b3b, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Logging\LogFile\MSG_TYPES.vb"

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

    '     Enum MSG_TYPES
    ' 
    '         FINEST
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace ApplicationServices.Debugging.Logging

    ''' <summary>
    ''' Logging levels, the types enumeration of the log file message.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum MSG_TYPES As Integer

        ''' <summary>
        ''' The normal information message.[WHITE]
        ''' </summary>
        ''' <remarks></remarks>
        <Description("INFOM")> INF = ConsoleColor.White
        ''' <summary>
        ''' The program error information message.[RED]
        ''' </summary>
        ''' <remarks></remarks>
        <Description("ERROR")> ERR = ConsoleColor.Red
        ''' <summary>
        ''' Warnning message from the program.[YELLOW]
        ''' </summary>
        ''' <remarks></remarks>
        <Description("WARNG")> WRN = ConsoleColor.Yellow
        ''' <summary>
        ''' The program debug information message.[BLUE]
        ''' </summary>
        ''' <remarks></remarks>
        <Description("DEBUG")> DEBUG = ConsoleColor.Blue

        ''' <summary>
        ''' Specialized Developer Info
        ''' </summary>
        FINEST
    End Enum
End Namespace
