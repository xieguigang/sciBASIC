# IRepositoryWrite`2
_namespace: [Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository](./index.md)_

Interface to support writing (and deletes) to a typed repository

> 
>  In this architecture there is a seperate read and write interface but often this
>  pattern has just the one interface for both functions
>  


### Methods

#### AddNew
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository.IRepositoryWrite`2.AddNew(`1)
```
Adds an entity that we know to be new and returns its assigned key

|Parameter Name|Remarks|
|--------------|-------|
|entity|
 The entity we are adding to the repository
 |


_returns: 
 The unique identifier for the entity
 _
> 
>  This is useful if the unique identifier is not an intrinsic property of
>  the entity - for example if it is a memory address or a GUID
>  

#### AddOrUpdate
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository.IRepositoryWrite`2.AddOrUpdate(`1,`0)
```
Add or update the entity

|Parameter Name|Remarks|
|--------------|-------|
|entity|
 The record to add or update on the repository
 |
|key|
 The key that uniquely identifies the record to add or update
 |


#### Delete
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository.IRepositoryWrite`2.Delete(`0)
```
Delete the entity uniquely identified by this key

|Parameter Name|Remarks|
|--------------|-------|
|key|
 The unique identifier of the record to delete
 |



