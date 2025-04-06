#Region "Microsoft.VisualBasic::96b98dcb765317e7d9924bb6995809f1, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Scaler\Mapper\ColorManager.vb"

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

    '   Total Lines: 58
    '    Code Lines: 23 (39.66%)
    ' Comment Lines: 25 (43.10%)
    '    - Xml Docs: 96.00%
    ' 
    '   Blank Lines: 10 (17.24%)
    '     File Size: 1.89 KB


    '     Class ColorProfile
    ' 
    '         Properties: DefaultColor
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors.Scaler

    ''' <summary>
    ''' A color collection
    ''' </summary>
    ''' <remarks>
    ''' includes sub class:
    ''' 
    ''' 1. <see cref="ValueScaleColorProfile"/>: a color mapper for the value range;
    ''' 2. <see cref="CategoryColorProfile"/>: a color mapper for the category terms
    ''' </remarks>
    Public MustInherit Class ColorProfile

        Protected colors As Color()

        ''' <summary>
        ''' default color value for missing value, example as: nothing, NaN, Inf, etc
        ''' </summary>
        ''' <returns></returns>
        Public Property DefaultColor As Color = Nothing

        ''' <summary>
        ''' Create profile with a pre-defined color set
        ''' </summary>
        ''' <param name="colorSchema"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(colorSchema As String)
            Me.colors = Designer.GetColors(colorSchema)
        End Sub

        ''' <summary>
        ''' Create profile with custom color set
        ''' </summary>
        ''' <param name="colors"></param>
        Sub New(colors As IEnumerable(Of Color))
            If Not colors Is Nothing Then
                Me.colors = colors.ToArray
            End If
        End Sub

        Public MustOverride Function GetColor(item As NamedValue(Of Double)) As Color

        ''' <summary>
        ''' view of the colors vector for mapping
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return colors.Select(Function(c) c.ToHtmlColor).GetJson
        End Function

    End Class

End Namespace
