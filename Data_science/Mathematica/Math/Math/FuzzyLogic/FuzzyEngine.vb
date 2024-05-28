#Region "Microsoft.VisualBasic::c84e53207506bf084cf9d0566a105f87, Data_science\Mathematica\Math\Math\FuzzyLogic\FuzzyEngine.vb"

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

'   Total Lines: 214
'    Code Lines: 131 (61.21%)
' Comment Lines: 36 (16.82%)
'    - Xml Docs: 52.78%
' 
'   Blank Lines: 47 (21.96%)
'     File Size: 7.63 KB


'     Class FuzzyEngine
' 
'         Properties: Consequent, FuzzyRuleCollection, LinguisticVariableCollection
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: Defuzzify, Evaluate, GetConsequent, Parse, (+2 Overloads) Save
'                   ToModel
' 
'         Sub: (+2 Overloads) Add
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

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace Logical.FuzzyLogic

    ''' <summary>
    ''' Represents the inferential engine.
    ''' </summary>
    Public Class FuzzyEngine : Implements ISaveHandle

        Sub New()
            Me.LinguisticVariableCollection = New LinguisticVariableCollection
            Me.FuzzyRuleCollection = New FuzzyRuleCollection
        End Sub

#Region "Private Methods"

        Private Function GetConsequent() As LinguisticVariable
            Return Me._LinguisticVariableCollection.Find(Me._Consequent)
        End Function

        Private Function Parse(text As String) As Double
            Dim counter As Integer = 0
            Dim firstMatch As Integer = 0

            If Not text.StartsWith("(") Then
                Dim tokens As String() = text.Split()
                Return Me._LinguisticVariableCollection.Find(tokens(0)).Fuzzify(tokens(2))
            End If

            For i As Integer = 0 To text.Length - 1
                If i >= text.Length Then
                    Exit For
                End If

                Select Case text(i)
                    Case "("c
                        counter += 1
                        If counter = 1 Then
                            firstMatch = i
                        End If

                    Case ")"c
                        counter -= 1
                        If (counter = 0) AndAlso (i > 0) Then
                            Dim substring As String = text.Substring(firstMatch + 1, i - firstMatch - 1)
                            Dim substringBrackets As String = text.Substring(firstMatch, i - firstMatch + 1)
                            Dim length As Integer = substringBrackets.Length
                            text = text.Replace(substringBrackets, Parse(substring).ToString())
                            i = i - (length - 1)
                        End If

                    Case Else


                End Select
            Next

            Return Evaluate(text)
        End Function

        Private Function Evaluate(text As String) As Double
            Dim tokens As String() = text.Split()
            Dim connective As String = ""
            Dim value As Double = 0

            Dim i As Integer = 0
            While i <= ((tokens.Length \ 2) + 1)
                Dim tokenValue As Double = Convert.ToDouble(tokens(i))

                Select Case connective
                    Case "AND"
                        If tokenValue < value Then
                            value = tokenValue
                        End If


                    Case "OR"
                        If tokenValue > value Then
                            value = tokenValue
                        End If

                    Case Else

                        value = tokenValue

                End Select

                If (i + 1) < tokens.Length Then
                    connective = tokens(i + 1)
                End If
                i = i + 2
            End While

            Return value
        End Function

#End Region

#Region "Public Properties"

        ''' <summary>
        ''' A collection of linguistic variables.
        ''' </summary>
        Public Property LinguisticVariableCollection() As LinguisticVariableCollection

        ''' <summary>
        ''' The consequent variable name.
        ''' </summary>
        Public Property Consequent() As String

        ''' <summary>
        ''' A collection of rules.
        ''' </summary>
        Public Property FuzzyRuleCollection() As FuzzyRuleCollection

        Public Sub Add(x As LinguisticVariable)
            Call LinguisticVariableCollection.Add(x)
        End Sub

        Public Sub Add(x As FuzzyRule)
            Call FuzzyRuleCollection.Add(x)
        End Sub
#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Calculates the defuzzification value with the CoG (Center of Gravity) technique.
        ''' </summary>
        ''' <returns>The defuzzification value.</returns>
        Public Function Defuzzify() As Double
            Dim numerator As Double = 0
            Dim denominator As Double = 0

            ' Reset values
            For Each membershipFunction As MembershipFunction In Me.GetConsequent().MembershipFunctionCollection
                membershipFunction.Value = 0
            Next

            For Each fuzzyRule As FuzzyRule In Me._FuzzyRuleCollection
                fuzzyRule.Value = Parse(fuzzyRule.Conditions())

                Dim tokens As String() = fuzzyRule.Text.Split()
                Dim membershipFunction As MembershipFunction = Me.GetConsequent().MembershipFunctionCollection.Find(tokens(tokens.Length - 1))

                If fuzzyRule.Value > membershipFunction.Value Then
                    membershipFunction.Value = fuzzyRule.Value
                End If
            Next

            For Each membershipFunction As MembershipFunction In Me.GetConsequent().MembershipFunctionCollection
                numerator += membershipFunction.Centorid() * membershipFunction.Area()
                denominator += membershipFunction.Area()
            Next

            Return numerator / denominator
        End Function

        ''' <summary>
        ''' Sets the FilePath property and saves the project into a FCL-like XML file.
        ''' </summary>
        Public Function ToModel() As Models.FuzzyModel
            Dim conseq = Me.LinguisticVariableCollection.Find(Consequent)
            Dim trace = (From x In Me.LinguisticVariableCollection
                         Where Not String.Equals(x.Key, Consequent, StringComparison.Ordinal)
                         Select x.Value).ToArray

            Dim model As New Models.FuzzyModel With {
                .Output = New Models.Value(conseq),
                .Input = trace.Select(Function(x) New Models.Value(x)).ToArray,
                .Defuzzify = New Models.Defuzzify(conseq),
                .Fuzzify = trace.Select(Function(x) New Models.Fuzzify(x)).ToArray,
                .Rules = New Models.RuleBlock(FuzzyRuleCollection)
            }

            Return model
        End Function

        Public Function Save(Path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using file As Stream = Path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Return Save(file, encoding)
            End Using
        End Function

        Public Function Save(Path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.CodePage)
        End Function

        Public Function Save(s As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using wr As New StreamWriter(s, encoding)
                Call wr.WriteLine(ToModel.GetXml)
                Call wr.Flush()
            End Using

            Return True
        End Function
#End Region
    End Class
End Namespace
