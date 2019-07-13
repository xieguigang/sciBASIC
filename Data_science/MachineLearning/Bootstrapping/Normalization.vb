#Region "Microsoft.VisualBasic::e6796b3d56e20a46676cbfc068e948a5, Data_science\MachineLearning\Bootstrapping\Normalization.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
