Imports System.ComponentModel
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SoftwareToolkits.XmlDoc.Serialization

    Public Class member : Implements IMember

        <XmlAttribute> Public Property name As String Implements IMember.name
            Get
                Return _name
            End Get
            Set(value As String)
                _name = value
                _memberRef = memberName.RefParser(value)
            End Set
        End Property
        Public Property summary As String
        Public Property typeparam As typeparam
        Public Property param As param()
        Public Property returns As String
        Public Property remarks As String

        Dim _name As String

        Public ReadOnly Property memberRef As memberName

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Structure memberName

        Public Property Type As memberTypes
        Public Property Name As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function RefParser(name As String) As memberName
            Dim type As Char = name.First
            name = Mid(name, 3)
            Return New memberName With {
                .Name = name,
                .Type = DocAPI.Types(type)
            }
        End Function
    End Structure

    Public Enum memberTypes
        ''' <summary>
        ''' T
        ''' </summary>
        <Description("T")> Type
        ''' <summary>
        ''' F
        ''' </summary>
        <Description("F")> Filed
        ''' <summary>
        ''' M
        ''' </summary>
        <Description("M")> Method
        ''' <summary>
        ''' P
        ''' </summary>
        <Description("P")> [Property]
        ''' <summary>
        ''' E
        ''' </summary>
        <Description("E")> [Event]
    End Enum

    Public Class param : Implements IMember

        <XmlAttribute> Public Property name As String Implements IMember.name
        <XmlText> Public Property text As String

        Public Overrides Function ToString() As String
            Return $"{name} = {text}"
        End Function
    End Class

    Public Class typeparam : Inherits param
    End Class

    Public Structure CrossReferred
        <XmlAttribute> Public Property cref As String
    End Structure
End Namespace