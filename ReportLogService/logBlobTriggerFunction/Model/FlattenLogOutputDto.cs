using System;
using System.Collections.Generic;
using System.Text;

namespace logBlobTriggerFunction.Model
{
	public class FlattenLogOutputDto
	{
		public int Id { get; set; }
		public DateTime DateCreated { get; set; }
		public int MessageId { get; set; }
		public DateTime Timestamp { get; set; }
		public string Channel { get; set; }
		public string Severity { get; set; }
		public string Type { get; set; }
		public string Message { get; set; }
		public string ActivityName { get; set; }
	}
}
