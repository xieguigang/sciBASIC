#Region "Microsoft.VisualBasic::fb13666e7b35ce0f0618b9c5f84ef5e5, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\ClusteringAlgorithm\ClusteringAlgorithm.vb"

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
    '    Code Lines: 6
    ' Comment Lines: 17
    '   Blank Lines: 4
    '     File Size: 1.31 KB


    ' Interface ClusteringAlgorithm
    ' 
    '     Function: performClustering, performFlatClustering, performWeightedClustering
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' <p/>
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' <p/>
' http://www.apache.org/licenses/LICENSE-2.0
' <p/>
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Public Interface ClusteringAlgorithm

    Function performClustering(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy) As Cluster
    Function performWeightedClustering(distances As Double()(), clusterNames As String(), weights As Double(), linkageStrategy As LinkageStrategy) As Cluster
    Function performFlatClustering(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy, threshold As Double) As IList(Of Cluster)

End Interface
