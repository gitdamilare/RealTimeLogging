using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.ModelDto
{
    public class ReturnModel 
    {
        public bool Error { get; set; } = false;
        public string Message { get; set; }
        //public object Data { get; set; }

        public ReturnModel(bool error, string message)
		{
            Error = error;
            Message = message;     
        }
    }
    public class ReturnModel<T> where T : class
    {
        public bool Error { get; set; } = false;
        public string Message { get; set; }
        public List<T> Data { get; set; }
    }


    public class ReturnModelObject<T> where T : class
    {
        public bool Error { get; set; } = false;
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class ReturnModelEList<T> where T : class
    {
        public bool Error { get; set; } = false;
        public string Message { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
    public class ReturnModelMessageList<T> where T : class
    {
        public bool Error { get; set; } = false;
        public IEnumerable<T> Message { get; set; }
        public T Data { get; set; }
    }
}
