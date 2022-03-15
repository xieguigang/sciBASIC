#Region "Microsoft.VisualBasic::a7fdf9972bc354f9de754c115e60c2cd, sciBASIC#\vs_solutions\dev\VisualStudio\sourceMap\mappingEncode.vb"

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

    '   Total Lines: 64
    '    Code Lines: 49
    ' Comment Lines: 6
    '   Blank Lines: 9
    '     File Size: 2.46 KB


    '     Module mappingEncode
    ' 
    '         Function: encode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace SourceMap

    Public Module mappingEncode

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="loci"></param>
        ''' <param name="file">target source file after encode</param>
        ''' <returns></returns>
        <Extension>
        Public Function encode(loci As IEnumerable(Of StackFrame), file$) As sourceMap
            Dim lines As New List(Of mappingLine())
            Dim symbols As New Index(Of String)
            Dim files = loci.GroupBy(Function(f) f.File).ToArray
            Dim sourceFile As IGrouping(Of Integer, StackFrame)()
            Dim mapLine As New List(Of mappingLine)
            Dim symbolName As String

            For i As Integer = 0 To files.Length - 1
                sourceFile = files(i) _
                    .GroupBy(Function(a) If(a.Line = "n/a", 0, Integer.Parse(a.Line))) _
                    .OrderBy(Function(a) a.Key) _
                    .ToArray

                For Each line As IGrouping(Of Integer, StackFrame) In sourceFile
                    For Each col As StackFrame In line
                        symbolName = col.Method.Method.Trim(""""c)

                        If Not symbolName Like symbols Then
                            Call symbols.Add(symbolName)
                        End If

                        mapLine += New mappingLine With {
                            .fileIndex = i,
                            .nameIndex = symbols(symbolName),
                            .sourceCol = 1,
                            .sourceLine = line.Key,
                            .targetCol = 1
                        }
                    Next

                    lines += mapLine.PopAll
                Next
            Next

            Return New sourceMap With {
                .version = 3,
                .file = file,
                .sourceRoot = "",
                .sources = files _
                    .Select(Function(filegroup) filegroup.Key) _
                    .ToArray,
                .mappings = lines.Select(Function(line) line.JoinBy(",")).JoinBy(";"),
                .names = symbols.Objects
            }
        End Function
    End Module
End Namespace
