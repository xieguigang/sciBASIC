#Region "Microsoft.VisualBasic::f9c15fb13d735dc3fbf1d7cd2ca53bd0, ..\sciBASIC#\gr\Datavisualization.Network\NetworkCanvas\Styling\StyleMapper.vb"

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

Imports System.Drawing
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Quantile
Imports Microsoft.VisualBasic.Scripting

Namespace Styling

    Public Module StyleMapper

        Public Enum MapperTypes
            Continuous
            Discrete
            Passthrough
        End Enum

        Public Function GetProperty(Of T)() As Dictionary(Of String, Func(Of T, Object))
            Dim type As Type = GetType(T)
            Dim properties As PropertyInfo() = type _
            .GetProperties(PublicProperty) _
            .Where(Function(p) p.GetIndexParameters.IsNullOrEmpty) _
            .ToArray
            Dim out As New Dictionary(Of String, Func(Of T, Object))

            For Each prop As PropertyInfo In properties
                Dim getMethod = Emit.Delegates.PropertyGet(type, prop)
                Call out.Add(prop.Name, Function(x As T) getMethod(x))
            Next

            Return out
        End Function

        <Extension>
        Public Function NumericMapping(Of T As INetComponent)(source As IEnumerable(Of T), key$, range As DoubleRange) As Map(Of T, Double)()
            Dim properties = GetProperty(Of T)()
            Dim array As T() = source.ToArray
            Dim flag As T = array(Scan0)
            Dim [get] As Func(Of T, Double)

            If flag.HasProperty(key) Then
                [get] = Function(x) x(key).ParseNumeric
            ElseIf properties.ContainsKey(key) Then
                Dim getValue = properties(key)
                [get] = Function(x) CType(getValue(x), Double)
            Else
                [get] = Function(null) range.Min
            End If

            Dim out As New List(Of Map(Of T, Double))
            Dim quantiles#() = array.Select([get]).QuantileLevels

            For i As Integer = 0 To quantiles.Length - 1
                out += New Map(Of T, Double) With {
                    .Key = array(i),
                    .Maps = range.Min + range.Length * quantiles(i)
                }
            Next

            Return out
        End Function

        ''' <summary>
        ''' This function works based on <see cref="NumericMapping(Of T)"/>.(函数获得的是连续的颜色映射)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="key$"></param>
        ''' <param name="colorSchema$"></param>
        ''' <param name="level%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorMapping(Of T As INetComponent)(source As IEnumerable(Of T), key$, colorSchema$, Optional level% = 100) As Map(Of T, Color)()
            Dim levels As Map(Of T, Double)() = source.NumericMapping(key, "0 -> 1")
            Dim out As New List(Of Map(Of T, Color))
            Dim colors As Color() = Designer.GetColors(colorSchema, level)

            For Each x As Map(Of T, Double) In levels
                out += New Map(Of T, Color) With {
                    .Key = x.Key,
                    .Maps = colors(x.Maps * level)
                }
            Next

            Return out
        End Function
    End Module
End Namespace
