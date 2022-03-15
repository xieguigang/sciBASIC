#Region "Microsoft.VisualBasic::6a05fd4bf8761d08c9ac2da83197fbb4, sciBASIC#\tutorials\core.test\XmlBugTest.vb"

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

    '   Total Lines: 294
    '    Code Lines: 206
    ' Comment Lines: 38
    '   Blank Lines: 50
    '     File Size: 9.96 KB


    ' Module XmlBugTest
    ' 
    '     Function: getCompleteEntity
    ' 
    '     Sub: Main, test2
    ' 
    '     Class Working
    ' 
    '         Properties: p1, p2, p3, p4
    ' 
    '     Structure REST
    ' 
    '         Properties: [return]
    ' 
    '         Function: ParsingRESTData
    ' 
    '     Class ChEBIEntity_XmlCommentBug
    ' 
    '         Properties: charge, chebiAsciiName, chebiId, ChemicalStructures, Citations
    '                     CompoundOrigins, DatabaseLinks, definition, entityStar, Formula
    '                     Formulae, inchi, inchiKey, IupacNames, mass
    '                     OntologyChildren, OntologyParents, RegistryNumbers, SecondaryChEBIIds, smiles
    '                     status, Synonyms
    ' 
    '     Class ChEBIEntity
    ' 
    '         Properties: charge, chebiAsciiName, chebiId, ChemicalStructures, Citations
    '                     CompoundOrigins, DatabaseLinks, definition, entityStar, Formula
    '                     Formulae, inchi, inchiKey, IupacNames, mass
    '                     OntologyChildren, OntologyParents, RegistryNumbers, SecondaryChEBIIds, smiles
    '                     status, Synonyms
    ' 
    '     Class CompoundOrigin
    ' 
    '         Properties: componentAccession, componentText, SourceAccession, SourceType, speciesAccession
    '                     speciesText
    ' 
    '         Function: ToString
    ' 
    '     Class OntologyParents
    ' 
    '         Properties: chebiId, chebiName, cyclicRelationship, status, type
    ' 
    '         Function: ToString
    ' 
    '     Class DatabaseLinks
    ' 
    '         Properties: data, type
    ' 
    '         Function: ToString
    ' 
    '     Class ChemicalStructures
    ' 
    '         Properties: [structure], defaultStructure, dimension, type
    ' 
    '     Class Synonyms
    ' 
    '         Properties: data, source, type
    ' 
    '         Function: ToString
    ' 
    '     Class RegistryNumbers
    ' 
    '         Properties: data, source, type
    ' 
    '         Function: ToString
    ' 
    '     Class Formulae
    ' 
    '         Properties: data, source
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Linq
Imports TestProject.XmlCommentBug

Module XmlBugTest

    Sub Main()

        Call test2()

        Dim Working1 = GetCompleteEntity(51311, False)
        Dim notWorking = GetCompleteEntity(51311, True)

        Pause()
    End Sub

    Public Function getCompleteEntity(chebiId$, bugtest As Boolean) As Object
        Dim url$ = $"http://www.ebi.ac.uk/webservices/chebi/2.0/test/getCompleteEntity?chebiId={chebiId}"
        Dim xml$ = url.GET

        Return REST.ParsingRESTData(xml, bugtest)
    End Function

    Sub test2()
        Dim Xml$ = New Working With {.p1 = "sfdsfsdfs", .p2 = Now, .p3 = 454353453, .p4 = {"qqqqqqqqq", "eeeeeeee", "2222", "asdasd"}}.GetXml

        ' reload
        Dim obj As Working = Xml.LoadFromXml(Of Working)

        Call obj.GetJson.__DEBUG_ECHO
    End Sub
End Module

