#Region "Microsoft.VisualBasic::52c7b5d623f7837bb5050e174713ab0c, vs_solutions\dev\VisualStudio\sourceMap\mappingLine.vb"

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

    '     Class mappingLine
    ' 
    '         Properties: fileIndex, isEmpty, nameIndex, sourceCol, sourceLine
    '                     targetCol
    ' 
    '         Function: GetStackFrame, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace SourceMap

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class mappingLine

        ''' <summary>
        ''' 第一位，表示这个位置在（转换后的代码的）的第几列。
        ''' </summary>
        ''' <returns></returns>
        Public Property targetCol As Integer
        ''' <summary>
        ''' 第二位，表示这个位置属于sources属性中的哪一个文件。
        ''' </summary>
        ''' <returns></returns>
        Public Property fileIndex As Integer
        ''' <summary>
        ''' 第三位，表示这个位置属于转换前代码的第几行。
        ''' </summary>
        ''' <returns></returns>
        Public Property sourceLine As Integer
        ''' <summary>
        ''' 第四位，表示这个位置属于转换前代码的第几列。
        ''' </summary>
        ''' <returns></returns>
        Public Property sourceCol As Integer
        ''' <summary>
        ''' 第五位，表示这个位置属于names属性中的哪一个变量。
        ''' </summary>
        ''' <returns></returns>
        Public Property nameIndex As Integer

        Private ReadOnly Property isEmpty As Boolean
            Get
                Return targetCol = 0 AndAlso
                    fileIndex = 0 AndAlso
                    sourceLine = 0 AndAlso
                    sourceCol = 0 AndAlso
                    nameIndex = 0
            End Get
        End Property

        Public Function GetStackFrame(map As sourceMap) As StackFrame
            If isEmpty Then
                ' return empty info
                Return New StackFrame
            End If

            Return New StackFrame With {
                .File = map.sources(fileIndex),
                .Line = sourceLine,
                .Method = New Method With {
                    .Method = map.names.ElementAtOrDefault(nameIndex, "N/A")
                }
            }
        End Function

        Public Overrides Function ToString() As String
            Return New Integer() {
                targetCol, fileIndex, sourceLine, sourceCol, nameIndex
            } _
                .Select(AddressOf base64VLQ.base64VLQ_encode) _
                .JoinBy("")
        End Function
    End Class
End Namespace
