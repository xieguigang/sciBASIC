#Region "Microsoft.VisualBasic::3368ebe9b7c4bdcd8d69e3c4f3fd02ff, Data_science\Mathematica\Math\Math\FuzzyLogic\MembershipFunction.vb"

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

    '   Total Lines: 121
    '    Code Lines: 44
    ' Comment Lines: 54
    '   Blank Lines: 23
    '     File Size: 3.82 KB


    '     Class MembershipFunction
    ' 
    '         Properties: Name, Value, X0, X1, X2
    '                     X3
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Area, Centorid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "GNU Lesser General Public License"
'
'This file is part of DotFuzzy.
'
'DotFuzzy is free software: you can redistribute it and/or modify
'it under the terms of the GNU Lesser General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'DotFuzzy is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU Lesser General Public License for more details.
'
'You should have received a copy of the GNU Lesser General Public License
'along with DotFuzzy.  If not, see <http://www.gnu.org/licenses/>.
'

#End Region

Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.Language

Namespace Logical.FuzzyLogic

    ''' <summary>
    ''' Represents a membership function.
    ''' </summary>
    Public Class MembershipFunction

#Region "Constructors"

        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <param name="name">The name that identificates the membership function.</param>
        Public Sub New(name As String)
            Me.Name = name
        End Sub

        ''' <param name="name">The name that identificates the linguistic variable.</param>
        ''' <param name="x0">The value of the (x0, 0) point.</param>
        ''' <param name="x1">The value of the (x1, 1) point.</param>
        ''' <param name="x2">The value of the (x2, 1) point.</param>
        ''' <param name="x3">The value of the (x3, 0) point.</param>
        Public Sub New(name As String, x0 As Double, x1 As Double, x2 As Double, x3 As Double)
            Me.Name = name
            Me.X0 = x0
            Me.X1 = x1
            Me.X2 = x2
            Me.X3 = x3
        End Sub

#End Region

#Region "Public Properties"

        ''' <summary>
        ''' The name that identificates the membership function.
        ''' </summary>
        Public Property Name() As String

        ''' <summary>
        ''' The value of the (x0, 0) point.
        ''' </summary>
        Public Property X0() As Double

        ''' <summary>
        ''' The value of the (x1, 1) point.
        ''' </summary>
        Public Property X1() As Double

        ''' <summary>
        ''' The value of the (x2, 1) point.
        ''' </summary>
        Public Property X2() As Double

        ''' <summary>
        ''' The value of the (x3, 0) point.
        ''' </summary>
        Public Property X3() As Double

        ''' <summary>
        ''' The value of membership function after evaluation process.
        ''' </summary>
        Public Property Value() As Double

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Calculate the centroid of a trapezoidal membership function.
        ''' </summary>
        ''' <returns>The value of centroid.</returns>
        Public Function Centorid() As Double
            Dim a As Double = Me._X2 - Me._X1
            Dim b As Double = Me._X3 - Me._X0
            Dim c As Double = Me._X1 - Me._X0

            Return ((2 * a * c) + (a * a) + (c * b) + (a * b) + (b * b)) / (3 * (a + b)) + Me._X0
        End Function

        ''' <summary>
        ''' Calculate the area of a trapezoidal membership function.
        ''' </summary>
        ''' <returns>The value of area.</returns>
        Public Function Area() As Double
            Dim a As Double = Me.Centorid() - Me._X0
            Dim b As Double = Me._X3 - Me._X0

            Return (Me._Value * (b + (b - (a * Me._Value)))) / 2
        End Function

#End Region
    End Class
End Namespace
