#Region "Microsoft.VisualBasic::5dbbfa8c593e00b60d42e0625ccbb1eb, Data_science\Mathematica\Math\Math\DownSampling\BucketBasedAlgorithm.vb"

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

    '   Total Lines: 64
    '    Code Lines: 30 (46.88%)
    ' Comment Lines: 19 (29.69%)
    '    - Xml Docs: 89.47%
    ' 
    '   Blank Lines: 15 (23.44%)
    '     File Size: 2.31 KB


    '     Class BucketBasedAlgorithm
    ' 
    '         Function: process
    ' 
    '         Sub: BucketFactory, SetSpliter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    ''' <summary>
    ''' General algorithm using buckets to downsample events:<br />
    ''' <ul>
    ''' <li>Prepare data.</li>
    ''' <li>Split events into buckets.</li>
    ''' <li>Calculate weight of events.</li>
    ''' <li>Select significant events from each bucket.</li>
    ''' </ul>
    ''' </summary>
    ''' <typeparam name="B"> Bucket class </typeparam>
    ''' <typeparam name="E"> Event class </typeparam>
    Public MustInherit Class BucketBasedAlgorithm(Of B As Bucket, E As ITimeSignal)
        Implements DownSamplingAlgorithm

        Protected Friend spliter As BucketSplitter(Of B, E)
        Protected Friend factory As BucketFactory(Of B)

        ''' <summary>
        ''' initialize data for down sampling
        ''' </summary>
        Protected Friend MustOverride Function prepare(data As IList(Of ITimeSignal)) As IList(Of E)

        ''' <summary>
        ''' calculating weight or something else
        ''' </summary>
        Protected Friend MustOverride Sub beforeSelect(buckets As IList(Of B), threshold As Integer)

        Public Overridable Function process(events As IList(Of ITimeSignal), threshold As Integer) As IList(Of ITimeSignal) Implements DownSamplingAlgorithm.process
            Dim dataSize As Integer = events.Count

            If threshold >= dataSize OrElse dataSize < 3 Then
                Return events
            End If

            Dim preparedData As IList(Of E) = prepare(events)
            Dim buckets As IList(Of B) = spliter.split(factory, preparedData, threshold)

            ' calculating weight or something else
            Call beforeSelect(buckets, threshold)

            Dim result As IList(Of ITimeSignal) = New List(Of ITimeSignal)(threshold)

            ' select from every bucket
            For Each bucket As Bucket In buckets
                bucket.selectInto(result)
            Next

            Return result
        End Function

        Public Sub SetSpliter(value As BucketSplitter(Of B, E))
            Me.spliter = value
        End Sub

        Public Sub BucketFactory(value As BucketFactory(Of B))
            Me.factory = value
        End Sub
    End Class

End Namespace
