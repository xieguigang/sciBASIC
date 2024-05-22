#Region "Microsoft.VisualBasic::c8e97cfbfb4575e09b64aa4c693c94b8, mime\application%pdf\PdfFileWriter\Graphics\Geometry.vb"

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

    '   Total Lines: 420
    '    Code Lines: 145 (34.52%)
    ' Comment Lines: 235 (55.95%)
    '    - Xml Docs: 84.26%
    ' 
    '   Blank Lines: 40 (9.52%)
    '     File Size: 13.38 KB


    ' Class PointD
    ' 
    '     Properties: X, Y
    ' 
    '     Constructor: (+5 Overloads) Sub New
    ' 
    ' Class SizeD
    ' 
    '     Properties: Height, Width
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    ' Class LineD
    ' 
    '     Properties: DX, DY, Length, P1, P2
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    ' Class BezierD
    ' 
    '     Properties: P1, P2, P3, P4
    ' 
    '     Constructor: (+4 Overloads) Sub New
    '     Function: CircleFirstQuarter, CircleFourthQuarter, CircleSecondQuarter, CircleThirdQuarter, OvalFirstQuarter
    '               OvalFourthQuarter, OvalSecondQuarter, OvalThirdQuarter
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	Geometry
'	double precision drawing support functions.
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports stdNum = System.Math

''' <summary>
''' Point in double precision class
''' </summary>
Public Class PointD
    ''' <summary>
    ''' Gets or sets X
    ''' </summary>
    Public Property X As Double

    ''' <summary>
    ''' Gets or sets Y
    ''' </summary>
    Public Property Y As Double

    ''' <summary>
    ''' PointD copy constructor
    ''' </summary>
    ''' <param name="Other">Other point</param>
    Public Sub New(Other As PointD)
        X = Other.X
        Y = Other.Y
        Return
    End Sub

    ''' <summary>
    ''' PointD constructor
    ''' </summary>
    ''' <param name="X">X</param>
    ''' <param name="Y">Y</param>
    Public Sub New(X As Double, Y As Double)
        Me.X = X
        Me.Y = Y
        Return
    End Sub

    ''' <summary>
    ''' PointD constructor
    ''' </summary>
    ''' <param name="Center">Center point</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="Alpha">Angle</param>
    Public Sub New(Center As PointD, Radius As Double, Alpha As Double)
        X = Center.X + Radius * stdNum.Cos(Alpha)
        Y = Center.Y + Radius * stdNum.Sin(Alpha)
        Return
    End Sub

    ''' <summary>
    ''' PointD constructor
    ''' </summary>
    ''' <param name="CenterX">Center X</param>
    ''' <param name="CenterY">Center Y</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="Alpha">Angle</param>
    Public Sub New(CenterX As Double, CenterY As Double, Radius As Double, Alpha As Double)
        X = CenterX + Radius * stdNum.Cos(Alpha)
        Y = CenterY + Radius * stdNum.Sin(Alpha)
        Return
    End Sub

    ''' <summary>
    ''' PointD constructor
    ''' </summary>
    ''' <param name="L1">Line 1</param>
    ''' <param name="L2">Line 2</param>
    Public Sub New(L1 As LineD, L2 As LineD)
        Dim Denom = L1.DX * L2.DY - L1.DY * L2.DX

        If Denom = 0.0 Then
            X = Double.NaN
            Y = Double.NaN
            Return
        End If

        Dim L1DXY = L1.P2.X * L1.P1.Y - L1.P2.Y * L1.P1.X
        Dim L2DXY = L2.P2.X * L2.P1.Y - L2.P2.Y * L2.P1.X
        X = (L1DXY * L2.DX - L2DXY * L1.DX) / Denom
        Y = (L1DXY * L2.DY - L2DXY * L1.DY) / Denom
        Return
    End Sub
End Class

''' <summary>
''' Size in double precision class
''' </summary>
Public Class SizeD
    ''' <summary>
    ''' Width
    ''' </summary>
    Public Property Width As Double

    ''' <summary>
    ''' Height
    ''' </summary>
    Public Property Height As Double

    ''' <summary>
    ''' Default constructor
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' SizeD constructor
    ''' </summary>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    Public Sub New(Width As Double, Height As Double)
        Me.Width = Width
        Me.Height = Height
        Return
    End Sub
End Class

