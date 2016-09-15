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
using System.Diagnostics;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Commands {
    internal sealed class ShowDocumentationCommand : Command {
        public override void DoCommand(object sender, EventArgs args) {
            Process.Start(@"https://go.microsoft.com/fwlink/?linkid=785972");
        }

        public override int CommandId {
            get { return (int)PkgCmdId.cmdidShowDocumentation; }
        }
    }
}
