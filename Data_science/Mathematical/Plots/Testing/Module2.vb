#Region "Microsoft.VisualBasic::f1857d8bb7c46d490748367d4b0e27cd, ..\sciBASIC#\Data_science\Mathematical\Plots\Testing\Module2.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Correlations.Correlations

Module Module2

    <Extension>
    Public Function Correlations(a#(), b#(), Optional compute As ICorrelation = Nothing) As Dictionary(Of String, Dictionary(Of String, String))
        If compute Is Nothing Then
            compute = AddressOf GetPearson
        End If

        Dim Time#() = a.Sequence.ToArray(AddressOf Val)
        Dim ta = compute(a, Time)
        Dim tb = compute(b, Time)


    End Function
End Module

