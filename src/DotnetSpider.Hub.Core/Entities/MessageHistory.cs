﻿using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Hub.Core.Entities
{
	public class MessageHistory : AuditedEntity
	{
		[Required]
		[StringLength(32)]
		public virtual string NodeId { get; set; }

		[StringLength(100)]
		public virtual string Name { get; set; }

		[StringLength(100)]
		[Required]
		public virtual string ApplicationName { get; set; }

		[Required]
		public virtual string TaskId { get; set; }

		/// <summary>
		/// 版本信息
		/// </summary>
		[StringLength(100)]
		public virtual string Package { get; set; }

		/// <summary>
		/// 附加参数
		/// </summary>
		[StringLength(500)]
		public virtual string Arguments { get; set; }
	}
}