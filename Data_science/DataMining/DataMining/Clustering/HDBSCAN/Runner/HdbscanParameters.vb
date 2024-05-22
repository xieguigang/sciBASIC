#Region "Microsoft.VisualBasic::9205eea14b0b6c1c04737152af3b6093, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Runner\HdbscanParameters.vb"

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
    '    Code Lines: 14 (82.35%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (17.65%)
    '     File Size: 667 B


    '     Class HdbscanParameters
    ' 
    '         Properties: CacheDistance, Constraints, DataSet, DistanceFunction, Distances
    '                     MaxDegreeOfParallelism, MinClusterSize, MinPoints
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Distance
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Hdbscanstar

Namespace HDBSCAN.Runner
    Public Class HdbscanParameters(Of T)
        Public Property CacheDistance As Boolean = True
        Public Property MaxDegreeOfParallelism As Integer = 1

        Public Property Distances As Double()()
        Public Property DataSet As T()
        Public Property DistanceFunction As IDistanceCalculator(Of T)

        Public Property MinPoints As Integer
        Public Property MinClusterSize As Integer
        Public Property Constraints As List(Of HdbscanConstraint)
    End Class
End Namespace
