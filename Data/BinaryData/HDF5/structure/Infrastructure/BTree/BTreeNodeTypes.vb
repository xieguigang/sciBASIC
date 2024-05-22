#Region "Microsoft.VisualBasic::e8912bbf5bf11cdfbd0c609327373012, Data\BinaryData\HDF5\structure\Infrastructure\BTree\BTreeNodeTypes.vb"

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
    '    Code Lines: 6 (31.58%)
    ' Comment Lines: 10 (52.63%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (15.79%)
    '     File Size: 541 B


    '     Enum BTreeNodeTypes
    ' 
    '         group, raw_data
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace struct

    ''' <summary>
    ''' Each B-tree points to a particular type of data. This field indicates the type of data 
    ''' as well as implying the maximum degree K of the tree and the size of each Key field.
    ''' </summary>
    Public Enum BTreeNodeTypes

        ''' <summary>
        ''' This tree points to group nodes.
        ''' </summary>
        group
        ''' <summary>
        ''' This tree points to raw data chunk nodes.
        ''' </summary>
        raw_data
    End Enum

End Namespace
