Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace FuzzyLogic.Models

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
            Terms = x.MembershipFunctionCollection.ToArray(Function(m) New MembershipFunction(m))
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
    End Class

    Public Class RuleBlock : Inherits ClassObject

        <XmlAttribute> Public Property [AND] As String
        <XmlAttribute> Public Property [OR] As String
        <XmlElement> Public Property Rules As Rule()
    End Class
End Namespace