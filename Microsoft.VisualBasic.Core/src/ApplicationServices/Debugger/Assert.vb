#Region "Microsoft.VisualBasic::276c64f851dc419160b00c9a8e554e6c, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Assert.vb"

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

    '   Total Lines: 24
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 1.10 KB


    '     Class Assert
    ' 
    '         Sub: AreEqual, IsTrue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My

Namespace ApplicationServices.Debugging

    Public Class Assert

        Public Shared Sub AreEqual(Of T As IComparable(Of T))(w As T, v As T, <CallerMemberName> Optional caller As String = Nothing)
            If w.CompareTo(v) = 0 Then
                Call Log4VB.Print($"[{caller}] AreEqual" & vbCrLf, ConsoleColor.White, ConsoleColor.Green)
            Else
                Call Log4VB.Print($"[{caller}] {w} and {v} those two value are not equal!" & vbCrLf, ConsoleColor.Yellow, ConsoleColor.Red)
            End If
        End Sub

        Public Shared Sub IsTrue(v As Boolean, <CallerMemberName> Optional caller As String = Nothing)
            If v Then
                Call Log4VB.Print($"[{caller}] assert is {v.ToString.ToUpper}" & vbCrLf, ConsoleColor.White, ConsoleColor.Green)
            Else
                Call Log4VB.Print($"[{caller}] assert is {v.ToString.ToUpper}" & vbCrLf, ConsoleColor.Yellow, ConsoleColor.Red)
            End If
        End Sub
    End Class
End Namespace
