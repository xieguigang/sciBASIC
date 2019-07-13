#Region "Microsoft.VisualBasic::26d34881265a9011c2271b70bc919f8d, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\LinkageStrategy.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Interface LinkageStrategy
    ' 
    '     Function: CalculateDistance
    ' 
    ' Class SingleLinkageStrategy
    ' 
    '     Function: CalculateDistance
    ' 
    ' Class WeightedLinkageStrategy
    ' 
    '     Function: CalculateDistance
    ' 
    ' Class CompleteLinkageStrategy
    ' 
    '     Function: CalculateDistance
    ' 
    ' Class AverageLinkageStrategy
    ' 
    '     Function: CalculateDistance
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Public Interface LinkageStrategy
    Function CalculateDistance(distances As ICollection(Of Distance)) As Distance
End Interface

Public Class SingleLinkageStrategy
    Implements LinkageStrategy

    Public Function CalculateDistance(distances As ICollection(Of Distance)) As Distance Implements LinkageStrategy.CalculateDistance
        Dim min As Double = Double.NaN

        For Each dist As Distance In distances
            If Double.IsNaN(min) OrElse dist.Distance < min Then min = dist.Distance
        Next

        Return New Distance(min)
    End Function
End Class


Public Class WeightedLinkageStrategy
    Implements LinkageStrategy

    Public Function CalculateDistance(distances As ICollection(Of Distance)) As Distance Implements LinkageStrategy.CalculateDistance
        Dim sum As Double = 0
        Dim weightTotal As Double = 0

        For Each distance As Distance In distances
            weightTotal += distance.Weight
            sum += distance.Distance * distance.Weight
        Next

        Return New Distance(sum / weightTotal, weightTotal)
    End Function
End Class

Public Class CompleteLinkageStrategy
    Implements LinkageStrategy

    Public Function CalculateDistance(distances As ICollection(Of Distance)) As Distance Implements LinkageStrategy.CalculateDistance
        Dim max As Double = Double.NaN

        For Each dist As Distance In distances
            If Double.IsNaN(max) OrElse dist.Distance > max Then max = dist.Distance
        Next

        Return New Distance(max)
    End Function
End Class

Public Class AverageLinkageStrategy
    Implements LinkageStrategy

    Public Function CalculateDistance(distances As ICollection(Of Distance)) As Distance Implements LinkageStrategy.CalculateDistance
        Dim sum As Double = 0
        Dim result As Double

        For Each dist As Distance In distances
            sum += dist.Distance
        Next

        If distances.Count > 0 Then
            result = sum / distances.Count
        Else
            result = 0.0
        End If

        Return New Distance(result)
    End Function
End Class
