#Region "Microsoft.VisualBasic::4358362e02e761139632d02b3fbd33b4, Data_science\Mathematica\Math\DataFittings\LOESS\LOESSModel.vb"

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
    ' Comment Lines: 3 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 257 B


    ' Class LOESSModel
    ' 
    '     Properties: Degree, Span, X, Y
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' LOESS模型，存储训练数据和参数
''' </summary>
Public Class LOESSModel
    Public Property X As Double()
    Public Property Y As Double()
    Public Property Span As Double
    Public Property Degree As Integer
End Class
