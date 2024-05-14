#Region "Microsoft.VisualBasic::0acd3b5596f83e19e475b58fbb0a4f8c, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SvgPath\Commands\Q.vb"

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

    '   Total Lines: 39
    '    Code Lines: 32
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.31 KB


    '     Class Q
    ' 
    '         Properties: X, X1, Y, Y1
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: MapTokens, Scale, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVG.PathHelper

    Public Class Q : Inherits Command
        Public Property X1 As Double
        Public Property Y1 As Double
        Public Property X As Double
        Public Property Y As Double

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
            X1 = Double.Parse(tokens(0))
            Y1 = Double.Parse(tokens(1))
            X = Double.Parse(tokens(2))
            Y = Double.Parse(tokens(3))
        End Sub

        Public Overrides Sub Scale(factor As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "q"c, "Q"c)}{X1},{Y1} {X},{Y}"
        End Function
    End Class
End Namespace
