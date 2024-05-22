#Region "Microsoft.VisualBasic::253a43d83cb9c6a4fbf586176fc839c6, Data_science\Mathematica\Math\GeneticProgramming\model\UnaryExpression.vb"

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

    '   Total Lines: 12
    '    Code Lines: 7 (58.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (41.67%)
    '     File Size: 230 B


    '     Interface UnaryExpression
    ' 
    '         Properties: Child
    ' 
    '         Function: swapChild
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace model

    Public Interface UnaryExpression
        Inherits Expression

        Property Child As Expression

        Function swapChild(newChild As Expression) As Expression

    End Interface

End Namespace
