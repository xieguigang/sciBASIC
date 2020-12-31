#Region "Microsoft.VisualBasic::f2189b00bc555b193f0464393a7ea948, Data_science\DataMining\DynamicProgramming\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: PopulateAlignments
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch

<HideModuleName>
Public Module Extensions

    ''' <summary>
    ''' This funktion provide a easy way to write a computed alignment into a fasta file
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="nw"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function PopulateAlignments(Of T)(nw As Workspace(Of T)) As IEnumerable(Of GlobalAlign(Of T))
        For i As Integer = 0 To nw.NumberOfAlignments - 1
            Yield New GlobalAlign(Of T)(nw.m_toChar) With {
                .score = nw.Score,
                .query = nw.getAligned1(i),
                .subject = nw.getAligned2(i)
            }
        Next
    End Function
End Module
