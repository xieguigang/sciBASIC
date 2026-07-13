#Region "Microsoft.VisualBasic::69c667e12913986627a3f9bd067fad21, Data_science\Visualization\DataPlot\Advanced\BoxGroup.vb"

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
    '    Code Lines: 6 (66.67%)
    ' Comment Lines: 1 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (22.22%)
    '     File Size: 233 B


    ' Class BoxGroup
    ' 
    '     Properties: Color, Data, Name
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Drawing

''' <summary>盒须图分组</summary>
Public Class BoxGroup
    Public Property Name As String = ""
    Public Property Data As Double() = {}
    Public Property Color As Color? = Nothing
End Class

