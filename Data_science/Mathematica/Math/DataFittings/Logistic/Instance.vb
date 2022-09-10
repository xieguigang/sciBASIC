#Region "Microsoft.VisualBasic::6d2635bf635f89f7414c290d5ad04a79, sciBASIC#\Data_science\Mathematica\Math\DataFittings\Logistic\Instance.vb"

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

    '   Total Lines: 38
    '    Code Lines: 21
    ' Comment Lines: 11
    '   Blank Lines: 6
    '     File Size: 928 B


    ' Class Instance
    ' 
    '     Properties: featureSize, label, x
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' classify training model
''' </summary>
Public Class Instance

    ''' <summary>
    ''' the real label data
    ''' </summary>
    ''' <returns></returns>
    Public Property label As Integer
    ''' <summary>
    ''' the object properties vector
    ''' </summary>
    ''' <returns></returns>
    Public Property x As Double()

    Public ReadOnly Property featureSize As Integer
        Get
            Return x.Length
        End Get
    End Property

    Public Sub New(label As Integer, x As Integer())
        Me.label = label
        Me.x = x.Select(Function(d) CDbl(d)).ToArray
    End Sub

    Public Sub New(label As Integer, x As Double())
        Me.label = label
        Me.x = x
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{label}] {x.GetJson}"
    End Function
End Class
