#Region "Microsoft.VisualBasic::9688c1ee2d246d3adeb21e84798208ef, Data_science\Mathematica\Math\Math\FuzzyLogic\Models\Models.vb"

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

    '   Total Lines: 131
    '    Code Lines: 89 (67.94%)
    ' Comment Lines: 9 (6.87%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 33 (25.19%)
    '     File Size: 3.43 KB


    '     Enum Tokens
    ' 
    '         [Operator], CloseStack, Comparer, OpenStack, UNDEFINE
    '         WhiteSpace
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class LogicalToken
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Value
    ' 
    '         Properties: Identifier, Range, Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Term
    ' 
    '         Properties: Identifier, Points
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Fuzzify
    ' 
    '         Properties: Identifier, Terms
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Defuzzify
    ' 
    '         Properties: Accu, Method
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Rule
    ' 
    '         Properties: Expression, Identifier
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class RuleBlock
    ' 
    '         Properties: [AND], [OR], Rules
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Logical.FuzzyLogic.Models

    Public Enum Tokens
        UNDEFINE
        ''' <summary>
        ''' And Or Not Xor Nor Nand
        ''' </summary>
        [Operator]
        ''' <summary>
        ''' &lt;&lt;, &lt;, &lt;=, >, =>, >>, ~=, =, &lt;>
        ''' </summary>
        Comparer
        ''' <summary>
        ''' Space or VbTab
        ''' </summary>
        WhiteSpace
        OpenStack
        CloseStack
    End Enum

    Public Class LogicalToken : Inherits CodeToken(Of Tokens)

        Sub New(name As Tokens, text$)
            Call MyBase.New(name, text)
        End Sub
    End Class

    Public Class Value
        Implements INamedValue

        <XmlAttribute> Public Property Identifier As String Implements INamedValue.Key
        <XmlAttribute> Public Property Type As String
        <XmlAttribute> Public Property Range As Double()

        Sub New()
        End Sub

        Sub New(x As LinguisticVariable)
            Identifier = x.Name
            Type = "REAL"
            Range = {x.MinValue, x.MaxValue}
        End Sub
    End Class

    Public Class Term
        Implements INamedValue

        <XmlAttribute> Public Property Identifier As String Implements INamedValue.Key
        <XmlAttribute> Public Property Points As Double()

        Sub New()
        End Sub

        Sub New(x As MembershipFunction)
            Identifier = x.Name
            Points = {x.X0, x.X1, x.X2, x.X3}
        End Sub

    End Class

    Public Class Fuzzify
        Implements INamedValue

        <XmlAttribute> Public Property Identifier As String Implements INamedValue.Key
        <XmlElement> Public Property Terms As Term()

        Sub New()
        End Sub

        Sub New(x As LinguisticVariable)
            Identifier = x.Name
            Terms = x.MembershipFunctionCollection.Select(Function(m) New Term(m)).ToArray
        End Sub
    End Class

    Public Class Defuzzify : Inherits Fuzzify

        <XmlAttribute> Public Property Method As String
        <XmlAttribute> Public Property Accu As String

        Sub New()
        End Sub

        Sub New(x As LinguisticVariable)
            Call MyBase.New(x)

            Method = "CoG"
            Accu = "MAX"
        End Sub

    End Class

    Public Class Rule
        Implements INamedValue

        <XmlAttribute> Public Property Identifier As String Implements INamedValue.Key
        <XmlAttribute> Public Property Expression As String

        Sub New()

        End Sub

        Sub New(i As Integer, rule As FuzzyRule)
            Identifier = i
            Expression = rule.Text
        End Sub
    End Class

    Public Class RuleBlock

        <XmlAttribute> Public Property [AND] As String
        <XmlAttribute> Public Property [OR] As String
        <XmlElement> Public Property Rules As Rule()

        Sub New()

        End Sub

        Sub New(rules As IEnumerable(Of FuzzyRule))
            Me.[AND] = "MIN"
            Me.[OR] = "MAX"
            Me.Rules = rules.Select(Function(x, i) New Rule(i, x)).ToArray
        End Sub
    End Class
End Namespace
