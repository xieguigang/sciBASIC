#Region "Microsoft.VisualBasic::bb2dc61814a9ef8c6094f2a3d1b9516a, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Repository\Repository.vb"

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

    '   Total Lines: 204
    '    Code Lines: 40
    ' Comment Lines: 134
    '   Blank Lines: 30
    '     File Size: 7.75 KB


    '     Interface IKeyedEntity
    ' 
    '         Properties: Key
    ' 
    '     Class ClientRecord
    ' 
    '         Properties: ClientUniqueKey, Code
    ' 
    '     Interface IRepositoryRead
    ' 
    '         Function: Exists, GetAll, GetByKey, GetWhere
    ' 
    '     Interface IRepositoryWrite
    ' 
    '         Function: AddNew
    ' 
    '         Sub: AddOrUpdate, Delete
    ' 
    '     Interface IRepository
    ' 
    ' 
    ' 
    '     Interface IClientRepository
    ' 
    ' 
    ' 
    '     Class RepositoryReadException
    ' 
    '         Properties: Fatal
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataSourceModel.Repository

    ' http://www.codeproject.com/Reference/731015/The-repository-pattern-in-VB-Net

    ' The repository pattern (in VB.Net)

    ' Background
    ' The repository pattern Is a method To introduce a shearing layer between your business objects And
    ' the data access/persistence technology you are Using And this especially useful In unit testing As
    ' the alternative(mocking an entire data access library) can be quite heart breaking.

    ' Motivation
    ' The repository pattern introduces the following advantages over the traditional three-tier architecture over an ORM:

    ' 1. The classes persisted by an ORM (Entity framework Or the Like) need To have a good deal Of information about how they are stored.
    '    This Is Not ideal because When you make a change To the underlying storage you would need To change the business objects As well.
    ' 2. Not all persistence Is in the form of a relational database - the repository can be backed by a blended storage made of files,
    '    database tables And NoSQL records As well.
    ' 3. Some fields exist only In order To allow navigation To a record Or To identify child records - these fields should Not
    '    be passed up into the business layer If they have no business meaning.

    ''' <summary>
    ''' Interface defining any item we can store in a repository and can identify by
    ''' an unique key
    ''' </summary>
    ''' <remarks>
    ''' This interface is typed so we can make type-safe code for retrieving the entity
    ''' (don't pass in an integer if the entity is keyed by string etc.)
    ''' </remarks>
    Public Interface IKeyedEntity(Of TKeyType)

        ''' <summary>
        ''' Get the key to find the entity by
        ''' </summary>
        Property Key As TKeyType

    End Interface

    ''' <summary>
    ''' Record for storing a client record in the common database
    ''' </summary>
    Public NotInheritable Class ClientRecord
        Implements IKeyedEntity(Of Integer)

        ''' <summary>
        ''' The unique number by which we know this client
        ''' </summary>
        ''' <remarks>
        ''' Every client has an unique id but this is not needed publically
        ''' </remarks>
        Public Property ClientUniqueKey As Integer Implements IKeyedEntity(Of Integer).Key

        ' Other non-key properties can go here
        ''' <summary>
        ''' The short code for the client
        ''' </summary>
        ''' <remarks>
        ''' e.g. MCL for Merrion Computing Ltd etc.
        ''' </remarks>
        Public Property Code As String
    End Class

    ''' <summary>
    ''' Interface to support reading entities from the backing store
    ''' </summary>
    ''' <typeparam name="TEntity">
    ''' The key-identified type of entity we are reading
    ''' </typeparam>
    ''' <typeparam name="TKey">
    ''' The type of the key
    ''' </typeparam>
    ''' <remarks>
    ''' In this architecture there is a seperate read and write interface but often this
    ''' pattern has just the one interface for both functions
    ''' </remarks>
    Public Interface IRepositoryRead(Of TKey, TEntity As IKeyedEntity(Of TKey))

        ''' <summary>
        ''' Does a record exist in the repository identified by this key
        ''' </summary>
        ''' <param name="key">
        ''' The unique identifier of the entity we are looking for
        ''' </param>
        Function Exists(key As TKey) As Boolean

        ''' <summary>
        ''' Get the entity uniquely identified by the given key
        ''' </summary>
        ''' <param name="key">
        ''' The unique identifier to use to get the entity
        ''' </param>
        Function GetByKey(key As TKey) As TEntity

        ''' <summary>
        ''' Get a set of entities from the repository that match the where clause
        ''' </summary>
        ''' <param name="clause">
        ''' A function to apply to filter the results from the repository
        ''' </param>
        Function GetWhere(clause As Func(Of TEntity, Boolean)) As IReadOnlyDictionary(Of TKey, TEntity)

        ''' <summary>
        ''' Get all of this type of thing from the repository
        ''' </summary>
        ''' <remarks>
        ''' returns an IQueryable so this request can be filtered further
        ''' </remarks>
        Function GetAll() As IReadOnlyDictionary(Of TKey, TEntity)

    End Interface

    ''' <summary>
    ''' Interface to support writing (and deletes) to a typed repository
    ''' </summary>
    ''' <typeparam name="TEntity">
    ''' The type of entity in the repository
    ''' </typeparam>
    ''' <typeparam name="TKey">
    ''' The type of the key to uniquely identify the entity
    ''' </typeparam>
    ''' <remarks>
    ''' In this architecture there is a seperate read and write interface but often this
    ''' pattern has just the one interface for both functions
    ''' </remarks>
    Public Interface IRepositoryWrite(Of TKey, TEntity As IKeyedEntity(Of TKey))

        ''' <summary>
        ''' Delete the entity uniquely identified by this key
        ''' </summary>
        ''' <param name="key">
        ''' The unique identifier of the record to delete
        ''' </param>
        Sub Delete(key As TKey)

        ''' <summary>
        ''' Add or update the entity
        ''' </summary>
        ''' <param name="entity">
        ''' The record to add or update on the repository
        ''' </param>
        ''' <param name="key" >
        ''' The key that uniquely identifies the record to add or update
        ''' </param>
        Sub AddOrUpdate(entity As TEntity, key As TKey)

        ''' <summary>
        ''' Adds an entity that we know to be new and returns its assigned key
        ''' </summary>
        ''' <param name="entity">
        ''' The entity we are adding to the repository
        ''' </param>
        ''' <returns>
        ''' The unique identifier for the entity
        ''' </returns>
        ''' <remarks>
        ''' This is useful if the unique identifier is not an intrinsic property of
        ''' the entity - for example if it is a memory address or a GUID
        ''' </remarks>
        Function AddNew(entity As TEntity) As TKey

    End Interface

    ''' <summary>
    ''' Read/write repository of typed entites
    ''' </summary>
    ''' <typeparam name="TKey">
    ''' The type by which the entity is uniquely identified
    ''' </typeparam>
    ''' <typeparam name="TEntity">
    ''' The type of entity in the repository
    ''' </typeparam>
    Public Interface IRepository(Of TKey, TEntity As IKeyedEntity(Of TKey))
        Inherits IRepositoryRead(Of TKey, TEntity)
        Inherits IRepositoryWrite(Of TKey, TEntity)

    End Interface

    Public Interface IClientRepository
        Inherits IRepository(Of Integer, ClientRecord)

    End Interface

    ''' <summary>
    ''' An exception that occured when reading from the repository backing store
    ''' </summary>
    ''' <remarks>
    ''' The inner exception is from whatever
    ''' </remarks>
    Public Class RepositoryReadException
        Inherits Exception

        Public ReadOnly Property Fatal As Boolean

        Public Sub New(message As String, innerExcption As Exception, fatalInit As Boolean)
            MyBase.New(message, innerExcption)
            Fatal = fatalInit
        End Sub

        Public Sub New(message As String, fatalInit As Boolean)
            MyBase.New(message)
            Fatal = fatalInit
        End Sub
    End Class
End Namespace
