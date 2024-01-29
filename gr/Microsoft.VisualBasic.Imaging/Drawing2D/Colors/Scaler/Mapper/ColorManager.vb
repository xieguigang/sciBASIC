#Region "Microsoft.VisualBasic::3e3d9250e48d51b96de211eff0a7a5d0, sciBASIC#\Data_science\Visualization\Plots\g\ColorManager.vb"

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

    '   Total Lines: 99
    '    Code Lines: 70
    ' Comment Lines: 8
    '   Blank Lines: 21
    '     File Size: 3.56 KB


    '     Class ColorProfile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class CategoryColorProfile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) GetColor
    ' 
    '     Class ValueScaleColorProfile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetColor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors.Scaler

    Public MustInherit Class ColorProfile

        Protected colors As Color()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(colorSchema As String)
            Me.colors = Designer.GetColors(colorSchema)
        End Sub

        Sub New(colors As IEnumerable(Of Color))
            Me.colors = colors.ToArray
        End Sub

        Public MustOverride Function GetColor(item As NamedValue(Of Double)) As Color

        Public Overrides Function ToString() As String
            Return colors.Select(Function(c) c.ToHtmlColor).GetJson
        End Function

    End Class

End Namespace
