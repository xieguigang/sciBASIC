#Region "Microsoft.VisualBasic::229217c2aa27af00b0a6dd66a82484e4, Data_science\Mathematica\Math\Math\DownSampling\MaxMin\PIPlotBucketFactory.vb"

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
    '     File Size: 786 B


    '     Class PIPlotBucketFactory
    ' 
    '         Function: (+3 Overloads) newBucket
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.MaxMin

    Public Class PIPlotBucketFactory
        Implements BucketFactory(Of PIPlotBucket)

        Public Overridable Function newBucket() As PIPlotBucket Implements BucketFactory(Of PIPlotBucket).newBucket
            Return New PIPlotBucket()
        End Function

        Public Overridable Function newBucket(size As Integer) As PIPlotBucket Implements BucketFactory(Of PIPlotBucket).newBucket
            Return New PIPlotBucket(size)
        End Function

        Public Overridable Function newBucket(e As ITimeSignal) As PIPlotBucket Implements BucketFactory(Of PIPlotBucket).newBucket
            Return New PIPlotBucket(e)
        End Function

    End Class

End Namespace
