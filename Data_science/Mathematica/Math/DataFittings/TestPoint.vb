#Region "Microsoft.VisualBasic::7da5369975bb158fc9590ccf9947006c, Data_science\Mathematica\Math\DataFittings\TestPoint.vb"

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

    '   Total Lines: 37
    '    Code Lines: 29 (78.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (21.62%)
    '     File Size: 1.09 KB


    ' Interface IFitError
    ' 
    '     Properties: Y, Yfit
    ' 
    ' Class TestPoint
    ' 
    '     Properties: Err, Y, Yfit
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Math.Scripting

Public Interface IFitError

    Property Y As Double
    Property Yfit As Double

End Interface

<XmlType("point", [Namespace]:="http://scibasic.net/math/Bootstrapping")>
Public Class TestPoint : Inherits DataPoint
    Implements IFitError

    <XmlAttribute("y")>
    Public Overrides Property Y As Double Implements IFitError.Y
    <XmlAttribute("fx")>
    Public Property Yfit As Double Implements IFitError.Yfit

    <XmlIgnore>
    Public ReadOnly Property Err As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Y - Yfit
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{X.ToString("F2")}, {Y.ToString("F2")}] {Yfit.ToString("F2")}"
    End Function

    Public Overloads Shared Narrowing Operator CType(point As TestPoint) As PointF
        Return New PointF(point.X, point.Y)
    End Operator
End Class
