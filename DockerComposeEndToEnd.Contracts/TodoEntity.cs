using MongoDB.Bson.Serialization.Attributes;

namespace DockerComposeEndToEnd.Contracts
{
	public class TodoEntity
	{
		[BsonId]
		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Content { get; set; }
	}
}
