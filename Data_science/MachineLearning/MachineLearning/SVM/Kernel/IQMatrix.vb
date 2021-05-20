#Region "Microsoft.VisualBasic::150ee4edc251ece5969ceeb8680ab871, Data_science\MachineLearning\MachineLearning\SVM\Kernel\IQMatrix.vb"

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

    '     Interface IQMatrix
    ' 
    '         Function: GetQ, GetQD
    ' 
    '         Sub: SwapIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVM

    Friend Interface IQMatrix
        Function GetQ(column As Integer, len As Integer) As Single()
        Function GetQD() As Double()
        Sub SwapIndex(i As Integer, j As Integer)
    End Interface
End Namespace
