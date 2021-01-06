#Region "Microsoft.VisualBasic::9d6856d7d7f90f406d597f26e764f32f, vs_solutions\dev\ApplicationServices\EventLog.vb"

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

    '     Class EventLog
    ' 
    '         Properties: Initialized, Product, Services
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __install, (+3 Overloads) LogException, ToString, (+6 Overloads) WriteEntry
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Debugging.Logging

    ''' <summary>
    ''' Provides interaction with Windows event logs.(这个日志入口点对象的创建应该调用于安装程序的模块之中，并且以管理员权限执行)
    ''' </summary>
    ''' 
    <InstallerType("System.Diagnostics.EventLogInstaller, System.Configuration.Install, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")>
    <MonitoringDescription("EventLogDesc")>
    Public Class EventLog

        Public ReadOnly Property Product As String
        Public ReadOnly Property Services As String
        Public ReadOnly Property Initialized As Boolean

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Product">Source</param>
        ''' <param name="Services"></param>
        Sub New(Services As String, Product As String)
            Me.Product = Product
            Me.Services = Services
            Try
                Me.Initialized = __install(Services, Product)
            Catch ex As Exception  ' 没有管理员权限
                Me.Initialized = False
            End Try
        End Sub

        Private Shared Function __install(Services As String, Product As String) As Boolean
            If Not (System.Diagnostics.EventLog.SourceExists(Product, ".")) Then
                Dim evscd As New EventSourceCreationData(Product, Services) With {
                    .MachineName = Environment.MachineName
                }
                Try
                    Call System.Diagnostics.EventLog.CreateEventSource(evscd)
                Catch ex As Exception
                    Call ex.PrintException
                    Return False
                End Try
            End If

            Return True
        End Function

        ''' <summary>
        ''' Writes a localized entry to the event log.
        ''' </summary>
        ''' <param name="EventType">An <see cref="System.Diagnostics.EventLogEntryType"/> value that indicates the event type.</param>
        ''' <param name="category">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.</param>
        ''' <returns></returns>
        Public Function WriteEntry(message As String,
                                   Optional EventType As EventLogEntryType = EventLogEntryType.Information,
                                   Optional category As Integer = 1) As Boolean
            Try
#If DEBUG Then
                Call message.__DEBUG_ECHO
#End If
                Using evLog As New System.Diagnostics.EventLog(Services, ".", Product)
                    Call evLog.WriteEvent(New EventInstance(10001 + category, category, EventType), {message})
                End Using
            Catch ex As Exception
                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' Writes a localized entry to the event log.
        ''' </summary>
        ''' <param name="EventType">An <see cref="System.Diagnostics.EventLogEntryType"/> value that indicates the event type.</param>
        ''' <param name="category">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.</param>
        ''' <returns></returns>
        Public Function WriteEntry(Message As IEnumerable(Of String),
                                   Optional EventType As EventLogEntryType = EventLogEntryType.Information,
                                   Optional category As Integer = 0) As Boolean
            Try
#If DEBUG Then
                Dim s As String = Message.JoinBy(vbCrLf)
                Call s.__DEBUG_ECHO
#End If
                Using evLog As New System.Diagnostics.EventLog(Services, ".", Product)
                    Dim data As Object() = Message.Select(Function(str) DirectCast(str, Object)).ToArray
                    Call evLog.WriteEvent(New EventInstance(10001 + category * 55, category, EventType), data)
                End Using
            Catch ex As Exception

                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' Writes a localized entry to the event log.
        ''' </summary>
        ''' <param name="EventType">An <see cref="System.Diagnostics.EventLogEntryType"/> value that indicates the event type.</param>
        ''' <param name="category">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.</param>
        ''' <returns></returns>
        Public Function WriteEntry(Message As System.Collections.Generic.IEnumerable(Of String),
                                   Trace As String,
                                   Optional EventType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information,
                                   Optional category As Integer = 0) As Boolean
            Message = Message.Join("USER_TRACE: " & Trace)
            Return WriteEntry(Message, EventType, category)
        End Function

        ''' <summary>
        ''' Writes a localized entry to the event log.
        ''' </summary>
        ''' <param name="EventType">An <see cref="System.Diagnostics.EventLogEntryType"/> value that indicates the event type.</param>
        ''' <param name="category">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.</param>
        ''' <returns></returns>
        Public Function WriteEntry(Message As System.Collections.Generic.IEnumerable(Of String),
                                   Trace As MethodBase,
                                   Optional EventType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information,
                                   Optional category As Integer = 0) As Boolean
            Return WriteEntry(Message, Trace.GetFullName, EventType, category)
        End Function

        ''' <summary>
        ''' Writes a localized entry to the event log.
        ''' </summary>
        ''' <param name="EventType">An <see cref="System.Diagnostics.EventLogEntryType"/> value that indicates the event type.</param>
        ''' <param name="category">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.</param>
        ''' <returns></returns>
        Public Function WriteEntry(Message As String, Trace As String, Optional EventType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information, Optional category As Integer = 2) As Boolean
            Return WriteEntry({NameOf(Message) & ": " & Message, NameOf(Trace) & ": " & Trace}, EventType, category)
        End Function

        ''' <summary>
        ''' Writes a localized entry to the event log.
        ''' </summary>
        ''' <param name="EventType">An <see cref="System.Diagnostics.EventLogEntryType"/> value that indicates the event type.</param>
        ''' <param name="category">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.</param>
        ''' <returns></returns>
        Public Function WriteEntry(Message As String,
                                   Trace As MethodBase,
                                   Optional EventType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information,
                                   Optional category As Integer = 2) As Boolean
            Return WriteEntry(Message, Trace.GetFullName, EventType, category)
        End Function

        ''' <summary>
        ''' Writes a localized entry to the event log.
        ''' </summary>
        ''' <param name="category">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.</param>
        ''' <returns></returns>
        ''' <param name="Trace">可以不需要进行额外的处理，编译器会自动在这个参数填充调用栈的位置</param>
        ''' 
