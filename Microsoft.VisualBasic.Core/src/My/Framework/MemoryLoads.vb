#Region "Microsoft.VisualBasic::119e1fb4aaa060b2be0b1246e4747f9d, Microsoft.VisualBasic.Core\src\My\Framework\MemoryLoads.vb"

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
    '    Code Lines: 7 (35.00%)
    ' Comment Lines: 12 (60.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (5.00%)
    '     File Size: 542 B


    '     Enum MemoryLoads
    ' 
    '         Heavy, Light, Max
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace My.FrameworkInternal

    ''' <summary>
    ''' config memory usage by the framework
    ''' </summary>
    Public Enum MemoryLoads As Byte
        ''' <summary>
        ''' lazy load
        ''' </summary>
        Light
        ''' <summary>
        ''' load all file data into memory when file size less than 2GB
        ''' </summary>
        Heavy
        ''' <summary>
        ''' load all data into memory even data file size is greater than 2GB
        ''' </summary>
        Max
    End Enum
End Namespace
