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