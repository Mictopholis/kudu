﻿using System;
using System.Web;
using Kudu.Core.SourceControl.Git;
using Newtonsoft.Json.Linq;

namespace Kudu.Services.GitServer.ServiceHookHandlers
{
    public class CodebaseHqHandler : JsonServiceHookHandler
    {
        public CodebaseHqHandler(IGitServer gitServer)
            : base(gitServer)
        {
        }

        public override bool TryGetRepositoryInfo(HttpRequest request, out RepositoryInfo repositoryInfo)
        {
            repositoryInfo = null;
            if (request.UserAgent != null &&
                request.UserAgent.StartsWith("Codebasehq", StringComparison.OrdinalIgnoreCase))
            {
                return base.TryGetRepositoryInfo(request, out repositoryInfo);
            }
            return false;
        }

        protected override RepositoryInfo GetRepositoryInfo(HttpRequest request, JObject payload)
        {
            var info = base.GetRepositoryInfo(request, payload);

            // CodebaseHq format, see http://support.codebasehq.com/kb/howtos/repository-push-commit-notifications
            var repository = payload.Value<JObject>("repository");
            var urls = repository.Value<JObject>("clone_urls");
            info.IsPrivate = repository.Value<bool>("private");

            if (info.IsPrivate)
            {
                info.RepositoryUrl = urls.Value<string>("ssh");
            }
            else
            {
                // use http clone url if it's a public repo
                info.RepositoryUrl = urls.Value<string>("http");                
            }

            info.Deployer = "CodebaseHQ";
            return info;
        }
    }
}