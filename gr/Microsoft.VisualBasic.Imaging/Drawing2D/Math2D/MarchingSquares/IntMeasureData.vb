#Region "Microsoft.VisualBasic::e75217060c9d8eac0923753acdaa9ef6, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\IntMeasureData.vb"

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

    '     Structure IntMeasureData
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' 这个是对实际数据做映射为绘图数据之后的结果
    ''' </summary>
    Public Structure IntMeasureData

        Public X As Integer
        Public Y As Integer
        Public Z As Double

        Public Sub New(md As MeasureData, x_num As Integer, y_num As Integer)
            X = CInt(md.X * x_num)

            If X >= x_num Then
                X = x_num - 1
            End If

            Y = CInt(md.Y * y_num)

            If Y >= y_num Then
                Y = y_num - 1
            End If

            Z = md.Z
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}] {Z}"
        End Function
    End Structure

End Namespace
