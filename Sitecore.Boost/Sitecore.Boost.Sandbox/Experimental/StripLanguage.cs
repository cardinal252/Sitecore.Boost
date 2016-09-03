using System;
using System.Linq;
using System.Web;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines.PreprocessRequest;
using Sitecore.Text;
using Sitecore.Web;

namespace RenderingsTest
{
    public class StripLanguage : Sitecore.Pipelines.PreprocessRequest.StripLanguage
    {
        public override void Process(PreprocessRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!Settings.Languages.AlwaysStripLanguage)
                return;
            Language language = ExtractLanguage(args.Context.Request);
            if (language == null)
                return;
            Context.Language = language;
            Context.Data.FilePathLanguage = language;
            RewriteUrl(args.Context, language);
            Tracer.Info(string.Format("Language changed to \"{0}\" as request url contains language embedded in the file path.", language.Name));
        }

        private static void RewriteUrl(HttpContext context, Language embeddedLanguage)
        {
            Assert.ArgumentNotNull(context, "context");
            Assert.ArgumentNotNull(embeddedLanguage, "embeddedLanguage");
            HttpRequest request = context.Request;
            string filePath = request.FilePath.Substring(embeddedLanguage.Name.Length + 1);
            if (!string.IsNullOrEmpty(filePath) && filePath.StartsWith(".", StringComparison.InvariantCulture))
                filePath = string.Empty;
            if (string.IsNullOrEmpty(filePath))
                filePath = string.Format("/{0}", Settings.DefaultPageName);
            if (!UseRedirect(filePath))
            {
                context.RewritePath(filePath, request.PathInfo, StringUtil.RemovePrefix('?', request.Url.Query));
            }
            else
            {
                UrlString urlString = new UrlString(filePath + request.Url.Query);
                urlString["sc_lang"] = embeddedLanguage.Name;
                context.Response.Redirect(urlString.ToString(), true);
            }
        }

        private static bool UseRedirect(string filePath)
        {
            Assert.IsNotNullOrEmpty(filePath, "filePath");
            return Settings.RedirectUrlPrefixes.Any(path => filePath.StartsWith(path, StringComparison.InvariantCulture));
        }

        private Language ExtractLanguage(HttpRequest request)
        {
            Assert.ArgumentNotNull(request, "request");
            string languageName = WebUtil.ExtractLanguageName(request.FilePath);
            if (string.IsNullOrEmpty(languageName))
            {
                return null;
            }
            Language result;

            // this was the problem line - getting the culture info every time is SLOW.. It's also a ridiculous idea!!
            // return !Language.TryParse(languageName, out result) ? null : result;

            // so I replaced with this...
            return !QuickerLanguageParser.TryParse(languageName, out result) ? null : result;
        }


    }
}
