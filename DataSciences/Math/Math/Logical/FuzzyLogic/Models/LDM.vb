#Region "Microsoft.VisualBasic::10a817703b91cbd01af065c6f0957838, ..\visualbasic_App\Scripting\Math\Math\Logical\FuzzyLogic\Models\LDM.vb"

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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Logical.FuzzyLogic.Models

    Public Class Value : Inherits ClassObject
        Implements sIdEnumerable

        <XmlAttribute> Public Property Identifier As String Implements sIdEnumerable.Identifier
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

    Public Class Term : Inherits ClassObject
        Implements sIdEnumerable

        <XmlAttribute> Public Property Identifier As String Implements sIdEnumerable.Identifier
        <XmlAttribute> Public Property Points As Double()

        Sub New()
        End Sub

        Sub New(x As MembershipFunction)
            Identifier = x.Name
            Points = {x.X0, x.X1, x.X2, x.X3}
        End Sub

    End Class

    Public Class Fuzzify : Inherits ClassObject
        Implements sIdEnumerable

        <XmlAttribute> Public Property Identifier As String Implements sIdEnumerable.Identifier
        <XmlElement> Public Property Terms As Term()

        Sub New()
        End Sub

        Sub New(x As LinguisticVariable)
            Identifier = x.Name
            Terms = x.MembershipFunctionCollection.ToArray(Function(m) New Term(m))
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

    Public Class Rule : Inherits ClassObject
        Implements sIdEnumerable

        <XmlAttribute> Public Property Identifier As String Implements sIdEnumerable.Identifier
        <XmlAttribute> Public Property Expression As String

        Sub New()

        End Sub

        Sub New(i As Integer, rule As FuzzyRule)
            Identifier = i
            Expression = rule.Text
        End Sub
    End Class

    Public Class RuleBlock : Inherits ClassObject

        <XmlAttribute> Public Property [AND] As String
        <XmlAttribute> Public Property [OR] As String
        <XmlElement> Public Property Rules As Rule()

        Sub New()

        End Sub

        Sub New(rules As IEnumerable(Of FuzzyRule))
            Me.[AND] = "MIN"
            Me.[OR] = "MAX"
            Me.Rules = rules.ToArray(Function(x, i) New Rule(i, x))
        End Sub
    End Class
End Namespace
