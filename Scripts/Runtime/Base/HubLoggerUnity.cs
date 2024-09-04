// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Friflo.Json.Fliox.Hub;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    public sealed class HubLoggerUnity : IHubLogger
    {
        public void Log(HubLog hubLog, string message, Exception exception) {
            var fullMessage     = exception == null ? message : $"{message}, exception: {exception}";
            switch (hubLog) {
                case HubLog.Error:
                    UnityEngine.Debug.LogError(fullMessage);
                    break;                    
                case HubLog.Info:
                    UnityEngine.Debug.Log(fullMessage);
                    break;
            }
        }

        public void Log(HubLog hubLog, StringBuilder message, Exception exception) {
            Log(hubLog, message.ToString(), exception);
        }
    }
}