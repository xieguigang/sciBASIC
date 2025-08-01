#Region "Microsoft.VisualBasic::1fe08ea35d8b8cae9cbd1a3196b1c887, Data_science\Mathematica\Math\Math\DownSampling\BucketSplitter.vb"

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

    '   Total Lines: 14
    '    Code Lines: 6 (42.86%)
    ' Comment Lines: 3 (21.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (35.71%)
    '     File Size: 382 B


    '     Interface BucketSplitter
    ' 
    '         Function: split
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    ''' <summary>
    ''' Split up events into buckets
    ''' </summary>
    Public Interface BucketSplitter(Of B As Bucket, E As ITimeSignal)

        Function split(factory As BucketFactory(Of B), data As IList(Of E), threshold As Integer) As IList(Of B)

    End Interface

End Namespace
