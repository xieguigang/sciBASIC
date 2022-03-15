#Region "Microsoft.VisualBasic::3782d485329399bd28bfc40c0fd349f5, sciBASIC#\Data_science\MachineLearning\CellularAutomaton\Extensions.vb"

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

    '   Total Lines: 49
    '    Code Lines: 41
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.96 KB


    ' Module Extensions
    ' 
    '     Function: CreateCountSnapshotBuckets, CreateSnapshotMatrix
    ' 
    '     Sub: TakeSnapshots
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

<HideModuleName>
Public Module Extensions

    <Extension>
    Public Sub TakeSnapshots(Of T As Individual)(simulator As Simulator(Of T), getKey As Func(Of T, String), counts As Dictionary(Of String, List(Of Integer)))
        Dim groups = simulator.Snapshot.GroupBy(getKey).ToDictionary(Function(n) n.Key, Function(n) n.Count)

        For Each type In counts.Keys
            If groups.ContainsKey(type) Then
                counts(type).Add(groups(type))
            Else
                counts(type).Add(0)
            End If
        Next
    End Sub

    Public Function CreateCountSnapshotBuckets(Of T As Structure)() As Dictionary(Of String, List(Of Integer))
        Dim snapshots As New Dictionary(Of String, List(Of Integer))

        For Each statuKey In Enums(Of T)()
            snapshots.Add(statuKey.ToString, New List(Of Integer))
        Next

        Return snapshots
    End Function

    <Extension>
    Public Function CreateSnapshotMatrix(Of T As {New, INamedValue, IDynamicMeta(Of Double)})(snapshots As Dictionary(Of String, List(Of Integer))) As T()
        Dim ticks As Integer() = snapshots.First.Value.Sequence.ToArray
        Dim matrix As T() = ticks _
            .Select(Function(i)
                        Return New T With {
                            .Key = i,
                            .Properties = snapshots _
                                .ToDictionary(Function(d) d.Key,
                                              Function(d)
                                                  Return CDbl(d.Value(i))
                                              End Function)
                        }
                    End Function) _
            .ToArray

        Return matrix
    End Function
End Module
