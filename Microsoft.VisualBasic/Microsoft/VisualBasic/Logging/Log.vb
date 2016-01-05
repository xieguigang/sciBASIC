Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Security
Imports System.Security.Permissions
Imports System.Text

Namespace Microsoft.VisualBasic.Logging
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class Log
        ' Methods
        <SecuritySafeCritical> _
        Public Sub New()
            Me.m_TraceSource = New DefaultTraceSource("DefaultSource")
            If Not Me.m_TraceSource.HasBeenConfigured Then
                Me.InitializeWithDefaultsSinceNoConfigExists
            End If
            AddHandler AppDomain.CurrentDomain.ProcessExit, New EventHandler(AddressOf Me.CloseOnProcessExit)
        End Sub

        <SecuritySafeCritical>
        Public Sub New(name As String)
            Me.m_TraceSource = New DefaultTraceSource(name)
            If Not Me.m_TraceSource.HasBeenConfigured Then
                Me.InitializeWithDefaultsSinceNoConfigExists()
            End If
        End Sub

        <SecuritySafeCritical>
        Private Sub CloseOnProcessExit(sender As Object, e As EventArgs)
            RemoveHandler AppDomain.CurrentDomain.ProcessExit, New EventHandler(AddressOf Me.CloseOnProcessExit)
            Me.TraceSource.Close()
        End Sub

        Private Shared Function InitializeIDHash() As Dictionary(Of TraceEventType, Integer)
            Dim dictionary1 As New Dictionary(Of TraceEventType, Integer)(10)
            dictionary1.Add(TraceEventType.Information, 0)
            dictionary1.Add(TraceEventType.Warning, 1)
            dictionary1.Add(TraceEventType.Error, 2)
            dictionary1.Add(TraceEventType.Critical, 3)
            dictionary1.Add(TraceEventType.Start, 4)
            dictionary1.Add(TraceEventType.Stop, 5)
            dictionary1.Add(TraceEventType.Suspend, 6)
            dictionary1.Add(TraceEventType.Resume, 7)
            dictionary1.Add(TraceEventType.Verbose, 8)
            dictionary1.Add(TraceEventType.Transfer, 9)
            Return dictionary1
        End Function

        <SecuritySafeCritical>
        Protected Friend Overridable Sub InitializeWithDefaultsSinceNoConfigExists()
            Me.m_TraceSource.Listeners.Add(New FileLogTraceListener("FileLog"))
            Me.m_TraceSource.Switch.Level = SourceLevels.Information
        End Sub

        Private Function TraceEventTypeToId(traceEventValue As TraceEventType) As Integer
            If Log.m_IdHash.ContainsKey(traceEventValue) Then
                Return Log.m_IdHash.Item(traceEventValue)
            End If
            Return 0
        End Function

        Public Sub WriteEntry(message As String)
            Me.WriteEntry(message, TraceEventType.Information, Me.TraceEventTypeToId(TraceEventType.Information))
        End Sub

        Public Sub WriteEntry(message As String, severity As TraceEventType)
            Me.WriteEntry(message, severity, Me.TraceEventTypeToId(severity))
        End Sub

        Public Sub WriteEntry(message As String, severity As TraceEventType, id As Integer)
            If (message Is Nothing) Then
                message = ""
            End If
            Me.m_TraceSource.TraceEvent(severity, id, message)
        End Sub

        Public Sub WriteException(ex As Exception)
            Me.WriteException(ex, TraceEventType.Error, "", Me.TraceEventTypeToId(TraceEventType.Error))
        End Sub

        Public Sub WriteException(ex As Exception, severity As TraceEventType, additionalInfo As String)
            Me.WriteException(ex, severity, additionalInfo, Me.TraceEventTypeToId(severity))
        End Sub

        Public Sub WriteException(ex As Exception, severity As TraceEventType, additionalInfo As String, id As Integer)
            If (ex Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("ex")
            End If
            Dim builder As New StringBuilder
            builder.Append(ex.Message)
            If (additionalInfo <> "") Then
                builder.Append(" ")
                builder.Append(additionalInfo)
            End If
            Me.m_TraceSource.TraceEvent(severity, id, builder.ToString)
        End Sub


        ' Properties
        Public ReadOnly Property DefaultFileLogWriter As FileLogTraceListener
            <SecuritySafeCritical>
            Get
                Return DirectCast(Me.TraceSource.Listeners.Item("FileLog"), FileLogTraceListener)
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property TraceSource As TraceSource
            Get
                Return Me.m_TraceSource
            End Get
        End Property


        ' Fields
        Private Const DEFAULT_FILE_LOG_TRACE_LISTENER_NAME As String = "FileLog"
        Private Shared m_IdHash As Dictionary(Of TraceEventType, Integer) = Log.InitializeIDHash
        Private m_TraceSource As DefaultTraceSource
        Private Const WINAPP_SOURCE_NAME As String = "DefaultSource"

        ' Nested Types
        Friend NotInheritable Class DefaultTraceSource
            Inherits TraceSource
            ' Methods
            Public Sub New(name As String)
                MyBase.New(name)
            End Sub

            Protected Overrides Function GetSupportedAttributes() As String()
                Me.m_HasBeenInitializedFromConfigFile = True
                Return MyBase.GetSupportedAttributes
            End Function


            ' Properties
            Public ReadOnly Property HasBeenConfigured As Boolean
                Get
                    If (Me.listenerAttributes Is Nothing) Then
                        Me.listenerAttributes = MyBase.Attributes
                    End If
                    Return Me.m_HasBeenInitializedFromConfigFile
                End Get
            End Property


            ' Fields
            Private listenerAttributes As StringDictionary
            Private m_HasBeenInitializedFromConfigFile As Boolean
        End Class
    End Class
End Namespace

