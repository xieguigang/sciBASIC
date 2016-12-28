# IRepositoryRead`2
_namespace: [Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository](./index.md)_

Interface to support reading entities from the backing store

> 
>  In this architecture there is a seperate read and write interface but often this
>  pattern has just the one interface for both functions
>  


### Methods

#### Exists
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository.IRepositoryRead`2.Exists(`0)
```
Does a record exist in the repository identified by this key

|Parameter Name|Remarks|
|--------------|-------|
|key|
 The unique identifier of the entity we are looking for
 |


#### GetAll
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository.IRepositoryRead`2.GetAll
```
Get all of this type of thing from the repository
> 
>  returns an IQueryable so this request can be filtered further
>  

#### GetByKey
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository.IRepositoryRead`2.GetByKey(`0)
```
Get the entity uniquely identified by the given key

|Parameter Name|Remarks|
|--------------|-------|
|key|
 The unique identifier to use to get the entity
 |


#### GetWhere
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository.IRepositoryRead`2.GetWhere(System.Func{`1,System.Boolean})
```
Get a set of entities from the repository that match the where clause

|Parameter Name|Remarks|
|--------------|-------|
|clause|
 A function to apply to filter the results from the repository
 |



