#Region "Microsoft.VisualBasic::01b6453c24d88ba62bb8afa00f92de01, Data_science\Visualization\Plots\3D\Serial3D.vb"

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

    '   Total Lines: 30
    '    Code Lines: 18 (60.00%)
    ' Comment Lines: 7 (23.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (16.67%)
    '     File Size: 1.03 KB


    '     Class Serial3D
    ' 
    '         Properties: Color, Points, PointSize, Shape, Title
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    ''' <summary>
    ''' Scatter serial data in 3D
    ''' </summary>
    Public Class Serial3D

        Public Property Title As String
        Public Property Color As Color
        Public Property Shape As LegendStyles

        ''' <summary>
        ''' The point data which tagged a name label, if the label is empty, then will not display on the plot.
        ''' </summary>
        ''' <returns></returns>
        Public Property Points As NamedValue(Of Point3D)()
        Public Property PointSize As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{Shape.ToString}, {Color.ToString}] {Title}"
        End Function
    End Class
End Namespace