''' <summary>
''' Line in double precision class
''' </summary>
Public Class LineD
    ''' <summary>
    ''' Gets or sets point 1
    ''' </summary>
    Public Property P1 As PointD

    ''' <summary>
    ''' Gets or sets point 2
    ''' </summary>
    Public Property P2 As PointD

    ''' <summary>
    ''' LineD constructor (two points)
    ''' </summary>
    ''' <param name="P1">Point 1</param>
    ''' <param name="P2">Point 2</param>
    Public Sub New(P1 As PointD, P2 As PointD)
        Me.P1 = P1
        Me.P2 = P2
        Return
    End Sub

    ''' <summary>
    ''' LineD constructor (coordinates)
    ''' </summary>
    ''' <param name="X1">Point1 X</param>
    ''' <param name="Y1">Point1 Y</param>
    ''' <param name="X2">Point2 X</param>
    ''' <param name="Y2">Point2 Y</param>
    Public Sub New(X1 As Double, Y1 As Double, X2 As Double, Y2 As Double)
        P1 = New PointD(X1, Y1)
        P2 = New PointD(X2, Y2)
        Return
    End Sub

    ''' <summary>
    ''' Delta X
    ''' </summary>
    Public ReadOnly Property DX As Double
        Get
            Return P2.X - P1.X
        End Get
    End Property

    ''' <summary>
    ''' Delta Y
    ''' </summary>
    Public ReadOnly Property DY As Double
        Get
            Return P2.Y - P1.Y
        End Get
    End Property

    ''' <summary>
    ''' Line length
    ''' </summary>
    Public ReadOnly Property Length As Double
        Get
            Return stdNum.Sqrt(DX * DX + DY * DY)
        End Get
    End Property
End Class

'
' Bezier in double precision
'

