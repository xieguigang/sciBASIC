#Region "Microsoft.VisualBasic::2a4d04afb02bf131d60746245962ea53, Data_science\Mathematica\Math\Randomizer\FastRandom\ThreadSafeFastRandom.vb"

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

    '   Total Lines: 123
    '    Code Lines: 72 (58.54%)
    ' Comment Lines: 24 (19.51%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 27 (21.95%)
    '     File Size: 4.44 KB


    ' Module ThreadSafeFastRandom
    ' 
    '     Function: (+3 Overloads) [Next], GetGlobalSeed, NextFloat
    ' 
    '     Sub: NextFloats
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Public Module ThreadSafeFastRandom

    ReadOnly _global As New Random()

    <ThreadStatic>
    Dim _local As FastRandom

    Private Function GetGlobalSeed() As Integer
        Dim seed As Integer

        SyncLock ThreadSafeFastRandom._global
            seed = ThreadSafeFastRandom._global.Next()
        End SyncLock

        Return seed
    End Function

    ''' <summary>
    ''' Returns a non-negative random integer.
    ''' </summary>
    ''' <returns>A 32-bit signed integer that is greater than or equal to 0 and less than System.Int32.MaxValue.</returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [Next]() As Integer
        Dim inst = ThreadSafeFastRandom._local

        If inst Is Nothing Then
            Dim seed As Integer
            seed = ThreadSafeFastRandom.GetGlobalSeed()
            inst = New FastRandom(seed)

            ThreadSafeFastRandom._local = inst
        End If

        Return inst.Next()
    End Function

    ''' <summary>
    ''' Returns a non-negative random integer that is less than the specified maximum.
    ''' </summary>
    ''' <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to 0.</param>
    ''' <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue; that is, the range of return values ordinarily includes 0 but not maxValue. However,
    ''' if maxValue equals 0, maxValue is returned.</returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [Next](maxValue As Integer) As Integer
        Dim inst = ThreadSafeFastRandom._local

        If inst Is Nothing Then
            Dim seed As Integer
            seed = ThreadSafeFastRandom.GetGlobalSeed()
            inst = New FastRandom(seed)

            ThreadSafeFastRandom._local = inst
        End If

        Dim ans As Integer

        Do
            ans = inst.Next(maxValue)
        Loop While ans = maxValue

        Return ans
    End Function

    ''' <summary>
    ''' Returns a random integer that is within a specified range.
    ''' </summary>
    ''' <param name="minValue">The inclusive lower bound of the random number returned.</param>
    ''' <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
    ''' <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue
    ''' equals maxValue, minValue is returned.</returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [Next](minValue As Integer, maxValue As Integer) As Integer
        Dim inst = ThreadSafeFastRandom._local

        If inst Is Nothing Then
            Dim seed As Integer
            seed = ThreadSafeFastRandom.GetGlobalSeed()
            inst = New FastRandom(seed)

            ThreadSafeFastRandom._local = inst
        End If

        Return inst.Next(minValue, maxValue)
    End Function

    ''' <summary>
    ''' Generates a random float. Values returned are from 0.0 up to but not including 1.0.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function NextFloat() As Single
        Dim inst = ThreadSafeFastRandom._local

        If inst Is Nothing Then
            Dim seed As Integer
            seed = ThreadSafeFastRandom.GetGlobalSeed()
            inst = New FastRandom(seed)

            ThreadSafeFastRandom._local = inst
        End If

        Return inst.NextFloat()
    End Function

    ''' <summary>
    ''' Fills the elements of a specified array of bytes with random numbers.
    ''' </summary>
    ''' <param name="buffer">An array of bytes to contain random numbers.</param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub NextFloats(buffer As Double())
        Dim inst As FastRandom = ThreadSafeFastRandom._local

        If inst Is Nothing Then
            Dim seed As Integer
            seed = ThreadSafeFastRandom.GetGlobalSeed()
            inst = New FastRandom(seed)
            ThreadSafeFastRandom._local = inst
        End If

        inst.NextFloats(buffer)
    End Sub
End Module
