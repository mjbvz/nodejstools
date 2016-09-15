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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools.Logging;
using Microsoft.NodejsTools.Options;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Commands {
    internal sealed class DiagnosticsCommand : Command {
        private static readonly string[] _interestingDteProperties = new[] {
            "StartupFile",
            "WorkingDirectory",
            "PublishUrl",
            "SearchPath",
            "CommandLineArguments"
        };

        private static readonly string[] _interestingFileExtensions = new[] {
            ".js",
            ".jsx",
            ".tsx",
            ".d.ts",
            ".ts",
            ".html"
        };

        public DiagnosticsCommand(IServiceProvider serviceProvider) { }

        public override int CommandId {
            get { return (int)PkgCmdId.cmdidDiagnostics; }
        }

        public override void DoCommand(object sender, EventArgs args) {
            var dlg = new DiagnosticsForm("Gathering data...");

            ThreadPool.QueueUserWorkItem(x => {
                var data = GetData();
                try {
                    dlg.BeginInvoke((Action)(() => {
                        dlg.TextBox.Text = data;
                        dlg.TextBox.SelectAll();
                    }));
                } catch (InvalidOperationException) {
                    // Window has been closed already
                }
            });
            dlg.ShowDialog();
        }

        private string GetData() {
            var res = new StringBuilder();
            res.AppendLine("Use Ctrl-C to copy contents");
            res.AppendLine();
            res.AppendLine(GetSolutionInfo());
            res.AppendLine(GetEventsAndStatsInfo());
            res.AppendLine(GetLoadedAssemblyInfo());
            return res.ToString();
        }

        private static string GetSolutionInfo() {
            var res = new StringBuilder();

            var dte = (EnvDTE.DTE)NodejsPackage.GetGlobalService(typeof(EnvDTE.DTE));
            res.AppendLine("Projects:");

            foreach (EnvDTE.Project project in dte.Solution.Projects) {
                res.AppendLine(Indent(1, GetProjectInfo(project)));
            }

            return res.ToString();
        }

        private static string GetProjectInfo(EnvDTE.Project project) {
            var res = new StringBuilder();
            string name;
            try {
                // Some projects will throw rather than give us a unique
                // name. They are not ours, so we will ignore them.
                name = project.UniqueName;
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }
                bool isNodejsProject = false;
                try {
                    isNodejsProject = Utilities.GuidEquals(Guids.NodejsProjectFactoryString, project.Kind);
                } catch (Exception ex2) {
                    if (ex2.IsCriticalException()) {
                        throw;
                    }
                }
                if (isNodejsProject) {
                    // Actually, it was one of our projects, so we do care
                    // about the exception. We'll add it to the output,
                    // rather than crashing.
                    res.AppendLine("Project: " + ex.Message);
                    res.AppendLine(Indent(2, "Kind: Node.js"));
                }
                return res.ToString();
            }
            res.AppendLine("Project: " + name);
            res.AppendLine(Indent(1, GetProjectPropertiesInfo(project)));
            return res.ToString();
        }

        private static string GetProjectPropertiesInfo(EnvDTE.Project project) {
            var res = new StringBuilder();
            if (Utilities.GuidEquals(Guids.NodejsBaseProjectFactoryString, project.Kind)) {
                res.AppendLine("Kind: Node.js");
                foreach (var prop in _interestingDteProperties) {
                    res.AppendLine(prop + ": " + GetProjectProperty(project, prop));
                }
                var njsProj = project.GetNodejsProject();
                if (njsProj != null) {
                    res.AppendLine(GetNodeJsProjectProperties(njsProj));
                }
            } else {
                res.AppendLine("Kind: " + project.Kind);
            }
            return res.ToString();
        }

        private static string GetNodeJsProjectProperties(Project.NodejsProjectNode project) {
            var res = new StringBuilder();
            res.AppendLine(GetProjectNpmInfo(project));
            res.AppendLine(GetProjectFileInfo(project));
            return res.ToString();
        }

        /// <summary>
        /// Stores information about a collection of files of a given type.
        /// </summary>
        private class FileTypeInfo {
            private int _count = 0;
            private int _maxLineLength = 0;
            private int _totalLineLength = 0;

            public int Count {
                get { return _count; }
            }

            public int MaxLineLength {
                get { return _maxLineLength; }
            }

            public int AverageLineLength {
                get { return _count > 0 ? _totalLineLength / _count : 0; }
            }

            public void UpdateForFile(string file) {
                try {
                    int length = File.ReadLines(file).Count();
                    ++_count;
                    _totalLineLength += length;
                    _maxLineLength = Math.Max(_maxLineLength, length);
                } catch (IOException) {
                    // noop
                }
            }
        }

        private static string GetProjectFileInfo(Project.NodejsProjectNode project) {
            var fileTypeInfo = new Dictionary<string, FileTypeInfo>();
            foreach (var node in project.DiskNodes) {
                var file = node.Key;
                var matchedExt = _interestingFileExtensions.Where(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (!string.IsNullOrEmpty(matchedExt)) {
                    var recordKey = string.Format("{0} ({1})", matchedExt, node.Value.ItemNode?.IsExcluded ?? true ? "excluded from project" : "included in project");
                    FileTypeInfo record;
                    if (!fileTypeInfo.TryGetValue(recordKey, out record)) {
                        record = fileTypeInfo[recordKey] = new FileTypeInfo();
                    }
                    record.UpdateForFile(file);
                }
            }

            var res = new StringBuilder();

            res.AppendLine("Project Info:");
            foreach (var entry in fileTypeInfo) {
                res.AppendLine(Indent(1, entry.Key + ":"));
                res.AppendLine(Indent(2, "Number of Files: " + entry.Value.Count));
                res.AppendLine(Indent(2, "Average Line Count: " + entry.Value.AverageLineLength));
                res.AppendLine(Indent(2, "Max Line Count: " + entry.Value.MaxLineLength));
            }

            return res.ToString();
        }

        private static string GetProjectNpmInfo(Project.NodejsProjectNode project) {
            var modules = project?.ModulesNode?.RootPackage?.Modules;
            if (modules == null) {
                return "";
            }

            var res = new StringBuilder();
            res.AppendLine(string.Format("Top Level Node Packages ({0}):", modules.Count()));
            foreach (var module in modules) {
                res.AppendLine(Indent(1, string.Format("{0} {1} ", module.Name, module.Version)));
            }
            return res.ToString();
        }

        private static string GetProjectProperty(EnvDTE.Project project, string name) {
            try {
                var item = project.Properties.Item(name);
                if (item != null && item.Value != null) {
                    return item.Value.ToString();
                }
            } catch {
                // noop
            }
            return "<undefined>";
        }

        private static string GetEventsAndStatsInfo() {
            var res = new StringBuilder();
            res.AppendLine("Logged events/stats:");

            try {
                var inMemLogger = NodejsPackage.Instance.GetComponentModel().GetService<InMemoryLogger>();
                res.AppendLine(inMemLogger.ToString());
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }
                res.AppendLine(Indent(1, "Failed to access event log."));
                res.AppendLine(ex.ToString());
            }
            return res.ToString();
        }

        private static string GetLoadedAssemblyInfo() {
            var res = new StringBuilder();
            res.AppendLine("Loaded assemblies:");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(assem => assem.FullName)) {
                var assemFileVersion = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).OfType<AssemblyFileVersionAttribute>().FirstOrDefault();

                res.AppendLine(Indent(1, string.Format("{0}, FileVersion={1}",
                    assembly.FullName,
                    assemFileVersion == null ? "(null)" : assemFileVersion.Version)));
            }
            return res.ToString();
        }

        private static string Indent(int count, string text) {
            var indent = new string(' ', count * 4);
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var indentedText = lines.Select(line =>
                string.IsNullOrWhiteSpace(line) ? line : indent + line);
            return string.Join(Environment.NewLine, indentedText);
        }
    }
}
