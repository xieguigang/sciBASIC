#Region "Microsoft.VisualBasic::7e96b3a06bf92ff0538880b8bef42b5d, ..\sciBASIC#\mime\text%yaml\yaml\Serialization\Serialization.vb"

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
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.MIME.text.yaml.Grammar
Imports Microsoft.VisualBasic.MIME.text.yaml.Syntax
Imports Microsoft.VisualBasic.Text

Public Module Serialization

    <Extension>
    Public Function LoadYAML(Of T)(path As String) As T
        Dim input As New TextInput(path.GET)
        Dim success As Boolean
        Dim parser As New YamlParser()
        Dim yamlStream As YamlStream = parser.ParseYamlStream(input, success)

        If success Then
            Return yamlStream.Load(Of T)
        Else
            Dim ex As New Exception(parser.GetEorrorMessages())
            Throw New Exception(path.ToFileURL, ex)
        End If
    End Function

    <Extension>
    Public Function Load(Of T)(yaml As YamlStream) As T
        Dim type As Type = GetType(T)
        Dim maps As Dictionary(Of MappingEntry) = yaml.Enumerative.FirstOrDefault

        If maps Is Nothing Then
            Return Nothing
        Else
            Return DirectCast(maps.__setMaps(type), T)
        End If
    End Function

    <Extension>
    Private Function __setMaps(maps As Dictionary(Of MappingEntry), type As Type) As Object
        Dim obj As Object = Activator.CreateInstance(type)
        Dim schema As BindProperty(Of DataFrameColumnAttribute)() =
            DataFrameColumnAttribute.LoadMapping(type, mapsAll:=True) _
                                    .Values _
                                    .ToArray
        Dim value As Object
        Dim s As String

        For Each prop As BindProperty(Of DataFrameColumnAttribute) In schema
            If Not maps.ContainsKey(prop.Identity) Then
                Continue For
            End If

            If prop.IsPrimitive Then
                s = maps(prop.Identity).Value.ToString
                value = Scripting.CTypeDynamic(s, prop.Type)
            Else
                Try
                    Dim subMaps As Dictionary(Of MappingEntry)
                    value = maps(prop.Identity).Value
                    subMaps = New Dictionary(Of MappingEntry)(DirectCast(value, Mapping).Enties)
                    value = subMaps.__setMaps(prop.Type)
                Catch ex As Exception
                    ex = New Exception($"Dim {prop.Identity} As {prop.Type.FullName}", ex)
                    ex = New Exception(type.FullName, ex)
#If DEBUG Then
                    Call ex.PrintException
#End If
                    Throw ex
                End Try
            End If

            Call prop.SetValue(obj, value)
        Next

        Return obj
    End Function

    <Extension>
    Public Function WriteYAML(Of T)(
                    obj As T,
                    path As String,
           Optional encoding As Encodings = Encodings.Unicode) _
                             As Boolean

        Dim sb As New StringBuilder(1024)
        Dim schema = DataFrameColumnAttribute.LoadMapping(Of T)(mapsAll:=True)

        Throw New NotImplementedException
    End Function
End Module
