#Region "Microsoft.VisualBasic::b76fc85e206fb5f0190c0a35a2e5b4ed, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\Powerset.vb"

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

    '   Total Lines: 20
    '    Code Lines: 13 (65.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (35.00%)
    '     File Size: 536 B


    '     Class Powerset
    ' 
    '         Function: calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ShapleyValue

    Public Class Powerset

        Public Shared ReadOnly nullSet As New HashSet(Of Integer)()

        Public Shared Function calculate(nbElements As Integer) As IEnumerable(Of Integer())
            Dim inputSet As New HashSet(Of Integer)()

            For i As Integer = 1 To nbElements
                inputSet.Add(i)
            Next

            Dim result As IEnumerable(Of Integer()) = inputSet.PowerSet()

            Return result
        End Function

    End Class
End Namespace

