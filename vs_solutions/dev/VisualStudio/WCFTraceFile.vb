#Region "Microsoft.VisualBasic::aafd6af1281499665f84b36579893673, vs_solutions\dev\VisualStudio\WCFTraceFile.vb"

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

    '   Total Lines: 61
    '    Code Lines: 46 (75.41%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (24.59%)
    '     File Size: 1.73 KB


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
