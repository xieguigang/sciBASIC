#Region "Microsoft.VisualBasic::e6796b3d56e20a46676cbfc068e948a5, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Normalization.vb"

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

    '   Total Lines: 86
    '    Code Lines: 63
    ' Comment Lines: 15
    '   Blank Lines: 8
    '     File Size: 3.14 KB


    ' Module Normalization
    ' 
    '     Function: Build, Normalize, Trim
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Interpolation

''' <summary>
''' Methods for raw data processing
''' </summary>
Public Module Normalization

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
                line.Value _
                .Select(Function(x) x.Point) _
                .OrderBy(Function(x) x.X)
            Let intr As PointF() =
                CubicSpline.RecalcSpline(pts, expected) _
                .ToArray
            Select New NamedValue(Of PointF()) With {
                .Name = line.Name,
                .Value = intr
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
            raw.Value.ToDictionary(
            Function(x) CSng(x.Time),
            Function(p) New PointF(CSng(p.Time), CSng(p.Y)))
        Dim i As VBInteger = Scan0
        Dim preX As Value(Of Single) = intr.Value(++i).X

        Do While ++i < intr.Value.Length - 1
            If +preX > intr.Value(i).X Then ' 出现圈了

            End If
        Loop

        Throw New NotImplementedException
    End Function

    <Extension>
    Public Function Build(data As IEnumerable(Of NamedValue(Of TimeValue()))) As ODEsOut
        Dim array As NamedValue(Of TimeValue())() =
            data.ToArray
        Return New ODEsOut With {
            .x = array(Scan0).Value _
                .Select(Function(x) x.Time).ToArray,
            .y = array _
                .Select(Function(x) New NamedCollection(Of Double) With {
                    .Name = x.Name,
                    .Value = x.Value _
                        .Select(Function(o) o.Y).ToArray
                }).ToDictionary
        }
    End Function
End Module
