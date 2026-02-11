#Region "Microsoft.VisualBasic::2d44365a2057b1d4acd515b8950d5639, Microsoft.VisualBasic.Core\src\LLMs.vb"

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

    '   Total Lines: 23
    '    Code Lines: 13 (56.52%)
    ' Comment Lines: 7 (30.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (13.04%)
    '     File Size: 639 B


    ' Module LLMs
    ' 
    '     Function: LLMsTalk
    ' 
    '     Sub: HookOllama
    ' 
    ' /********************************************************************************/

#End Region

Public Module LLMs

    Dim ollama As Func(Of String, String)

    Public Sub HookOllama(chat As Func(Of String, String))
        ollama = chat
    End Sub

    ''' <summary>
    ''' A proxy function for LLMs talk in framework runtime
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <returns>
    ''' this proxy function will returns nothing if there is no ollama client is hooked
    ''' </returns>
    Public Function LLMsTalk(msg As String) As String
        If ollama Is Nothing Then
            Return Nothing
        Else
            Return ollama(msg)
        End If
    End Function
End Module

