#Region "Microsoft.VisualBasic::5c014b7f105d1e96f52440f02858b35e, ..\sciBASIC#\Data_science\Bootstrapping\Darwinism\GAF\GAFModel.vb"

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

Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Calculus

Namespace Darwinism.GAF

    Public MustInherit Class Model : Inherits MonteCarlo.Model

        Sub New()
            Call MyBase.New()
        End Sub

        Protected Sub New(vars As var())
            Call MyBase.New(vars)
        End Sub

#Region "Not Required"
        Public NotOverridable Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Return Nothing
        End Function

        Public NotOverridable Overrides Function params() As VariableModel()
            Return Nothing
        End Function

        Public NotOverridable Overrides Function yinit() As VariableModel()
            Return Nothing
        End Function
#End Region

    End Class

    Public MustInherit Class RefModel : Inherits MonteCarlo.RefModel

#Region "Not Required"
        Public NotOverridable Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Return Nothing
        End Function

        Public NotOverridable Overrides Function params() As VariableModel()
            Return Nothing
        End Function

        Public NotOverridable Overrides Function yinit() As VariableModel()
            Return Nothing
        End Function
#End Region

    End Class
End Namespace
