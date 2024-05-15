#Region "Microsoft.VisualBasic::64e1c6ac91a3ecf0921a7bfd7da9880a, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\LineEdit\Handler.vb"

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

    '   Total Lines: 39
    '    Code Lines: 30
    ' Comment Lines: 1
    '   Blank Lines: 8
    '     File Size: 1.58 KB


    '     Structure Handler
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Alt, Control
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Terminal.LineEdit

    Friend Structure Handler
        Public CKI As ConsoleKeyInfo
        Public KeyHandler As KeyHandler
        Public ResetCompletion As Boolean

        Public Sub New(key As ConsoleKey, h As KeyHandler, Optional resetCompletion As Boolean = True)
            CKI = New ConsoleKeyInfo(ASCII.NUL, key, False, False, False)
            KeyHandler = h
            Me.ResetCompletion = resetCompletion
        End Sub

        Public Sub New(c As Char, h As KeyHandler, Optional resetCompletion As Boolean = True)
            KeyHandler = h
            ' Use the "Zoom" as a flag that we only have a character.
            CKI = New ConsoleKeyInfo(c, ConsoleKey.Zoom, False, False, False)
            Me.ResetCompletion = resetCompletion
        End Sub

        Public Sub New(cki As ConsoleKeyInfo, h As KeyHandler, Optional resetCompletion As Boolean = True)
            Me.CKI = cki
            KeyHandler = h
            Me.ResetCompletion = resetCompletion
        End Sub

        Public Shared Function Control(c As Char, h As KeyHandler, Optional resetCompletion As Boolean = True) As Handler
            Return New Handler(ChrW(AscW(c) - ASCII.A + 1), h, resetCompletion)
        End Function

        Public Shared Function Alt(c As Char, k As ConsoleKey, h As KeyHandler) As Handler
            Dim cki As New ConsoleKeyInfo(c, k, False, True, False)
            Return New Handler(cki, h)
        End Function
    End Structure

End Namespace