''' <summary>
''' Bezier curve class
''' </summary>
''' <remarks>
''' All dimensions are in double precision.
''' </remarks>
Public Class BezierD
    ''' <summary>
    ''' Bezier P1
    ''' </summary>
    Public Property P1 As PointD

    ''' <summary>
    ''' Bezier P2
    ''' </summary>
    Public Property P2 As PointD
    ''' <summary>
    ''' Bezier P3
    ''' </summary>
    Public Property P3 As PointD
    ''' <summary>
    ''' Bezier P4
    ''' </summary>
    Public Property P4 As PointD

    ''' <summary>
    ''' Circle factor
    ''' </summary>
    ''' <remarks>The circle factor makes Bezier curve to look like a circle.</remarks>
    Private Shared CircleFactor As Double = (stdNum.Sqrt(2.0) - 1) / 0.75

    ''' <summary>
    ''' Bezier constructor
    ''' </summary>
    ''' <param name="P1">P1</param>
    ''' <param name="P2">P2</param>
    ''' <param name="P3">P3</param>
    ''' <param name="P4">P4</param>
    Public Sub New(P1 As PointD, P2 As PointD, P3 As PointD, P4 As PointD)
        Me.P1 = P1
        Me.P2 = P2
        Me.P3 = P3
        Me.P4 = P4
        Return
    End Sub

    ''' <summary>
    ''' Bezier constructor
    ''' </summary>
    ''' <param name="X1">P1-X</param>
    ''' <param name="Y1">P1-Y</param>
    ''' <param name="X2">P2-X</param>
    ''' <param name="Y2">P2-Y</param>
    ''' <param name="X3">P3-X</param>
    ''' <param name="Y3">P3-Y</param>
    ''' <param name="X4">P4-X</param>
    ''' <param name="Y4">P4-Y</param>
    Public Sub New(X1 As Double, Y1 As Double, X2 As Double, Y2 As Double, X3 As Double, Y3 As Double, X4 As Double, Y4 As Double)
        P1 = New PointD(X1, Y1)
        P2 = New PointD(X2, Y2)
        P3 = New PointD(X3, Y3)
        P4 = New PointD(X4, Y4)
        Return
    End Sub

    ''' <summary>
    ''' Bezier constructor
    ''' </summary>
    ''' <param name="P1">P1</param>
    ''' <param name="Factor2">Factor2</param>
    ''' <param name="Alpha2">Alpha2</param>
    ''' <param name="Factor3">Factor3</param>
    ''' <param name="Alpha3">Alpha3</param>
    ''' <param name="P4">P4</param>
    Public Sub New(P1 As PointD, Factor2 As Double, Alpha2 As Double, Factor3 As Double, Alpha3 As Double, P4 As PointD)
        ' save two end points
        Me.P1 = P1
        Me.P4 = P4

        ' distance between end points
        Dim Line As LineD = New LineD(P1, P4)
        Dim Length = Line.Length

        If Length = 0 Then
            P2 = P1
            P3 = P4
            Return
        End If

        ' angle of line between end points
        Dim Alpha = stdNum.Atan2(Line.DY, Line.DX)
        P2 = New PointD(P1, Factor2 * Length, Alpha + Alpha2)
        P3 = New PointD(P4, Factor3 * Length, Alpha + Alpha3)
        Return
    End Sub

    ''' <summary>
    ''' BezierD constructor from quadratic bezier points
    ''' </summary>
    ''' <param name="QP1">Quadratic Bezier point 1</param>
    ''' <param name="QP2">Quadratic Bezier point 2</param>
    ''' <param name="QP3">Quadratic Bezier point 3</param>
    Public Sub New(QP1 As PointD, QP2 As PointD, QP3 As PointD)
        '	Any quadratic spline can be expressed as a cubic (where the cubic term is zero).
        '	The end points of the cubic will be the same as the quadratic's.
        '	CP1 = QP1
        '	CP4 = QP3
        '	The two control points for the cubic are:
        '	CP2 = QP1 + 2/3 *(QP2-QP1)
        '	CP3 = QP3 + 2/3 *(QP2-QP3)
        P1 = New PointD(QP1)
        P2 = New PointD(QP1.X + 2 * (QP2.X - QP1.X) / 3, QP1.Y + 2 * (QP2.Y - QP1.Y) / 3)
        P3 = New PointD(QP3.X + 2 * (QP2.X - QP3.X) / 3, QP3.Y + 2 * (QP2.Y - QP3.Y) / 3)
        P4 = New PointD(QP3)
        Return
    End Sub

    ''' <summary>
    ''' Bezier first quarter circle
    ''' </summary>
    ''' <param name="X">Center X</param>
    ''' <param name="Y">Center Y</param>
    ''' <param name="Radius">Radius</param>
    ''' <returns>Bezier curve</returns>
    Public Shared Function CircleFirstQuarter(X As Double, Y As Double, Radius As Double) As BezierD
        Return New BezierD(X + Radius, Y, X + Radius, Y + CircleFactor * Radius, X + CircleFactor * Radius, Y + Radius, X, Y + Radius)
    End Function

    ''' <summary>
    ''' Bezier second quarter circle
    ''' </summary>
    ''' <param name="X">Center X</param>
    ''' <param name="Y">Center Y</param>
    ''' <param name="Radius">Radius</param>
    ''' <returns>Bezier curve</returns>
    Public Shared Function CircleSecondQuarter(X As Double, Y As Double, Radius As Double) As BezierD
        Return New BezierD(X, Y + Radius, X - CircleFactor * Radius, Y + Radius, X - Radius, Y + CircleFactor * Radius, X - Radius, Y)
    End Function

    ''' <summary>
    ''' Bezier third quarter circle
    ''' </summary>
    ''' <param name="X">Center X</param>
    ''' <param name="Y">Center Y</param>
    ''' <param name="Radius">Radius</param>
    ''' <returns>Bezier curve</returns>
    Public Shared Function CircleThirdQuarter(X As Double, Y As Double, Radius As Double) As BezierD
        Return New BezierD(X - Radius, Y, X - Radius, Y - CircleFactor * Radius, X - CircleFactor * Radius, Y - Radius, X, Y - Radius)
    End Function

    ''' <summary>
    ''' Bezier fourth quarter circle
    ''' </summary>
    ''' <param name="X">Center X</param>
    ''' <param name="Y">Center Y</param>
    ''' <param name="Radius">Radius</param>
    ''' <returns>Bezier curve</returns>
    Public Shared Function CircleFourthQuarter(X As Double, Y As Double, Radius As Double) As BezierD
        Return New BezierD(X, Y - Radius, X + CircleFactor * Radius, Y - Radius, X + Radius, Y - CircleFactor * Radius, X + Radius, Y)
    End Function

    ''' <summary>
    ''' Oval first quarter
    ''' </summary>
    ''' <param name="X">Center X</param>
    ''' <param name="Y">Center Y</param>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    ''' <returns>Bezier curve</returns>
    Public Shared Function OvalFirstQuarter(X As Double, Y As Double, Width As Double, Height As Double) As BezierD
        Return New BezierD(X + Width, Y, X + Width, Y + CircleFactor * Height, X + CircleFactor * Width, Y + Height, X, Y + Height)
    End Function

    ''' <summary>
    ''' Oval second quarter
    ''' </summary>
    ''' <param name="X">Center X</param>
    ''' <param name="Y">Center Y</param>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    ''' <returns>Bezier curve</returns>
    Public Shared Function OvalSecondQuarter(X As Double, Y As Double, Width As Double, Height As Double) As BezierD
        Return New BezierD(X, Y + Height, X - CircleFactor * Width, Y + Height, X - Width, Y + CircleFactor * Height, X - Width, Y)
    End Function

    ''' <summary>
    ''' Oval third quarter
    ''' </summary>
    ''' <param name="X">Center X</param>
    ''' <param name="Y">Center Y</param>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    ''' <returns>Bezier curve</returns>
    Public Shared Function OvalThirdQuarter(X As Double, Y As Double, Width As Double, Height As Double) As BezierD
        Return New BezierD(X - Width, Y, X - Width, Y - CircleFactor * Height, X - CircleFactor * Width, Y - Height, X, Y - Height)
    End Function

    ''' <summary>
    ''' Oval fourth quarter circle
    ''' </summary>
    ''' <param name="X">Center X</param>
    ''' <param name="Y">Center Y</param>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    ''' <returns>Bezier curve</returns>
    Public Shared Function OvalFourthQuarter(X As Double, Y As Double, Width As Double, Height As Double) As BezierD
        Return New BezierD(X, Y - Height, X + CircleFactor * Width, Y - Height, X + Width, Y - CircleFactor * Height, X + Width, Y)
    End Function
End Class
