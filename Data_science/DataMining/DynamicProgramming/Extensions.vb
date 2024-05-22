#Region "Microsoft.VisualBasic::f1d4f5a6188d163ae6522b74ec309481, Data_science\DataMining\DynamicProgramming\Extensions.vb"

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

    '   Total Lines: 33
    '    Code Lines: 24 (72.73%)
    ' Comment Lines: 6 (18.18%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (9.09%)
    '     File Size: 1.20 KB


    ' Module Extensions
    ' 
    '     Function: GetGeneralCharSymbol, PopulateAlignments
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch
Imports Microsoft.VisualBasic.Text

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

    Public Function GetGeneralCharSymbol() As GenericSymbol(Of Char)
        Return New GenericSymbol(Of Char)(
            equals:=Function(a, b) a = b,
            similarity:=Function(a, b) If(a = b, 1, 0),
            toChar:=Function(c) c,
            empty:=Function() ASCII.NUL
        )
    End Function
End Module
