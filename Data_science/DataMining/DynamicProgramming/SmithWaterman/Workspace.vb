#Region "Microsoft.VisualBasic::e9d65084b6ac0c81cc8f9828949af17b, Data_science\DataMining\DynamicProgramming\SmithWaterman\Workspace.vb"

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

    '   Total Lines: 34
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.27 KB


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
