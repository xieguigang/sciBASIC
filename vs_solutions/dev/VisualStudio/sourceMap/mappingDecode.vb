#Region "Microsoft.VisualBasic::d050dc125f2e05fbca1ec5699ac806f6, vs_solutions\dev\VisualStudio\sourceMap\mappingDecode.vb"

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

    '     Module mappingDecode
    ' 
    '         Function: decodeMappings, decodeVLQ
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace SourceMap

    Public Module mappingDecode

        <Extension>
        Public Iterator Function decodeMappings(map As sourceMap) As IEnumerable(Of mappingLine)
            Dim lines As String() = map.mappings.Split(";"c)

            For Each line As String In lines
                If line.StringEmpty Then
                    Yield New mappingLine
                Else
                    For Each col As String In line.Split(","c)
                        Yield decodeVLQ(encode:=col)
                    Next
                End If
            Next
        End Function

        Private Function decodeVLQ(encode As String) As mappingLine
            Dim locations As Integer() = base64VLQ.getIntegers(encode).ToArray

            If locations.Length < 5 Then ReDim Preserve locations(4)

            Return New mappingLine With {
                .targetCol = locations(0),
                .fileIndex = locations(1),
                .sourceLine = locations(2),
                .sourceCol = locations(3),
                .nameIndex = locations(4)
            }
        End Function
    End Module
End Namespace
