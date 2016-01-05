Imports Microsoft.VisualBasic.DataVisualization.DocumentFormat.Csv.Reflection
Imports Microsoft.VisualBasic.Datavisualization.DocumentFormat.Extensions

<Xml.Serialization.XmlRoot("entrySet", namespace:="net:sf:psidev:mi")>
Public Class EntrySet
    <Xml.Serialization.XmlAttribute("level")> Public Property Level As Integer
    <Xml.Serialization.XmlAttribute("version")> Public Property Version As Integer
    <Xml.Serialization.XmlElement("entry")> Public Property Entries As Entry()

    Public Class Entry
        <Xml.Serialization.XmlArray("experimentList")> Public Property ExperimentList As ExperimentDescription()
        <Xml.Serialization.XmlArray("interactorList")> Public Property InteractorList As Interactor()
        <Xml.Serialization.XmlArray("interactionList")> Public Property InteractionList As Interaction()
    End Class

    <Xml.Serialization.XmlType("experimentDescription")> Public Class ExperimentDescription : Inherits StringDB.XmlCommon.DataItem

        <Xml.Serialization.XmlElement("names")> Public Property Names As StringDB.XmlCommon.Names
        <Xml.Serialization.XmlElement("bibref")> Public Property Bibref As __bibref

        Public Overrides Function ToString() As String
            Return Names.ToString
        End Function

        Public Class __bibref
            <Xml.Serialization.XmlElement("xref")> Public Property Xref As StringDB.XmlCommon.Xref
        End Class

        <Xml.Serialization.XmlArray("hostOrganismList")> Public Property HostOrganismList As __hostOrganism()

        <Xml.Serialization.XmlType("hostOrganism")> Public Class __hostOrganism
            <Xml.Serialization.XmlAttribute("ncbiTaxId")> Public Property ncbiTaxId As String
            <Xml.Serialization.XmlElement("names")> Public Property Names As StringDB.XmlCommon.Names
        End Class

        Public Class interactionDetectionMethod
            Public Property Names As StringDB.XmlCommon.Names
            Public Property Xref As StringDB.XmlCommon.Xref
        End Class
        Public Class participantIdentificationMethod
            Public Property Names As StringDB.XmlCommon.Names
            Public Property Xref As StringDB.XmlCommon.Xref
        End Class
    End Class

    <Xml.Serialization.XmlType("interactor")> Public Class Interactor : Inherits StringDB.XmlCommon.DataItem

        <Xml.Serialization.XmlElement("names")> Public Property Names As StringDB.XmlCommon.Names
        <Xml.Serialization.XmlElement("xref")> Public Property Xref As StringDB.XmlCommon.Xref
        <Xml.Serialization.XmlElement("interactorType")> Public Property InteractorType As __interactorType

        Public Overrides Function ToString() As String
            Return Xref.PrimaryReference.ToString
        End Function

        Public Class __interactorType
            <Xml.Serialization.XmlElement("names")> Public Property Names As StringDB.XmlCommon.Names
            <Xml.Serialization.XmlElement("xref")> Public Property Xref As StringDB.XmlCommon.Xref
        End Class
    End Class

    <Xml.Serialization.XmlType("interaction")> Public Class Interaction : Inherits StringDB.XmlCommon.DataItem

        <Xml.Serialization.XmlArray("experimentList")> Public Property ExperimentList As ExperimentRef()

        <Xml.Serialization.XmlType("experimentRef")> Public Class ExperimentRef
            <Xml.Serialization.XmlText> Public Property value As String
        End Class

        <Xml.Serialization.XmlArray("participantList")> Public Property ParticipantList As Participant()

        <Xml.Serialization.XmlType("participant")> Public Class Participant
            <Xml.Serialization.XmlAttribute("id")> Public Property ID As String
            <Xml.Serialization.XmlElement("interactorRef")> Public Property InteractorRef As Integer
        End Class

        <Xml.Serialization.XmlArray("confidenceList")> Public Property ConfidenceList As confidence()

        Public Class confidence
            <Xml.Serialization.XmlElement("unit")> Public Property Unit As __unit
            <Xml.Serialization.XmlElement> Public Property value As Double

            Public Overrides Function ToString() As String
                Return value
            End Function

            <Xml.Serialization.XmlType("unit")> Public Class __unit
                <Xml.Serialization.XmlElement("names")> Public Property Names As StringDB.XmlCommon.Names
            End Class
        End Class
    End Class

    Public Function GetInteractor(Id As Integer) As Interactor
        Dim LQuery = (From item In Me.Entries.First.InteractorList Where Id = item.Id Select item).ToArray
        If LQuery.IsNullOrEmpty Then
            Return Nothing
        Else
            Return LQuery.First
        End If
    End Function

    Public Function ExtractNetwork() As StringDB.XmlCommon.NetworkNode()
        Dim NodeList As StringDB.XmlCommon.NetworkNode() = (
            From Interaction As Interaction
            In Me.Entries.First.InteractionList
            Let Generation = Function() As StringDB.XmlCommon.NetworkNode
                                 Dim Node As StringDB.XmlCommon.NetworkNode = New StringDB.XmlCommon.NetworkNode
                                 Node.Confidence = Interaction.ConfidenceList.First.value
                                 Node.FromNode = GetInteractor(Interaction.ParticipantList.First.InteractorRef).Xref.PrimaryReference.Id
                                 Node.ToNode = GetInteractor(Interaction.ParticipantList.Last.InteractorRef).Xref.PrimaryReference.Id
                                 Return Node
                             End Function Select Generation()).ToArray
        Return NodeList
    End Function
End Class

Namespace StringDB.XmlCommon

    Public MustInherit Class DataItem
        <Xml.Serialization.XmlAttribute("id")> Public Overridable Property Id As Integer

        Public Overrides Function ToString() As String
            Return Id
        End Function
    End Class

    Public Class NetworkNode : Inherits Datavisualization.Network.NetworkNode
    End Class

    Public Class Names
        <Xml.Serialization.XmlElement("shortLabel")> Public Property ShortLabel As String
        <Xml.Serialization.XmlElement("fullName")> Public Property FullName As String

        Public Overrides Function ToString() As String
            Return String.Format("({0}) {1}", ShortLabel, FullName)
        End Function
    End Class

    Public Class Xref
        <Xml.Serialization.XmlElement("primaryRef")> Public Property PrimaryReference As Reference
        <Xml.Serialization.XmlElement("secondaryRef")> Public Property SecondaryReference As Reference()

        Public Overrides Function ToString() As String
            Return PrimaryReference.ToString
        End Function

        Public Class Reference
            <Xml.Serialization.XmlAttribute("db")> Public Property Db As String
            <Xml.Serialization.XmlAttribute("id")> Public Property Id As String
            <Xml.Serialization.XmlAttribute("dbAc")> Public Property dbAc As String
            <Xml.Serialization.XmlAttribute("refType")> Public Property refType As String
            <Xml.Serialization.XmlAttribute("refTypeAc")> Public Property refTypeAc As String

            Public Overrides Function ToString() As String
                Return String.Format("{0}: {1}", Db, Id)
            End Function
        End Class
    End Class
End Namespace