#If NET_40 = 0 Then
        Public Function LogException(ex As Exception, <CallerMemberName> Optional Trace As String = "", Optional category As Integer = 3) As Boolean
#Else
        Public Function LogException(ex As Exception, Optional Trace As String = "", Optional category As Integer = 3) As Boolean
#End If
            Dim Data As String = ""

            If Not ex.Data Is Nothing Then
                Try
                    Data = String.Join(vbCrLf, (From obj In ex.Data.Keys Select obj.ToString & "  ==> " & ex.Data(obj).ToString).ToArray)
                Catch
                End Try
            End If

#If NET_40 = 0 Then
            Dim MSG_DATA As New List(Of String) From {
                "Exception: " & Scripting.InputHandler.ToString(ex.Message),
                "InnerException: " & Scripting.InputHandler.ToString(ex.InnerException),
                "HelpLink: " & Scripting.InputHandler.ToString(ex.HelpLink),
                "Handle: " & Scripting.InputHandler.ToString(ex.HResult),
                "Source: " & Scripting.InputHandler.ToString(ex.Source)
            }
#Else
            Dim MSG_DATA As New List(Of String) From {
                "Exception: " & Scripting.InputHandler.ToString(ex.Message),
                "InnerException: " & Scripting.InputHandler.ToString(ex.InnerException),
                "HelpLink: " & Scripting.InputHandler.ToString(ex.HelpLink),
                "Source: " & Scripting.InputHandler.ToString(ex.Source)
            }
#End If
            Dim Stacks As String() = Strings.Split(Scripting.InputHandler.ToString(ex.StackTrace), vbCrLf)
            Dim exTrace As String = If(ex.TargetSite Is Nothing OrElse ex.TargetSite.DeclaringType Is Nothing, "null", ex.TargetSite.DeclaringType.FullName)
            Call MSG_DATA.AddRange((From deepth As Integer In Stacks.Sequence Select $"stack__{Stacks.Length - deepth}:  {Stacks(deepth)}").ToArray)
            Call MSG_DATA.AddRange({"Data " & Data, $"Trace: {exTrace}:{Scripting.ToString(ex.TargetSite)}", "USER_TRACE: " & Trace})
            Return WriteEntry(MSG_DATA, EventLogEntryType.Error, category)
        End Function

        ''' <summary>
        ''' Writes a localized entry to the event log.
        ''' </summary>
        ''' <param name="category">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event.</param>
        ''' <returns></returns>
        Public Function LogException(ex As Exception, Trace As MethodBase, Optional category As Integer = 3) As Boolean
            Return LogException(ex, Trace.GetFullName, category)
        End Function

        ''' <summary>
        ''' $"{<see cref="Services"/>}//{<see cref="Product"/>}"
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"{Services}//{Product}"
        End Function
    End Class
End Namespace
