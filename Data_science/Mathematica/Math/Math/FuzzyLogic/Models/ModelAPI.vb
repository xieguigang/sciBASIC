#Region "Microsoft.VisualBasic::39895d29fd09a958b135ec3c2e0d5745, Data_science\Mathematica\Math\Math\FuzzyLogic\Models\ModelAPI.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class FuzzyModel
    ' 
    '         Properties: Defuzzify, Fuzzify, Input, Output, Rules
    ' 
    '         Function: FromXml, Load, (+2 Overloads) Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text

Namespace Logical.FuzzyLogic.Models

    Public Class FuzzyModel : Implements ISaveHandle

        <XmlElement> Public Property Input As Value()
        Public Property Output As Value
        <XmlElement> Public Property Fuzzify As Fuzzify()
        Public Property Defuzzify As Defuzzify
        Public Property Rules As RuleBlock

        Public Function Save(FilePath As String, Encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Return Me.GetXml.SaveTo(FilePath, Encoding)
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

        Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(path, encoding.CodePage)
        End Function
    End Class
End Namespace
