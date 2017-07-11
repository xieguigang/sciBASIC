#Region "Microsoft.VisualBasic::2f78510289e2a3228f86d3c01b70c1f8, ..\sciBASIC#\Data_science\Bootstrapping\Monte-Carlo\Models\ValueRange.vb"

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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus

Namespace MonteCarlo

    ''' <summary>
    ''' Value range of the variable, like <see cref="var"/>
    ''' </summary>
    Public Class ValueRange : Inherits DoubleRange
        Implements ICloneable

        <XmlAttribute>
        Public Property Name As String

        Sub New(min#, max#)
            MyBase.New(min, max)
        End Sub

        Sub New(name$, min#, max#)
            MyBase.New(min, max)
            _Name = name
        End Sub

        Sub New()
        End Sub

        Public Function GetValue() As Double
            Return GetRandom(Min, Max)()
        End Function

        ''' <summary>
        ''' Copy the range and name property value
        ''' </summary>
        ''' <returns></returns>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New ValueRange(Min, Max) With {
                .Name = Name
            }
        End Function

        ''' <summary>
        ''' Ranged random value provider
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRandomModel() As NamedValue(Of IValueProvider)
            Return New NamedValue(Of IValueProvider) With {
                .Name = Name,
                .Value = AddressOf GetValue
            }
        End Function

        Public Overrides Function ToString() As String
            Return Name & " --> " & "{ min:=" & Min & ", max:=" & Max & " }"
        End Function
    End Class
End Namespace
