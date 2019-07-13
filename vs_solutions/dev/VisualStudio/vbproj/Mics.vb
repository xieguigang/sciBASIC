#Region "Microsoft.VisualBasic::aa017c8df5fd4e9d20fb9ff6198503b4, vs_solutions\dev\VisualStudio\vbproj\Mics.vb"

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

    '     Class Target
    ' 
    '         Properties: Name
    ' 
    '     Class Import
    ' 
    '         Properties: Condition, Label, Project
    ' 
    '         Function: ToString
    ' 
    '     Class ConditionValue
    ' 
    '         Properties: Condition, value
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace vbproj

    Public Class Target
        <XmlAttribute>
        Public Property Name As String
    End Class

    Public Class Import

        <XmlAttribute> Public Property Project As String
        <XmlAttribute> Public Property Condition As String
        <XmlAttribute> Public Property Label As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class ConditionValue

        <XmlAttribute>
        Public Property Condition As String
        <XmlText>
        Public Property value As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
