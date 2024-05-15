#Region "Microsoft.VisualBasic::05b6c2fec4c87e15722b6014b7ffc19b, Data_science\DataMining\DataMining\ComponentModel\Encoder\Variable\CategoricalEncoder.vb"

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

    '   Total Lines: 82
    '    Code Lines: 68
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 3.18 KB


    '     Class CategoricalEncoder
    ' 
    '         Properties: properties
    ' 
    '         Function: CreateEncoder, (+2 Overloads) EncodeBinary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ComponentModel.Encoder.Variable

    Public Class CategoricalEncoder

        Public Property properties As Dictionary(Of String, String())
            Get
                Return propertyNames _
                    .ToDictionary(Function(d) d.Name,
                                  Function(d)
                                      Return d.Value _
                                          .Select(Function(x) x.value) _
                                          .ToArray
                                  End Function)
            End Get
            Set(value As Dictionary(Of String, String()))
                propertyNames = value _
                    .Select(Function(t)
                                Return New NamedValue(Of (String, String)()) With {
                                    .Name = t.Key,
                                    .Value = t.Value _
                                        .Select(Function(v) ($"{t.Key}.{v}", v)) _
                                        .ToArray
                                }
                            End Function) _
                    .ToArray
            End Set
        End Property

        Dim propertyNames As NamedValue(Of (propertyName$, value$)())()

        Public Shared Function EncodeBinary(content As IEnumerable(Of Categorical), Optional ByRef encoder As CategoricalEncoder = Nothing) As IEnumerable(Of Binary)
            Dim raw As Categorical() = content.ToArray

            If encoder Is Nothing Then
                encoder = CreateEncoder(raw)
            End If

            Return encoder.EncodeBinary(content)
        End Function

        Private Shared Function CreateEncoder(raw As Categorical()) As CategoricalEncoder
            Dim propList As New Dictionary(Of String, String())
            Dim nlen As Integer = raw(Scan0).Length

            For i As Integer = 0 To nlen - 1
#Disable Warning
                propList($"#{i + 1}") = raw _
                    .Select(Function(r) r(i)) _
                    .Distinct _
                    .OrderBy(Function(str) str) _
                    .ToArray
#Enable Warning
            Next

            Return New CategoricalEncoder With {
                .properties = propList
            }
        End Function

        Public Iterator Function EncodeBinary(contents As IEnumerable(Of Categorical)) As IEnumerable(Of Binary)
            For Each item As Categorical In contents
                Dim bin As New List(Of Boolean)

                For i As Integer = 0 To propertyNames.Length - 1
                    Dim dimension = propertyNames(i)
                    Dim value As String = item(i)

                    For j As Integer = 0 To dimension.Value.Length - 1
                        bin.Add(dimension.Value(i).value = value)
                    Next
                Next

                Yield New Binary With {
                    .entityVector = bin.ToArray,
                    .id = item.id
                }
            Next
        End Function
    End Class
End Namespace
