#Region "Microsoft.VisualBasic::468c5a52e731d03f9ddc4bb4a3c282e1, Microsoft.VisualBasic.Core\src\Data\ILabeledMatrix.vb"

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

    '   Total Lines: 15
    '    Code Lines: 5 (33.33%)
    ' Comment Lines: 7 (46.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (20.00%)
    '     File Size: 363 B


    '     Interface ILabeledMatrix
    ' 
    '         Function: GetLabels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' dataframe helper
    ''' </summary>
    Public Interface ILabeledMatrix

        ''' <summary>
        ''' get the labels of each row data
        ''' </summary>
        ''' <returns></returns>
        Function GetLabels() As IEnumerable(Of String)

    End Interface
End Namespace
