#Region "Microsoft.VisualBasic::2d14c75dd20f9151efdb77eb52b8746b, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\RandomPermutations.vb"

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

    '   Total Lines: 32
    '    Code Lines: 23 (71.88%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (28.12%)
    '     File Size: 970 B


    '     Class RandomPermutations
    ' 
    '         Function: (+2 Overloads) getRandom
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions

Namespace ShapleyValue

    Public Class RandomPermutations

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function getRandom(min As Integer, max As Integer) As Integer
            Return rand.NextInteger(min, max + 1)
        End Function

        Public Shared Function getRandom(size As Long) As IList(Of Integer)

            Dim res As IList(Of Integer) = New List(Of Integer)()
            Dim temp As IList(Of Integer) = New List(Of Integer)()
            For i As Integer = 1 To size
                temp.Add(i)
            Next

            While temp.Count > 0
                Dim random = getRandom(0, temp.Count - 1)
                res.Add(temp(random))
                temp.RemoveAt(random)
            End While

            Return res
        End Function

    End Class

End Namespace

