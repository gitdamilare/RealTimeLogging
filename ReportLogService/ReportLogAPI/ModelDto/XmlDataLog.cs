using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.ModelDto
{
	//[System.SerializableAttribute()]
	//[System.ComponentModel.DesignerCategoryAttribute("code")]
	//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	//[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public class XmlDataLog
	{
		public List<Log> Logs { get; set; }
	}


	//[System.SerializableAttribute()]
	//[System.ComponentModel.DesignerCategoryAttribute("code")]
	//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	//[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public class Log
	{
		public string MessageId { get; set; }
		public string TimeStamp { get; set; }
		public string Channel { get; set; }
		public string Type { get; set; }
		public string Severity { get; set; }
		public string RootActivityId { get; set; }
		public string ParentActivityId { get; set; }
		public string ActivityId { get; set; }
		public string ActivityName { get; set; }
		public string Message { get; set; }
	}
}
