#Region "Microsoft.VisualBasic::7a20566e73748360dced94e40916c499, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\Logging\LogFile\ISupportLoggingClient.vb"

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

Namespace ApplicationServices.Debugging.Logging

    Public Interface ISupportLoggingClient
        Inherits System.IDisposable

#Region "Public Property"

        ReadOnly Property Logging As Logging.LogFile

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Save the log file into the filesystem.(保存日志数据到文件系统之中)
        ''' </summary>
        ''' <returns></returns>
        Function WriteLog() As Boolean
#End Region

    End Interface
End Namespace
