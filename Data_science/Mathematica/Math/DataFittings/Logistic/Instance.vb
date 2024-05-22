#Region "Microsoft.VisualBasic::74999d925b1135b696144ab7cd4b30b8, Data_science\Mathematica\Math\DataFittings\Logistic\Instance.vb"

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

    '   Total Lines: 74
    '    Code Lines: 44 (59.46%)
    ' Comment Lines: 19 (25.68%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (14.86%)
    '     File Size: 2.25 KB


    ' Class Instance
    ' 
    '     Properties: featureSize, label, x
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: Load, ToString, ZScore
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' classify training model
''' </summary>
Public Class Instance

    ''' <summary>
    ''' the real label data
    ''' </summary>
    ''' <returns></returns>
    Public Property label As Double
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

    Sub New(label As Double, x As IEnumerable(Of Double))
        Me.label = label
        Me.x = x.ToArray
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{label}] {x.GetJson}"
    End Function

    Public Shared Function ZScore(data As Instance(), size As Integer) As Instance()
        For i As Integer = 0 To size - 1
            Dim offset As Integer = i
            Dim v As New Vector(data.Select(Function(a) a.x(offset)))
            Dim z As Double() = New Vector(v).Z

            For j As Integer = 0 To data.Length - 1
                data(j).x(i) = z(j)
            Next
        Next

        Return data
    End Function

    ''' <summary>
    ''' load raw dataset helper function
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="featureSet"></param>
    ''' <param name="label"></param>
    ''' <returns></returns>
    Public Shared Iterator Function Load(Of T As {DynamicPropertyBase(Of Double)})(data As IEnumerable(Of T), featureSet As String(), label As String) As IEnumerable(Of Instance)
        For Each row As T In data
            Yield New Instance(row(label), row(featureSet))
        Next
    End Function
End Class
