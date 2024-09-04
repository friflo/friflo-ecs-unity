// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Friflo.Engine.ECS;
using Friflo.Engine.Hub;
using Friflo.Engine.Unity.Internal;
using Friflo.Json.Fliox.Hub.Host;
using UnityEngine;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity {
    
public enum HubType
{
    None,
    Host,
    Server,
    Client,
}

[DisallowMultipleComponent]
[AddComponentMenu("Scripts/Friflo/ECS Hub")] // removes also (Script) suffix in Inspector
// ReSharper disable once InconsistentNaming
public class ECSHub : MonoBehaviour
{
    public string   host;
    public int      port;
    
    private EntityStore entityStore;
    
    [NonSerialized]
    public HubType hubType;
    
    private HubHttpServer   httpServer;

    public  bool            IsServerRunning         => httpServer?.server?.IsRunning ?? false;
    public  bool            IsClientConnected       => false; // remoteClient?.IsConnected ?? false;

    
    // Start is called before the first frame update
    void Start()
    {
        entityStore = new EntityStore(PidType.UsePidAsId);
        // RunServer();
    }
    
    static readonly DatabaseSchema Schema = DatabaseSchema.Create<StoreClient>();
    
    private void RunServer() {
        httpServer      = new HubHttpServer();
        var database    = new MemoryDatabase("test", Schema) { 
            SmallValueSize  = 1024,
            Pretty          = false,
            ContainerType   = MemoryType.Concurrent // requires Concurrent containers as HTTP server access from various threads  
        };
        var storeCommands   = new StoreCommands(entityStore);
        database.AddCommands(storeCommands);
        // Note: Each server session requires a SharedEnv.
        // Otherwise when start / stop Server multiple times the response for
        // /fliox/?cluster { "task": "query", "cont": "schemas" } is empty
        var sharedEnv = new SharedEnv();
        sharedEnv.Logger = new HubLoggerUnity();
        
        var hub = httpServer.CreateServerHub(port, database, sharedEnv);
        
        
        var serverThread = new Thread(() => { 
            hubType    = HubType.Server;
            httpServer.Run();
        });
        serverThread.Start(); 
    }
    
    public void OpenExplorer() {
        var url = $"http://{host}:{port}";
        Application.OpenURL(url);
    }
    
    public void StopHub() {
        httpServer.Stop();
        httpServer = null;
        hubType    = HubType.None;
    }
    
    public void StartServer() {
        RunServer();
    }
        
    public void StartClient() {
    }
    
    void OnDestroy() {
        httpServer?.Stop();
    }
}

}
