#Region "Microsoft.VisualBasic::c0592b931ae7ea2fa8d114407fcd4d92, ..\visualbasic_App\gr\Datavisualization.Network\NetworkCanvas\Config.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Serialization.JSON

<IniMapIO("#/ForceDirectedArgs.ini")>
Public Class Config

    Public Property ForceDirectedArgs As ForceDirectedArgs

    Public Const DefaultStiffness As Single = 81.76!
    Public Const DefaultRepulsion As Single = 20000.0!
    Public Const DefaultDamping As Single = 0.5!

    Public Shared Function Load() As ForceDirectedArgs
        Dim b As Boolean = False
        Dim ini As Config = LoadProfile(Of Config)(b)

        If Not b Then
            ini.ForceDirectedArgs = New ForceDirectedArgs With {
                .Damping = DefaultDamping,
                .Repulsion = DefaultRepulsion,
                .Stiffness = DefaultStiffness
            }
            Call ini.WriteProfile
        End If

        Return ini.ForceDirectedArgs
    End Function
End Class

<ClassName(NameOf(ForceDirectedArgs))>
Public Class ForceDirectedArgs
    <DataFrameColumn> Public Property Stiffness As Single = 41.76!
    <DataFrameColumn> Public Property Repulsion As Single = 10000.0!
    <DataFrameColumn> Public Property Damping As Single = 0.4!

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
