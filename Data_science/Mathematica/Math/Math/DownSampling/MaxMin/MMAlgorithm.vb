#Region "Microsoft.VisualBasic::8e3d188403e081d62c48a6ad0fa80767, Data_science\Mathematica\Math\Math\DownSampling\MaxMin\MMAlgorithm.vb"

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

    '   Total Lines: 28
    '    Code Lines: 17 (60.71%)
    ' Comment Lines: 3 (10.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (28.57%)
    '     File Size: 875 B


    '     Class MMAlgorithm
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

Namespace DownSampling.MaxMin

    ''' <summary>
    ''' Select events with maximum or minimum value in bucket
    ''' </summary>
    Public Class MMAlgorithm : Inherits BucketBasedAlgorithm(Of MMBucket, ITimeSignal)

        Public Sub New()
            BucketFactory(New MMBucketFactory)
            SetSpliter(New FixedTimeBucketSplitter(Of MMBucket, ITimeSignal))
        End Sub

        Protected Friend Overrides Function prepare(data As IList(Of ITimeSignal)) As IList(Of ITimeSignal)
            Return data
        End Function

        Protected Friend Overrides Sub beforeSelect(buckets As IList(Of MMBucket), threshold As Integer)

        End Sub

        Public Overrides Function ToString() As String
            Return "MaxMin"
        End Function

    End Class
End Namespace
