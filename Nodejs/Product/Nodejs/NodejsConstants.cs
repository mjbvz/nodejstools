﻿//*********************************************************//
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

namespace Microsoft.NodejsTools {
    internal class NodejsConstants {
        internal const string JavaScriptExtension = ".js";
        internal const string TypeScriptExtension = ".ts";
        internal const string TypeScriptDeclarationExtension = ".d.ts";
        internal const string MapExtension = ".map";
        internal const string NodejsProjectExtension = ".njsproj";

        internal const string JavaScript = "JavaScript";
        internal const string CSS = "CSS";
        internal const string Nodejs = "Node.js";

        internal const string IssueTrackerUrl = "https://go.microsoft.com/fwlink/?LinkId=507637";

        internal const string DebuggerPort = "DebuggerPort";
        internal const string Environment = "Environment";
        internal const string EnvironmentVariables = "EnvironmentVariables";
        internal const string LaunchUrl = "LaunchUrl";
        internal const string NodeExePath = "NodeExePath";
        internal const string NodeExeArguments = "NodeExeArguments";
        internal const string NodejsPort = "NodejsPort";
        internal const string ProjectFileFilter = "Node.js Project File (*.njsproj)\n*.njsproj\nAll Files (*.*)\n*.*\n";
        internal const string ScriptArguments = "ScriptArguments";
        internal const string StartWebBrowser = "StartWebBrowser";

        internal const string NodeModulesFolder = "node_modules";
        internal const string NodeModulesStagingFolder = "node_modules\\.staging\\";
        internal const string BowerComponentsFolder = "bower_components";
        internal const string AnalysisIgnoredDirectories = "AnalysisIgnoredDirectories";
        internal const string AnalysisMaxFileSize = "AnalysisMaxFileSize";
        internal const string DefaultIntellisenseCompletionCommittedBy = "{}[]().,:;+-*/%&|^!~=<>?@#'\"\\";

        /// <summary>
        /// The name of the package.json file
        /// </summary>
        internal const string PackageJsonFile = "package.json";

        internal const string BaseRegistryKey = "NodejsTools";

        internal const string TypeScriptCfgProperty = "CfgPropertyPagesGuidsAddTypeScript";

        internal const ushort DefaultDebuggerPort = 5858;

        internal const string TypeScriptCompileItemType = "TypeScriptCompile";
        internal const string EnableTypeScript = "EnableTypeScript";
        internal const string TypeScriptSourceMap = "TypeScriptSourceMap";
        internal const string TypeScriptModuleKind = "TypeScriptModuleKind";
        internal const string CommonJSModuleKind = "CommonJS";
        internal const string TypeScript = "TypeScript";

        internal const string NodeToolsProcessIdEnvironmentVariable = "_NTVS_PID";

        /// <summary>
        /// Checks whether a relative and double-backslashed seperated path contains a folder name.
        /// </summary>
        internal static bool ContainsNodeModulesOrBowerComponentsFolder(string path) {
            string tmp = "\\" + path + "\\";
            return tmp.IndexOf("\\" + NodeModulesFolder + "\\", StringComparison.OrdinalIgnoreCase) >= 0
                || tmp.IndexOf("\\" + BowerComponentsFolder + "\\", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
