#Region "Microsoft.VisualBasic::6b345e0a87c16340054aa0590d6d0c3e, vs_solutions\dev\VisualStudio\WCFTraceFile.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 199
    '    Code Lines: 49 (24.62%)
    ' Comment Lines: 116 (58.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 34 (17.09%)
    '     File Size: 7.89 KB


    ' Class WCFTraceFile
    ' 
    '     Function: ReadFile
    ' 
    ' Class E2ETraceEvent
    ' 
    '     Properties: ApplicationData, System
    ' 
    ' Class SystemEvent
    ' 
    '     Properties: Channel, Computer, Correlation, EventID, Execution
    '                 Level, Source, SubType, TimeCreated, Type
    ' 
    ' Class Execution
    ' 
    '     Properties: ProcessID, ProcessName, ThreadID
    ' 
    ' Class SystemTime
    ' 
    '     Properties: SystemTime
    ' 
    ' Class TagValue
    ' 
    '     Properties: Name, Value
    ' 
    ' Class Correlation
    ' 
    '     Properties: ActivityID
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Text.Xml.Linq

''' <summary>
''' Utility class for reading and parsing Windows Communication Foundation (WCF) trace log files (.svclog).
''' Provides methods to deserialize E2E trace events from XML-formatted trace files.
''' </summary>
Public Class WCFTraceFile

    ''' <summary>
    ''' Reads and parses a WCF trace log file (.svclog) into a sequence of <see cref="E2ETraceEvent"/>  objects.
    ''' </summary>
    ''' <param name="svclog">The file path or XML content string of the .svclog trace file.</param>
    ''' <returns>An enumerable collection of <see cref="E2ETraceEvent"/>  instances parsed from the trace file.</returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function ReadFile(svclog As String) As IEnumerable(Of E2ETraceEvent)
        Return svclog.LoadUltraLargeXMLDataSet(Of E2ETraceEvent)()
    End Function

End Class

''' <summary>
''' Represents an End-to-End (E2E) trace event from a WCF trace log.
''' Contains system-level event information and optional application-specific data.
''' Maps to the "E2ETraceEvent" XML element in the Microsoft E2E trace event schema.
''' </summary>
<XmlType("E2ETraceEvent", [Namespace]:="http://schemas.microsoft.com/2004/06/E2ETraceEvent")>
Public Class E2ETraceEvent

    ''' <summary>
    ''' Gets or sets the system-level event information associated with this trace event.
    ''' Contains metadata such as event ID, type, severity level, timestamp, source, and execution context.
    ''' </summary>
    ''' <returns>A <see cref="SystemEvent"/>  instance containing the system event details.</returns>
    Public Property System As SystemEvent

    ''' <summary>
    ''' Gets or sets the application-specific data payload attached to this trace event.
    ''' This may contain custom logging information, exception details, or diagnostic messages from the application.
    ''' </summary>
    ''' <returns>A string containing the application data content.</returns>
    Public Property ApplicationData As String

End Class

