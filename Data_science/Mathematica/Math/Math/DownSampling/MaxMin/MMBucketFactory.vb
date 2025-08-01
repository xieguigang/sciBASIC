#Region "Microsoft.VisualBasic::a8ef79e30c1c52c69d57957e24f249da, Data_science\Mathematica\Math\Math\DownSampling\MaxMin\MMBucketFactory.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15 (68.18%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (31.82%)
    '     File Size: 742 B


    '     Class MMBucketFactory
    ' 
    '         Function: (+3 Overloads) newBucket
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.MaxMin

    Public Class MMBucketFactory
        Implements BucketFactory(Of MMBucket)

        Public Overridable Function newBucket() As MMBucket Implements BucketFactory(Of MMBucket).newBucket
            Return New MMBucket()
        End Function

        Public Overridable Function newBucket(size As Integer) As MMBucket Implements BucketFactory(Of MMBucket).newBucket
            Return New MMBucket(size)
        End Function

        Public Overridable Function newBucket(e As ITimeSignal) As MMBucket Implements BucketFactory(Of MMBucket).newBucket
            Return New MMBucket(e)
        End Function

    End Class

End Namespace
