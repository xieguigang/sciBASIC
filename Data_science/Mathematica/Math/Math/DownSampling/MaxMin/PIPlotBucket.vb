#Region "Microsoft.VisualBasic::e8506b0c98c7b8eb514ed5c3ff4e36df, Data_science\Mathematica\Math\Math\DownSampling\MaxMin\PIPlotBucket.vb"

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

    '   Total Lines: 41
    '    Code Lines: 27 (65.85%)
    ' Comment Lines: 3 (7.32%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (26.83%)
    '     File Size: 1.12 KB


    '     Class PIPlotBucket
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: selectInto
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.MaxMin


    ''' <summary>
    ''' Bucket that selects the first, the last event and events with maximum or minimum value
    ''' </summary>
    Public Class PIPlotBucket
        Inherits MMBucket

        Public Sub New()
        End Sub

        Public Sub New(size As Integer)
            MyBase.New(size)
        End Sub

        Public Sub New(e As ITimeSignal)
            MyBase.New(e)
        End Sub

        Public Overrides Sub selectInto(result As IList(Of ITimeSignal))
            Dim temp As IList(Of ITimeSignal) = New List(Of ITimeSignal)()
            MyBase.selectInto(temp)
            Dim [set] As New HashSet(Of ITimeSignal)()
            If temp.Count > 0 Then
                [set].Add(events(0))

                For Each item In temp
                    Call [set].Add(item)
                Next

                [set].Add(events(events.Count - 1))
            End If
            CType(result, List(Of ITimeSignal)).AddRange([set])
        End Sub

    End Class

End Namespace
