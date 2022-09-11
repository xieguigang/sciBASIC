#Region "Microsoft.VisualBasic::d1beee87cd78002fa903c31cea166ceb, sciBASIC#\Data_science\Mathematica\Math\DataFrame\DataFrame\DataFrame.vb"

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

    '   Total Lines: 26
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 828 B


    ' Class DataFrame
    ' 
    '     Properties: dims, features, rownames
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class DataFrame

    Public Property features As New Dictionary(Of String, FeatureVector)
    Public Property rownames As String()

    Public ReadOnly Property dims As Size
        Get
            Return New Size(width:=features.Count, height:=rownames.Length)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim size As Size = dims
        Dim featureSet As String = features _
            .ToDictionary(Function(a) a.Key,
                          Function(a)
                              Return a.Value.type.Name.ToLower
                          End Function) _
            .GetJson

        Return $"[{size.Width}x{size.Height}] {featureSet}"
    End Function
End Class

