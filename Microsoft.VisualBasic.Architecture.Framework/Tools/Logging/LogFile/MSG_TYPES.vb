#Region "Microsoft.VisualBasic::d96c4daf89d163def8ca7372847995f2, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\Logging\LogFile\MSG_TYPES.vb"

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

Imports System.Text

Namespace Logging

    ''' <summary>
    ''' The types enumeration of the log file message.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum MSG_TYPES As Integer

        ''' <summary>
        ''' The normal information message.[WHITE]
        ''' </summary>
        ''' <remarks></remarks>
        INF
        ''' <summary>
        ''' The program error information message.[RED]
        ''' </summary>
        ''' <remarks></remarks>
        ERR
        ''' <summary>
        ''' Warnning message from the program.[YELLOW]
        ''' </summary>
        ''' <remarks></remarks>
        WRN
        ''' <summary>
        ''' The program debug information message.[BLUE]
        ''' </summary>
        ''' <remarks></remarks>
        DEBUG
    End Enum

End Namespace
