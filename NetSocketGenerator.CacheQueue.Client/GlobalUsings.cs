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

global using NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;
global using NetSocketGenerator.CacheQueue.Contracts.Constants;
global using NetSocketGenerator.CacheQueue.Client.Modules;
global using NetSocketGenerator.CacheQueue.Client.Contexts;
global using NetSocketGenerator.CacheQueue.Client.Delegates;
global using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Strings;
global using NetSocketGenerator.CacheQueue.Contracts.Messages;
global using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache;
global using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Doubles;
global using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Integers;
global using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Longs;
global using NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.ULongs;

