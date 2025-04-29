#Region "Microsoft.VisualBasic::f0e65f1904fb4f63b5a148c1c554ab3b, vs_solutions\dev\VisualStudio\test\Module2.vb"

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
    '     File Size: 671 B


    ' Module Module2
    ' 
    '     Sub: Main
    ' 
    ' Class aaaa
    ' 
    '     Function: bbbb
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Module Module2

    Sub Main()
        Dim method = GetType(aaaa).GetMethod(NameOf(aaaa.bbbb))
        Dim result = New MethodAnalyzer().AnalyzeMethod(method)

        Call Console.WriteLine(result.ToString)
        Call Pause()
    End Sub
End Module

Public Class aaaa

    Public Function bbbb(a As String, b As Double) As Object
        Dim rand As New Random
        Dim d As Long = rand.NextDouble * b
        Return New NamedValue(Of Single)(a & Now.ToShortDateString, CDbl(d))
    End Function

End Class
