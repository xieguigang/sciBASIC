Imports System.Xml.Serialization

Namespace SoftwareToolkits.XmlDoc

    Public Class member : Implements IMember

        <XmlAttribute> Public Property name As String Implements IMember.name
        Public Property summary As String
        Public Property typeparam As typeparam
        Public Property param As param()
        Public Property returns As String
        Public Property remarks As String
    End Class

    Public Class param : Implements IMember

        <XmlAttribute> Public Property name As String Implements IMember.name
        <XmlText> Public Property text As String
    End Class

    Public Class typeparam : Inherits param
    End Class

    Public Structure CrossReferred
        <XmlAttribute> Public Property cref As String
    End Structure
End Namespace