#Region "Microsoft.VisualBasic::8cb246e90e164a052216fd40b107d4a1, ..\sciBASIC#\Data_science\Mathematica\Math\Math\FuzzyLogic\Models\ModelAPI.vb"

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

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel

Namespace Logical.FuzzyLogic.Models

    Public Class FuzzyModel : Inherits ITextFile

        <XmlElement> Public Property Input As Value()
        Public Property Output As Value
        <XmlElement> Public Property Fuzzify As Fuzzify()
        Public Property Defuzzify As Defuzzify
        Public Property Rules As RuleBlock

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Return Me.GetXml.SaveTo(getPath(FilePath), Encoding)
        End Function

        ''' <summary>
        ''' Sets the FilePath property and loads a project from a FCL-like XML file.
        ''' </summary>
        Public Function Load() As FuzzyEngine
            Dim engine As New FuzzyEngine

            For Each node As Value In Input
                engine.Add(New LinguisticVariable(node.Identifier))
            Next

            engine.Consequent = Output.Identifier
            engine.Add(New LinguisticVariable(engine.Consequent))

            For Each node As Models.Fuzzify In Fuzzify.Join(Defuzzify)
                Dim linguisticVariable As LinguisticVariable =
                    engine.LinguisticVariableCollection ^ node.Identifier

                For Each termNode In node.Terms
                    Dim msf As New MembershipFunction(termNode.Identifier,
                                                      termNode.Points(0),
                                                      termNode.Points(1),
                                                      termNode.Points(2),
                                                      termNode.Points(3))
                    linguisticVariable.MembershipFunctionCollection.Add(msf)
                Next
            Next

            For Each node As Rule In Rules.Rules
                engine.Add(New FuzzyRule(node.Expression))
            Next

            Return engine
        End Function

        Public Shared Function FromXml(path As String) As FuzzyEngine
            Return path.LoadXml(Of FuzzyModel).Load
        End Function
    End Class
End Namespace
