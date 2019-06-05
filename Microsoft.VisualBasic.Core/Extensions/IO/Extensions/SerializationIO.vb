#Region "Microsoft.VisualBasic::7a765edf1bb9fdcc491562405755fbd8, Microsoft.VisualBasic.Core\Extensions\IO\Extensions\SerializationIO.vb"

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

    ' Module SerializationIO
    ' 
    '     Function: DumpSerial, SolveListStream
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Public Module SerializationIO

    ''' <summary>
    ''' Save as a tsv file, with data format like: 
    ''' 
    ''' ```
    ''' <see cref="NamedValue(Of String).Name"/>\t<see cref="NamedValue(Of String).Value"/>\t<see cref="NamedValue(Of String).Description"/>
    ''' ```
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveAsTabularMapping(source As IEnumerable(Of NamedValue(Of String)),
                                         path$,
                                         Optional saveDescrib As Boolean = False,
                                         Optional saveHeaders$() = Nothing,
                                         Optional encoding As Encodings = Encodings.ASCII) As Boolean
        Dim content = source _
            .Select(Function(row)
                        With row
                            If saveDescrib Then
                                Return $"{ .Name}{ASCII.TAB}{ .Value}{ASCII.TAB}{ .Description}"
                            Else
                                Return $"{ .Name}{ASCII.TAB}{ .Value}"
                            End If
                        End With
                    End Function)

        If saveHeaders.IsNullOrEmpty Then
            Return content.SaveTo(path, encoding.CodePage)
        Else
            Return {saveHeaders.JoinBy(ASCII.TAB)}.JoinIterates(content).SaveTo(path, encoding.CodePage)
        End If
    End Function

    <Extension>
    Public Function SolveListStream(path$, Optional encoding As Encoding = Nothing) As IEnumerable(Of String)
        Select Case path.ExtensionSuffix.ToLower
            Case "", "txt"
                Return path.IterateAllLines
            Case "json"
                Return path.LoadJSON(Of String())
            Case "xml"
                Return path.LoadXml(Of String())
            Case Else
                Throw New NotImplementedException(path)
        End Select
    End Function

    <Extension>
    Public Function DumpSerial(points As IEnumerable(Of PointF), csv$, Optional encoding As Encodings = Encodings.UTF8WithoutBOM) As Boolean
        Using writer As StreamWriter = csv.OpenWriter(encoding)
            Call writer.WriteLine("X,Y")

            For Each pt As PointF In points
                Call writer.WriteLine($"{pt.X},{pt.Y}")
            Next
        End Using

        Return True
    End Function
End Module
