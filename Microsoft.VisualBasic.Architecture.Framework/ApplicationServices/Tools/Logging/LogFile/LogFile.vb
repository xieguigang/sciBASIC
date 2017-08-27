#Region "Microsoft.VisualBasic::0d1304b649890642b584336377f8bf07, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\Logging\LogFile\LogFile.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.SoftwareToolkits
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Terminal.STDIO__
Imports System.IO

Namespace Logging

    ''' <summary>
    ''' 日志文件记录模块.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class LogFile : Inherits ITextFile

        Implements ISaveHandle
        Implements IDisposable
        Implements I_ConsoleDeviceHandle

        Dim _LogsEntry As New List(Of LogEntry)
        Dim _RecordCounts As Long

        ''' <summary>
        ''' Indicated that write the <see cref="MSG_TYPES.ERR">Error type</see> message to the console screen, this 
        ''' property will override the WriteToScreen parameter in function <see cref="LogFile.WriteLine"></see> when the 
        ''' message type is <see cref="MSG_TYPES.ERR">Error type</see>.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SuppressError As Boolean = True
        ''' <summary>
        ''' Indicated that write the <see cref="MSG_TYPES.WRN">Warn type</see> message to the console screen, this 
        ''' property will override the WriteToScreen parameter in function <see cref="LogFile.WriteLine"></see> when the 
        ''' message type is <see cref="MSG_TYPES.WRN">Warn type</see>.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SuppressWarns As Boolean = True

        ''' <summary>
        ''' 是否采用彩色的输出，默认为关闭：
        ''' <see cref="Logging.MSG_TYPES.INF">一般的消息</see> - 白色; 
        ''' <see cref="Logging.MSG_TYPES.WRN">警告级别的消息</see> - 黄色; 
        ''' <see cref="Logging.MSG_TYPES.ERR">错误级别的消息</see> - 红色
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ColorfulOutput As Boolean = False

        ''' <summary>
        ''' 没有路径名称和拓展名，仅包含有单独的文件名
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FileName As String
            Get
                Return MyBase.FilePath.BaseName
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

        Public Sub LogException(Msg As String, [Object] As String, Optional WriteToScreen As Boolean = True)
            Call WriteLine(Msg, [Object], Type:=MSG_TYPES.ERR, WriteToScreen:=WriteToScreen)
        End Sub

        Public Sub LogException(ex As Exception)
            Call WriteLine(ex.ToString, "", Type:=MSG_TYPES.ERR, WriteToScreen:=True)
        End Sub

        ''' <summary>
        ''' 向日志文件之中写入数据
        ''' </summary>
        ''' <param name="Msg"></param>
        ''' <param name="[Object]"></param>
        ''' <param name="Type"></param>
        ''' <param name="WriteToScreen"></param>
        Public Sub WriteLine(Msg As String, [Object] As String, Optional Type As MSG_TYPES = MSG_TYPES.INF, Optional WriteToScreen As Boolean = True)
            If Not Mute AndAlso WriteToScreen Then
                Call __printLogs(Msg, Type)
            End If

            Dim LogEntry As New LogEntry With {
                .Msg = Msg,
                .Object = [Object],
                .Time = Now,
                .Type = Type
            }

            If Me._autoFlush Then  '实时写入文件的，则不在内存之中记录数据了
                Call __pending(LogEntry)
            Else
                Call Me._LogsEntry.Add(LogEntry)
            End If

            _RecordCounts += 1
        End Sub

        Dim __autoFlushQueue As New ThreadQueue

        Private Sub __pending(x As LogEntry)
            Call __autoFlushQueue.AddToQueue(Sub() Call _file.WriteLine(x.ToString))
        End Sub

        Private Sub __printLogs(Message As String, Type As MSG_TYPES)
            If Me.SuppressError AndAlso Type = MSG_TYPES.ERR Then
                Return   'Do Nothing
            ElseIf Me.SuppressWarns AndAlso Type = MSG_TYPES.WRN Then
                Return    'Do Nothing
            Else
                Call __setColor(Type)
            End If

            Call Console.WriteLine(Message)
        End Sub

        Private Sub __setColor(Type As MSG_TYPES)
            If Not Me.ColorfulOutput Then Return

            Select Case Type
                Case MSG_TYPES.INF : Console.ForegroundColor = ConsoleColor.White
                Case MSG_TYPES.WRN : Console.ForegroundColor = ConsoleColor.Yellow
                Case MSG_TYPES.ERR : Console.ForegroundColor = ConsoleColor.Magenta
                Case MSG_TYPES.DEBUG : Console.ForegroundColor = ConsoleColor.Blue
                Case Else
                    Console.ForegroundColor = ConsoleColor.White
            End Select
        End Sub

        ''' <summary>
        ''' 当这个设置为真之后，终端就不会再有任何的输出了
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Mute As Boolean

        Public Sub AutoFlush(state As Boolean)
            _autoFlush = state

            If _autoFlush Then
                Call FileIO.FileSystem.WriteAllText(Me.FilePath, GenerateDocument, append:=True)
            End If
        End Sub

        Dim _autoFlush As Boolean
        Dim _file As StreamWriter

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Path">This logfile will saved to.</param>
        ''' <remarks></remarks>
        ''' <param name="bufferSize">当日志的记录数目达到这个数目的时候就会将日志数据写入到文件之中</param>
        Public Sub New(Path As String, Optional AutoFlush As Boolean = True, Optional bufferSize As Integer = 1024)
            Me.FilePath = FileIO.FileSystem.GetFileInfo(Path).FullName
            Me._autoFlush = AutoFlush

            If Not Path.FileExists Then
                Call "".SaveTo(Path)
            End If

            Dim file As New FileStream(Path, FileMode.Append)

            _file = New StreamWriter(file)
            _file.WriteLine($"//{vbTab}[{Now.ToString}]{vbTab}{New String("=", 25)}  START WRITE LOGGING SECTION  {New String("=", 25)}" & vbCrLf)
        End Sub

        ''' <summary>
        ''' 会自动拓展已经存在的日志数据
        ''' </summary>
        ''' <remarks></remarks>
        Public Function SaveLog() As Boolean
            Dim bufs As New StringBuilder(GenerateDocument)

            Call bufs.AppendLine(vbCrLf & $"//{vbTab}{New String("=", 25)}  END OF LOG FILE  {New String("=", 25)}")
            Call bufs.AppendLine(vbCrLf)
            Call __autoFlushQueue.AddToQueue(Sub() _file.WriteLine(bufs.ToString))

            Return True
        End Function

        Private Function GenerateDocument() As String
            Dim strBuffer = String.Join(vbCrLf, (From line In Me._LogsEntry Select str = line.ToString).ToArray)
            Call Me._LogsEntry.Clear()
            Return strBuffer
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("[{0} records]'{1}'", _RecordCounts, ToFileURL(FilePath))
        End Function

        Public Function ReadLine() As String Implements I_ConsoleDeviceHandle.ReadLine
            Return ""
        End Function

        Public Sub WriteLine(s As String) Implements I_ConsoleDeviceHandle.WriteLine
            Call WriteLine(s, Type:=MSG_TYPES.INF, [Object]:="", WriteToScreen:=True)
        End Sub

        Public Sub WriteLine(s As String())
            Dim str As String = String.Join(vbCrLf, s)
            Call WriteLine(str, Type:=MSG_TYPES.INF, [Object]:="", WriteToScreen:=True)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="args">{[Object] As String, Optional Type As MsgType = MsgType.INF, Optional WriteToScreen As Boolean = True}</param>
        ''' <remarks></remarks>
        Public Sub WriteLine(s As String, ParamArray args() As String) Implements I_ConsoleDeviceHandle.WriteLine
            Dim [Object] As String = IIf(String.IsNullOrEmpty(args(0)), "", args(0))
            Call WriteLine(s, Type:=MSG_TYPES.INF, [Object]:=[Object], WriteToScreen:=True)
        End Sub

        Public Overloads Function Read() As Integer Implements I_ConsoleDeviceHandle.Read
            Return -1
        End Function

        ''' <summary>
        ''' 日志文件在保存的时候默认是追加的方式
        ''' </summary>
        ''' <param name="FilePath"></param>
        ''' <param name="Encoding"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            FilePath = getPath(FilePath)
            Return SaveLog()
        End Function

        ''' <summary>
        ''' 给出用于调试的系统的信息摘要
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function SystemInfo() As String
            Dim sBuilder As New StringBuilder(1024)

            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.BuildVersion)}:={OSVersionInfo.BuildVersion}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.Edition)}:={OSVersionInfo.Edition}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.MajorVersion)}:={OSVersionInfo.MajorVersion}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.MinorVersion)}:={OSVersionInfo.MinorVersion}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.WindowsName)}:={OSVersionInfo.WindowsName}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.OSBits)}:={OSVersionInfo.OSBits}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.ProcessorBits)}:={OSVersionInfo.ProcessorBits}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.ProgramBits)}:={OSVersionInfo.ProgramBits}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.RevisionVersion)}:={OSVersionInfo.RevisionVersion}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.ServicePack)}:={OSVersionInfo.ServicePack}")
            Call sBuilder.AppendLine($"{NameOf(OSVersionInfo.Version)}:={OSVersionInfo.Version.ToString}")

            Return sBuilder.ToString
        End Function

        Protected Overrides Function __getDefaultPath() As String
            Return FilePath
        End Function

        Protected Overrides Sub Dispose(disposing As Boolean)
            Call __autoFlushQueue.WaitQueue()
            Call _file.Dispose()

            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
