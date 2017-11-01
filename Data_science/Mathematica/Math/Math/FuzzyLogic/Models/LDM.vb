#Region "Microsoft.VisualBasic::fb2fe4803e11fa164acd5d2daf9d1b06, ..\sciBASIC#\Data_science\Mathematica\Math\Math\FuzzyLogic\Models\LDM.vb"

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

    Public Class Value : Inherits BaseClass
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

    Public Class Term : Inherits BaseClass
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

    Public Class Fuzzify : Inherits BaseClass
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

    Public Class Rule : Inherits BaseClass
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

    Public Class RuleBlock : Inherits BaseClass

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
