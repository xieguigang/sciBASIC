#Region "Microsoft.VisualBasic::d683442136b583450775eadeb6a173e1, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/SignalProcessing/SignalProcessing//COW/FunctionElement.vb"

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

    '   Total Lines: 29
    '    Code Lines: 19
    ' Comment Lines: 3
    '   Blank Lines: 7
    '     File Size: 771 B


    '     Class FunctionElement
    ' 
    '         Properties: Score, Trace, Warp
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.Serialization.JSON

Namespace COW

    ''' <summary>
    ''' This class is used in dynamic programming algorithm of CowAlignment.cs.
    ''' </summary>
    Friend Class FunctionElement

        Public Property Warp As Integer
        Public Property Score As Double
        Public Property Trace As TraceDirection

        Public Sub New(score As Double, trace As TraceDirection)
            _Score = score
            _Trace = trace
        End Sub

        Public Sub New(score As Double, warp As Integer)
            _Score = score
            _Warp = warp
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

