#Region "Microsoft.VisualBasic::a46c9d81b6a0aedcaf5e4655df1c051a, Microsoft.VisualBasic.Core\src\Language\Language\C\RandomNumbers.vb"

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

'   Total Lines: 65
'    Code Lines: 27 (41.54%)
' Comment Lines: 29 (44.62%)
'    - Xml Docs: 41.38%
' 
'   Blank Lines: 9 (13.85%)
'     File Size: 2.51 KB


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

Imports System.Runtime.CompilerServices
Imports System.Threading

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

        ''' <summary>
        ''' use thread local for may thread safe
        ''' </summary>
        Dim r As New ThreadLocal(Of Random)(Function() New Random(Guid.NewGuid().GetHashCode()))

        Sub New()
            Call randomize()
        End Sub

        ''' <summary>
        ''' <see cref="System.Random.Next"/>.(线程安全的函数)
        ''' </summary>
        ''' <returns></returns>
        Public Function rand() As Integer
            Return r.Value.[Next]()
        End Function

        ''' <summary>
        ''' <see cref="System.Random.Next(Integer)"/>.(线程安全的函数)
        ''' </summary>
        ''' <param name="ceiling"></param>
        ''' <returns></returns>
        Public Function random(ceiling As Integer) As Integer
            Return r.Value.[Next](ceiling)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub randomize()
            r = New ThreadLocal(Of Random)(Function() New Random(Guid.NewGuid().GetHashCode()))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub srand(seed As Integer)
            r = New ThreadLocal(Of Random)(Function() New Random(seed))
        End Sub
    End Module
End Namespace
