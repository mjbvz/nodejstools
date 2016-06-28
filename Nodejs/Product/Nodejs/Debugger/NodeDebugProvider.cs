//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace.Extensions.VS.Debug;

namespace Microsoft.VisualStudio.Workspace.Extensions.VS.Debug {
    [ExportVsDebugLaunchTarget(MyProviderType, new string[] { ".myextension" })]
    internal class MyDebugLaunchProvider : IVsDebugLaunchTargetProvider {
        public object SetupDebugTargetInfo(
            object vsDebugTargetInfo,
            DebugLaunchActionContext debugLaunchContext) {
            IPropertySettings launchSettings = debugLaunchContext.LaunchConfiguration;
            VsDebugTargetInfo vsPythonTargetInfo = (VsDebugTargetInfo)vsDebugTargetInfo;
            vsPythonTargetInfo.clsidCustom = myDebugEngineType;
            return vsPythonTargetInfo;
        }
    }

    /// <summary>
    /// Extension to Vs Launch Debugger to handle js files from a Node Js project
    /// </summary>
    [ExportVsDebugLaunchTarget(ProviderType, new string[] { ".js" }, ProviderPriority.Lowest)]
    internal class VsNodeJsDebugLaunchProvider : IVsDebugLaunchTargetProvider {
        private const string NodejsRegPath = "Software\\Node.js";
        private const string InstallPath = "InstallPath";
        private const string ProviderType = "6C01D598-DE83-4D5B-B7E5-757FBA8443DD";
        private const string NodeExeKey = "nodeExe";

        private const string NodeJsSchema =
@"{
  ""definitions"": {
    ""nodejs"": {
      ""type"": ""object"",
      ""properties"": {
        ""type"": {""type"": ""string"",""enum"": [ ""nodejs"" ]},
        ""nodeExe"": { ""type"": ""string"" }
      }
    },
    ""nodejsFile"": {
      ""allOf"": [
        { ""$ref"": ""#/definitions/default"" },
        { ""$ref"": ""#/definitions/nodejs"" }
      ]
    }
  },
    ""defaults"": {
        ""nodejs"": { ""$ref"": ""#/definitions/nodejs"" }
    },
    ""configuration"": ""#/definitions/nodejsFile""
}";

        private static readonly Guid NodeJsToolsDebuggerId = new Guid("0a638dac-429b-4973-ada0-e8dcdfb29b61");

        /// <inheritdoc />
        public void SetupDebugTargetInfo(
            ref VsDebugTargetInfo vsDebugTargetInfo,
            DebugLaunchActionContext debugLaunchContext) {
            string target = vsDebugTargetInfo.bstrExe;
            vsDebugTargetInfo.bstrExe = debugLaunchContext.LaunchConfiguration.GetValue<string>(NodeExeKey, GetPathToNodeExecutableFromEnvironment());
            string nodeJsArgs = vsDebugTargetInfo.bstrArg;
            vsDebugTargetInfo.bstrArg = "\"" + target + "\"";
            if (!string.IsNullOrEmpty(nodeJsArgs)) {
                vsDebugTargetInfo.bstrArg += " ";
                vsDebugTargetInfo.bstrArg += nodeJsArgs;
            }

            vsDebugTargetInfo.clsidCustom = NodeJsToolsDebuggerId;
            vsDebugTargetInfo.bstrOptions = "WAIT_ON_ABNORMAL_EXIT=true";
            vsDebugTargetInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

       

        /// <summary>
        /// Export ILaunchConfigurationProvider
        /// </summary>
        [ExportLaunchConfigurationProvider(
            LaunchConfigurationProviderType,
            new string[] { ".js" },
            "nodejs",
            NodeJsSchema)]
        public class LaunchConfigurationProvider : ILaunchConfigurationProvider {
            private const string LaunchConfigurationProviderType = "1DB21619-2C53-4BEF-84E4-B1C4D6771A51";

            /// <inheritdoc />
            public void CustomizeLaunchConfiguration(DebugLaunchActionContext debugLaunchActionContext, IPropertySettings launchSettings) {
            }

            /// <inheritdoc />
            public bool IsDebugLaunchActionSupported(DebugLaunchActionContext debugLaunchActionContext) {
                throw new NotImplementedException();
            }
        }
    }
}
