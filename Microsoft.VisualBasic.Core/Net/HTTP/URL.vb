#Region "Microsoft.VisualBasic::35583026f2d6070a7b91c461a292f95d, Microsoft.VisualBasic.Core\Net\HTTP\URL.vb"

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

'     Class URL
' 
'         Properties: hashcode, hostName, path, port, protocol
'                     query
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: BuildUrl, GetValues, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Net.Http

    Public Class URL

        ''' <summary>
        ''' the url query parameters
        ''' </summary>
        ''' <returns></returns>
        Public Property query As Dictionary(Of String, String())
        Public Property path As String
        Public Property hostName As String
        Public Property port As Integer
        Public Property protocol As String
        ''' <summary>
        ''' #....
        ''' </summary>
        ''' <returns></returns>
        Public Property hashcode As String

        Default Public ReadOnly Property getArgumentVal(query As String) As String
            Get
                With GetValues(query)
                    If .IsNothing Then
                        Return Nothing
                    Else
                        Return DirectCast(.GetValue(Scan0), String)
                    End If
                End With
            End Get
        End Property

        Sub New(url As String)
            With url.GetTagValue("?", trim:=True, failureNoName:=False)
                Dim tokens = .Value.Split("#"c)
                Dim tmp$

                If tokens.Length > 1 Then
                    hashcode = tokens.Last
                    tmp = tokens.Take(tokens.Length - 1).JoinBy("#")
                Else
                    tmp = tokens.FirstOrDefault
                End If

                query = tmp _
                    .Split("&"c) _
                    .ParseUrlQueryParameters() _
                    .ToDictionary(allStrings:=True) _
                    .TryCast(Of Dictionary(Of String, String()))

                url = .Name
            End With

            For Each type As String In WebServiceUtils.Protocols
                If Strings.InStr(url, type, CompareMethod.Text) = 1 Then
                    protocol = type
                    Exit For
                End If
            Next

            If protocol.StringEmpty Then
                protocol = "http://"
                port = 80
                hostName = "localhost"
                path = url.Trim("/"c)
            Else
                url = url.Substring(protocol.Length + 1)

                With url.GetTagValue("/", trim:=False, failureNoName:=False)
                    hostName = .Name
                    path = .Value

                    With hostName.GetTagValue(":", False, True)
                        hostName = .Name

                        If .Value = "" Then
                            Select Case protocol
                                Case "http://"
                                    port = 80
                                Case "https://"
                                    port = 443
                                Case "ftp://"
                                    port = 21
                                Case "sftp://"
                                    port = 22
                                Case Else
                                    port = -1
                            End Select
                        Else
                            port = Integer.Parse(.Value)
                        End If
                    End With
                End With
            End If
        End Sub

        Private Sub New()
        End Sub

        Public Function GetValues(query As String) As String()
            With LCase(query)
                If .DoCall(AddressOf _query.ContainsKey) Then
                    Return .DoCall(Function(key) _query(key))
                Else
                    Return Nothing
                End If
            End With
        End Function

        Public Overrides Function ToString() As String
            Return $"{protocol}{hostName}:{port}/{path}?{query.Select(Function(q) q.Value.Select(Function(val) $"{q.Key}={UrlEncode(val)}")).IteratesALL.JoinBy("&")}#{hashcode}"
        End Function

        Public Shared Function BuildUrl(url As String, query As IEnumerable(Of NamedValue(Of String))) As URL
            Return New URL With {
                .hostName = "",
                .port = -1,
                .path = url,
                .hashcode = "",
                .protocol = "?",
                .query = query _
                    .GroupBy(Function(a) a.Name.ToLower) _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return a _
                                          .Select(Function(t) t.Value) _
                                          .ToArray
                                  End Function)
            }
        End Function
    End Class
End Namespace
