#Region "Microsoft.VisualBasic::d813928dc077075814d5aaad6fbf489b, Microsoft.VisualBasic.Core\src\Extensions\ValueTypes\TupleHelper.vb"

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

    '   Total Lines: 29
    '    Code Lines: 22
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 842 B


    ' Module TupleHelper
    ' 
    '     Sub: (+3 Overloads) [Set]
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

''' <summary>
''' Deconstruct of the tuple data via byref out
''' </summary>
Public Module TupleHelper

    <Extension>
    Public Sub [Set](Of T1, T2)(t As (T1, T2), <Out> ByRef a As T1, <Out> ByRef b As T2)
        a = t.Item1
        b = t.Item2
    End Sub

    <Extension>
    Public Sub [Set](Of T1, T2, T3)(t As (T1, T2, T3), <Out> ByRef a As T1, <Out> ByRef b As T2, <Out> ByRef c As T3)
        a = t.Item1
        b = t.Item2
        c = t.Item3
    End Sub

    <Extension>
    Public Sub [Set](Of T1, T2, T3, T4)(t As (T1, T2, T3, T4), <Out> ByRef a As T1, <Out> ByRef b As T2, <Out> ByRef c As T3, <Out> ByRef d As T4)
        a = t.Item1
        b = t.Item2
        c = t.Item3
        d = t.Item4
    End Sub
End Module
