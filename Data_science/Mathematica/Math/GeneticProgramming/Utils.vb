#Region "Microsoft.VisualBasic::d0f8d8b03af7165bf9c957bbf6513ad5, Data_science\Mathematica\Math\GeneticProgramming\Utils.vb"

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

    '   Total Lines: 19
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 450 B


    ' Module Utils
    ' 
    '     Sub: addAll, removeAll
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Friend Module Utils

    <Extension()>
    Public Sub addAll(Of T)([set] As ISet(Of T), data As IEnumerable(Of T))
        For Each x As T In data
            [set].Add(x)
        Next
    End Sub

    <Extension()>
    Public Sub removeAll(Of T)([set] As ISet(Of T), data As IEnumerable(Of T))
        For Each x As T In data
            [set].Remove(x)
        Next
    End Sub

End Module
