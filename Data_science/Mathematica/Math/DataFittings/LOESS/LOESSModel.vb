#Region "Microsoft.VisualBasic::ee10c82c88abd217967767581a82709e, Data_science\Mathematica\Math\DataFittings\LOESS\LOESSModel.vb"

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

    '   Total Lines: 10
    '    Code Lines: 6 (60.00%)
    ' Comment Lines: 3 (30.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (10.00%)
    '     File Size: 261 B


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

