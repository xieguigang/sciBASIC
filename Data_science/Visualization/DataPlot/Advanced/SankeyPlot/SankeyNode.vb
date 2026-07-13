#Region "Microsoft.VisualBasic::191f14f623b204be472313477086d44c, Data_science\Visualization\DataPlot\Advanced\SankeyPlot\SankeyNode.vb"

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

    '   Total Lines: 18
    '    Code Lines: 15 (83.33%)
    ' Comment Lines: 1 (5.56%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (11.11%)
    '     File Size: 596 B


    ' Class SankeyNode
    ' 
    '     Properties: Color, Height, Id, InFlow, InOffsets
    '                 Label, Layer, OutFlow, OutOffsets, Total
    '                 X, Y
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Drawing

Public Class SankeyNode
    Public Property Id As String
    Public Property Label As String
    Public Property Layer As Integer
    Public Property Color As Color? = Nothing
    ' 运行时
    Public Property InFlow As Double
    Public Property OutFlow As Double
    Public Property Total As Double
    Public Property X As Single
    Public Property Y As Single
    Public Property Height As Single
    Public Property OutOffsets As New Dictionary(Of String, Double)()
    Public Property InOffsets As New Dictionary(Of String, Double)()
End Class

