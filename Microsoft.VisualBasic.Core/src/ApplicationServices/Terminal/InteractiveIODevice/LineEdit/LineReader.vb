#Region "Microsoft.VisualBasic::8a9d1014a836969d4de5ed4cee39ed1a, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\LineEdit\LineReader.vb"

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

    '   Total Lines: 25
    '    Code Lines: 19 (76.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (24.00%)
    '     File Size: 779 B


    '     Class LineReader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ReadLine
    ' 
    '         Sub: SetPrompt
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.STDIO__

Namespace ApplicationServices.Terminal.LineEdit

    Public Class LineReader : Implements IShellDevice

        Dim prompt As String
        Dim line As LineEditor

        Sub New(reader As LineEditor)
            line = reader
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetPrompt(s As String) Implements IShellDevice.SetPrompt
            prompt = s
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadLine() As String Implements IShellDevice.ReadLine
            Return line.Edit(prompt, "")
        End Function
    End Class
End Namespace
