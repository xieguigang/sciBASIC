#Region "Microsoft.VisualBasic::25f4d9dd0564b9182edb3d12c3c923cf, Data_science\Mathematica\Math\Math\FuzzyLogic\Models\LDM.vb"

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

Namespace Logical.FuzzyLogic.Models

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
