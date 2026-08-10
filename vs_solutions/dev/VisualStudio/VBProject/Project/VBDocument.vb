#Region "Microsoft.VisualBasic::567e416ac9b8362ebae3348b5cabc3ea, vs_solutions\dev\VisualStudio\VBProject\Project\VBDocument.vb"

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

    '   Total Lines: 53
    '    Code Lines: 23 (43.40%)
    ' Comment Lines: 20 (37.74%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (18.87%)
    '     File Size: 1.58 KB


    '     Class VBDocument
    ' 
    '         Properties: [Imports], FileName, Types
    ' 
    '         Function: ToString
    ' 
    '     Class [Imports]
    ' 
    '         Properties: [Alias], [Namespace]
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.CodeDOM
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace VBProj

    Public Class VBDocument

        ''' <summary>
        ''' relative path to the vbproj file
        ''' </summary>
        ''' <returns></returns>
        Public Property FileName As String
        ''' <summary>
        ''' namespace imports list
        ''' </summary>
        ''' <returns></returns>
        Public Property [Imports] As String()
        ''' <summary>
        ''' language symbols that parsed from current vb.net source file document text
        ''' </summary>
        ''' <returns></returns>
        Public Property Types As Dictionary(Of String, LanguageSymbolType)

        Public Overrides Function ToString() As String
            Return $"{FileName.FileName} {Types.Keys.GetJson}"
        End Function

    End Class

    Public Class [Imports]

        ''' <summary>
        ''' Imports XXX
        ''' </summary>
        ''' <returns></returns>
        Public Property [Namespace] As String
        ''' <summary>
        ''' Imports X = XXX
        ''' </summary>
        ''' <returns></returns>
        Public Property [Alias] As String

        Public Overrides Function ToString() As String
            If [Alias].StringEmpty(, True) Then
                Return $"Imports {[Namespace]}"
            Else
                Return $"Imports {[Alias]} = {[Namespace]}"
            End If
        End Function

    End Class

End Namespace
