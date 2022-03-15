#Region "Microsoft.VisualBasic::86bf442bb41ea4eee0d47035b9175240, sciBASIC#\Microsoft.VisualBasic.Core\src\Language\Language\C\RandomNumbers.vb"

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

    '   Total Lines: 61
    '    Code Lines: 24
    ' Comment Lines: 29
    '   Blank Lines: 8
    '     File Size: 2.35 KB


    '     Module RandomNumbers
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: rand, random
    ' 
    '         Sub: randomize, srand
    ' 
    ' 
    ' /********************************************************************************/

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
    ''' This class provides the ability to simulate the behavior of the C/C++ functions for 
    '''	generating random numbers, using the .NET Framework <see cref="System.Random"/> class.
    '''	
    '''	+ ``rand`` converts to the parameterless overload of NextNumber
    '''	+ ``random`` converts to the single-parameter overload of NextNumber
    '''	+ ``randomize`` converts to the parameterless overload of Seed
    '''	+ ``srand`` converts to the single-parameter overload of Seed
    ''' </summary>
    Public Module RandomNumbers

        Dim r As Random

        Sub New()
            Call randomize()
        End Sub

        ''' <summary>
        ''' <see cref="System.Random.Next"/>.(线程安全的函数)
        ''' </summary>
        ''' <returns></returns>
        Public Function rand() As Integer
            SyncLock r
                Return r.[Next]()
            End SyncLock
        End Function

        ''' <summary>
        ''' <see cref="System.Random.Next(Integer)"/>.(线程安全的函数)
        ''' </summary>
        ''' <param name="ceiling"></param>
        ''' <returns></returns>
        Public Function random(ceiling As Integer) As Integer
            SyncLock r
                Return r.[Next](ceiling)
            End SyncLock
        End Function

        Public Sub randomize()
            r = New Random(Now.Millisecond)
        End Sub

        Public Sub srand(seed__1 As Integer)
            r = New Random(seed__1)
        End Sub
    End Module
End Namespace
