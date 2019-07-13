#Region "Microsoft.VisualBasic::ab00a99e8d49a425b35339f455296714, Data\DataFrame\Extensions\ProfileStreams.vb"

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

    ' Module ProfileStreams
    ' 
    '     Function: GenerateProfiles, LoadProfiles, WriteProfiles
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Provides the ``*.ini`` file like config data function
''' </summary>
Public Module ProfileStreams

    ''' <summary>
    ''' Save target configuration model as the csv table
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="x"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function WriteProfiles(Of T As {New, IProfile})(x As T, path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim buf As ProfileTable() = x.GenerateProfiles
        Return buf.SaveTo(path, encoding:=encoding.CodePage)
    End Function

    <Extension>
    Public Function GenerateProfiles(Of T As {New, IProfile})(x As T) As ProfileTable()
        Dim settings As New Settings(Of T)(x)
        Dim buf As ProfileTable() =
            LinqAPI.Exec(Of ProfileTable) <= From config As BindMapping
                                             In settings.AllItems
                                             Select New ProfileTable(config)
        Return buf
    End Function

    ''' <summary>
    ''' Load config data from the csv table into the configuration model
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadProfiles(Of T As {New, IProfile})(path As String, Optional encoding As Encodings = Encodings.UTF8) As T
        Dim buf As ProfileTable() = path.LoadCsv(Of ProfileTable)(encoding:=encoding.CodePage).ToArray
        Dim config As Settings(Of T) = Settings(Of T).CreateEmpty

        For Each x As ProfileTable In buf
            Call x.SetValue(config)
        Next

        Return config.SettingsData
    End Function
End Module
