Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Project
    <XmlAttribute> Public Property ToolsVersion As String
    <XmlAttribute> Public Property DefaultTargets As String

    <XmlElement("Import")>
    Public Property [Imports] As Import()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class Import
    <XmlAttribute> Public Property Project As String
    <XmlAttribute> Public Property Condition As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class PropertyGroup
    Public Property Configuration As ConditionValue
    Public Property Platform As ConditionValue
    Public Property ProjectGuid As String
    Public Property OutputType As String
    Public Property RootNamespace As String
    Public Property AssemblyName As String
    Public Property FileAlignment As String
    Public Property MyType As String
    Public Property TargetFrameworkVersion As String
    Public Property TargetFrameworkProfile
    Public Property PublishUrl As String
    Public Property Install As String
    Public Property InstallFrom As String
    Public Property UpdateEnabled As String
    Public Property UpdateMode As String
    Public Property UpdateInterval As String
    Public Property UpdateIntervalUnits As String
    Public Property UpdatePeriodically As String
    Public Property UpdateRequired As String
    Public Property MapFileExtensions As String
    Public Property ApplicationRevision As String
    Public Property ApplicationVersion As String
    Public Property IsWebBootstrapper As String
    Public Property UseApplicationTrust As String
    Public Property BootstrapperEnabled As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class ConditionValue
    <XmlAttribute> Public Property Condition As String
    <XmlText> Public Property value As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class