﻿using GitHgMirror.Common.Models;
using GitHgMirror.CommonTypes;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;

namespace GitHgMirror.Common.Drivers
{
    public class MirroringConfigurationPartDriver : ContentPartDriver<MirroringConfigurationPart>
    {
        public Localizer T { get; set; }


        public MirroringConfigurationPartDriver()
        {
            T = NullLocalizer.Instance;
        }


        protected override DriverResult Editor(MirroringConfigurationPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_MirroringConfiguration_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts.MirroringConfiguration",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(MirroringConfigurationPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            var gitUriIsWellFormed = Uri.IsWellFormedUriString(part.GitCloneUrl, UriKind.Absolute);
            var hgUriIsWellFormed = Uri.IsWellFormedUriString(part.HgCloneUrl, UriKind.Absolute);

            if (!gitUriIsWellFormed)
            {
                updater.AddModelError("GitUrlMalformed", T("Sorry but this git URL doesn't seem to be a valid URL :-(."));
            }

            if (!hgUriIsWellFormed)
            {
                updater.AddModelError("HgUrlMalformed", T("Sorry but this hg URL doesn't seem to be a valid URL :-(."));
            }

            if (gitUriIsWellFormed && hgUriIsWellFormed)
            {
                var gitUri = new Uri(part.GitCloneUrl);
                var hgUri = new Uri(part.HgCloneUrl);

                if (!part.GitUrlIsHgUrl)
                {
                    // Try to convert to known URL formats that are correctly handled by hg-git
                    var gitUriBuilder = new UriBuilder(gitUri);
                    gitUriBuilder.Port = 0;
                    if (gitUri.Scheme != "git+https")
                    {
                        gitUriBuilder.Scheme = "git+https";
                    }
                    if (!gitUri.PathAndQuery.EndsWith(".git"))
                    {
                        var path = gitUriBuilder.Path;
                        gitUriBuilder.Path = (path.EndsWith("/") ? path.Substring(0, path.Length - 1) : path) + ".git";
                    }
                    gitUri = gitUriBuilder.Uri;
                    part.GitCloneUrl = gitUri.ToString();
                }

                part.Status = MirroringStatus.New.ToString();
            }

            return Editor(part, shapeHelper);
        }
    }
}