#Region "Microsoft.VisualBasic::2783105ae343001a575c73ed38db74af, Data_science\DataMining\DataMining\Clustering\DBSCAN\Extensions.vb"

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

    '   Total Lines: 27
    '    Code Lines: 23 (85.19%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (14.81%)
    '     File Size: 1.14 KB


    '     Module Extensions
    ' 
    '         Function: RunDbscanCluster
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq

Namespace DBSCAN

    <HideModuleName>
    Public Module Extensions

        <Extension>
        Public Iterator Function RunDbscanCluster(data As IEnumerable(Of EntityClusterModel), eps As Double, minPts As Integer) As IEnumerable(Of EntityClusterModel)
            With data.ToArray
                Dim metrix As New Metric(.Select(Function(v) v.Properties.Keys).IteratesALL)
                Dim dbscan As New DbscanAlgorithm(Of EntityClusterModel)(AddressOf metrix.DistanceTo)
                Dim result As NamedCollection(Of EntityClusterModel)() = .DoCall(Function(vec) dbscan.ComputeClusterDBSCAN(vec, eps, minPts))

                For Each cluster In result
                    For Each c As EntityClusterModel In cluster
                        c.Cluster = cluster.name
                        Yield c
                    Next
                Next
            End With
        End Function
    End Module
End Namespace