''' <summary>
''' Represents the system-level event metadata for a WCF trace event.
''' Maps to the "System" XML element in the Windows event log schema.
''' Contains information about the event identifier, type, severity, timestamp, source, correlation, and execution context.
''' </summary>
<XmlType("System", [Namespace]:="http://schemas.microsoft.com/2004/06/windows/eventlog/system")>
Public Class SystemEvent

    ''' <summary>
    ''' Gets or sets the event identifier that uniquely identifies the type of event.
    ''' </summary>
    ''' <returns>A string representing the event ID.</returns>
    Public Property EventID As String

    ''' <summary>
    ''' Gets or sets the type of the event (e.g., Error, Warning, Information, etc.).
    ''' </summary>
    ''' <returns>A string indicating the event type.</returns>
    Public Property Type As String

    ''' <summary>
    ''' Gets or sets the sub-type of the event, providing additional classification detail.
    ''' </summary>
    ''' <returns>A <see cref="TagValue"/>  instance containing the sub-type name and value.</returns>
    Public Property SubType As TagValue

    ''' <summary>
    ''' Gets or sets the severity level of the event (e.g., Critical, Error, Warning, Information, Verbose).
    ''' </summary>
    ''' <returns>A string representing the event level.</returns>
    Public Property Level As String

    ''' <summary>
    ''' Gets or sets the timestamp when the event was created.
    ''' </summary>
    ''' <returns>A <see cref="SystemTime"/>  instance containing the creation time.</returns>
    Public Property TimeCreated As SystemTime

    ''' <summary>
    ''' Gets or sets the source of the event, identifying the component or service that generated the trace.
    ''' </summary>
    ''' <returns>A <see cref="TagValue"/>  instance containing the source name and value.</returns>
    Public Property Source As TagValue

    ''' <summary>
    ''' Gets or sets the correlation information for this event, used to correlate related trace events across activities.
    ''' </summary>
    ''' <returns>A <see cref="Correlation"/>  instance containing the activity identifier.</returns>
    Public Property Correlation As Correlation

    ''' <summary>
    ''' Gets or sets the execution context information, including process and thread identifiers.
    ''' </summary>
    ''' <returns>An <see cref="Execution"/>  instance containing process and thread details.</returns>
    Public Property Execution As Execution

    ''' <summary>
    ''' Gets or sets the event channel through which the event was logged.
    ''' </summary>
    ''' <returns>A string representing the event channel.</returns>
    Public Property Channel As String

    ''' <summary>
    ''' Gets or sets the name of the computer where the event originated.
    ''' </summary>
    ''' <returns>A string containing the computer name.</returns>
    Public Property Computer As String

End Class

''' <summary>
''' Represents the execution context information for a trace event.
''' Contains the process name, process identifier, and thread identifier as XML attributes.
''' </summary>
Public Class Execution

    ''' <summary>
    ''' Gets or sets the name of the process that generated the trace event.
    ''' </summary>
    ''' <returns>A string containing the process name.</returns>
    <XmlAttribute>
    Public Property ProcessName As String

    ''' <summary>
    ''' Gets or sets the numeric identifier of the process that generated the trace event.
    ''' </summary>
    ''' <returns>A string containing the process ID.</returns>
    <XmlAttribute>
    Public Property ProcessID As String

    ''' <summary>
    ''' Gets or sets the identifier of the thread within the process that generated the trace event.
    ''' </summary>
    ''' <returns>A string containing the thread ID.</returns>
    <XmlAttribute>
    Public Property ThreadID As String

End Class

''' <summary>
''' Represents a system timestamp value, serialized as an XML attribute.
''' Used to capture the creation time of a trace event.
''' </summary>
Public Class SystemTime

    ''' <summary>
    ''' Gets or sets the system time value when the event was created.
    ''' Typically formatted as an ISO8601 date/time string.
    ''' </summary>
    ''' <returns>A string representing the system timestamp.</returns>
    <XmlAttribute>
    Public Property SystemTime As String

End Class

''' <summary>
''' Represents a key-value pair where the name is stored as an XML attribute
''' and the value is stored as the XML element's text content.
''' Commonly used for representing sub-type, source, and other tagged metadata in WCF trace events.
''' </summary>
Public Class TagValue

    ''' <summary>
    ''' Gets or sets the attribute name of this key-value pair.
    ''' </summary>
    ''' <returns>A string containing the attribute name.</returns>
    <XmlAttribute>
    Public Property Name As String

    ''' <summary>
    ''' Gets or sets the text content value of this key-value pair.
    ''' </summary>
    ''' <returns>A string containing the text value.</returns>
    <XmlText>
    Public Property Value As String

End Class

''' <summary>
''' Represents activity correlation information for a WCF trace event.
''' Used to correlate related trace events across service boundaries by sharing a common activity identifier.
''' </summary>
Public Class Correlation

    ''' <summary>
    ''' Gets or sets the activity identifier used for correlating related trace events.
    ''' Typically a globally unique identifier (GUID) that links events belonging to the same logical operation.
    ''' </summary>
    ''' <returns>A string containing the activity ID.</returns>
    <XmlAttribute>
    Public Property ActivityID As String

End Class
