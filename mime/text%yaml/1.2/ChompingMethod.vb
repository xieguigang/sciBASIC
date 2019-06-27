#Region "Microsoft.VisualBasic::bedcc4bc233daac4c1f0f8f6ed56313c, mime\text%yaml\1.2\ChompingMethod.vb"

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

    '     Enum ChompingMethod
    ' 
    '         Clip, Keep, Strip
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text

Namespace Grammar

    ''' <summary>
    ''' How line breaks after last non empty line in block scalar are handled.
    ''' </summary>
    Public Enum ChompingMethod
        ''' <summary>
        ''' Keep one
        ''' </summary>
        Clip

        ''' <summary>
        ''' Keep none
        ''' </summary>
        Strip

        ''' <summary>
        ''' Keep all
        ''' </summary>
        Keep
    End Enum
End Namespace
