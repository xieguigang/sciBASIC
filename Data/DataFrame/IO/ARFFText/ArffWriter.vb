#Region "Microsoft.VisualBasic::6dc739553eb6154c6690089fc77726db, Data\DataFrame\IO\ARFFText\ArffWriter.vb"

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

    '   Total Lines: 83
    '    Code Lines: 64 (77.11%)
    ' Comment Lines: 1 (1.20%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (21.69%)
    '     File Size: 2.75 KB


    '     Module ArffWriter
    ' 
    '         Function: desc
    ' 
    '         Sub: (+2 Overloads) WriteText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Linq

Namespace IO.ArffFile

    Public Module ArffWriter

        Public Sub WriteText(d As DataFrame, s As Stream)
            Using str As New StreamWriter(s)
                Call d.WriteText(str)
            End Using
        End Sub

        Friend Const DataframeArffRowNames As String = "_row.names"

        <Extension>
        Public Sub WriteText(d As DataFrame, s As TextWriter)
            Dim offsets As String() = d.featureNames

            Call s.WriteLine($"@relation {If(d.name, "no named").CLIToken}")
            Call s.WriteLine()

            If Not d.description.StringEmpty Then
                For Each line As String In d.description.LineTokens
                    Call s.WriteLine($"% {line}")
                Next

                Call s.WriteLine()
            End If

            If Not d.rownames.IsNullOrEmpty Then
                Call s.WriteLine("@attribute " & DataframeArffRowNames)
            End If

            For Each col As String In offsets
                Call s.WriteLine($"@attribute {col.CLIToken} {ArffWriter.desc(d(col))}")
            Next

            Call s.WriteLine()
            Call s.WriteLine("@data")

            Dim rows As Integer = d.nsamples
            Dim checkRowNames As Boolean = Not d.rownames.IsNullOrEmpty

            For i As Integer = 0 To rows - 1
                Dim row As Object() = d.row(i)
                Dim line As String = New RowObject(row).AsLine

                If checkRowNames Then
                    Call s.Write($"{d.rownames(i).CLIToken},")
                End If

                Call s.WriteLine(line)
            Next

            Call s.Flush()
        End Sub

        Private Function desc(v As FeatureVector) As String
            Select Case v.type
                Case GetType(Integer) : Return "int"
                Case GetType(Long) : Return "long"
                Case GetType(Single) : Return "float"
                Case GetType(Double) : Return "numeric"
                Case GetType(Boolean) : Return "boolean"
                Case Else
                    Dim uniq = v.vector _
                        .AsObjectEnumerator _
                        .Distinct _
                        .ToArray

                    If v.size / uniq.Length > 10 Then
                        ' is enum
                        Return "{" & New RowObject(uniq).AsLine & "}"
                    Else
                        Return "chr"
                    End If
            End Select
        End Function
    End Module
End Namespace
