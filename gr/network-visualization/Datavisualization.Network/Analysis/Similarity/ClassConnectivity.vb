#Region "Microsoft.VisualBasic::dec49edf59a839fc8dd091ae1c836fcd, gr\network-visualization\Datavisualization.Network\Analysis\Similarity\ClassConnectivity.vb"

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
    '    Code Lines: 35
    ' Comment Lines: 1
    '   Blank Lines: 5
    '     File Size: 1.70 KB


    '     Module ClassConnectivity
    ' 
    '         Function: MeasureCosValue, nodeGroupCounts
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Analysis.SimilarityImpl

    Module ClassConnectivity

        Public Function MeasureCosValue(a As Node, b As Node) As Double
            Dim atypes As Dictionary(Of String, Integer) = a.nodeGroupCounts
            Dim btypes As Dictionary(Of String, Integer) = b.nodeGroupCounts
            Dim allGroups As Index(Of String) = atypes.Keys.AsList + btypes.Keys
            Dim av As New Vector(allGroups.EnumerateMapKeys.Select(AddressOf atypes.TryGetValue))
            Dim bv As New Vector(allGroups.EnumerateMapKeys.Select(AddressOf btypes.TryGetValue))

            If av.Length = 1 Then
                ' 20200318 deal with the NaN result value.
                If av(Scan0) = bv(Scan0) Then
                    Return 1
                Else
                    Return 0
                End If
            ElseIf av.Length = 0 Then
                Return 0
            Else
                Dim cos As Double = Math.SSM(av, bv)
                Return cos
            End If
        End Function

        <Extension>
        Private Function nodeGroupCounts(v As Node) As Dictionary(Of String, Integer)
            Return (From type As String In v.AllNodeTypes Group By type Into Count) _
                .ToDictionary(Function(group) group.type,
                              Function(group)
                                  Return group.Count
                              End Function)
        End Function
    End Module
End Namespace
