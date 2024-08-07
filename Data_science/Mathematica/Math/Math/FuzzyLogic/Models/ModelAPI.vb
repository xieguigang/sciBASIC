﻿#Region "Microsoft.VisualBasic::8112eee074f18744d99ecd62c70917bb, Data_science\Mathematica\Math\Math\FuzzyLogic\Models\ModelAPI.vb"

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

    '   Total Lines: 74
    '    Code Lines: 56 (75.68%)
    ' Comment Lines: 3 (4.05%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (20.27%)
    '     File Size: 2.92 KB


    '     Class FuzzyModel
    ' 
    '         Properties: Defuzzify, Fuzzify, Input, Output, Rules
    ' 
    '         Function: FromXml, Load, (+3 Overloads) Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
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
            Using file As Stream = FilePath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Return Save(file, Encoding)
            End Using
        End Function

        Public Function Save(s As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using wr As New StreamWriter(s, encoding)
                Call wr.WriteLine(Me.GetXml)
                Call wr.Flush()
            End Using

            Return True
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
