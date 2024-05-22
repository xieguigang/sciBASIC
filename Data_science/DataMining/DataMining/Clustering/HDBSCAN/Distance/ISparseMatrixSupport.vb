#Region "Microsoft.VisualBasic::670c8819e3f719df605b560e2b46ec9e, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Distance\ISparseMatrixSupport.vb"

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

    '   Total Lines: 8
    '    Code Lines: 5 (62.50%)
    ' Comment Lines: 3 (37.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 294 B


    '     Interface ISparseMatrixSupport
    ' 
    '         Function: GetMostCommonDistanceValueForSparseMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HDBSCAN.Distance
    Public Interface ISparseMatrixSupport
        ''' <summary>
        ''' Indicate the most common distance value for sparse matrix.
        ''' </summary>
        Function GetMostCommonDistanceValueForSparseMatrix() As Double
    End Interface
End Namespace
