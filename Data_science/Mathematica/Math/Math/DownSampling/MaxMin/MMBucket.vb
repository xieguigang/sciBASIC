#Region "Microsoft.VisualBasic::c827412ffc30a6c997c1bc7313696d87, Data_science\Mathematica\Math\Math\DownSampling\MaxMin\MMBucket.vb"

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

    '   Total Lines: 65
    '    Code Lines: 51 (78.46%)
    ' Comment Lines: 3 (4.62%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (16.92%)
    '     File Size: 2.12 KB


    '     Class MMBucket
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: add, selectInto
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.MaxMin

    ''' <summary>
    ''' Bucket that selects events with maximum or minimum value
    ''' </summary>
    Public Class MMBucket : Implements Bucket

        Protected Friend events As New List(Of ITimeSignal)()

        Public Sub New()
        End Sub

        Public Sub New(e As ITimeSignal)
            events.Add(e)
        End Sub

        Public Sub New(size As Integer)

        End Sub

        Public Overridable Sub selectInto(result As IList(Of ITimeSignal)) Implements Bucket.selectInto
            If events.Count <= 1 Then
                CType(result, List(Of ITimeSignal)).AddRange(events)
                Return
            End If
            Dim maxEvt As ITimeSignal = Nothing
            Dim minEvt As ITimeSignal = Nothing
            Dim max As Double = Double.Epsilon
            Dim min As Double = Double.MaxValue
            For Each e As ITimeSignal In events
                Dim val As Double = e.intensity
                If val > max Then
                    maxEvt = e
                    max = e.intensity
                End If
                If val < min Then
                    minEvt = e
                    min = e.intensity
                End If
            Next e
            If maxEvt IsNot Nothing AndAlso minEvt IsNot Nothing Then
                Dim maxFirst As Boolean = maxEvt.time < minEvt.time
                If maxFirst Then
                    result.Add(maxEvt)
                    result.Add(minEvt)
                Else
                    result.Add(minEvt)
                    result.Add(maxEvt)
                End If
            ElseIf maxEvt Is Nothing AndAlso minEvt IsNot Nothing Then
                result.Add(minEvt)
            ElseIf maxEvt IsNot Nothing AndAlso minEvt Is Nothing Then
                result.Add(maxEvt)
            End If
        End Sub

        Public Overridable Sub add(e As ITimeSignal) Implements Bucket.add
            events.Add(e)
        End Sub

    End Class

End Namespace
