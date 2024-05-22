#Region "Microsoft.VisualBasic::6e9e22f206938a65c1a1b69dfefadadf, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Scaler\ColorRange.vb"

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

    '   Total Lines: 32
    '    Code Lines: 22 (68.75%)
    ' Comment Lines: 5 (15.62%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (15.62%)
    '     File Size: 1.11 KB


    '     Structure ColorRange
    ' 
    '         Properties: Level, Points
    ' 
    '         Function: GetMinDistance, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors.Scaler

    Public Structure ColorRange : Implements INamedValue

        Public Property Level$ Implements INamedValue.Key
        Public Property Points As Color()

        ''' <summary>
        ''' 返回和最近的一个颜色点的距离值
        ''' </summary>
        ''' <param name="color"></param>
        ''' <returns></returns>
        Public Function GetMinDistance(color As Color) As Double
            With color
                Dim array As Double() = { .R, .G, .B}
                Return Points.Min(
                    Function(x)
                        Return DistanceMethods.EuclideanDistance(array, New Double() {x.R, x.G, x.B})
                    End Function)
            End With
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
