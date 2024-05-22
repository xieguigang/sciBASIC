#Region "Microsoft.VisualBasic::99119a63f12643437907543ea0afd405, Microsoft.VisualBasic.Core\test\test\objToStringTest.vb"

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
    '    Code Lines: 17 (73.91%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (26.09%)
    '     File Size: 854 B


    ' Module objToStringTest
    ' 
    '     Sub: Main1
    ' 
    ' Class overridesToString
    ' 
    '     Function: ToString
    ' 
    ' Class notOverridesToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Module objToStringTest

    Sub Main1()
        Dim m1 = GetType(overridesToString).GetMethods.Where(Function(m) m.Name = "ToString" AndAlso m.GetParameters.IsNullOrEmpty AndAlso m.GetGenericArguments.IsNullOrEmpty).First
        Dim m2 = GetType(notOverridesToString).GetMethods.Where(Function(m) m.Name = "ToString" AndAlso m.GetParameters.IsNullOrEmpty AndAlso m.GetGenericArguments.IsNullOrEmpty).First
        Dim objToString = GetType(Object).GetMethod("ToString")

        Console.WriteLine(m1.DeclaringType Is objToString.DeclaringType)
        Console.WriteLine(m2.DeclaringType Is objToString.DeclaringType)

        Pause()
    End Sub
End Module

Public Class overridesToString
    Public Overrides Function ToString() As String
        Return "1"
    End Function
End Class

Public Class notOverridesToString

End Class
