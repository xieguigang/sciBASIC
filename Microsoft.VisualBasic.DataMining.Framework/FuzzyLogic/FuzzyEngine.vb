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
Imports System.Xml
Imports Microsoft.VisualBasic.Linq

Namespace FuzzyLogic

    ''' <summary>
    ''' Represents the inferential engine.
    ''' </summary>
    Public Class FuzzyEngine

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

        Private Sub ReadVariable(xmlNode As XmlNode)
            Dim linguisticVariable As LinguisticVariable = Me._LinguisticVariableCollection.Find(xmlNode.Attributes("NAME").InnerText)

            For Each termNode As XmlNode In xmlNode.ChildNodes
                Dim points As String() = termNode.Attributes("POINTS").InnerText.Split()
                linguisticVariable.MembershipFunctionCollection.Add(New MembershipFunction(termNode.Attributes("NAME").InnerText, Convert.ToDouble(points(0)), Convert.ToDouble(points(1)), Convert.ToDouble(points(2)), Convert.ToDouble(points(3))))
            Next
        End Sub

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
        ''' <param name="path">Path of the destination document.</param>
        Public Sub Save(path As String)
            Dim model As New Models.FuzzyModel

            model.Output = New Models.Value(Me.LinguisticVariableCollection.Find(Consequent))
            model.Input = (From x In Me.LinguisticVariableCollection
                           Where Not String.Equals(x.Key, Consequent, StringComparison.Ordinal)
                           Select x).ToArray(Function(x) New Models.Value(x))
            model.Defuzzify = New Models.Defuzzify(Me.LinguisticVariableCollection.Find(Consequent))

            For Each linguisticVariable As LinguisticVariable In Me._LinguisticVariableCollection
                If linguisticVariable.Name = Me._Consequent Then
                    xmlTextWriter.WriteStartElement("DEFUZZIFY")
                    xmlTextWriter.WriteAttributeString("METHOD", "CoG")
                    xmlTextWriter.WriteAttributeString("ACCU", "MAX")
                Else
                    xmlTextWriter.WriteStartElement("FUZZIFY")
                End If

                xmlTextWriter.WriteAttributeString("NAME", linguisticVariable.Name)

                For Each membershipFunction As MembershipFunction In linguisticVariable.MembershipFunctionCollection
                    xmlTextWriter.WriteStartElement("TERM")
                    xmlTextWriter.WriteAttributeString("NAME", membershipFunction.Name)
                    xmlTextWriter.WriteAttributeString("POINTS", membershipFunction.X0 & " " & membershipFunction.X1 & " " & membershipFunction.X2 & " " & membershipFunction.X3)
                    xmlTextWriter.WriteEndElement()
                Next

                xmlTextWriter.WriteEndElement()
            Next

            xmlTextWriter.WriteStartElement("RULEBLOCK")
            xmlTextWriter.WriteAttributeString("AND", "MIN")
            xmlTextWriter.WriteAttributeString("OR", "MAX")

            For Each fuzzyRule As FuzzyRule In Me._FuzzyRuleCollection
                i += 1
                xmlTextWriter.WriteStartElement("RULE")
                xmlTextWriter.WriteAttributeString("NUMBER", i.ToString())
                xmlTextWriter.WriteAttributeString("TEXT", fuzzyRule.Text)
                xmlTextWriter.WriteEndElement()
            Next

            xmlTextWriter.WriteEndElement()

            xmlTextWriter.WriteEndElement()
            xmlTextWriter.WriteEndDocument()
            xmlTextWriter.Close()
        End Sub

        ''' <summary>
        ''' Sets the FilePath property and loads a project from a FCL-like XML file.
        ''' </summary>
        ''' <param name="path">Path of the source file.</param>
        Public Sub Load(path As String)
            Dim xmlDocument As New XmlDocument()
            xmlDocument.Load(Me._FilePath)

            For Each xmlNode As XmlNode In xmlDocument.GetElementsByTagName("VAR_INPUT")
                Me.LinguisticVariableCollection.Add(New LinguisticVariable(xmlNode.Attributes("NAME").InnerText))
            Next

            Me._Consequent = xmlDocument.GetElementsByTagName("VAR_OUTPUT")(0).Attributes("NAME").InnerText
            Me.LinguisticVariableCollection.Add(New LinguisticVariable(Me._Consequent))

            For Each xmlNode As XmlNode In xmlDocument.GetElementsByTagName("FUZZIFY")
                ReadVariable(xmlNode)
            Next

            ReadVariable(xmlDocument.GetElementsByTagName("DEFUZZIFY")(0))

            For Each xmlNode As XmlNode In xmlDocument.GetElementsByTagName("RULE")
                Me._FuzzyRuleCollection.Add(New FuzzyRule(xmlNode.Attributes("TEXT").InnerText))
            Next
        End Sub

#End Region
    End Class
End Namespace