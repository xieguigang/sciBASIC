#Region "Microsoft.VisualBasic::6d2a46917f4ab48aff110629b730340c, gr\network-visualization\network_layout\SpringForce\Parameters.vb"

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

    '   Total Lines: 83
    '    Code Lines: 49
    ' Comment Lines: 20
    '   Blank Lines: 14
    '     File Size: 3.28 KB


    '     Class Parameters
    ' 
    '         Properties: ForceDirectedArgs
    ' 
    '         Function: Load
    ' 
    '     Class ForceDirectedArgs
    ' 
    '         Properties: Damping, Iterations, Repulsion, Stiffness
    ' 
    '         Function: DefaultNew, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SpringForce

    ''' <summary>
    ''' The network graph layout parameters
    ''' </summary>
    <IniMapIO("@" & Parameters.DefaultFileName)>
    Public Class Parameters

        Public Property ForceDirectedArgs As ForceDirectedArgs

        Public Const DefaultStiffness As Double = 81.76!
        Public Const DefaultRepulsion As Double = 2000.0!
        Public Const DefaultDamping As Double = 0.5!

        Public Const DefaultFileName$ = "ForceDirectedArgs.ini"

        Public Shared Function Load(Optional out$ = Nothing, Optional [default] As ForceDirectedArgs = Nothing) As ForceDirectedArgs
            Dim b As Boolean = False
            Dim ini As Parameters = LoadProfile(Of Parameters)(b, path:=out)

            If Not b Then
                ini.ForceDirectedArgs = [default] Or New ForceDirectedArgs With {
                    .Damping = DefaultDamping,
                    .Repulsion = DefaultRepulsion,
                    .Stiffness = DefaultStiffness
                }.AsDefault

                If out.StringEmpty Then
                    Call ini.WriteProfile
                Else
                    Call ini.WriteProfile(out)
                End If
            End If

            Return ini.ForceDirectedArgs
        End Function
    End Class

    ''' <summary>
    ''' Function parameters for <see cref="forceNetwork.doForceLayout"/>
    ''' </summary>
    <ClassName(NameOf(ForceDirectedArgs))>
    Public Class ForceDirectedArgs

        ''' <summary>
        ''' 刚度值,这个参数值越大,则节点间的距离越小,网络的变化越明显
        ''' 这个参数值越小,则节点间的距离越大,网络的变化越平缓
        ''' </summary>
        ''' <returns></returns>
        <DataFrameColumn> Public Property Stiffness As Double = 41.76!
        ''' <summary>
        ''' 节点之间的排斥力大小,这个值越大,则节点间的距离越远,反之节点之间的距离越近
        ''' </summary>
        ''' <returns></returns>
        <DataFrameColumn> Public Property Repulsion As Double = 10000.0!

        ''' <summary>
        ''' 阻尼,这个值越小,则变化越不明显,值接近于1的时候,网络的布局结构会变化非常大
        ''' 当这个参数值大于1的时候,网络将无法稳定下来
        ''' </summary>
        ''' <returns></returns>
        <DataFrameColumn> Public Property Damping As Double = 0.4!
        <DataFrameColumn> Public Property Iterations As Integer = 1000%

        Public Shared Function DefaultNew() As ForceDirectedArgs
            Return New ForceDirectedArgs With {
                .Damping = Parameters.DefaultDamping,
                .Iterations = 5000,
                .Repulsion = Parameters.DefaultRepulsion,
                .Stiffness = Parameters.DefaultStiffness
            }
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
