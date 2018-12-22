#Region "Microsoft.VisualBasic::87a91b33dbed7d9ea08027e159a992f0, Microsoft.VisualBasic.Core\ComponentModel\Settings\Inf\IniFile.vb"

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

'     Class IniFile
' 
'         Properties: path
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GetPrivateProfileString, ReadValue, ToString, WritePrivateProfileString
' 
'         Sub: WriteValue
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' Ini file I/O handler
    ''' </summary>
    Public Class IniFile

        Public ReadOnly Property path As String

        Public ReadOnly Property FileExists As Boolean
            Get
                Return path.FileLength > -1
            End Get
        End Property

        ''' <summary>
        ''' Open a ini file handle.
        ''' </summary>
        ''' <param name="INIPath"></param>
        Public Sub New(INIPath As String)
            path = IO.Path.GetFullPath(PathMapper.GetMapPath(INIPath))
        End Sub

        Public Overrides Function ToString() As String
            Return path.ToFileURL
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteValue(section As String, key As String, value As String)
            Call WritePrivateProfileString(section, key, value, Me.path)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadValue(section As String, key As String) As String
            Return GetPrivateProfileString(section, key, Me.path)
        End Function
    End Class
End Namespace
