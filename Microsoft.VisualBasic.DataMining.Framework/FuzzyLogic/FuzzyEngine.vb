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

Namespace FuzzyLogic

    ''' <summary>
    ''' Represents the inferential engine.
    ''' </summary>
    Public Class FuzzyEngine
#Region "Private Properties"

        Private m_linguisticVariableCollection As New LinguisticVariableCollection()
        Private m_consequent As String = [String].Empty
        Private m_fuzzyRuleCollection As New FuzzyRuleCollection()
        Private m_filePath As String = [String].Empty

#End Region

#Region "Private Methods"

        Private Function GetConsequent() As LinguisticVariable
            Return Me.m_linguisticVariableCollection.Find(Me.m_consequent)
        End Function

        Private Function Parse(text As String) As Double
            Dim counter As Integer = 0
            Dim firstMatch As Integer = 0

            If Not text.StartsWith("(") Then
                Dim tokens As String() = text.Split()
                Return Me.m_linguisticVariableCollection.Find(tokens(0)).Fuzzify(tokens(2))
            End If

            For i As Integer = 0 To text.Length - 1
                Select Case text(i)
                    Case "("c
                        counter += 1
                        If counter = 1 Then
                            firstMatch = i
                        End If
                        Exit Select

                    Case ")"c
                        counter -= 1
                        If (counter = 0) AndAlso (i > 0) Then
                            Dim substring As String = text.Substring(firstMatch + 1, i - firstMatch - 1)
                            Dim substringBrackets As String = text.Substring(firstMatch, i - firstMatch + 1)
                            Dim length As Integer = substringBrackets.Length
                            text = text.Replace(substringBrackets, Parse(substring).ToString())
                            i = i - (length - 1)
                        End If
                        Exit Select
                    Case Else

                        Exit Select
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
                        Exit Select

                    Case "OR"
                        If tokenValue > value Then
                            value = tokenValue
                        End If
                        Exit Select
                    Case Else

                        value = tokenValue
                        Exit Select
                End Select

                If (i + 1) < tokens.Length Then
                    connective = tokens(i + 1)
                End If
                i = i + 2
            End While

            Return value
        End Function

        Private Sub ReadVariable(xmlNode As XmlNode)
            Dim linguisticVariable As LinguisticVariable = Me.m_linguisticVariableCollection.Find(xmlNode.Attributes("NAME").InnerText)

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
            Get
                Return m_linguisticVariableCollection
            End Get
            Set
                m_linguisticVariableCollection = value
            End Set
        End Property

        ''' <summary>
        ''' The consequent variable name.
        ''' </summary>
        Public Property Consequent() As String
            Get
                Return m_consequent
            End Get
            Set
                m_consequent = value
            End Set
        End Property

        ''' <summary>
        ''' A collection of rules.
        ''' </summary>
        Public Property FuzzyRuleCollection() As FuzzyRuleCollection
            Get
                Return m_fuzzyRuleCollection
            End Get
            Set
                m_fuzzyRuleCollection = value
            End Set
        End Property

        ''' <summary>
        ''' The path of the FCL-like XML file in which save the project.
        ''' </summary>
        Public Property FilePath() As String
            Get
                Return m_filePath
            End Get
            Set
                m_filePath = value
            End Set
        End Property

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

            For Each fuzzyRule As FuzzyRule In Me.m_fuzzyRuleCollection
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
            Me.FilePath = path
            Me.Save()
        End Sub

        ''' <summary>
        ''' Saves the project into a FCL-like XML file.
        ''' </summary>
        Public Sub Save()
            If Me.m_filePath = [String].Empty Then
                Throw New Exception("FilePath not set")
            End If

            Dim i As Integer = 0
            Dim xmlTextWriter As New XmlTextWriter(Me.m_filePath, Encoding.UTF8)
            xmlTextWriter.Formatting = Formatting.Indented
            xmlTextWriter.WriteStartDocument(True)
            xmlTextWriter.WriteStartElement("FUNCTION_BLOCK")

            For Each linguisticVariable As LinguisticVariable In Me.m_linguisticVariableCollection
                If linguisticVariable.Name = Me.m_consequent Then
                    xmlTextWriter.WriteStartElement("VAR_OUTPUT")
                Else
                    xmlTextWriter.WriteStartElement("VAR_INPUT")
                End If

                xmlTextWriter.WriteAttributeString("NAME", linguisticVariable.Name)
                xmlTextWriter.WriteAttributeString("TYPE", "REAL")
                xmlTextWriter.WriteAttributeString("RANGE", linguisticVariable.MinValue().ToString() & " " & linguisticVariable.MaxValue().ToString())
                xmlTextWriter.WriteEndElement()
            Next

            For Each linguisticVariable As LinguisticVariable In Me.m_linguisticVariableCollection
                If linguisticVariable.Name = Me.m_consequent Then
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

            For Each fuzzyRule As FuzzyRule In Me.m_fuzzyRuleCollection
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
            Me.FilePath = path
            Me.Load()
        End Sub

        ''' <summary>
        ''' Loads a project from a FCL-like XML file.
        ''' </summary>
        Public Sub Load()
            If Me.m_filePath = [String].Empty Then
                Throw New Exception("FilePath not set")
            End If

            Dim xmlDocument As New XmlDocument()
            xmlDocument.Load(Me.m_filePath)

            For Each xmlNode As XmlNode In xmlDocument.GetElementsByTagName("VAR_INPUT")
                Me.LinguisticVariableCollection.Add(New LinguisticVariable(xmlNode.Attributes("NAME").InnerText))
            Next

            Me.m_consequent = xmlDocument.GetElementsByTagName("VAR_OUTPUT")(0).Attributes("NAME").InnerText
            Me.LinguisticVariableCollection.Add(New LinguisticVariable(Me.m_consequent))

            For Each xmlNode As XmlNode In xmlDocument.GetElementsByTagName("FUZZIFY")
                ReadVariable(xmlNode)
            Next

            ReadVariable(xmlDocument.GetElementsByTagName("DEFUZZIFY")(0))

            For Each xmlNode As XmlNode In xmlDocument.GetElementsByTagName("RULE")
                Me.m_fuzzyRuleCollection.Add(New FuzzyRule(xmlNode.Attributes("TEXT").InnerText))
            Next
        End Sub

#End Region
    End Class
End Namespace