#Region "Microsoft.VisualBasic::fab3ae5c290a92e4f4abb735728660a5, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Language\C\RandomNumbers.vb"

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

'----------------------------------------------------------------------------------------
'	Copyright © 2006 - 2012 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class provides the ability to simulate the behavior of the C/C++ functions for 
'	generating random numbers, using the .NET Framework System.Random class.
'	'rand' converts to the parameterless overload of NextNumber
'	'random' converts to the single-parameter overload of NextNumber
'	'randomize' converts to the parameterless overload of Seed
'	'srand' converts to the single-parameter overload of Seed
'----------------------------------------------------------------------------------------

Namespace Language.C

    ''' <summary>
    ''' 
    ''' </summary>
    Public Module RandomNumbers

        Dim r As Random

        Sub New()
            Call Seed()
        End Sub

        Public Function NextNumber() As Integer
            Return r.[Next]()
        End Function

        Public Function NextNumber(ceiling As Integer) As Integer
            Return r.[Next](ceiling)
        End Function

        Public Sub Seed()
            r = New Random()
        End Sub

        Public Sub Seed(seed__1 As Integer)
            r = New Random(seed__1)
        End Sub
    End Module
End Namespace
