#Region "Microsoft.VisualBasic::681a76c1690369ebf0f0cb2e3c36312d, ..\sciBASIC#\Data_science\Mathematica\Math\Math\FuzzyLogic\LinguisticVariable.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace Logical.FuzzyLogic

    ''' <summary>
    ''' Represents a linguistic variable.
    ''' </summary>
    Public Class LinguisticVariable : Inherits BaseClass
        Implements INamedValue

#Region "Constructors"

        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        Public Sub New()
            MembershipFunctionCollection = New MembershipFunctionCollection
        End Sub

        ''' <param name="name">The name that identificates the linguistic variable.</param>
        Public Sub New(name As String)
            Call Me.New
            Me.Name = name
        End Sub

        ''' <param name="name">The name that identificates the linguistic variable.</param>
        ''' <param name="membershipFunctionCollection">A membership functions collection for the lingusitic variable.</param>
        Public Sub New(name As String, membershipFunctionCollection As MembershipFunctionCollection)
            Me.Name = name
            Me.MembershipFunctionCollection = membershipFunctionCollection
        End Sub

#End Region

#Region "Public Properties"

        ''' <summary>
        ''' The name that identificates the linguistic variable.
        ''' </summary>
        Public Property Name() As String Implements INamedValue.Key

        ''' <summary>
        ''' A membership functions collection for the lingusitic variable.
        ''' </summary>
        Public Property MembershipFunctionCollection() As MembershipFunctionCollection

        ''' <summary>
        ''' The input value for the linguistic variable.
        ''' </summary>
        Public Property InputValue() As Double
#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Implements the fuzzification of the linguistic variable.
        ''' </summary>
        ''' <param name="membershipFunctionName">The membership function for which fuzzify the variable.</param>
        ''' <returns>The degree of membership.</returns>
        Public Function Fuzzify(membershipFunctionName As String) As Double
            Dim membershipFunction As MembershipFunction = Me._MembershipFunctionCollection.Find(membershipFunctionName)

            If (membershipFunction.X0 <= Me.InputValue) AndAlso (Me.InputValue < membershipFunction.X1) Then
                Return (Me.InputValue - membershipFunction.X0) / (membershipFunction.X1 - membershipFunction.X0)
            ElseIf (membershipFunction.X1 <= Me.InputValue) AndAlso (Me.InputValue <= membershipFunction.X2) Then
                Return 1
            ElseIf (membershipFunction.X2 < Me.InputValue) AndAlso (Me.InputValue <= membershipFunction.X3) Then
                Return (membershipFunction.X3 - Me.InputValue) / (membershipFunction.X3 - membershipFunction.X2)
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Returns the minimum value of the linguistic variable.
        ''' </summary>
        ''' <returns>The minimum value of the linguistic variable.</returns>
        Public Function MinValue() As Double
            Dim minValue__1 As Double = Me._MembershipFunctionCollection(0).X0

            For i As Integer = 1 To Me._MembershipFunctionCollection.Count - 1
                If Me._MembershipFunctionCollection(i).X0 < minValue__1 Then
                    minValue__1 = Me._MembershipFunctionCollection(i).X0
                End If
            Next

            Return minValue__1
        End Function

        ''' <summary>
        ''' Returns the maximum value of the linguistic variable.
        ''' </summary>
        ''' <returns>The maximum value of the linguistic variable.</returns>
        Public Function MaxValue() As Double
            Dim maxValue__1 As Double = Me._MembershipFunctionCollection(0).X3

            For i As Integer = 1 To Me._MembershipFunctionCollection.Count - 1
                If Me._MembershipFunctionCollection(i).X3 > maxValue__1 Then
                    maxValue__1 = Me._MembershipFunctionCollection(i).X3
                End If
            Next

            Return maxValue__1
        End Function

        ''' <summary>
        ''' Returns the difference between MaxValue() and MinValue().
        ''' </summary>
        ''' <returns>The difference between MaxValue() and MinValue().</returns>
        Public Function Range() As Double
            Return Me.MaxValue() - Me.MinValue()
        End Function

#End Region
    End Class
End Namespace
