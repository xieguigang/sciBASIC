Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Text.Xml.Linq

Public Class WCFTraceFile

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function ReadFile(svclog As String) As IEnumerable(Of E2ETraceEvent)
        Return svclog.LoadUltraLargeXMLDataSet(Of E2ETraceEvent)()
    End Function

End Class

<XmlType("E2ETraceEvent", [Namespace]:="http://schemas.microsoft.com/2004/06/E2ETraceEvent")>
Public Class E2ETraceEvent
    Public Property System As SystemEvent
    Public Property ApplicationData As String
End Class

<XmlType("System", [Namespace]:="http://schemas.microsoft.com/2004/06/windows/eventlog/system")>
Public Class SystemEvent
    Public Property EventID As String
    Public Property Type As String
    Public Property SubType As TagValue
    Public Property Level As String
    Public Property TimeCreated As SystemTime
    Public Property Source As TagValue
    Public Property Correlation As Correlation
    Public Property Execution As Execution
    Public Property Channel As String
    Public Property Computer As String
End Class

Public Class Execution
    <XmlAttribute> Public Property ProcessName As String
    <XmlAttribute> Public Property ProcessID As String
    <XmlAttribute> Public Property ThreadID As String
End Class

Public Class SystemTime

    <XmlAttribute>
    Public Property SystemTime As String

End Class

Public Class TagValue

    <XmlAttribute>
    Public Property Name As String
    <XmlText>
    Public Property Value As String

End Class

Public Class Correlation

    <XmlAttribute>
    Public Property ActivityID As String

End Class