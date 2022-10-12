#Region "Microsoft.VisualBasic::611a532b94040865ebc40dbe6140a610, sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\PCA\Algorithm.vb"

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

'   Total Lines: 6
'    Code Lines: 4
' Comment Lines: 0
'   Blank Lines: 2
'     File Size: 87 B


'     Module Algorithm
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace LinearAlgebra.Prcomp

    Public Module Algorithm

        ''' <summary>
        ''' 95%置信度水平
        ''' </summary>
        Const s95 As Double = 5.991

        ''' <summary>
        ''' 生成95%置信度区间的椭圆
        ''' </summary>
        ''' <param name="a">x的标准差</param>
        ''' <param name="b">y的标准差</param>
        ''' <returns></returns>
        Public Function Ellipse(a As Double, b As Double, Optional n As Integer = 100) As PointF()

        End Function
    End Module
End Namespace
