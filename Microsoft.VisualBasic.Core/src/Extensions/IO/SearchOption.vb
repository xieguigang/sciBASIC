#Region "Microsoft.VisualBasic::fb9d7825cca0cb03855537da0405f5e7, Microsoft.VisualBasic.Core\src\Extensions\IO\SearchOption.vb"

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

    '   Total Lines: 20
    '    Code Lines: 8
    ' Comment Lines: 9
    '   Blank Lines: 3
    '     File Size: 564 B


    '     Enum SearchOption
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NETSTANDARD1_2_OR_GREATER Then

Namespace FileIO

    ''' <summary>
    ''' Specifies whether to search all or only top-level directories.
    ''' </summary>
    Public Enum SearchOption

        ''' <summary>
        ''' Search only the specified directory and exclude subdirectories.
        ''' </summary>
        SearchTopLevelOnly = 2
        ''' <summary>
        ''' Search the specified directory and all subdirectories within it. Default.
        ''' </summary>
        SearchAllSubDirectories = 3
    End Enum
End Namespace
#End If
