using ReportLogAPI.Dtos.InputDto;
using ReportLogAPI.ModelDto;
using ReportLogEntityFrameworkCore.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Profile
{
	public class MainLogProfile : AutoMapper.Profile
	{
		public MainLogProfile()
		{
			#region
			CreateMap<MainLogDataInputDto, MainLogData>()
				.ForMember(e => e.MessageId, options => options.MapFrom(xx => xx.MessageId))
				.ForMember(e => e.Timestamp, options => options.MapFrom(xx => DateTime.Parse(xx.MessageId)))
				.ForMember(e => e.Channel, options => options.MapFrom(xx => xx.Channel))
				.ForMember(e => e.Type, options => options.MapFrom(xx => xx.Type))
				.ForMember(e => e.Severity, options => options.MapFrom(xx => MainLogData.SeverityCreator(xx.Severity)))
				.ForMember(e => e.Message, options => options.MapFrom(xx => xx.Message))
				.ReverseMap();
			#endregion
			
			#region
			CreateMap<Log, MainLogData>()
				.ForMember(e => e.MessageId, options => options.MapFrom(xx => xx.MessageId))
				.ForMember(e => e.Timestamp, options => options.MapFrom(xx => DateTime.Parse(xx.TimeStamp)))
				.ForMember(e => e.Channel, options => options.MapFrom(xx => xx.Channel))
				.ForMember(e => e.Type, options => options.MapFrom(xx => xx.Type))
				.ForMember(e => e.Severity, options => options.MapFrom(xx => MainLogData.SeverityCreator(xx.Severity)))
				.ForMember(e => e.Message, options => options.MapFrom(xx => xx.Message))
				.ForMember(e => e.ActivityId, options => options.MapFrom(xx => xx.ActivityId))
				.ReverseMap();
			#endregion

			#region
			CreateMap<Log, LogActivityData>()
				.ForMember(e => e.ActivityId, options => options.MapFrom(xx => xx.ActivityId))
				.ForMember(e => e.RootActivityId, options => options.MapFrom(xx => xx.RootActivityId))
				.ForMember(e => e.ParentActivityId, options => options.MapFrom(xx => xx.ParentActivityId))
				.ForMember(e => e.ActivityName, options => options.MapFrom(xx => xx.ActivityName))
				.ReverseMap();
			#endregion

			#region LogtoFlattenLogData
			CreateMap<Log, FlattenLogData>()
				.ForMember(e => e.MessageId, options => options.MapFrom(xx => int.Parse(xx.MessageId)))
				.ForMember(e => e.Timestamp, options => options.MapFrom(xx => DateTime.Parse(xx.TimeStamp)))
				.ForMember(e => e.Channel, options => options.MapFrom(xx => xx.Channel.Trim()))
				.ForMember(e => e.Type, options => options.MapFrom(xx => xx.Type.Trim()))
				.ForMember(e => e.Severity, options => options.MapFrom(xx => MainLogData.SeverityCreator(xx.Severity)))
				.ForMember(e => e.Message, options => options.MapFrom(xx => xx.Message.Trim()))
				.ForMember(e => e.ActivityId, options => options.MapFrom(xx => xx.ActivityId.Trim()))
				.ForMember(e => e.RootActivityId, options => options.MapFrom(xx => xx.RootActivityId.Trim()))
				.ForMember(e => e.ParentActivityId, options => options.MapFrom(xx => xx.ParentActivityId.Trim()))
				.ForMember(e => e.ActivityName, options => options.MapFrom(xx => xx.ActivityName.Trim()))
				.ReverseMap();
			#endregion

			#region LogProcessMapping
			CreateMap<LogProcessData, LogProcessOutputDto>()
				.ReverseMap();
			#endregion
		}
	}
}
