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

Namespace FuzzyLogic

    ''' <summary>
    ''' Represents a membership function.
    ''' </summary>
    Public Class MembershipFunction
#Region "Private Properties"

        Private m_name As String = [String].Empty
        Private m_x0 As Double = 0
        Private m_x1 As Double = 0
        Private m_x2 As Double = 0
        Private m_x3 As Double = 0
        Private m_value As Double = 0

#End Region

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
            Get
                Return m_name
            End Get
            Set
                m_name = m_value
            End Set
        End Property

        ''' <summary>
        ''' The value of the (x0, 0) point.
        ''' </summary>
        Public Property X0() As Double
            Get
                Return m_x0
            End Get
            Set
                m_x0 = m_value
            End Set
        End Property

        ''' <summary>
        ''' The value of the (x1, 1) point.
        ''' </summary>
        Public Property X1() As Double
            Get
                Return m_x1
            End Get
            Set
                m_x1 = m_value
            End Set
        End Property

        ''' <summary>
        ''' The value of the (x2, 1) point.
        ''' </summary>
        Public Property X2() As Double
            Get
                Return m_x2
            End Get
            Set
                m_x2 = m_value
            End Set
        End Property

        ''' <summary>
        ''' The value of the (x3, 0) point.
        ''' </summary>
        Public Property X3() As Double
            Get
                Return m_x3
            End Get
            Set
                m_x3 = m_value
            End Set
        End Property

        ''' <summary>
        ''' The value of membership function after evaluation process.
        ''' </summary>
        Public Property Value() As Double
            Get
                Return m_value
            End Get
            Set
                Me.m_value = m_value
            End Set
        End Property

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Calculate the centroid of a trapezoidal membership function.
        ''' </summary>
        ''' <returns>The value of centroid.</returns>
        Public Function Centorid() As Double
            Dim a As Double = Me.m_x2 - Me.m_x1
            Dim b As Double = Me.m_x3 - Me.m_x0
            Dim c As Double = Me.m_x1 - Me.m_x0

            Return ((2 * a * c) + (a * a) + (c * b) + (a * b) + (b * b)) / (3 * (a + b)) + Me.m_x0
        End Function

        ''' <summary>
        ''' Calculate the area of a trapezoidal membership function.
        ''' </summary>
        ''' <returns>The value of area.</returns>
        Public Function Area() As Double
            Dim a As Double = Me.Centorid() - Me.m_x0
            Dim b As Double = Me.m_x3 - Me.m_x0

            Return (Me.m_value * (b + (b - (a * Me.m_value)))) / 2
        End Function

#End Region
    End Class
End Namespace