#Region "Microsoft.VisualBasic::16ec340cf6b6bdf6afb77030a33f9f1b, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Exception\MethodFrame.vb"

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

    '   Total Lines: 30
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 854 B


    '     Class Method
    ' 
    '         Properties: [Module], [Namespace], Method
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace ApplicationServices.Debugging.Diagnostics

#Disable Warning BC40000 ' Type or member is obsolete
    <ClassInterface(ClassInterfaceType.AutoDual)>
    <ComVisible(True)>
    Public Class Method
#Enable Warning BC40000 ' Type or member is obsolete

        Public Property [Namespace] As String
        Public Property [Module] As String
        Public Property Method As String

        Sub New()
        End Sub

        Sub New(s As String)
            Dim t = s.Split("."c).AsList

            Method = t(-1)
            [Module] = t(-2)
            [Namespace] = t.Take(t.Count - 2).JoinBy(".")
        End Sub

        Public Overrides Function ToString() As String
            Return $"{[Namespace]}.{[Module]}.{Method}"
        End Function
    End Class
End Namespace
