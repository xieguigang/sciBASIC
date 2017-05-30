#Region "Microsoft.VisualBasic::6885a4989b4bb087c5df2cf35bcfaa5f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\TestProject\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Text

Module Module1

    Public Structure Foo

        Dim s$

        Public Shared Operator Like(f As Foo, s$) As Double
            Return Levenshtein.ComputeDistance(f.s, s).MatchSimilarity
        End Operator
    End Structure

    Sub New()



        'Dim test As Boolean

        '' If test Then Call method()
        '' Or
        'If test Then
        '    Call method()
        'End If

        'Dim i%

        'If i = 10 Then
        '    Call method()
        'End If

        'Dim test2 = Function(str$) As Boolean
        '                ' blabla
        '            End Function

        'If test2(str:="blabla") Then
        '    Call method()
        'End If

        'If Not test2(str:="blabla") Then
        '    Call Sub()
        '             ' blabla
        '         End Sub
        'End If


        '        Dim test As Boolean

        '        Call test ? method()

        '  Dim i%

        '        Call (i = 10) ? method()

        'Dim test2 = Function(str$) As Boolean
        '                ' blabla
        '            End Function

        '        Call test2(str:="blablabla") ? method()

        'Call (Not test2(str:="blabla")) ? Sub()
        '                                      ' balbala
        '                                  End Sub



    End Sub

End Module
