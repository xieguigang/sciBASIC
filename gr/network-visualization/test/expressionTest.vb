#Region "Microsoft.VisualBasic::d256a190d36d7dafe764a4ec2e7e3ec3, sciBASIC#\gr\network-visualization\test\expressionTest.vb"

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

    '   Total Lines: 16
    '    Code Lines: 10
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 407.00 B


    ' Module expressionTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Styling

Module expressionTest

    Const rangeMapper = "map(degree, [100,200])"
    Const discreteMapper = "map(domain, A=1, B=2, C= 99, G=8888)"

    Sub Main()

        Dim a = SyntaxExtensions.MapExpressionParser(rangeMapper)
        Dim b = SyntaxExtensions.MapExpressionParser(discreteMapper)

        Pause()

    End Sub
End Module
