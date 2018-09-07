#Region "Microsoft.VisualBasic::924fc1b8663c4354b3c9e32903e8a169, Microsoft.VisualBasic.Core\Text\LogFile.vb"

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

    '     Class LogFile
    ' 
    ' 
    '         Interface ISupportLoggingClient
    ' 
    '             Properties: Logging
    ' 
    '             Function: WriteLog
    ' 
    '         Enum MSG_TYPES
    ' 
    '             DEBUG, ERR, INF, WRN
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: ColorfulOutput, FileName, Mute, NowTimeNormalizedString, SuppressError
    '                 SuppressWarns
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: InternalFillBlanks, Read, ReadLine, Save, ToString
    ' 
    '     Sub: Dispose, (+2 Overloads) LogException, SaveLog, (+4 Overloads) WriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Logging

    ''' <summary>
    ''' 日志文件记录模块.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class LogFile : Inherits ComponentModel.ITextFile

        Implements Microsoft.VisualBasic.ComponentModel.ITextFile.I_FileSaveHandle
        Implements System.IDisposable
        Implements Microsoft.VisualBasic.ConsoleDevice.STDIO__.I_ConsoleDeviceHandle

        Public Interface ISupportLoggingClient
            Inherits System.IDisposable

#Region "Public Property"

            ReadOnly Property Logging As Logging.LogFile

#End Region

#Region "Public Methods"

            Function WriteLog() As Boolean

