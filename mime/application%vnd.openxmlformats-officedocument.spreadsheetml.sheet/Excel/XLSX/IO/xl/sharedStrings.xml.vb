#Region "Microsoft.VisualBasic::480977bfe62091e854d1732b0be43176, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\xl\sharedStrings.xml.vb"

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


    ' Code Statistics:

    '   Total Lines: 105
    '    Code Lines: 72 (68.57%)
    ' Comment Lines: 18 (17.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (14.29%)
    '     File Size: 3.30 KB


    '     Class sharedStrings
    ' 
    '         Properties: count, strings, uniqueCount
    ' 
    '         Function: ToHashTable
    '         Operators: +
    ' 
    '     Class si
    ' 
    '         Properties: phoneticPr, styled, t
    ' 
    '         Function: ToString
    ' 
    '     Class phoneticPr
    ' 
    '         Properties: fontId, type
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq

Namespace XLSX.XML.xl

    <XmlRoot("sst", [Namespace]:="http://schemas.openxmlformats.org/spreadsheetml/2006/main")>
    Public Class sharedStrings

        ''' <summary>
        ''' get count of the <see cref="strings"/>
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public ReadOnly Property count As Integer
            Get
                If strings.IsNullOrEmpty Then
                    Return 0
                Else
                    Return strings.Length
                End If
            End Get
        End Property

        <XmlAttribute>
        Public ReadOnly Property uniqueCount As Integer
            Get
                If strings.IsNullOrEmpty Then
                    Return 0
                End If

                Return strings.GroupBy(Function(si) si.t).Count
            End Get
        End Property

        <XmlElement("si")>
        Public Property strings As si()

        Public Function ToHashTable() As Dictionary(Of String, Integer)
            Return strings _
                .SeqIterator _
                .ToDictionary(Function(x) x.value.t,
                              Function(x)
                                  Return x.i
                              End Function)
        End Function

        ''' <summary>
        ''' Append new values to <see cref="strings"/>
        ''' </summary>
        ''' <param name="strings"></param>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Shared Operator +(strings As sharedStrings, table As Dictionary(Of String, Integer)) As sharedStrings
            Dim newValues = table _
                .OrderBy(Function(x) x.Value) _
                .Skip(strings.count) _
                .Select(Function(x)
                            Return New si With {
                                .t = x.Key
                            }
                        End Function) _
                .ToArray

            If newValues.Length > 0 Then
                strings.strings = strings.strings _
                    .JoinIterates(newValues) _
                    .ToArray
            End If

            Return strings
        End Operator
    End Class

    Public Class si

        Public Property t As String
        Public Property phoneticPr As phoneticPr

        ''' <summary>
        ''' a string with component part styled list
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("r")>
        Public Property styled As si()

        ''' <summary>
        ''' get string value of current shared string object.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            If styled.IsNullOrEmpty Then
                Return t
            Else
                Return styled _
                    .Select(Function(str) str.ToString) _
                    .JoinBy("")
            End If
        End Function
    End Class

    Public Class phoneticPr
        <XmlAttribute> Public Property fontId As String
        <XmlAttribute> Public Property type As String
    End Class
End Namespace
