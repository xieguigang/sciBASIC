#Region "Microsoft.VisualBasic::c7980cb658ac4085196df1cb1af8ec24, gr\network-visualization\network_layout\Radial\RadialLayoutParameters.vb"

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
    '    Code Lines: 10 (55.56%)
    ' Comment Lines: 3 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (27.78%)
    '     File Size: 531 B


    '     Class RadialLayoutParameters
    ' 
    '         Properties: Radius
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Radial

    ''' <summary>
    ''' 径向布局参数，可在 PropertyGrid 中编辑
    ''' </summary>
    Public Class RadialLayoutParameters

        <Category("布局"), DisplayName("环间距"), Description("每层同心圆环之间的间距（像素），<=0 时自动推算")>
        Public Property Radius As Double = Double.NaN

        Public Overrides Function ToString() As String
            Return "Radial"
        End Function
    End Class

End Namespace

