#Region "Microsoft.VisualBasic::c129e108dd8ea93e96922f71d91cda5a, Data_science\Mathematica\Math\Math\DownSampling\FixedNumBucketSplitter.vb"

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

    '   Total Lines: 45
    '    Code Lines: 31 (68.89%)
    ' Comment Lines: 4 (8.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (22.22%)
    '     File Size: 1.73 KB


    '     Class FixedNumBucketSplitter
    ' 
    '         Function: split
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    ''' <summary>
    ''' Assign the first event to the first bucket, the last event to the last bucket.<br />
    ''' Split the rest events into the rest (threshold - 2) buckets each containing approximately equal number of events
    ''' </summary>
    Public Class FixedNumBucketSplitter(Of B As Bucket, E As ITimeSignal)
        Implements BucketSplitter(Of B, E)

        Public Overridable Function split(factory As BucketFactory(Of B), data As IList(Of E), threshold As Integer) As IList(Of B) Implements BucketSplitter(Of B, E).split
            Dim bucketNum As Integer = threshold - 2
            Dim netSize As Integer = data.Count - 2
            Dim bucketSize As Integer = (netSize + bucketNum - 1) \ bucketNum

            Dim buckets As New List(Of B)(threshold)
            For i As Integer = 0 To threshold - 1
                buckets.Add(Nothing)
            Next i

            buckets(0) = factory.newBucket(data(0))
            buckets(threshold - 1) = factory.newBucket(data(data.Count - 1))

            For i As Integer = 0 To bucketNum - 1
                buckets(i + 1) = factory.newBucket(bucketSize)
            Next i
            Dim [step] As Double = netSize * 1.0 / bucketNum
            Dim curr As Double = [step]
            Dim bucketIndex As Integer = 1

            For i As Integer = 1 To netSize
                buckets(bucketIndex).add(data(i))
                If i > curr Then
                    bucketIndex += 1
                    curr += [step]
                End If
            Next

            Return buckets
        End Function

    End Class

End Namespace
