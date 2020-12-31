#Region "Microsoft.VisualBasic::b36b20e0b0e9c2dd267a749772d4b4c5, Data_science\DataMining\DynamicProgramming\SmithWaterman\Workspace.vb"

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

    '     Module Workspace
    ' 
    '         Function: CreateHSP, GetBestAlignment
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace SmithWaterman

    Public Module Workspace

        <Extension>
        Public Iterator Function CreateHSP(Of T)(sw As GSW(Of T), cutoff As Double) As IEnumerable(Of LocalHSPMatch(Of T))
            For Each match As Match In sw.Matches(cutoff)
                Yield New LocalHSPMatch(Of T)(match, sw.query, sw.subject, sw.symbol)
            Next
        End Function

        <Extension>
        Public Function GetBestAlignment(Of T)(hsp As IEnumerable(Of LocalHSPMatch(Of T))) As LocalHSPMatch(Of T)
            Dim query As IEnumerable(Of LocalHSPMatch(Of T)) =
                From x As LocalHSPMatch(Of T)
                In hsp _
                    .Select(Function(x) DirectCast(x, Match)) _
                    .DoCall(Function(list)
                                Return SimpleChaining.Chaining(list.ToArray, False)
                            End Function)
                Order By x.score Descending
            Dim lstb = query.ToArray

            If Not lstb.IsNullOrEmpty Then
                Return lstb.FirstOrDefault
            Else
                Return Nothing
            End If
        End Function
    End Module
End Namespace

