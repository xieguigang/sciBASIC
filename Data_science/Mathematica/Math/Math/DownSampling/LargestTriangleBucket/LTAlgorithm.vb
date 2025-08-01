#Region "Microsoft.VisualBasic::29ad53377cf59a8f26ac3f7d91afa526, Data_science\Mathematica\Math\Math\DownSampling\LargestTriangleBucket\LTAlgorithm.vb"

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

    '   Total Lines: 56
    '    Code Lines: 38 (67.86%)
    ' Comment Lines: 8 (14.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (17.86%)
    '     File Size: 2.03 KB


    '     Class LTAlgorithm
    ' 
    '         Properties: Wcalc
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: prepare, ToString
    ' 
    '         Sub: beforeSelect
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.LargestTriangleBucket

    ''' <summary>
    ''' Largest Triangle Bucket Algorithm family.
    ''' <ul>
    ''' <li>LTOB: Largest Triangle One Bucket</li>
    ''' <li>LTTB: Largest Triangle Three Bucket</li>
    ''' <li>LTD: Largest Triangle Dynamic (three bucket)</li>
    ''' </ul>
    ''' </summary>
    Public Class LTAlgorithm : Inherits BucketBasedAlgorithm(Of LTWeightedBucket, WeightedEvent)

        Protected Friend triangle As New Triangle()
        Protected Friend wcalc_Conflict As LTWeightCalculator

        Friend Sub New()
        End Sub

        Protected Friend Overrides Function prepare(data As IList(Of ITimeSignal)) As IList(Of WeightedEvent)
            Dim result As IList(Of WeightedEvent) = New List(Of WeightedEvent)(data.Count)
            For Each ITimeSignal As ITimeSignal In data
                result.Add(New WeightedEvent(ITimeSignal))
            Next ITimeSignal
            Return result
        End Function

        Protected Friend Overrides Sub beforeSelect(buckets As IList(Of LTWeightedBucket), threshold As Integer)
            wcalc_Conflict.calcWeight(triangle, buckets)
        End Sub

        Public Overridable WriteOnly Property Wcalc As LTWeightCalculator
            Set(wcalc As LTWeightCalculator)
                Me.wcalc_Conflict = wcalc
            End Set
        End Property

        Public Overrides Function ToString() As String
            Dim name As String = "LT"
            If TypeOf Me.wcalc_Conflict Is LTOneBucketWeightCalculator Then
                name &= "O"
            ElseIf TypeOf Me.wcalc_Conflict Is LTThreeBucketWeightCalculator Then
                name &= "T"
            End If
            If TypeOf Me.spliter Is LTDynamicBucketSplitter Then
                name &= "D"
            Else
                name &= "B"
            End If
            Return name
        End Function

    End Class

End Namespace
