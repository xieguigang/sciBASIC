#Region "Microsoft.VisualBasic::84e6536b7d47a62cb2de872fbc5366ca, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Logging\LogFile\LogFile.vb"

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

    '   Total Lines: 277
    '    Code Lines: 172
    ' Comment Lines: 60
    '   Blank Lines: 45
    '     File Size: 11.48 KB


    '     Class LogFile
    ' 
    '         Properties: fileName, filePath, MimeType, NowTimeNormalizedString
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: openFile, SaveLog, SystemInfo, ToString
    ' 
    '         Sub: Debug, (+2 Overloads) Dispose, info, (+2 Overloads) log, (+2 Overloads) LogException
    '              Save, (+2 Overloads) Trace, (+4 Overloads) WriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Namespace ApplicationServices.Debugging.Logging

    ''' <summary>
    ''' 日志文件记录模块。因为在Linux平台上面，Windows的日志记录API可能不会正常工作，
    ''' 所以需要这个日志模块来接替Windows的日志模块的工作
    ''' </summary>
    ''' <remarks>
    ''' 这个类模块将输入的信息格式化保存到文本文件之中，记录的信息包括信息头，消息文本，以及消息等级
    ''' </remarks>
    Public Class LogFile
        Implements IFileReference
        Implements IDisposable

        Dim buffer As TextWriter
        Dim counts&
        Dim split As LoggingDriver

        ''' <summary>
        ''' 没有路径名称和拓展名，仅包含有单独的文件名
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property fileName As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return filePath.BaseName
            End Get
        End Property

        ''' <summary>
        ''' 将时间字符串里面的":"符号去除之后，剩余的字符串可以用于作为路径来使用
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property NowTimeNormalizedString As String
            Get
                Return $"{Format(Now.Month, "00")}{ Format(Now.Day, "00")}{Format(Now.Hour, "00")}{Format(Now.Minute, "00")}{Format(Now.Second, "00")}"
            End Get
        End Property

        Public Property filePath As String Implements IFileReference.FilePath

        Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
            Get
                Return {New ContentType With {.FileExt = ".log", .Name = "Plant Log Text", .MIMEType = "plain/text"}}
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path">This logfile will saved to.</param>
        ''' <remarks></remarks>
        ''' <param name="bufferSize">当日志的记录数目达到这个数目的时候就会将日志数据写入到文件之中</param>
        Public Sub New(path As String,
                       Optional autoFlush As Boolean = True,
                       Optional bufferSize As Integer = 1024,
                       Optional append As Boolean = True,
                       Optional appendHeader As Boolean = True,
                       Optional split As LoggingDriver = Nothing)

            Me.buffer = New StreamWriter(openFile(path, append), Encoding.UTF8, bufferSize) With {
                .AutoFlush = autoFlush
            }

            If appendHeader Then
                Me.buffer.WriteLine(value:=$"//{vbTab}[{Now.ToString}]{vbTab}{New String("=", 25)}  START WRITE LOGGING SECTION  {New String("=", 25)}")
                Me.buffer.WriteLine()
            End If

            Me.filePath = FileIO.FileSystem.GetFileInfo(path).FullName
            Me.split = split
        End Sub

        Private Shared Function openFile(path As String, append As Boolean) As FileStream
            If Not append Then
                Call "".SaveTo(path)
            ElseIf Not path.FileExists Then
                Call "".SaveTo(path)
            End If

            Return New FileStream(path, If(append, FileMode.Append, FileMode.Truncate), access:=FileAccess.Write, share:=FileShare.ReadWrite)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Trace(toString As Func(Of String, Byte(), String), format As String, ParamArray bytes As Byte())
            Call Trace(toString(format, bytes))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Trace(format As String, ParamArray args As Object())
            Call Me.log(MSG_TYPES.INF, String.Format(format, args))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Debug(format As String, ParamArray args As Object())
            Call Me.log(MSG_TYPES.DEBUG, String.Format(format, args))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub info(msg As String, <CallerMemberName> Optional obj$ = Nothing)
            Call WriteLine(msg, obj, MSG_TYPES.INF)

            If Not split Is Nothing Then
                Call split(obj, msg, MSG_TYPES.INF)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub log(level As MSG_TYPES, msg As StringBuilder, <CallerMemberName> Optional obj$ = Nothing)
            Call log(level, msg.ToString, obj)
        End Sub

        Public Sub log(level As MSG_TYPES, msg As String, <CallerMemberName> Optional obj$ = Nothing)
            Call WriteLine(msg, obj, level)

            If Not split Is Nothing Then
                Call split(obj, msg, level)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub LogException(msg As String, <CallerMemberName> Optional obj$ = Nothing)
            Call WriteLine(msg, obj, type:=MSG_TYPES.ERR)

            If Not split Is Nothing Then
                Call split(obj, msg, MSG_TYPES.ERR)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub LogException(ex As Exception, <CallerMemberName> Optional obj$ = Nothing)
            Call WriteLine(ex.ToString, obj, type:=MSG_TYPES.ERR)

            If Not split Is Nothing Then
                Call split(obj, ex.ToString, MSG_TYPES.ERR)
            End If
        End Sub

        ''' <summary>
        ''' 向日志文件之中写入数据
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        Public Sub WriteLine(msg As String,
                             <CallerMemberName>
                             Optional obj As String = Nothing,
                             Optional type As MSG_TYPES = MSG_TYPES.INF)

            Dim log As New LogEntry With {
                .message = msg,
                .[object] = obj,
                .time = Now,
                .level = type
            }

            buffer.WriteLine(log.ToString)
            counts += 1
            saved = False
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{counts} records]'{filePath.ToFileURL}'"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteLine(Optional s As String = "")
            Call WriteLine(s, type:=MSG_TYPES.INF, obj:="")
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteLine(s As String())
            Call WriteLine(String.Join(vbCrLf, s), type:=MSG_TYPES.INF, obj:="")
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="args">{[Object] As String, Optional Type As MsgType = MsgType.INF, Optional WriteToScreen As Boolean = True}</param>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteLine(s As String, ParamArray args() As String)
            Call WriteLine(s, type:=MSG_TYPES.INF, obj:=If(String.IsNullOrEmpty(args(0)), "", args(0)))
        End Sub

        ''' <summary>
        ''' 给出用于调试的系统的信息摘要
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function SystemInfo() As String
            Dim sb As New StringBuilder(1024)

            Call sb.AppendLine($"{NameOf(OSVersionInfo.BuildVersion)}:={OSVersionInfo.BuildVersion}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.Edition)}:={OSVersionInfo.Edition}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.MajorVersion)}:={OSVersionInfo.MajorVersion}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.MinorVersion)}:={OSVersionInfo.MinorVersion}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.WindowsName)}:={OSVersionInfo.WindowsName}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.OSBits)}:={OSVersionInfo.OSBits}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.ProcessorBits)}:={OSVersionInfo.ProcessorBits}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.ProgramBits)}:={OSVersionInfo.ProgramBits}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.RevisionVersion)}:={OSVersionInfo.RevisionVersion}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.ServicePack)}:={OSVersionInfo.ServicePack}")
            Call sb.AppendLine($"{NameOf(OSVersionInfo.Version)}:={OSVersionInfo.Version.ToString}")

            Return sb.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Save()
            If saved Then
                Call buffer.Flush()
            Else
                Call SaveLog()
            End If
        End Sub

        Dim saved As Boolean = False

        ''' <summary>
        ''' 会自动拓展已经存在的日志数据
        ''' </summary>
        ''' <remarks></remarks>
        Private Function SaveLog() As Boolean
            Call buffer.WriteLine(vbCrLf & $"//{vbTab}{New String("=", 25)}  END OF LOG FILE  {New String("=", 25)}")
            Call buffer.WriteLine(vbCrLf)
            Call buffer.Flush()

            saved = True

            Return True
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call SaveLog()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
