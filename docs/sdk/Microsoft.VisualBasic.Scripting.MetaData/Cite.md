# Cite
_namespace: [Microsoft.VisualBasic.Scripting.MetaData](./index.md)_

EndNote tags



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Scripting.MetaData.Cite.#ctor(System.String)
```
%x 是文章的摘要，可以使用<p>或者<br/>来进行分段

|Parameter Name|Remarks|
|--------------|-------|
|EndNoteImports|-|

> 
>  %A Griffiths-Jones, Sam
>  %A Bateman, Alex
>  %A Marshall, Mhairi
>  %A Khanna, Ajay
>  %A Eddy, Sean R.
>  %T Rfam: an RNA family database
>  %0 Journal Article
>  %D 2003 
>  %8 January 1, 2003 
>  %J Nucleic Acids Research 
>  %P 439-441 
>  %R 10.1093/nar/gkg006 
>  %V 31 
>  %N 1 
>  %U http://nar.oxfordjournals.org/content/31/1/439.abstract 
>  %X Rfam Is a collection of multiple sequence alignments And covariance models representing non-coding RNA families. Rfam Is available on the web in the UK at http://www.sanger.ac.uk/Software/Rfam/ And in the US at http://rfam.wustl.edu/. These websites allow the user to search a query sequence against a library of covariance models, And view multiple sequence alignments And family annotation. The database can also be downloaded in flatfile form And searched locally using the INFERNAL package (http://infernal.wustl.edu/). The first release of Rfam (1.0) contains 25 families, which annotate over 50 000 non-coding RNA genes in the taxonomic divisions of the EMBL nucleotide database. 
>  

#### __formatAbstractPreview
```csharp
Microsoft.VisualBasic.Scripting.MetaData.Cite.__formatAbstractPreview(System.String)
```
摘要可能会有空值的

|Parameter Name|Remarks|
|--------------|-------|
|absLen|-|



### Properties

#### Abstract
文章的摘要，可以使用<p>或者<br/>来进行分段
#### Authors
author(eMail);author(eMail)
#### ISSN
ISSN_1; ISSN_2; ISSN_3
