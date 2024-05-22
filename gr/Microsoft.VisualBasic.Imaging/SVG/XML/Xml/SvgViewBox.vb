#Region "Microsoft.VisualBasic::55cfc20f22e2ce95b68ca1164814e238, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\SvgViewBox.vb"

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

    '   Total Lines: 36
    '    Code Lines: 28 (77.78%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (22.22%)
    '     File Size: 1.29 KB


    '     Structure SvgViewBox
    ' 
    '         Properties: Height, Left, Top, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Runtime.CompilerServices

Namespace SVG.XML

    Public Structure SvgViewBox

        Public Property Left As Double
        Public Property Top As Double
        Public Property Width As Double
        Public Property Height As Double

        Sub New(left As Double, top As Double, width As Double, height As Double)
            _Left = left
            _Top = top
            _Width = width
            _Height = height
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return String.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", Left, Top, Width, Height)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(v As Double()) As SvgViewBox
            Return New SvgViewBox(v(0), v(1), v(2), v(3))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(vb As (left As Double, top As Double, width As Double, height As Double)) As SvgViewBox
            Return New SvgViewBox(vb.left, vb.top, vb.width, vb.height)
        End Operator

    End Structure
End Namespace
