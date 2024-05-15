#Region "Microsoft.VisualBasic::f524fbb68291258c1f6914b7fa502e06, Data\DataFrame\test\Outlining\Class1.vb"

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

    '   Total Lines: 47
    '    Code Lines: 40
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 2.01 KB


    ' Class compound
    ' 
    '     Properties: Area, Checked, FIShCoverage, Formula, GroupArea
    '                 MetabolikaPathways, MetabolikaSearch, MolecularWeight, MS2, mzCloudBestMatch
    '                 mzCloudBestSimMatch, mzCloudMatches, mzCloudResults, mzCloudSearch, Name
    '                 NumOfMetabolikaPathways, PredictedCompositions, RT
    ' 
    ' Class match
    ' 
    '     Properties: BestMatch, BestSimMatch, Checked, CompoundClass, Formula
    '                 KEGG_ID, MolecularWeight, mzCloudID, mzCloudLibrary, Name
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Public Class compound

    Public Property Checked As String
    Public Property Name As String
    Public Property Formula As String
    <Column("Annotation Source: Predicted Compositions")> Public Property PredictedCompositions As String
    <Column("Annotation Source: mzCloud Search")> Public Property mzCloudSearch As String
    <Column("Annotation Source: Metabolika Search")> Public Property MetabolikaSearch As String
    <Column("FISh Coverage")> Public Property FIShCoverage As String
    <Column("Molecular Weight")> Public Property MolecularWeight As String
    <Column("RT [min]")> Public Property RT As String
    <Column("Area (Max.)")> Public Property Area As String
    <Column("# mzCloud Results")> Public Property mzCloudResults As String
    <Column("# Metabolika Pathways")> Public Property NumOfMetabolikaPathways As String
    <Column("Metabolika Pathways")> Public Property MetabolikaPathways As String
    <Column("mzCloud Best Match")> Public Property mzCloudBestMatch As String
    <Column("mzCloud Best Sim. Match")> Public Property mzCloudBestSimMatch As String
    Public Property MS2 As String
    <Column("Group Area: F3")> Public Property GroupArea As String

    Public Property mzCloudMatches As match()
End Class

Public Class match

    Public Property Checked As Boolean
    Public Property Name As String
    Public Property Formula As String

    <Column("Molecular Weight")>
    Public Property MolecularWeight As String
    <Column("Best Match")>
    Public Property BestMatch As String
    <Column("Best Sim. Match")>
    Public Property BestSimMatch As String
    <Column("mzCloud ID")>
    Public Property mzCloudID As String
    <Column("KEGG ID")>
    Public Property KEGG_ID As String
    <Column("Compound Class")>
    Public Property CompoundClass As String
    <Column("mzCloud Library")>
    Public Property mzCloudLibrary As String

End Class
