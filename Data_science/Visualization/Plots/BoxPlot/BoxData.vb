#Region "Microsoft.VisualBasic::2f35f759fb09b221749ef1d4f31ad972, Data_science\Visualization\Plots\BoxPlot\BoxData.vb"

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

    '   Total Lines: 71
    '    Code Lines: 42 (59.15%)
    ' Comment Lines: 21 (29.58%)
    '    - Xml Docs: 95.24%
    ' 
    '   Blank Lines: 8 (11.27%)
    '     File Size: 2.53 KB


    '     Class BoxData
    ' 
    '         Properties: GroupNames, Groups, SerialName
    ' 
    '         Function: Load, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BoxPlot

    ''' <summary>
    ''' Group data model for box plot
    ''' </summary>
    Public Class BoxData

        ''' <summary>
        ''' The sample groups
        ''' </summary>
        ''' <returns></returns>
        Public Property Groups As NamedValue(Of Vector)()
        ''' <summary>
        ''' The serials name
        ''' </summary>
        ''' <returns></returns>
        Public Property SerialName As String

        ''' <summary>
        ''' Get all of the name property values from <see cref="Groups"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GroupNames As String()
            Get
                Return Groups.Keys.ToArray
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"({SerialName}) {GroupNames.GetJson}"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path$">The csv file path</param>
        ''' <param name="groupDesigner"></param>
        ''' <returns></returns>
        Public Shared Iterator Function Load(path$, groupDesigner As NamedCollection(Of String)()) As IEnumerable(Of BoxData)
            Dim datasets As Dictionary(Of DataSet) = DataSet _
                .LoadDataSet(path) _
                .ToDictionary
            Dim serials$() = datasets.PropertyNames

            For Each name As String In serials
                Dim data = groupDesigner _
                    .Select(Function(x)
                                Dim values As Vector = datasets(x.value).Vector([property]:=name)
                                Return New NamedValue(Of Vector) With {
                                    .Name = x.name,
                                    .Description = x.description,
                                    .Value = values
                                }
                            End Function) _
                    .ToArray

                Yield New BoxData With {
                    .SerialName = name,
                    .Groups = data
                }
            Next
        End Function
    End Class
End Namespace
