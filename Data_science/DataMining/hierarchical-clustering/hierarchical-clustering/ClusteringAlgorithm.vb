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

