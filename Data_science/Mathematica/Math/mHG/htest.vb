#Region "Microsoft.VisualBasic::37a69f710e4ac1e14f893be887d20d80, Data_science\Mathematica\Math\mHG\htest.vb"

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

    '   Total Lines: 9
    '    Code Lines: 7 (77.78%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (22.22%)
    '     File Size: 267 B


    ' Class htest
    ' 
    '     Properties: b, n, parameters, pvalue, statistic
    ' 
    ' /********************************************************************************/

#End Region

Public Class htest

    Public Property statistic As Dictionary(Of String, Double)
    Public Property parameters As Dictionary(Of String, Double)
    Public Property pvalue As Double
    Public Property n As Integer
    Public Property b As Double

End Class
