#Region "Microsoft.VisualBasic::855eb33ea1f3dc3429afb02d669a7cdd, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Tools\Logging\LogFile\LogEntry.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Namespace Logging

    Public Structure LogEntry

        Public Property Msg As String
        Public Property [Object] As String
        Public Property [Type] As MSG_TYPES
        Public Property Time As Date

        ''' <summary>
        ''' 生成日志文档之中的一行记录数据
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim obj As String = TrimObject()
            Dim str As String

            If Msg.Contains(vbCr) OrElse Msg.Contains(vbLf) Then  '多行模式
                str = $"[{Time.ToString}][{Type.ToString}][{[obj]}]{vbCrLf}{Msg}"
            Else                '单行模式
                str = $"[{Time.ToString}][{Type.ToString}][{[obj]}] {Msg}"
            End If

            Return str & vbCrLf
        End Function

        Private Function TrimObject() As String
            Dim str As String = Me.Object.Replace(vbCr, " ").Replace(vbLf, " ")
            Return str
        End Function
    End Structure
End Namespace
