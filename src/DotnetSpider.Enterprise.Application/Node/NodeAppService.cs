﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotnetSpider.Enterprise.Application.Node.Dto;
using DotnetSpider.Enterprise.Core;
using DotnetSpider.Enterprise.Domain;
using DotnetSpider.Enterprise.EntityFrameworkCore;
using Newtonsoft.Json;
using DotnetSpider.Enterprise.Domain.Entities;
using AutoMapper;
using DotnetSpider.Enterprise.Application.Message;
using DotnetSpider.Enterprise.Application.Message.Dto;

namespace DotnetSpider.Enterprise.Application.Node
{
	public class NodeAppService : AppServiceBase, INodeAppService
	{
		private readonly IMessageAppService _messageAppService;

		public NodeAppService(ApplicationDbContext dbcontext, IMessageAppService messageAppService) : base(dbcontext)
		{
			_messageAppService = messageAppService;
		}

		public void Enable(string nodeId)
		{
			var node = DbContext.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
			if (node != null)
			{
				node.IsEnable = true;
			}
			DbContext.SaveChanges();
		}

		public void Disable(string nodeId)
		{
			var node = DbContext.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
			if (node != null)
			{
				node.IsEnable = false;
			}
			DbContext.SaveChanges();
		}

		public List<MessageOutputDto> Heartbeat(NodeHeartbeatInputDto input)
		{
			AddHeartbeat(input);
			RefreshOnlineStatus(input.NodeId);
			return _messageAppService.QueryMessages(input.NodeId);
		}

		public PagingQueryOutputDto QueryNodes(PagingQueryInputDto input)
		{
			PagingQueryOutputDto output = new PagingQueryOutputDto();
			switch (input.SortKey)
			{
				case "enable":
					{
						output = DbContext.Nodes.PageList(input, null, d => d.IsEnable);
						break;
					}
				case "nodeid":
					{
						output = DbContext.Nodes.PageList(input, null, d => d.NodeId);
						break;
					}
				case "createtime":
					{
						output = DbContext.Nodes.PageList(input, null, d => d.CreationTime);
						break;
					}
				default:
					{
						output = DbContext.Nodes.PageList(input, null, d => d.IsOnline);
						break;
					}
			}
			List<NodeOutputDto> nodeOutputs = new List<NodeOutputDto>();
			var nodes = output.Result as List<Domain.Entities.Node>;
			var timeoutHeartbeat = DateTime.Now.AddMinutes(-1);
			foreach (var node in nodes)
			{
				var nodeOutput = new NodeOutputDto();
				nodeOutput.CreationTime = node.CreationTime;
				nodeOutput.IsEnable = node.IsEnable;
				nodeOutput.NodeId = node.NodeId;
				var lastHeartbeat = DbContext.NodeHeartbeats.FirstOrDefault(h => h.NodeId == node.NodeId && h.CreationTime > timeoutHeartbeat);
				nodeOutput.IsOnline = lastHeartbeat == null ? false : true;
				if (lastHeartbeat != null)
				{
					nodeOutput.CPULoad = lastHeartbeat.CPULoad;
					nodeOutput.FreeMemory = lastHeartbeat.FreeMemory;
					nodeOutput.Ip = lastHeartbeat.Ip;
					nodeOutput.Os = lastHeartbeat.Os;
					nodeOutput.ProcessCount = lastHeartbeat.ProcessCount;
					nodeOutput.TotalMemory = lastHeartbeat.TotalMemory;
					nodeOutput.Version = lastHeartbeat.Version;
				}
				else
				{
					nodeOutput.CPULoad = 0;
					nodeOutput.FreeMemory = 0;
					nodeOutput.Ip = "UNKONW";
					nodeOutput.Os = "UNKONW";
					nodeOutput.ProcessCount = 0;
					nodeOutput.TotalMemory = 0;
					nodeOutput.Version = "UNKONW";
				}
				nodeOutputs.Add(nodeOutput);
			}
			output.Result = nodeOutputs;
			return output;
		}

		private void AddHeartbeat(NodeHeartbeatInputDto input)
		{
			var heartbeat = Mapper.Map<NodeHeartbeat>(input);
			DbContext.NodeHeartbeats.Add(heartbeat);
		}

		private void RefreshOnlineStatus(string nodeId)
		{
			var node = DbContext.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
			if (node != null)
			{
				node.IsOnline = true;
				node.LastModificationTime = DateTime.Now;
			}
			else
			{
				node = new Domain.Entities.Node();
				node.NodeId = nodeId;
				node.IsEnable = true;
				node.IsOnline = true;
				node.CreationTime = DateTime.Now;
				node.LastModificationTime = DateTime.Now;
				DbContext.Nodes.Add(node);
			}
		}
	}
}
