Imports System.Xml.Serialization

Namespace SoftwareToolkits.XmlDoc.Serialization

    ''' <summary>
    ''' .NET assembly generated XML comments documents file.
    ''' </summary>
    <XmlType("doc")> Public Class Doc
        Public Property assembly As assembly
        Public Property members As member()

        Public Overrides Function ToString() As String
            Return assembly.name
        End Function
    End Class

    Public Class assembly : Implements IMember

        Public Property name As String Implements IMember.name

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class

    Public Interface IMember
        Property name As String
    End Interface
End Namespace