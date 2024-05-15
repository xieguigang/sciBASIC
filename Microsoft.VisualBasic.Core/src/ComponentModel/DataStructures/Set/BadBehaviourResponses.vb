#Region "Microsoft.VisualBasic::bc03d914d599c8998230d843a443ae88, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Set\BadBehaviourResponses.vb"

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

    '   Total Lines: 18
    '    Code Lines: 6
    ' Comment Lines: 10
    '   Blank Lines: 2
    '     File Size: 545 B


    '     Enum BadBehaviourResponses
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' Enum of values to determine the aggressiveness of the response of the
    ''' class to bad data.
    ''' </summary>
    Public Enum BadBehaviourResponses
        ''' <summary>
        ''' If the user enters bad data, throw an exception they have to deal with.
        ''' </summary>
        BeAggressive = 0

        ''' <summary>
        ''' If the user enters bad data, just eat it quietly.
        ''' </summary>
        BeCool = 1
    End Enum
End Namespace