Namespace XmlCommentBug

    Public Class Working : Inherits XmlDataModel

        Public Property p1 As String
        Public Property p2 As Date
        Public Property p3 As Long

        <XmlElement>
        Public Property p4 As String()

    End Class

    <XmlRoot("getCompleteEntityResponse", [Namespace]:="http://www.ebi.ac.uk/webservices/chebi")>
    Public Structure REST

        <XmlElement("return")> Public Property [return] As ChEBIEntity

        Public Shared Function ParsingRESTData(result$, bugtest As Boolean) As Object
            Dim xml As XmlDocument = result.LoadXmlDocument
            Dim nodes = xml.GetElementsByTagName("S:Body")

            For Each node As XmlNode In nodes
                For Each rep As XmlNode In node.ChildNodes
                    For Each child As XmlNode In rep.ChildNodes
                        If child.Name <> "return" Then
                            Continue For
                        End If

                        result = child.InnerXml
                        result = $"<{NameOf(ChEBIEntity)}>" & result & $"</{NameOf(ChEBIEntity)}>"
                        result = result.Replace(" xmlns=""https://www.ebi.ac.uk/webservices/chebi""", "")

                        If bugtest Then
                            result = result.Replace("<ChEBIEntity>", "<ChEBIEntity_XmlCommentBug>").Replace("</ChEBIEntity>", "</ChEBIEntity_XmlCommentBug>")
                            Return result.CreateObjectFromXmlFragment(Of ChEBIEntity_XmlCommentBug)
                        Else
                            Return result.CreateObjectFromXmlFragment(Of ChEBIEntity)
                        End If
                    Next
                Next
            Next

            Throw New NotImplementedException
        End Function
    End Structure

    ''' <summary>
    ''' not working
    ''' </summary>
    Public Class ChEBIEntity_XmlCommentBug : Inherits XmlDataModel
        ' Implements INamedValue
        '  Implements IMolecule

        ''' <summary>
        ''' Chebi的主ID
        ''' </summary>
        ''' <returns></returns>
        Public Property chebiId As String 'Implements INamedValue.Key, IMolecule.ID

        Public Property chebiAsciiName As String 'Implements IMolecule.Name
        Public Property definition As String
        Public Property status As String
        Public Property smiles As String
        Public Property inchi As String
        Public Property inchiKey As String
        Public Property charge As Integer
        Public Property mass As Double 'Implements IMolecule.Mass
        Public Property entityStar As Integer
        <XmlElement>
        Public Property Synonyms As Synonyms()
        <XmlElement>
        Public Property IupacNames As Synonyms()
        <XmlElement>
        Public Property Citations As Synonyms()
        Public Property Formulae As Formulae
        ''' <summary>
        ''' 次级编号，和主编号都代表同一样物质
        ''' </summary>
        ''' <returns></returns>
        <XmlElement>
        Public Property SecondaryChEBIIds As String()
        <XmlElement>
        Public Property RegistryNumbers As RegistryNumbers()
        <XmlElement>
        Public Property ChemicalStructures As ChemicalStructures()
        <XmlElement>
        Public Property DatabaseLinks As DatabaseLinks()
        <XmlElement>
        Public Property OntologyParents As OntologyParents()
        <XmlElement>
        Public Property OntologyChildren As OntologyParents()
        <XmlElement>
        Public Property CompoundOrigins As CompoundOrigin()

        Private Property Formula As String ' Implements IMolecule.Formula
            Get
                Return Formulae.data
            End Get
            Set(value As String)
                Formulae.data = value
            End Set
        End Property
    End Class

    ''' <summary>
    ''' working
    ''' </summary>
    Public Class ChEBIEntity ': Inherits XmlDataModel
        ' Implements INamedValue
        '  Implements IMolecule

        ''' <summary>
        ''' Chebi的主ID
        ''' </summary>
        ''' <returns></returns>
        Public Property chebiId As String 'Implements INamedValue.Key, IMolecule.ID

        Public Property chebiAsciiName As String 'Implements IMolecule.Name
        Public Property definition As String
        Public Property status As String
        Public Property smiles As String
        Public Property inchi As String
        Public Property inchiKey As String
        Public Property charge As Integer
        Public Property mass As Double 'Implements IMolecule.Mass
        Public Property entityStar As Integer
        <XmlElement>
        Public Property Synonyms As Synonyms()
        <XmlElement>
        Public Property IupacNames As Synonyms()
        <XmlElement>
        Public Property Citations As Synonyms()
        Public Property Formulae As Formulae
        ''' <summary>
        ''' 次级编号，和主编号都代表同一样物质
        ''' </summary>
        ''' <returns></returns>
        <XmlElement>
        Public Property SecondaryChEBIIds As String()
        <XmlElement>
        Public Property RegistryNumbers As RegistryNumbers()
        <XmlElement>
        Public Property ChemicalStructures As ChemicalStructures()
        <XmlElement>
        Public Property DatabaseLinks As DatabaseLinks()
        <XmlElement>
        Public Property OntologyParents As OntologyParents()
        <XmlElement>
        Public Property OntologyChildren As OntologyParents()
        <XmlElement>
        Public Property CompoundOrigins As CompoundOrigin()

        Private Property Formula As String ' Implements IMolecule.Formula
            Get
                Return Formulae.data
            End Get
            Set(value As String)
                Formulae.data = value
            End Set
        End Property
    End Class

    Public Class CompoundOrigin
        Public Property speciesText As String
        Public Property speciesAccession As String
        Public Property componentText As String
        Public Property componentAccession As String
        Public Property SourceType As String
        Public Property SourceAccession As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class OntologyParents
        Public Property chebiName As String
        Public Property chebiId As String
        Public Property type As String
        Public Property status As String
        Public Property cyclicRelationship As Boolean

        Public Overrides Function ToString() As String
            Return chebiName
        End Function
    End Class

    Public Class DatabaseLinks

        Public Property data As String
        Public Property type As String

        Public Const HMDB_accession$ = "HMDB accession"
        Public Const KEGG_COMPOUND_accession$ = "KEGG COMPOUND accession"
        Public Const KEGG_DRUG_accession$ = "KEGG DRUG accession"

        Public Overrides Function ToString() As String
            Return $"[{type}] {data}"
        End Function
    End Class

    Public Class ChemicalStructures
        Public Property [structure] As String
        Public Property type As String
        Public Property dimension As String
        Public Property defaultStructure As String
    End Class

    Public Class Synonyms

        Public Property data As String
        Public Property source As String
        Public Property type As String

        Public Overrides Function ToString() As String
            Return $"[{source}] {data}"
        End Function
    End Class

    Public Class RegistryNumbers

        Public Property data As String
        Public Property source As String
        Public Property type As String

        Public Const Type_Reaxys$ = "Reaxys Registry Number"
        Public Const Type_Beilstein$ = "Beilstein Registry Number"
        Public Const Type_CAS$ = "CAS Registry Number"

        Public Overrides Function ToString() As String
            Return data
        End Function

    End Class

    ''' <summary>
    ''' 分子式
    ''' </summary>
    Public Class Formulae

        ''' <summary>
        ''' 分子式字符串
        ''' </summary>
        ''' <returns></returns>
        Public Property data As String
        ''' <summary>
        ''' 分子式的数据来源
        ''' </summary>
        ''' <returns></returns>
        Public Property source As String

        Public Overrides Function ToString() As String
            Return data
        End Function
    End Class
End Namespace
