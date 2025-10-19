using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using EventSourcingExample.DynamoDB.Events;

namespace EventSourcingExample.DynamoDB;

public class StudentDatabase
{
    private readonly IAmazonDynamoDB _amazonDynamoDb = new AmazonDynamoDBClient(RegionEndpoint.EUNorth1);
    private const string TableName = "students";
 
    private static readonly JsonSerializerOptions SerializerSettings = new()
    {
        AllowOutOfOrderMetadataProperties = true
    };
    
    // Event sourcing and NoSQL databases play very nicely together especially because of the partition key.
    // Partition key is not necessarily like a primary key in a database because the uniqueness of an item
    // in databases like DynamoDB is the combination of the partition key and the sort key.
    // As the name applies, Sort key is also sorted by default based on the value.
    
    public async Task AppendAsync<T>(T @event) where T : Event
    {
        @event.CreatedAtUtc = DateTime.UtcNow;
        var eventAsJson = JsonSerializer.Serialize(@event);
        var itemAsDocument = Document.FromJson(eventAsJson);
        var itemAsAttributes = itemAsDocument.ToAttributeMap();
        
        // DynamoDB actually supports transactions, meaning we can actually store that projection
        // (materialized view) as we're saving the new events, and they can fail together and
        // succeed together.
        var studentView = await GetStudentAsync(@event.StreamId) ?? new Student();
        studentView.Apply(@event);
        var studentAsJson = JsonSerializer.Serialize(studentView);
        var studentAsDocument = Document.FromJson(studentAsJson);
        var studentAsAttributes = studentAsDocument.ToAttributeMap();
        
        var transactionRequest = new TransactWriteItemsRequest
        {
            TransactItems =
            [
                new TransactWriteItem
                {
                    Put = new Put
                    {
                        TableName = TableName,
                        Item = itemAsAttributes
                    }
                },
                new TransactWriteItem
                {
                    Put = new Put
                    {
                        TableName = TableName,
                        Item = studentAsAttributes
                    }
                }
            ]
        };
        
        await _amazonDynamoDb.TransactWriteItemsAsync(transactionRequest);
    }

    public async Task<Student?> GetStudentAsync(Guid studentId)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S = $"{studentId.ToString()}_view" } },
                { "sk", new AttributeValue { S = $"{studentId.ToString()}_view" } }
            }
        };
        
        var response = await _amazonDynamoDb.GetItemAsync(request);
        if (response.Item.Count == 0)
        {
            return null;
        }
        
        var itemAsDocument = Document.FromAttributeMap(response.Item);
        var studentAsJson = itemAsDocument.ToJson();
        var student = JsonSerializer.Deserialize<Student>(studentAsJson);
        return student;
    }
    
    // public async Task<Student?> GetStudentAsync(Guid studentId)
    // {
    //     var request = new QueryRequest
    //     {
    //         TableName = TableName,
    //         KeyConditionExpression = "pk = :v_Pk",
    //         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    //         {
    //             { ":v_Pk", new AttributeValue { S = studentId.ToString() } }
    //         }
    //     };
    //     
    //     var response = await _amazonDynamoDb.QueryAsync(request);
    //     if (response.Count == 0)
    //     {
    //         return null;
    //     }
    //
    //     var itemsAsDocuments = response.Items.Select(Document.FromAttributeMap);
    //     var studentEvents = itemsAsDocuments
    //         .Select(x => JsonSerializer.Deserialize<Event>(x.ToJson(), SerializerSettings))
    //         .OrderBy(x => x!.CreatedAtUtc); // this might be redundant since we already have it ordered because of the sort key
    //     
    //     var student = new Student();
    //     foreach (var studentEvent in studentEvents)
    //     {
    //         student.Apply(studentEvent!);
    //     }
    //
    //     return student;
    // }
}