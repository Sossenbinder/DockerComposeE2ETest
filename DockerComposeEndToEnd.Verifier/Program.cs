using System.Net.Http.Json;
using DockerComposeEndToEnd.Contracts;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

Console.WriteLine("Starting verifier!");

var configuration = new ConfigurationBuilder()
	.AddEnvironmentVariables()
	.Build();

var testEntity = new TodoEntity()
{
	Name = "TestEntry",
	Content = "Dolor consectetuer tempor eirmod duis",
};

var httpClient = new HttpClient
{
	BaseAddress = new Uri(configuration["ApiEndpoint"]),
};

var postResponse = await httpClient.PostAsync("/addTodo", JsonContent.Create(testEntity));
var postResult = await postResponse.Content.ReadFromJsonAsync<InsertTodoResult>();

if (!postResponse.IsSuccessStatusCode || postResult is null)
{
	Console.WriteLine("Posting todo failed");
	Environment.Exit(201);
}
else
{
	Console.WriteLine("Posting successful");
}

var overriddenId = configuration["OverriddenTodoId"];
if (overriddenId is not null)
{
	var parsedOverriddenId = Guid.Parse(overriddenId);
	Console.WriteLine($"Overridden guid detected - {parsedOverriddenId}");
	postResult = new InsertTodoResult(parsedOverriddenId);
}
var getResponse = await httpClient.GetAsync($"/getTodo/{postResult.Id}");

if (!getResponse.IsSuccessStatusCode)
{
	Console.WriteLine("Getting todo failed");
	Environment.Exit(202);
}

var getResult = await getResponse.Content.ReadFromJsonAsync<GetTodoResult>();

if (!getResponse.IsSuccessStatusCode || getResult is null)
{
	Console.WriteLine("Getting todo failed");
	Environment.Exit(202);
}

Console.WriteLine("Posting successful");

var retrievedEntity = getResult.Todo;

var mongoDbEndpoint = configuration["MongoDbEndpoint"];

var mongoClient = new MongoClient(mongoDbEndpoint);
var collection = mongoClient.GetDatabase(MongoDefinitions.DatabaseName).GetCollection<TodoEntity>(MongoDefinitions.CollectionName);
var mongoDbEntity = await collection.Find(x => x.Id == retrievedEntity.Id).SingleOrDefaultAsync();

if (mongoDbEntity is null)
{
	Console.WriteLine("Couldn't retrieve todo from mongodb");
	Environment.Exit(203);
}
else
{
	Console.WriteLine("Retrieving successful");
}

if (testEntity.Content != mongoDbEntity.Content 
    || testEntity.Name != mongoDbEntity.Name 
    || postResult.Id != mongoDbEntity.Id)
{
	Console.WriteLine("Posted entity and mongodb entity don't match");
	Environment.Exit(204);
}
else
{
	Console.WriteLine("Posted entity is equal");
}