#End Region

        End Interface

        ''' <summary>
        ''' The types enumeration of the log file message.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum MSG_TYPES As Integer
            ''' <summary>
            ''' The normal information message.[WHITE]
            ''' </summary>
            ''' <remarks></remarks>
            INF
            ''' <summary>
            ''' The program error information message.[RED]
            ''' </summary>
            ''' <remarks></remarks>
            ERR
            ''' <summary>
            ''' Warnning message from the program.[YELLOW]
            ''' </summary>
            ''' <remarks></remarks>
            WRN
            ''' <summary>
            ''' The program debug information message.[BLUE]
            ''' </summary>
            ''' <remarks></remarks>
            DEBUG
        End Enum

        Dim _InternalLogsDataBuilder As StringBuilder = New StringBuilder(1024)
        Dim _InternalRecordCounts As Integer

        ''' <summary>
        ''' Indicated that write the <see cref="LogFile.MSG_TYPES.ERR">Error type</see> message to the console screen, this 
        ''' property will override the WriteToScreen parameter in function <see cref="LogFile.WriteLine"></see> when the 
        ''' message type is <see cref="LogFile.MSG_TYPES.ERR">Error type</see>.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SuppressError As Boolean = True
        ''' <summary>
        ''' Indicated that write the <see cref="LogFile.MSG_TYPES.WRN">Warn type</see> message to the console screen, this 
        ''' property will override the WriteToScreen parameter in function <see cref="LogFile.WriteLine"></see> when the 
        ''' message type is <see cref="LogFile.MSG_TYPES.WRN">Warn type</see>.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SuppressWarns As Boolean = True

        ''' <summary>
        ''' 是否采用彩色的输出，默认为关闭：
        ''' <see cref="Logging.LogFile.MSG_TYPES.INF">一般的消息</see> - 白色; 
        ''' <see cref="Logging.LogFile.MSG_TYPES.WRN">警告级别的消息</see> - 黄色; 
        ''' <see cref="Logging.LogFile.MSG_TYPES.ERR">错误级别的消息</see> - 红色
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
                Return IO.Path.GetFileNameWithoutExtension(path:=FilePath)
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
                Return String.Format("{0}{1}{2}{3}{4}", Format(Now.Month, "00"), Format(Now.Day, "00"), Format(Now.Hour, "00"), Format(Now.Minute, "00"), Format(Now.Second, "00"))
            End Get
        End Property

        Public Sub LogException(Msg As String, [Object] As String, Optional WriteToScreen As Boolean = True)
            Call WriteLine(Msg, [Object], Type:=MSG_TYPES.ERR, WriteToScreen:=WriteToScreen)
        End Sub

        Public Sub LogException(ex As Exception)
            Call WriteLine(ex.ToString, "", Type:=MSG_TYPES.ERR, WriteToScreen:=True)
        End Sub

        Public Sub WriteLine(Msg As String, [Object] As String, Optional Type As MSG_TYPES = MSG_TYPES.INF, Optional WriteToScreen As Boolean = True)
            Dim LoggingHead As String = InternalFillBlanks([Object], 20)
            Dim sBuilder As New StringBuilder(1024)
            Dim Lines As String() = Strings.Split(Msg.Replace(vbCr, ""), vbLf)

            If Lines.Count > 1 Then
                Call sBuilder.AppendLine(vbTab)
            End If

            If Me._addElapsedTime Then
                LoggingHead = String.Format("[p-DEBUG: {0}s] {1} - {2}  [{3}][{4}]: ", Me._Sw.ElapsedMilliseconds / 1000, Now.ToString, Now.Millisecond, Type.ToString, LoggingHead)
            Else
                LoggingHead = String.Format("{0} - {1}  [{2}][{3}]: ", Now.ToString, Now.Millisecond, Type.ToString, LoggingHead)
            End If

            If Not Mute AndAlso WriteToScreen Then
                If Me.SuppressError AndAlso Type = MSG_TYPES.ERR Then
                    'Do Nothing
                ElseIf Me.SuppressWarns AndAlso Type = MSG_TYPES.WRN Then
                    'Do Nothing
                Else
                    If Me.ColorfulOutput Then

                        Select Case Type
                            Case MSG_TYPES.INF : Console.ForegroundColor = ConsoleColor.White
                            Case MSG_TYPES.WRN : Console.ForegroundColor = ConsoleColor.Yellow
                            Case MSG_TYPES.ERR : Console.ForegroundColor = ConsoleColor.Magenta
                            Case MSG_TYPES.DEBUG : Console.ForegroundColor = ConsoleColor.Blue
                            Case Else
                                Console.ForegroundColor = ConsoleColor.White
                        End Select

                    End If

                    Call Console.WriteLine(Msg)
                End If
            End If

            Call sBuilder.AppendLine(Lines.First)
            If Lines.Count > 1 Then
                Call sBuilder.AppendLine(String.Join(vbCrLf & vbTab, Lines.Skip(1).ToArray))
                Call sBuilder.AppendLine()

                LoggingHead = vbCrLf & LoggingHead
            End If
            Call _InternalLogsDataBuilder.Append(String.Format("{0}{1}", LoggingHead, sBuilder.ToString))

            _InternalRecordCounts += 1
        End Sub

        ''' <summary>
        ''' 当这个设置为真之后，终端就不会再有任何的输出了
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Mute As Boolean

        Friend Function InternalFillBlanks(str As String, Blanks As Integer) As String
            Dim Blank As String = New String(" "c, IIf(Blanks < 0, 20, Blanks))
            Dim strlen As Integer = Len(str)

            If strlen < Blanks Then
                str = Mid(Blank, 1, Blanks - strlen) & str
            End If

            Return str
        End Function

        Dim _Sw As Stopwatch, _addElapsedTime As Boolean

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Path">This logfile will saved to.</param>
        ''' <remarks></remarks>
        ''' <param name="bufferSize">当日志的记录数目达到这个数目的时候就会将日志数据写入到文件之中</param>
        Public Sub New(Path As String,
                       Optional addTimeTag As Boolean = False,
                       Optional bufferSize As Integer = 1024)
            Dim strBuffer As String = New String("-", 40)

            strBuffer = String.Format("{0} {1} {2}", strBuffer, Now.ToLongDateString & " - " & Now.ToLongTimeString, strBuffer)

            Call _InternalLogsDataBuilder.AppendLine(strBuffer)
            Call _InternalLogsDataBuilder.AppendLine()

            MyBase.FilePath = Path

            Dim Dir As String = FileIO.FileSystem.GetParentPath(Path)
            Call FileIO.FileSystem.CreateDirectory(Dir)

            _addElapsedTime = addTimeTag
            If _addElapsedTime Then
                Me._Sw = Stopwatch.StartNew
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="appendToLogFile">Append this log data into the target log file if the file is exists on the filesystem, default option is override the exists file.</param>
        ''' <remarks></remarks>
        Public Sub SaveLog(Optional appendToLogFile As Boolean = False)
            Dim Dir As String = FileIO.FileSystem.GetParentPath(FilePath)
            Dim strBuffer As String = New String("-", 20)

            strBuffer = String.Format("{0}{1} END_OF_LOGGING {2}", vbCrLf, strBuffer, strBuffer)

            Call _InternalLogsDataBuilder.AppendLine(strBuffer)
            Call FileIO.FileSystem.CreateDirectory(Dir)

            strBuffer = If(appendToLogFile = True, vbCrLf & vbCrLf & vbCrLf, "")
            strBuffer = strBuffer & _InternalLogsDataBuilder.ToString

            Try
                Call FileIO.FileSystem.WriteAllText(FilePath, strBuffer, appendToLogFile)
            Catch ex As Exception
                Call Console.WriteLine("Exception occur while trying save the logfile into location: {0}", FilePath)
                Throw
            End Try
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("[{0} records]'{1}'", _InternalRecordCounts, Extensions.ToFileURL(FilePath))
        End Function

        Protected Overrides Sub Dispose(disposing As Boolean)
            Call SaveLog(appendToLogFile:=True)
            Call MyBase.Dispose(disposing)
        End Sub

        Public Function ReadLine() As String Implements Microsoft.VisualBasic.ConsoleDevice.STDIO__.I_ConsoleDeviceHandle.ReadLine
            Throw New NotImplementedException
        End Function

        Public Sub WriteLine(s As String) Implements Microsoft.VisualBasic.ConsoleDevice.STDIO__.I_ConsoleDeviceHandle.WriteLine
            Call WriteLine(s, Type:=MSG_TYPES.INF, [Object]:="", WriteToScreen:=True)
        End Sub

        Public Sub WriteLine(s As String())
            Dim str As String = String.Join(vbCrLf, s)
            Call WriteLine(str, Type:=MSG_TYPES.INF, [Object]:="", WriteToScreen:=True)
        End Sub

        Private Shared ReadOnly ArgumentsBlank As String() = New String() {"", "", "", ""}

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="args">{[Object] As String, Optional Type As MsgType = MsgType.INF, Optional WriteToScreen As Boolean = True}</param>
        ''' <remarks></remarks>
        Public Sub WriteLine(s As String, ParamArray args() As String) Implements Microsoft.VisualBasic.ConsoleDevice.STDIO__.I_ConsoleDeviceHandle.WriteLine
            Dim [Object] As String = IIf(String.IsNullOrEmpty(args(0)), "", args(0))
            Call WriteLine(s, Type:=MSG_TYPES.INF, [Object]:=[Object], WriteToScreen:=True)
        End Sub

        Public Overloads Function Read() As Integer Implements Microsoft.VisualBasic.ConsoleDevice.STDIO__.I_ConsoleDeviceHandle.Read
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' 直接进行覆盖的
        ''' </summary>
        ''' <param name="FilePath"></param>
        ''' <param name="Encoding"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            FilePath = getPath(FilePath)
            Call SaveLog(False)
            Return True
        End Function
    End Class
End Namespace
