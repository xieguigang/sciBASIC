#Region "Microsoft.VisualBasic::b1c42b4cfce8e11212ce4b5721b1461f, Data\DataFrame\DATA\MatrixProvider.vb"

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

    '   Total Lines: 17
    '    Code Lines: 6
    ' Comment Lines: 7
    '   Blank Lines: 4
    '     File Size: 404 B


    '     Interface MatrixProvider
    ' 
    '         Function: GetMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO

Namespace DATA

    ''' <summary>
    ''' A numeric data matrix provider
    ''' </summary>
    Public Interface MatrixProvider

        ''' <summary>
        ''' populate the matrix data in row by row
        ''' </summary>
        ''' <returns></returns>
        Function GetMatrix() As IEnumerable(Of DataSet)

    End Interface
End Namespace
