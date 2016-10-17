#Region "Microsoft.VisualBasic::f9b8b6f83934a5c6af0f10a66fde2316, ..\visualbasic_App\Data\DataFrame\Extensions\ProfileStreams.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Public Module ProfileStreams

    <Extension>
    Public Function WriteProfiles(Of T As IProfile)(x As T, path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim buf As ProfileTable() = x.GenerateProfiles
        Return buf.SaveTo(path, encoding.GetEncodings)
    End Function

    <Extension>
    Public Function GenerateProfiles(Of T As IProfile)(x As T) As ProfileTable()
        Dim settings As New Settings(Of T)(x)
        Dim buf As ProfileTable() =
            LinqAPI.Exec(Of ProfileTable) <= From config As BindMapping
                                             In settings.AllItems
                                             Select New ProfileTable(config)
        Return buf
    End Function

    <Extension>
    Public Function LoadProfiles(Of T As IProfile)(path As String, Optional encoding As Encodings = Encodings.UTF8) As T
        Dim buf As ProfileTable() = path.LoadCsv(Of ProfileTable)(encoding:=encoding.GetEncodings).ToArray
        Dim config As Settings(Of T) = Settings(Of T).CreateEmpty

        For Each x As ProfileTable In buf
            Call x.SetValue(config)
        Next

        Return config.SettingsData
    End Function
End Module
