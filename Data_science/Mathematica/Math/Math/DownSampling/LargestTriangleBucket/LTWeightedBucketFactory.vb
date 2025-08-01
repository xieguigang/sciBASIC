#Region "Microsoft.VisualBasic::d17d048025cd438f29fd027edcd1e9c7, Data_science\Mathematica\Math\Math\DownSampling\LargestTriangleBucket\LTWeightedBucketFactory.vb"

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

    '   Total Lines: 21
    '    Code Lines: 14 (66.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (33.33%)
    '     File Size: 865 B


    '     Class LTWeightedBucketFactory
    ' 
    '         Function: (+3 Overloads) newBucket
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.LargestTriangleBucket

    Public Class LTWeightedBucketFactory : Implements BucketFactory(Of LTWeightedBucket)

        Public Overridable Function newBucket() As LTWeightedBucket Implements BucketFactory(Of LTWeightedBucket).newBucket
            Return New LTWeightedBucket()
        End Function

        Public Overridable Function newBucket(size As Integer) As LTWeightedBucket Implements BucketFactory(Of LTWeightedBucket).newBucket
            Return New LTWeightedBucket(size)
        End Function

        Public Overridable Function newBucket(e As ITimeSignal) As LTWeightedBucket Implements BucketFactory(Of LTWeightedBucket).newBucket
            Return New LTWeightedBucket(DirectCast(e, WeightedEvent))
        End Function

    End Class

End Namespace
