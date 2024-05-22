#Region "Microsoft.VisualBasic::0a0bbc8ac1e56ade120fa13e75217397, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Runner\HdbscanResult.vb"

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

    '   Total Lines: 9
    '    Code Lines: 8 (88.89%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 1 (11.11%)
    '     File Size: 316 B


    '     Class HdbscanResult
    ' 
    '         Properties: HasInfiniteStability, Labels, OutliersScore
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Hdbscanstar

Namespace HDBSCAN.Runner
    Public Class HdbscanResult
        Public Property Labels As Integer()
        Public Property OutliersScore As List(Of OutlierScore)
        Public Property HasInfiniteStability As Boolean
    End Class
End Namespace
