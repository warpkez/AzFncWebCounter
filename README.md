## AzFncWebCounter

A website hit counter that uses an Azure Function to increment a counter in a Cosmos DB database.

Example of the JSON document stored in the Cosmos DB database:
```json
{
	"id" : "uuid",
	"counter" : 1000,
	"Timestamp" : "2018-01-01T00:00:00.000Z"
}
```

The id is a GUID assigned to the website, the counter is an integer, and the timestamp is a date/time.  The id is also the partition key for the database, so it would be possible to use one database to track traffic across multiple websites.

If there is an error, the id will be set to error and the counter will be set to -1, and the timestamp will be set to the current time.

It serves no real purpose other than to demonstrate how to use Azure Functions and Cosmos DB.  It is not intended to be used in a production environment.

It could be extended to use Entity Framework to store more information about the hit, such as the IP address, browser, etc. but as a PoC it is not necessary.
