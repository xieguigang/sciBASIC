#Region "Microsoft.VisualBasic::5ad8e7cc90455bdeb5d1ca8d3fe0cee6, Data\BinaryData\HDF5\dataset\filters\IFilter.vb"

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

    '   Total Lines: 25
    '    Code Lines: 7 (28.00%)
    ' Comment Lines: 13 (52.00%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 5 (20.00%)
    '     File Size: 763 B


    '     Interface IFilter
    ' 
    '         Properties: id, name
    ' 
    '         Function: decode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace dataset.filters

    ''' <summary>
    ''' Interface to be implemented to be a HDF5 filter.
    ''' 
    ''' @author James Mudd
    ''' </summary>
    Public Interface IFilter

        ''' <summary>
        ''' Gets the ID of this filter, this must match the ID in the dataset header.
        ''' </summary>
        ''' <returns> the ID of this filter </returns>
        ReadOnly Property id As Integer

        ''' <summary>
        ''' Gets the name of this filter e.g. 'deflate', 'shuffle'
        ''' </summary>
        ''' <returns> the name of this filter </returns>
        ReadOnly Property name As String

        Function decode(encodedData As Byte(), filterData As Integer()) As Byte()

    End Interface
End Namespace
