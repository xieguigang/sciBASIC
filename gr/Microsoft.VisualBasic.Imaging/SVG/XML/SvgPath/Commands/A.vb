#Region "Microsoft.VisualBasic::b6eb8c3cc3f12b600a804ff043169463, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SvgPath\Commands\A.vb"

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
    '    Code Lines: 53
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 2.17 KB


    '     Class A
    ' 
    '         Properties: Large, Rx, Ry, Sweep, X
    '                     XRot, Y
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

    Public Class A : Inherits Command
        Public Property Rx As Double
        Public Property Ry As Double
        Public Property XRot As Double
        Public Property Large As Boolean
        Public Property Sweep As Boolean
        Public Property X As Double
        Public Property Y As Double

        Public Sub New(rx As Double, ry As Double, xRot As Double, large As Boolean, sweep As Boolean, x As Double, y As Double)
            Me.Rx = rx
            Me.Ry = ry
            Me.XRot = xRot
            Me.Large = large
            Me.Sweep = sweep
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
            If (tokens.Count = 7) Then
                Me.Rx = Double.Parse(tokens(0))
                Me.Ry = Double.Parse(tokens(1))
                Me.XRot = Double.Parse(tokens(2))
                Me.Large = System.Convert.ToBoolean(Integer.Parse(tokens(3)))
                Me.Sweep = System.Convert.ToBoolean(Integer.Parse(tokens(4)))
                Me.X = Double.Parse(tokens(5))
                Me.Y = Double.Parse(tokens(6))
            End If
        End Sub


        Public Overrides Sub Scale(factor As Double)
            Rx *= factor
            Ry *= factor
            X *= factor
            Y *= factor
        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)
            X += deltaX
            Y += deltaY
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "a"c, "A"c)}{Rx},{Ry} {XRot} {Convert.ToInt32(Large)},{Convert.ToInt32(Sweep)} {X},{Y}"
        End Function

    End Class
End Namespace
