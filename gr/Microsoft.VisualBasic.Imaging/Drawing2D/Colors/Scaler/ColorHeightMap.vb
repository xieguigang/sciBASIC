#Region "Microsoft.VisualBasic::d66d0fdad825a2eec5a6796e045516ec, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Scaler\ColorHeightMap.vb"

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

    '   Total Lines: 63
    '    Code Lines: 33 (52.38%)
    ' Comment Lines: 20 (31.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (15.87%)
    '     File Size: 2.01 KB


    '     Class ColorHeightMap
    ' 
    '         Properties: Levels
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetScale, GetVector, ScaleLevels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Namespace Drawing2D.Colors.Scaler

    ''' <summary>
    ''' Map a color to a single numeric value
    ''' </summary>
    ''' <remarks>
    ''' Extract numeric value from a raster data
    ''' </remarks>
    Public Class ColorHeightMap : Implements IBucketVector

        Dim ruler As Color()

        Public ReadOnly Property Levels As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ruler.Length
            End Get
        End Property

        Sub New(ParamArray ruler As Color())
            Me.ruler = ruler
        End Sub

        ''' <summary>
        ''' scale the scalar color palette to a specific level.
        ''' </summary>
        ''' <param name="level"></param>
        ''' <returns></returns>
        Public Function ScaleLevels(level As Integer) As ColorHeightMap
            ruler = Designer.CubicSpline(ruler, level, interpolate:=True)
            Return Me
        End Function

        ''' <summary>
        ''' mapping color to level
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        Public Function GetScale(c As Color) As Single
            Dim v As Double() = New Double(ruler.Length - 1) {}

            For i As Integer = 0 To v.Length - 1
                v(i) = ruler(i).EuclideanDistance(c)
            Next

            Return CSng(which.Min(v))
        End Function

        ''' <summary>
        ''' use for html view in R# scripting
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetVector() As IEnumerable Implements IBucketVector.GetVector
            Return ruler.Select(Function(c) c.ToHtmlColor)
        End Function
    End Class
End Namespace
