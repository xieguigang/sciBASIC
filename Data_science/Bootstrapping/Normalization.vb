#Region "Microsoft.VisualBasic::7dfeb8b6b2e5780510ee634d9a0628eb, ..\sciBASIC#\Data_science\Bootstrapping\Normalization.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Mathematical.Interpolation

''' <summary>
''' Methods for raw data processing
''' </summary>
Public Module Normalization

    Public Structure TimeValue

        Public Time#, value#

        Public ReadOnly Property Point As PointF
            Get
                Return New PointF(Time, value)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{Time}]  {value}"
        End Function
    End Structure

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data">raw data</param>
    ''' <param name="expected%"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Normalize(data As IEnumerable(Of NamedValue(Of TimeValue())), Optional expected% = 5000) As ODEsOut
        Dim raw As Dictionary(Of NamedValue(Of TimeValue())) = data.ToDictionary
        Dim inter As NamedValue(Of PointF())() =
            LinqAPI.Exec(Of NamedValue(Of PointF())) <=
 _
            From line As NamedValue(Of TimeValue())
            In raw.Values.AsParallel
            Let pts As IEnumerable(Of PointF) =
                line.x _
                .Select(Function(x) x.Point) _
                .OrderBy(Function(x) x.X)
            Let intr As PointF() =
                CubicSpline.RecalcSpline(pts, expected) _
                .ToArray
            Select New NamedValue(Of PointF()) With {
                .Name = line.Name,
                .x = intr
            }

        Return inter _
            .Select(Function(l) l.Trim(raw:=raw(l.Name))) _
            .Build
    End Function

    ''' <summary>
    ''' 默认假设raw数据里面的时间点都是不重复的
    ''' </summary>
    ''' <param name="intr"></param>
    ''' <param name="raw"></param>
    ''' <returns></returns>
    <Extension>
    Private Function Trim(intr As NamedValue(Of PointF()), raw As NamedValue(Of TimeValue())) As NamedValue(Of TimeValue())
        Dim times As Dictionary(Of Single, PointF) =
            raw.x.ToDictionary(
            Function(x) CSng(x.Time),
            Function(p) New PointF(CSng(p.Time), CSng(p.value)))
        Dim i As int = Scan0
        Dim preX As Value(Of Single) = intr.x(++i).X

        Do While ++i < intr.x.Length - 1
            If preX > intr.x(i).X Then ' 出现圈了

            End If
        Loop

        Throw New NotImplementedException
    End Function

    <Extension>
    Public Function Build(data As IEnumerable(Of NamedValue(Of TimeValue()))) As ODEsOut
        Dim array As NamedValue(Of TimeValue())() =
            data.ToArray
        Return New ODEsOut With {
            .x = array(Scan0).x _
                .ToArray(Function(x) x.Time),
            .y = array _
                .Select(Function(x) New NamedValue(Of Double()) With {
                    .Name = x.Name,
                    .x = x.x _
                        .ToArray(Function(o) o.value)
                }).ToDictionary
        }
    End Function
End Module

