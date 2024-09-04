// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Friflo.Json.Fliox.Hub;
using Friflo.Json.Fliox.Hub.DB.Cluster;
using Friflo.Json.Fliox.Hub.DB.Monitor;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Json.Fliox.Hub.Host.Event;
using Friflo.Json.Fliox.Hub.Remote;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity.Internal {
    
internal class HubHttpServer 
{
#region private fields
    // private             EventProcessorQueue     processor;
    internal             HttpServer              server;

    #endregion
    
    internal FlioxHub CreateServerHub(int port, EntityDatabase database, SharedEnv env) {
        var httpHost    = CreateHttpHost(database, env, out FlioxHub hub);
        var hostEnv     = httpHost.hub.GetFeature<RemoteHostEnv>();
    //  hostMetrics     = hostEnv.metrics;
        var endpoint    = $"http://+:{port}/";
        server          = new HttpServer(endpoint, httpHost);
        return hub;
    }
    
    internal void Run() {
        server.Start();
        try {
            server.Run();
        }
        catch (ThreadAbortException) {
            server.Logger.Log(HubLog.Info, "HTTP server thread aborted"); 
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
        
    internal void Stop() {
        server?.Stop();
        server?.Dispose();
    }
    
    /// <summary> blueprint to showcase a minimal feature set of a <see cref="HttpHost"/> </summary>
    private static HttpHost CreateHttpHost(EntityDatabase database, SharedEnv env, out FlioxHub hub)
    {
        // Note: non-concurrent memory database can be used as all read / write access is executed via the UI thread
    //    var schema          = DatabaseSchema.CreateFromType(schemaType);

        hub                 = new FlioxHub(database, env);
        hub.EventDispatcher = new EventDispatcher(EventDispatching.QueueSend); // enables Pub-Sub (sending events for subscriptions)
        
        hub.Info.projectName= database.Schema.Name;
        hub.AddExtensionDB (new ClusterDB("cluster", hub));     // required by HubExplorer
        hub.AddExtensionDB (new MonitorDB("monitor", hub));     // expose monitor stats as extension database
            
        var httpHost        = new HttpHost(hub, "/fliox/", env) {
            AcceptWebSocketType = AcceptWebSocketType.FrifloWebSockets
        };
        var explorerPath    = System.IO.Directory.GetCurrentDirectory() + "/Assets/Friflo.Engine.Unity/www~";
        httpHost.AddHandler (new StaticFileHandler(explorerPath));
        var hostEnv         = httpHost.hub.GetFeature<RemoteHostEnv>();
        hostEnv.useReaderPool   = true;
        hostEnv.logMessages     = false;
            
    //  _ = CreateWebRtcServer(httpHost, env);
        return httpHost;
    }
}

}