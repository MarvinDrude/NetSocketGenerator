global using System.Net.Sockets;
global using System.Threading.Tasks.Sources;
global using System.IO.Pipelines;
global using System.Diagnostics.CodeAnalysis;
global using System.Buffers;
global using System.Collections.Concurrent;
global using System.Runtime.InteropServices;
global using System.Threading.Channels;
global using System.Security.Cryptography.X509Certificates;
global using System.Net;
global using System.Net.Security;
global using System.Security.Authentication;
global using System.Text;
global using System.Buffers.Binary;

global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.DependencyInjection;

global using NetSocketGenerator.Enums;
global using NetSocketGenerator.Extensions;
global using NetSocketGenerator.Helpers;
global using NetSocketGenerator.Internal.Options;
global using NetSocketGenerator.Internal.Factories;
global using NetSocketGenerator.Internal.Settings;
global using NetSocketGenerator.Tcp.Interfaces;
global using NetSocketGenerator.Tcp.Frames;
global using NetSocketGenerator.Events;
global using NetSocketGenerator.Events.Delegates;
global using NetSocketGenerator.Internal;
global using NetSocketGenerator.Tcp;
global using NetSocketGenerator.Tcp.Serializers;
global using NetSocketGenerator.Tcp.Services;
global using NetSocketGenerator.Attributes;
global using NetSocketGenerator.Acknowledge;

global using NetSocketGenerator.CacheQueue.Configuration.Server;
global using NetSocketGenerator.CacheQueue.Serializers;

global using NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;
global using NetSocketGenerator.CacheQueue.Models.Queues;
global using NetSocketGenerator.CacheQueue.Contracts.Constants;
global using NetSocketGenerator.CacheQueue.Server;
global using NetSocketGenerator.CacheQueue.Models.Common;
