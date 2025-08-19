#Region "Microsoft.VisualBasic::c98ef8615c153e31e0772ac20b50ea3b, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\struct\RuleSet.vb"

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

    '   Total Lines: 60
    '    Code Lines: 49 (81.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (18.33%)
    '     File Size: 2.10 KB


    '     Class RuleSet
    ' 
    '         Properties: size
    ' 
    '         Function: GenericEnumerator
    ' 
    '         Sub: load
    '         Class CSharpImpl
    ' 
    '             Function: __Assign
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace GraphEmbedding.struct

    Public Class RuleSet : Implements Enumeration(Of Rule)

        ReadOnly lstRules As New List(Of Rule)

        Public ReadOnly Property size As Integer
            Get
                Return lstRules.Count
            End Get
        End Property

        Public Overridable Sub load(fnInput As String)
            Dim reader As New StreamReader(fnInput)
            Dim line As Value(Of String) = ""

            lstRules.Clear()

            While Not (line = reader.ReadLine()) Is Nothing
                Dim tokens = line.Split(ASCII.TAB)
                Dim rule As Rule = New Rule()
                Dim ruleString = tokens(0)
                rule.confidence = Double.Parse(tokens(1))
                Dim ruleStrings = ruleString.Split(","c)
                rule.add(New Relation(Integer.Parse(ruleStrings(ruleStrings.Length - 1)), 1))
                For i = 0 To ruleStrings.Length - 1 - 1
                    Dim rel = Integer.Parse(ruleStrings(i))
                    Dim dir = 1
                    If rel < 0 Then
                        rel = -1 * rel
                        dir = -1
                    End If
                    rule.add(New Relation(rel, dir))
                Next
                lstRules.Add(rule)
            End While
            reader.Close()
        End Sub

        Public Iterator Function GenericEnumerator() As IEnumerator(Of Rule) Implements Enumeration(Of Rule).GenericEnumerator
            For Each rule As Rule In lstRules
                Yield rule
            Next
        End Function
    End Class

End Namespace
