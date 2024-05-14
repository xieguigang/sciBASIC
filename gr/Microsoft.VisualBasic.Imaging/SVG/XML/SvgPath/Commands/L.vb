#Region "Microsoft.VisualBasic::541e011a669f2e7a5967645c10811835, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SvgPath\Commands\L.vb"

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

    '   Total Lines: 43
    '    Code Lines: 34
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.25 KB


    '     Class L
    ' 
    '         Properties: X, Y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: MapTokens, Scale, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVG.PathHelper

    Public Class L : Inherits Command
        Public Property X As Double
        Public Property Y As Double

        Public Sub New(x As Double, y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        Public Sub New(text As String, Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
            Dim tokens = Parse(text)
            Me.MapTokens(tokens)
        End Sub

        Public Sub New(tokens As List(Of String), Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
            Me.MapTokens(tokens)
        End Sub

        Private Sub MapTokens(tokens As List(Of String))
            X = Double.Parse(tokens(0))
            Y = Double.Parse(tokens(1))
        End Sub

        Public Overrides Sub Scale(factor As Double)
            X *= factor
            Y *= factor
        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)
            X += deltaX
            Y += deltaY
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "l"c, "L"c)}{X} {Y}"
        End Function

    End Class
End Namespace
