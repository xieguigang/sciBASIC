#Region "Microsoft.VisualBasic::c4cea1c00260d011986f51c42ce52471, Data_science\Mathematica\Math\Math\DownSampling\Bucket.vb"

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

    '   Total Lines: 15
    '    Code Lines: 7 (46.67%)
    ' Comment Lines: 3 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (33.33%)
    '     File Size: 361 B


    '     Interface Bucket
    ' 
    '         Sub: add, selectInto
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    ''' <summary>
    ''' A bucket holds a subset of events and select significant events from it
    ''' </summary>
    Public Interface Bucket

        Sub selectInto(result As IList(Of ITimeSignal))

        Sub add(e As ITimeSignal)

    End Interface
End Namespace
