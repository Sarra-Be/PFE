namespace stage_api.NewFolder
{
	public class CreateTableRequest
	{

		public string TableName { get; set; }


		public List<TableAttribute> Attributes { get; set; }
		public string CreatedById { get; set; }
	}
}
